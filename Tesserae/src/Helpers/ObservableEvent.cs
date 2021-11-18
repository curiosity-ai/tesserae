namespace Tesserae
{
    [H5.Name("tss.ObservableEvent")]
    public static class ObservableEvent
    {
        public delegate void ValueChanged<T>(T value);
    }
}