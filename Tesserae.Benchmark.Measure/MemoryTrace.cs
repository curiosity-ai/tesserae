namespace Tesserae.Benchmark.Measure;

internal sealed record MemoryProbe(
    DateTimeOffset Timestamp,
    string         CurrentSample,
    long           UsedJsHeapBytes,
    long           TotalJsHeapBytes,
    long           NodeCount,
    long           ListenerCount,
    long           DocumentCount,
    long           FrameCount,
    long           JsEventListenerCount);

internal sealed record LeakDiagnosis(
    long   FirstUsedHeapBytes,
    long   LastUsedHeapBytes,
    long   FirstNodeCount,
    long   LastNodeCount,
    long   FirstListenerCount,
    long   LastListenerCount,
    double HeapGrowthBytesPerMin,
    double NodeGrowthPerMin,
    double ListenerGrowthPerMin,
    double RSquaredHeap,
    string Verdict);

internal static class MemoryTrace
{
    public static LeakDiagnosis Diagnose(IReadOnlyList<MemoryProbe> probes)
    {
        if (probes.Count < 2)
        {
            return new LeakDiagnosis(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, "insufficient data");
        }

        var first = probes[0];
        var last  = probes[^1];

        // Linear regression on (elapsedSeconds, usedHeap) to compute growth rate and goodness-of-fit.
        var times  = probes.Select(p => (p.Timestamp - first.Timestamp).TotalSeconds).ToArray();
        var heaps  = probes.Select(p => (double)p.UsedJsHeapBytes).ToArray();

        var (slope, _, r2) = LinearRegression(times, heaps);

        var nodeSlope = LinearRegression(times, probes.Select(p => (double)p.NodeCount).ToArray()).slope;
        var listSlope = LinearRegression(times, probes.Select(p => (double)p.ListenerCount).ToArray()).slope;

        var heapPerMin = slope     * 60.0;
        var nodePerMin = nodeSlope * 60.0;
        var listPerMin = listSlope * 60.0;

        // Heuristic verdict. The thresholds intentionally err on the side of
        // flagging something so reviewers see the trend; final attribution is
        // expected to come from comparing this against a baseline run.
        string verdict;
        if (heapPerMin > 4 * 1024 * 1024 && r2 > 0.6)
            verdict = $"likely leak: heap grew {Format(heapPerMin)}/min (R²={r2:F2})";
        else if (nodePerMin > 500 && r2 > 0.4)
            verdict = $"detached-DOM suspected: +{nodePerMin:F0} DOM nodes/min";
        else if (listPerMin > 200 && r2 > 0.4)
            verdict = $"listener leak suspected: +{listPerMin:F0} listeners/min";
        else if (heapPerMin > 1 * 1024 * 1024)
            verdict = $"mild growth: heap +{Format(heapPerMin)}/min (R²={r2:F2})";
        else
            verdict = $"stable: heap drift {Format(heapPerMin)}/min";

        return new LeakDiagnosis(
            FirstUsedHeapBytes:   first.UsedJsHeapBytes,
            LastUsedHeapBytes:    last.UsedJsHeapBytes,
            FirstNodeCount:       first.NodeCount,
            LastNodeCount:        last.NodeCount,
            FirstListenerCount:   first.ListenerCount,
            LastListenerCount:    last.ListenerCount,
            HeapGrowthBytesPerMin: heapPerMin,
            NodeGrowthPerMin:      nodePerMin,
            ListenerGrowthPerMin:  listPerMin,
            RSquaredHeap:          r2,
            Verdict:               verdict);
    }

    private static (double slope, double intercept, double r2) LinearRegression(double[] x, double[] y)
    {
        int n = x.Length;
        if (n < 2) return (0, 0, 0);

        double sumX = 0, sumY = 0;
        for (int i = 0; i < n; i++) { sumX += x[i]; sumY += y[i]; }
        var meanX = sumX / n;
        var meanY = sumY / n;

        double num = 0, denX = 0, denY = 0;
        for (int i = 0; i < n; i++)
        {
            var dx = x[i] - meanX;
            var dy = y[i] - meanY;
            num  += dx * dy;
            denX += dx * dx;
            denY += dy * dy;
        }

        if (denX == 0) return (0, meanY, 0);

        var slope     = num / denX;
        var intercept = meanY - slope * meanX;
        var r2        = denY == 0 ? 1.0 : (num * num) / (denX * denY);
        return (slope, intercept, r2);
    }

    public static string Format(double bytes)
    {
        var sign = bytes < 0 ? "-" : "+";
        var abs  = Math.Abs(bytes);
        if (abs >= 1024 * 1024 * 1024) return $"{sign}{abs / (1024.0 * 1024.0 * 1024.0):F2} GiB";
        if (abs >= 1024 * 1024)        return $"{sign}{abs / (1024.0 * 1024.0):F2} MiB";
        if (abs >= 1024)               return $"{sign}{abs / 1024.0:F2} KiB";
        return $"{sign}{abs:F0} B";
    }
}
