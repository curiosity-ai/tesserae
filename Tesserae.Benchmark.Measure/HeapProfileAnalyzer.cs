namespace Tesserae.Benchmark.Measure;

internal sealed record TopAllocator(
    string FunctionName,
    string ScriptUrl,
    int    LineNumber,
    long   SelfBytes,
    long   TotalBytes,
    int    SampleCount);

internal static class HeapProfileAnalyzer
{
    public static IReadOnlyList<TopAllocator> AnalyzeAllocators(V8HeapProfile profile, int top)
    {
        var rows = new List<TopAllocator>();
        Walk(profile.Head, rows);

        return rows
            .Where(r => r.SelfBytes > 0)
            .OrderByDescending(r => r.SelfBytes)
            .Take(top)
            .ToList();
    }

    private static long Walk(V8HeapNode node, List<TopAllocator> rows)
    {
        long subtotal = node.SelfSize;
        foreach (var child in node.Children)
            subtotal += Walk(child, rows);

        if (!IsSyntheticNode(node))
        {
            rows.Add(new TopAllocator(
                FunctionName: PrettifyFunctionName(node.CallFrame.FunctionName),
                ScriptUrl:    StripBaseUrl(node.CallFrame.Url),
                LineNumber:   node.CallFrame.LineNumber,
                SelfBytes:    node.SelfSize,
                TotalBytes:   subtotal,
                SampleCount:  0));
        }

        return subtotal;
    }

    public static long TotalSampledBytes(V8HeapProfile profile) =>
        profile.Samples.Sum(s => s.Size);

    private static bool IsSyntheticNode(V8HeapNode node)
    {
        var name = node.CallFrame.FunctionName;
        return name == "(root)"
            || name == "(GC)"
            || (string.IsNullOrEmpty(name) && string.IsNullOrEmpty(node.CallFrame.Url));
    }

    private static string PrettifyFunctionName(string name) =>
        string.IsNullOrEmpty(name) ? "(anonymous)" : name;

    private static string StripBaseUrl(string url)
    {
        if (string.IsNullOrEmpty(url)) return "";
        int idx = url.IndexOf("//");
        if (idx >= 0)
        {
            int slash = url.IndexOf('/', idx + 2);
            if (slash >= 0) return url[slash..];
        }
        return url;
    }
}
