using System.Text;
using System.Text.Json;

namespace Tesserae.Benchmark.Measure;

internal static class ReportWriter
{
    public static void Write(BenchmarkReport report, BenchmarkOptions options)
    {
        Directory.CreateDirectory(options.OutputDirectory);

        var markdown = BuildMarkdown(report, options);
        File.WriteAllText(Path.Combine(options.OutputDirectory, "report.md"), markdown);

        var json = JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(Path.Combine(options.OutputDirectory, "report.json"), json);

        Console.WriteLine();
        Console.WriteLine(markdown);
        Console.WriteLine();
        Console.WriteLine($"[done] artifacts written to {options.OutputDirectory}");
    }

    private static string BuildMarkdown(BenchmarkReport r, BenchmarkOptions options)
    {
        var b = new StringBuilder();

        b.AppendLine("# Tesserae benchmark report");
        b.AppendLine();
        b.AppendLine($"- Started: `{r.StartedUtc:u}`  ");
        b.AppendLine($"- Finished: `{r.FinishedUtc:u}`  ");
        b.AppendLine($"- Wall time: `{(r.FinishedUtc - r.StartedUtc):c}`  ");
        b.AppendLine($"- Harness: `{r.HarnessUrl}`  ");
        b.AppendLine($"- CPU sampling interval: `{options.CpuSamplingIntervalMicros} µs`  ");
        b.AppendLine($"- Heap sampling interval: `{options.HeapSamplingIntervalBytes} bytes`  ");
        b.AppendLine($"- Time per sample: `{options.SamplePerSample.TotalMilliseconds:F0} ms`  ");
        b.AppendLine();

        b.AppendLine("## Summary");
        b.AppendLine();
        b.AppendLine($"- Samples cycled: **{r.SamplesCycled}**");
        b.AppendLine($"- Total renders: **{r.TotalRenders}**");
        b.AppendLine($"- Total interactions fired: **{r.TotalInteractions}**");
        b.AppendLine($"- Failures: **{r.Failures}**");
        b.AppendLine($"- CPU sample count: **{r.CpuSampleCount}** over {r.CpuProfileDurationMs:F0} ms");
        b.AppendLine($"- Heap sampled bytes: **{MemoryTrace.Format(r.HeapSampledBytes)}**");
        b.AppendLine();

        b.AppendLine("## Memory leak diagnosis");
        b.AppendLine();
        var leak = r.LeakDiagnosis;
        b.AppendLine($"> **{leak.Verdict}**");
        b.AppendLine();
        b.AppendLine("|                   | first probe | last probe | growth/min |");
        b.AppendLine("|-------------------|-------------|------------|------------|");
        b.AppendLine($"| Used JS heap     | {MemoryTrace.Format(leak.FirstUsedHeapBytes)} | {MemoryTrace.Format(leak.LastUsedHeapBytes)} | {MemoryTrace.Format(leak.HeapGrowthBytesPerMin)} (R²={leak.RSquaredHeap:F2}) |");
        b.AppendLine($"| DOM nodes        | {leak.FirstNodeCount} | {leak.LastNodeCount} | {leak.NodeGrowthPerMin:+0.#;-0.#;0} |");
        b.AppendLine($"| JS listeners     | {leak.FirstListenerCount} | {leak.LastListenerCount} | {leak.ListenerGrowthPerMin:+0.#;-0.#;0} |");
        b.AppendLine();

        b.AppendLine($"## Hot methods (top {r.HotMethods.Count} by self CPU time)");
        b.AppendLine();
        if (r.HotMethods.Count == 0)
        {
            b.AppendLine("_No CPU samples captured._");
        }
        else
        {
            b.AppendLine("| # | Function | Self ms | Total ms | Hits | Source |");
            b.AppendLine("|---|----------|--------:|---------:|-----:|--------|");
            int rank = 1;
            foreach (var m in r.HotMethods)
            {
                b.AppendLine($"| {rank++} | `{Sanitize(m.FunctionName)}` | {m.SelfTimeMs:F1} | {m.TotalTimeMs:F1} | {m.SampleHits} | `{Sanitize(m.ScriptUrl)}:{m.LineNumber}` |");
            }
        }
        b.AppendLine();

        b.AppendLine($"## Top allocators (top {r.TopAllocators.Count} by self bytes)");
        b.AppendLine();
        if (r.TopAllocators.Count == 0)
        {
            b.AppendLine("_No heap allocation samples captured._");
        }
        else
        {
            b.AppendLine("| # | Function | Self | Subtree | Source |");
            b.AppendLine("|---|----------|-----:|--------:|--------|");
            int rank = 1;
            foreach (var a in r.TopAllocators)
            {
                b.AppendLine($"| {rank++} | `{Sanitize(a.FunctionName)}` | {MemoryTrace.Format(a.SelfBytes)} | {MemoryTrace.Format(a.TotalBytes)} | `{Sanitize(a.ScriptUrl)}:{a.LineNumber}` |");
            }
        }
        b.AppendLine();

        b.AppendLine("## Per-sample stats");
        b.AppendLine();
        b.AppendLine("| Sample | Renders | Interactions | Wall ms | Failures |");
        b.AppendLine("|--------|--------:|-------------:|--------:|---------:|");
        foreach (var s in r.SampleStats)
        {
            b.AppendLine($"| {Sanitize(s.Name)} | {s.Renders} | {s.Interactions} | {s.WallTimeMs:F0} | {s.Failures} |");
        }
        b.AppendLine();

        if (r.ConsoleErrors.Count > 0)
        {
            b.AppendLine("## Console errors / warnings");
            b.AppendLine();
            foreach (var err in r.ConsoleErrors)
            {
                b.AppendLine($"- `{Sanitize(err)}`");
            }
            b.AppendLine();
        }

        b.AppendLine("## Artifacts");
        b.AppendLine();
        b.AppendLine("- `cpu-profile.json` — open with chrome://inspect, DevTools Performance tab, or Speedscope");
        b.AppendLine("- `heap-profile.json` — V8 sampling allocation profile, open in Chrome DevTools Memory tab (\"Load profile\")");
        b.AppendLine("- `memory-trace.csv` — per-probe time series for trend plotting");
        b.AppendLine("- `report.json` — machine-readable view of this report");
        return b.ToString();
    }

    private static string Sanitize(string s) =>
        string.IsNullOrEmpty(s) ? "" : s.Replace("|", "\\|").Replace("`", "'");
}
