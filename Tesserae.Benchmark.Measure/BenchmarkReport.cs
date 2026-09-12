namespace Tesserae.Benchmark.Measure;

internal sealed record BenchmarkReport(
    DateTimeOffset                StartedUtc,
    DateTimeOffset                FinishedUtc,
    string                        HarnessUrl,
    int                           SamplesCycled,
    int                           TotalRenders,
    int                           TotalInteractions,
    int                           Failures,
    IReadOnlyList<SampleStats>    SampleStats,
    double                        CpuProfileDurationMs,
    int                           CpuSampleCount,
    long                          HeapSampledBytes,
    IReadOnlyList<HotMethod>      HotMethods,
    IReadOnlyList<TopAllocator>   TopAllocators,
    IReadOnlyList<MemoryProbe>    MemoryProbes,
    LeakDiagnosis                 LeakDiagnosis,
    IReadOnlyList<string>         ConsoleErrors);
