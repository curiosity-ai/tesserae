using System.Diagnostics;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Playwright;

namespace Tesserae.Benchmark.Measure;

internal sealed class BenchmarkRunner
{
    private readonly BenchmarkOptions _options;
    private readonly string _baseUrl;

    public BenchmarkRunner(BenchmarkOptions options, string baseUrl)
    {
        _options = options;
        _baseUrl = baseUrl;
    }

    public async Task<BenchmarkReport> RunAsync(CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(_options.OutputDirectory);

        Console.WriteLine("[playwright] launching chromium");

        using var playwright = await Playwright.CreateAsync();

        var launchOptions = new BrowserTypeLaunchOptions
        {
            Headless = !_options.Headed,
            // Disabling extensions / background networking gives us a quieter
            // baseline and keeps non-app code out of the CPU profile.
            Args = new[]
            {
                "--disable-extensions",
                "--disable-background-networking",
                "--disable-component-update",
                "--disable-default-apps",
                "--disable-sync",
                "--no-first-run",
                "--no-default-browser-check",
                "--js-flags=--expose-gc",
                "--enable-precise-memory-info"
            }
        };

        await using var browser = await playwright.Chromium.LaunchAsync(launchOptions);
        var context = await browser.NewContextAsync(new BrowserNewContextOptions
        {
            ViewportSize = new ViewportSize { Width = 1280, Height = 800 }
        });
        var page    = await context.NewPageAsync();
        var cdp     = await context.NewCDPSessionAsync(page);

        var consoleErrors = new List<string>();
        page.PageError += (_, e) => consoleErrors.Add(e);
        page.Console   += (_, msg) =>
        {
            if (msg.Type == "error" || msg.Type == "warning")
                consoleErrors.Add($"[{msg.Type}] {msg.Text}");
        };

        var landingUrl = _options.UseMinified
            ? $"{_baseUrl}/index.html?min=1"  // index.html doesn't actually do anything with this — kept as a marker for the report.
            : $"{_baseUrl}/index.html";

        Console.WriteLine($"[playwright] navigating to {landingUrl}");
        await page.GotoAsync(landingUrl, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle, Timeout = 30000 });

        // Wait until the harness publishes its API.
        await page.WaitForFunctionAsync("() => window.tssBenchmark && window.tssBenchmark.ready === true", null, new PageWaitForFunctionOptions { Timeout = 30000 });

        var samples = await page.EvaluateAsync<string[]>("() => window.tssBenchmark.samples");
        if (samples == null || samples.Length == 0)
            throw new InvalidOperationException("Harness exposed no samples.");

        if (!string.IsNullOrEmpty(_options.SampleFilter))
        {
            var re = new Regex(_options.SampleFilter, RegexOptions.IgnoreCase);
            samples = samples.Where(s => re.IsMatch(s)).ToArray();
            if (samples.Length == 0)
                throw new InvalidOperationException($"Filter '{_options.SampleFilter}' matched zero samples.");
        }

        Console.WriteLine($"[playwright] harness exposes {samples.Length} samples");

        // --- enable profilers ---------------------------------------------------
        await cdp.SendAsync("Performance.enable");
        await cdp.SendAsync("Profiler.enable");
        await cdp.SendAsync("HeapProfiler.enable");

        await cdp.SendAsync("Profiler.setSamplingInterval", new Dictionary<string, object>
        {
            ["interval"] = _options.CpuSamplingIntervalMicros
        });

        // Force a baseline GC so the heap profile / leak diagnosis aren't
        // dominated by load-time scaffolding.
        await cdp.SendAsync("HeapProfiler.collectGarbage");
        await Task.Delay(250, cancellationToken);

        await cdp.SendAsync("Profiler.start");
        await cdp.SendAsync("HeapProfiler.startSampling", new Dictionary<string, object>
        {
            ["samplingInterval"] = _options.HeapSamplingIntervalBytes
        });

        // --- main measurement loop ---------------------------------------------
        var probes = new List<MemoryProbe>();
        var sampleStats = new Dictionary<string, SampleStats>();

        var runStart = DateTimeOffset.UtcNow;
        var deadline = runStart + _options.Duration;
        var lastProbe = runStart - _options.MemoryProbeInterval;

        int cursor = 0;
        int totalRenders = 0;
        int totalExercises = 0;

        Console.WriteLine($"[run] cycling samples for {_options.Duration:c}");

        while (DateTimeOffset.UtcNow < deadline && !cancellationToken.IsCancellationRequested)
        {
            var sampleName = samples[cursor];
            cursor = (cursor + 1) % samples.Length;

            var sampleStart = Stopwatch.StartNew();

            try
            {
                // Drive the harness via routing — the page's router observes the
                // hash and rebuilds the DOM, which is exactly what we want to
                // measure.
                await page.EvaluateAsync("name => window.tssBenchmark.render(name)", sampleName);

                // Give the DOM a chance to settle before exercising it.
                await page.WaitForFunctionAsync("name => window.tssBenchmark.current() === name", sampleName, new PageWaitForFunctionOptions { Timeout = 10000 });

                // Lightweight per-sample interactions implemented inside the harness.
                var interactions = await page.EvaluateAsync<int>("() => window.tssBenchmark.exercise()");

                await Task.Delay(_options.SamplePerSample, cancellationToken);

                totalRenders++;
                totalExercises++;

                if (!sampleStats.TryGetValue(sampleName, out var stat))
                    sampleStats[sampleName] = stat = new SampleStats(sampleName);
                stat.Renders++;
                stat.Interactions += interactions;
                stat.WallTimeMs   += sampleStart.Elapsed.TotalMilliseconds;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[run] sample {sampleName} failed: {ex.Message}");
                if (!sampleStats.TryGetValue(sampleName, out var stat))
                    sampleStats[sampleName] = stat = new SampleStats(sampleName);
                stat.Failures++;
            }

            // Sample memory probes on their own cadence so we don't tie them
            // 1-to-1 to renders (some samples are much slower than others).
            if (DateTimeOffset.UtcNow - lastProbe >= _options.MemoryProbeInterval)
            {
                lastProbe = DateTimeOffset.UtcNow;
                var probe = await CollectMemoryProbe(page, cdp, sampleName);
                probes.Add(probe);
                if (probes.Count == 1 || probes.Count % 10 == 0)
                {
                    Console.WriteLine($"[mem ] {DateTime.Now:HH:mm:ss} usedHeap={MemoryTrace.Format(probe.UsedJsHeapBytes)} nodes={probe.NodeCount} listeners={probe.ListenerCount} sample={sampleName}");
                }
            }
        }

        Console.WriteLine($"[run ] done — {totalRenders} renders across {sampleStats.Count} samples");

        // Final post-GC probe to make leak detection less noisy.
        await cdp.SendAsync("HeapProfiler.collectGarbage");
        await Task.Delay(500, cancellationToken);
        probes.Add(await CollectMemoryProbe(page, cdp, "<final-after-gc>"));

        // --- stop profilers + collect raw output -------------------------------
        Console.WriteLine("[stop] collecting profiles");
        var cpuJson  = ExtractProfileJson(await cdp.SendAsync("Profiler.stop"));
        var heapJson = ExtractProfileJson(await cdp.SendAsync("HeapProfiler.stopSampling"));

        File.WriteAllText(Path.Combine(_options.OutputDirectory, "cpu-profile.json"),  cpuJson);
        File.WriteAllText(Path.Combine(_options.OutputDirectory, "heap-profile.json"), heapJson);

        var cpuProfile  = JsonSerializer.Deserialize<V8CpuProfile>(cpuJson)  ?? new V8CpuProfile();
        var heapProfile = JsonSerializer.Deserialize<V8HeapProfile>(heapJson) ?? new V8HeapProfile();

        var hotMethods    = CpuProfileAnalyzer.AnalyzeHotMethods(cpuProfile, _options.TopMethods);
        var topAllocators = HeapProfileAnalyzer.AnalyzeAllocators(heapProfile, _options.TopAllocators);
        var leak          = MemoryTrace.Diagnose(probes);

        var report = new BenchmarkReport(
            StartedUtc:          runStart,
            FinishedUtc:         DateTimeOffset.UtcNow,
            HarnessUrl:          landingUrl,
            SamplesCycled:       sampleStats.Count,
            TotalRenders:        totalRenders,
            TotalInteractions:   sampleStats.Values.Sum(s => s.Interactions),
            Failures:            sampleStats.Values.Sum(s => s.Failures),
            SampleStats:         sampleStats.Values.OrderBy(s => s.Name).ToList(),
            CpuProfileDurationMs: (cpuProfile.EndTime - cpuProfile.StartTime) / 1000.0,
            CpuSampleCount:      cpuProfile.Samples?.Count ?? 0,
            HeapSampledBytes:    HeapProfileAnalyzer.TotalSampledBytes(heapProfile),
            HotMethods:          hotMethods,
            TopAllocators:       topAllocators,
            MemoryProbes:        probes,
            LeakDiagnosis:       leak,
            ConsoleErrors:       consoleErrors.Distinct().Take(50).ToList());

        WriteMemoryCsv(probes);

        await context.CloseAsync();
        return report;
    }

    private async Task<MemoryProbe> CollectMemoryProbe(IPage page, ICDPSession cdp, string sampleName)
    {
        long usedHeap   = 0;
        long totalHeap  = 0;
        long nodes      = 0;
        long listeners  = 0;
        long documents  = 0;
        long frames     = 0;
        long jsListeners = 0;

        try
        {
            var metricsResp = await cdp.SendAsync("Performance.getMetrics");
            if (metricsResp != null && metricsResp.Value.TryGetProperty("metrics", out var arr))
            {
                foreach (var m in arr.EnumerateArray())
                {
                    var name  = m.GetProperty("name").GetString();
                    var value = m.GetProperty("value").GetDouble();
                    switch (name)
                    {
                        case "JSHeapUsedSize":   usedHeap   = (long)value; break;
                        case "JSHeapTotalSize":  totalHeap  = (long)value; break;
                        case "Nodes":            nodes      = (long)value; break;
                        case "Documents":        documents  = (long)value; break;
                        case "Frames":           frames     = (long)value; break;
                        case "JSEventListeners": jsListeners = (long)value; break;
                    }
                }
            }
        }
        catch { }

        try
        {
            var counters = await cdp.SendAsync("Memory.getDOMCounters");
            if (counters != null)
            {
                if (counters.Value.TryGetProperty("nodes", out var n)) nodes = n.GetInt64();
                if (counters.Value.TryGetProperty("jsEventListeners", out var l)) listeners = l.GetInt64();
                if (counters.Value.TryGetProperty("documents", out var d)) documents = d.GetInt64();
            }
        }
        catch { }

        if (listeners == 0) listeners = jsListeners;

        if (usedHeap == 0)
        {
            try { usedHeap = await page.EvaluateAsync<long>("() => (performance.memory && performance.memory.usedJSHeapSize) ? Math.floor(performance.memory.usedJSHeapSize) : 0"); }
            catch { }
        }

        return new MemoryProbe(
            Timestamp:           DateTimeOffset.UtcNow,
            CurrentSample:       sampleName,
            UsedJsHeapBytes:     usedHeap,
            TotalJsHeapBytes:    totalHeap,
            NodeCount:           nodes,
            ListenerCount:       listeners,
            DocumentCount:       documents,
            FrameCount:          frames,
            JsEventListenerCount: jsListeners);
    }

    private void WriteMemoryCsv(IReadOnlyList<MemoryProbe> probes)
    {
        var path = Path.Combine(_options.OutputDirectory, "memory-trace.csv");
        using var w = new StreamWriter(path);
        w.WriteLine("timestamp,sample,used_js_heap_bytes,total_js_heap_bytes,nodes,listeners,documents,frames,js_event_listeners");
        foreach (var p in probes)
        {
            w.WriteLine($"{p.Timestamp:O},{Csv(p.CurrentSample)},{p.UsedJsHeapBytes},{p.TotalJsHeapBytes},{p.NodeCount},{p.ListenerCount},{p.DocumentCount},{p.FrameCount},{p.JsEventListenerCount}");
        }
    }

    private static string Csv(string s) =>
        s.Contains(',') || s.Contains('"') ? "\"" + s.Replace("\"", "\"\"") + "\"" : s;

    private static string ExtractProfileJson(JsonElement? response)
    {
        if (!response.HasValue) return "{}";
        if (response.Value.TryGetProperty("profile", out var profile))
            return profile.GetRawText();
        return response.Value.GetRawText();
    }
}

internal sealed class SampleStats
{
    public SampleStats(string name) { Name = name; }
    public string Name { get; }
    public int Renders { get; set; }
    public int Interactions { get; set; }
    public int Failures { get; set; }
    public double WallTimeMs { get; set; }
}
