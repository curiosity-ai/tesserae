namespace Tesserae.Benchmark.Measure;

internal sealed record HotMethod(
    string FunctionName,
    string ScriptUrl,
    int    LineNumber,
    double SelfTimeMs,
    double TotalTimeMs,
    int    SampleHits);

internal static class CpuProfileAnalyzer
{
    // Aggregates a V8 CPU profile into a sorted list of self-hot methods.
    // V8 reports timeDeltas in microseconds and assigns each sample to the
    // node currently on top of the call stack — accumulating those deltas
    // gives us self time; propagating each sample's delta up its ancestor
    // chain gives us total time.
    public static IReadOnlyList<HotMethod> AnalyzeHotMethods(V8CpuProfile profile, int top)
    {
        if (profile.Samples == null || profile.TimeDeltas == null || profile.Samples.Count == 0)
            return Array.Empty<HotMethod>();

        var byId      = profile.Nodes.ToDictionary(n => n.Id);
        var selfMicros = new Dictionary<int, long>();
        var totalMicros = new Dictionary<int, long>();
        var hits      = new Dictionary<int, int>();

        var parentOf = BuildParentMap(profile);

        var count = Math.Min(profile.Samples.Count, profile.TimeDeltas.Count);
        for (int i = 0; i < count; i++)
        {
            var sampleId  = profile.Samples[i];
            var delta     = profile.TimeDeltas[i];
            if (delta < 0) delta = 0;

            selfMicros[sampleId] = selfMicros.GetValueOrDefault(sampleId) + delta;
            hits[sampleId]       = hits.GetValueOrDefault(sampleId) + 1;

            var cursor = sampleId;
            var seen   = new HashSet<int>();
            while (cursor != 0 && seen.Add(cursor))
            {
                totalMicros[cursor] = totalMicros.GetValueOrDefault(cursor) + delta;
                if (!parentOf.TryGetValue(cursor, out var parent)) break;
                cursor = parent;
            }
        }

        return selfMicros
            .Where(kv => byId.ContainsKey(kv.Key))
            .Select(kv =>
            {
                var node = byId[kv.Key];
                if (IsSyntheticNode(node)) return null;
                return new HotMethod(
                    FunctionName: PrettifyFunctionName(node.CallFrame.FunctionName),
                    ScriptUrl:    StripBaseUrl(node.CallFrame.Url),
                    LineNumber:   node.CallFrame.LineNumber,
                    SelfTimeMs:   kv.Value / 1000.0,
                    TotalTimeMs:  totalMicros.GetValueOrDefault(kv.Key) / 1000.0,
                    SampleHits:   hits.GetValueOrDefault(kv.Key));
            })
            .Where(h => h != null)
            .Cast<HotMethod>()
            .OrderByDescending(h => h.SelfTimeMs)
            .Take(top)
            .ToList();
    }

    private static Dictionary<int, int> BuildParentMap(V8CpuProfile profile)
    {
        var map = new Dictionary<int, int>();
        foreach (var node in profile.Nodes)
        {
            if (node.Children == null) continue;
            foreach (var child in node.Children)
                map[child] = node.Id;
        }
        return map;
    }

    private static bool IsSyntheticNode(V8CpuNode node)
    {
        var name = node.CallFrame.FunctionName;
        return name == "(root)"
            || name == "(idle)"
            || name == "(program)"
            || name == "(garbage collector)";
    }

    private static string PrettifyFunctionName(string name)
    {
        if (string.IsNullOrEmpty(name)) return "(anonymous)";
        return name;
    }

    private static string StripBaseUrl(string url)
    {
        if (string.IsNullOrEmpty(url)) return "";
        // We serve everything off http://localhost:PORT/ — collapse that prefix
        // so report rows stay readable.
        int idx = url.IndexOf("//");
        if (idx >= 0)
        {
            int slash = url.IndexOf('/', idx + 2);
            if (slash >= 0) return url[slash..];
        }
        return url;
    }
}
