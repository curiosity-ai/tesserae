namespace Tesserae.Benchmark.Measure;

internal sealed class BenchmarkOptions
{
    public string HarnessPath { get; set; } = string.Empty;
    public string OutputDirectory { get; set; } = string.Empty;
    public TimeSpan Duration { get; set; } = TimeSpan.FromMinutes(3);
    public int CpuSamplingIntervalMicros { get; set; } = 100;
    public int HeapSamplingIntervalBytes { get; set; } = 32 * 1024;
    public TimeSpan SamplePerSample { get; set; } = TimeSpan.FromMilliseconds(750);
    public TimeSpan MemoryProbeInterval { get; set; } = TimeSpan.FromSeconds(2);
    public bool Headed { get; set; }
    public int Port { get; set; }
    public bool SkipBuild { get; set; }
    public int TopMethods { get; set; } = 30;
    public int TopAllocators { get; set; } = 30;
    public bool UseMinified { get; set; }
    public string? SampleFilter { get; set; }

    public static BenchmarkOptions Parse(string[] args)
    {
        var o = new BenchmarkOptions();

        var here = AppContext.BaseDirectory;

        // Default harness lookup: walk up from bin/ until we find the sibling
        // Tesserae.Benchmark.Tests project, then point at its h5 output. This
        // makes `dotnet run` work out of the box from a normal checkout.
        var defaultHarness = FindDefaultHarness(here);
        if (defaultHarness != null) o.HarnessPath = defaultHarness;

        o.OutputDirectory = Path.Combine(Environment.CurrentDirectory, "benchmark-report-" + DateTime.Now.ToString("yyyyMMdd-HHmmss"));

        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--harness":
                case "-h":
                    o.HarnessPath = args[++i];
                    break;
                case "--output":
                case "-o":
                    o.OutputDirectory = args[++i];
                    break;
                case "--duration":
                case "-d":
                    o.Duration = ParseDuration(args[++i]);
                    break;
                case "--cpu-interval-us":
                    o.CpuSamplingIntervalMicros = int.Parse(args[++i]);
                    break;
                case "--heap-interval-bytes":
                    o.HeapSamplingIntervalBytes = int.Parse(args[++i]);
                    break;
                case "--sample-time":
                    o.SamplePerSample = ParseDuration(args[++i]);
                    break;
                case "--memory-interval":
                    o.MemoryProbeInterval = ParseDuration(args[++i]);
                    break;
                case "--port":
                case "-p":
                    o.Port = int.Parse(args[++i]);
                    break;
                case "--headed":
                    o.Headed = true;
                    break;
                case "--skip-build":
                    o.SkipBuild = true;
                    break;
                case "--minified":
                    o.UseMinified = true;
                    break;
                case "--top-methods":
                    o.TopMethods = int.Parse(args[++i]);
                    break;
                case "--top-allocators":
                    o.TopAllocators = int.Parse(args[++i]);
                    break;
                case "--samples":
                    o.SampleFilter = args[++i];
                    break;
                case "--help":
                case "-?":
                    PrintHelp();
                    Environment.Exit(0);
                    break;
                default:
                    Console.Error.WriteLine($"Unknown option: {args[i]}");
                    PrintHelp();
                    Environment.Exit(1);
                    break;
            }
        }

        return o;
    }

    private static string? FindDefaultHarness(string startDir)
    {
        var dir = new DirectoryInfo(startDir);
        for (int i = 0; i < 8 && dir != null; i++, dir = dir.Parent)
        {
            var candidate = Path.Combine(dir.FullName, "Tesserae.Benchmark.Tests", "bin", "Debug", "netstandard2.0", "h5");
            if (Directory.Exists(candidate) && File.Exists(Path.Combine(candidate, "index.html")))
                return candidate;
        }
        // If not yet built, return the expected location so we can build into it.
        dir = new DirectoryInfo(startDir);
        for (int i = 0; i < 8 && dir != null; i++, dir = dir.Parent)
        {
            if (Directory.Exists(Path.Combine(dir.FullName, "Tesserae.Benchmark.Tests")))
                return Path.Combine(dir.FullName, "Tesserae.Benchmark.Tests", "bin", "Debug", "netstandard2.0", "h5");
        }
        return null;
    }

    private static TimeSpan ParseDuration(string s)
    {
        s = s.Trim();
        if (s.EndsWith("ms")) return TimeSpan.FromMilliseconds(double.Parse(s[..^2]));
        if (s.EndsWith("s"))  return TimeSpan.FromSeconds(double.Parse(s[..^1]));
        if (s.EndsWith("m"))  return TimeSpan.FromMinutes(double.Parse(s[..^1]));
        if (s.EndsWith("h"))  return TimeSpan.FromHours(double.Parse(s[..^1]));
        return TimeSpan.FromSeconds(double.Parse(s));
    }

    public static void PrintHelp()
    {
        Console.WriteLine("""
Tesserae.Benchmark.Measure - Playwright/CDP-based performance profiler for Tesserae.Benchmark.Tests.

Usage:
  dotnet run --project Tesserae.Benchmark.Measure -- [options]

Options:
  --harness <path>           Path to the built Tesserae.Benchmark.Tests h5 output directory.
                             (defaults to ../Tesserae.Benchmark.Tests/bin/Debug/netstandard2.0/h5)
  --output <path>            Directory to write reports into (default: ./benchmark-report-<ts>)
  --duration <time>          Total measurement window (default: 3m). Accepts 250ms, 30s, 3m, 1h.
  --sample-time <time>       How long to keep each sample mounted (default: 750ms).
  --memory-interval <time>   Cadence of heap-usage probes (default: 2s).
  --cpu-interval-us <us>     V8 CPU sampler interval in microseconds (default: 100).
  --heap-interval-bytes <n>  V8 heap sampling interval in bytes (default: 32768).
  --port <port>              Local HTTP port (default: random free port).
  --headed                   Run Chromium with a visible window for inspection.
  --skip-build               Don't rebuild Tesserae.Benchmark.Tests before measuring.
  --minified                 Load app.min.js + tss.min.js for a release-shaped profile.
  --top-methods <n>          Hot-methods table size (default: 30).
  --top-allocators <n>       Top-allocators table size (default: 30).
  --samples <regex>          Only cycle samples whose names match this regex.

Output: writes cpu-profile.json, heap-profile.json, memory-trace.csv, report.md,
        and report.json into the output directory.
""");
    }
}
