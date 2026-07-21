namespace Tesserae
{
    /// <summary>
    /// An observable that emits whenever a wrapped event fires, exposing the event handler as a stream of values.
    /// </summary>
    [Transpose.Name("tss.ObservableEvent")]
    public static class ObservableEvent
    {
        public delegate void ValueChanged<T>(T value);
    }
}