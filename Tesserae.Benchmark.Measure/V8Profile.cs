using System.Text.Json.Serialization;

namespace Tesserae.Benchmark.Measure;

// Subset of the V8 CPU profile schema produced by Profiler.stop (CDP).
// https://chromedevtools.github.io/devtools-protocol/tot/Profiler/#type-Profile
internal sealed class V8CpuProfile
{
    [JsonPropertyName("nodes")]      public List<V8CpuNode> Nodes { get; set; } = new();
    [JsonPropertyName("startTime")]  public long   StartTime  { get; set; }
    [JsonPropertyName("endTime")]    public long   EndTime    { get; set; }
    [JsonPropertyName("samples")]    public List<int>?  Samples    { get; set; }
    [JsonPropertyName("timeDeltas")] public List<int>?  TimeDeltas { get; set; }
}

internal sealed class V8CpuNode
{
    [JsonPropertyName("id")]        public int       Id        { get; set; }
    [JsonPropertyName("callFrame")] public CallFrame CallFrame { get; set; } = new();
    [JsonPropertyName("hitCount")]  public int       HitCount  { get; set; }
    [JsonPropertyName("children")]  public List<int>? Children { get; set; }
    [JsonPropertyName("deoptReason")] public string? DeoptReason { get; set; }
}

internal sealed class CallFrame
{
    [JsonPropertyName("functionName")] public string FunctionName { get; set; } = "";
    [JsonPropertyName("scriptId")]     public string ScriptId     { get; set; } = "";
    [JsonPropertyName("url")]          public string Url          { get; set; } = "";
    [JsonPropertyName("lineNumber")]   public int    LineNumber   { get; set; }
    [JsonPropertyName("columnNumber")] public int    ColumnNumber { get; set; }
}

// Subset of the V8 sampling heap profile schema produced by HeapProfiler.stopSampling.
// https://chromedevtools.github.io/devtools-protocol/tot/HeapProfiler/#type-SamplingHeapProfile
internal sealed class V8HeapProfile
{
    [JsonPropertyName("head")]    public V8HeapNode Head     { get; set; } = new();
    [JsonPropertyName("samples")] public List<V8HeapSample> Samples { get; set; } = new();
}

internal sealed class V8HeapNode
{
    [JsonPropertyName("callFrame")] public CallFrame      CallFrame { get; set; } = new();
    [JsonPropertyName("selfSize")]  public long           SelfSize  { get; set; }
    [JsonPropertyName("id")]        public int            Id        { get; set; }
    [JsonPropertyName("children")]  public List<V8HeapNode> Children { get; set; } = new();
}

internal sealed class V8HeapSample
{
    [JsonPropertyName("size")]    public long Size    { get; set; }
    [JsonPropertyName("nodeId")]  public int  NodeId  { get; set; }
    [JsonPropertyName("ordinal")] public long Ordinal { get; set; }
}
