namespace Tesserae
{
    public static class ObservableEvent
    {
        public delegate void ValueChanged<T>(T value);
    }

    public interface IObservable<T> : IObservable
    {
        event ObservableEvent.ValueChanged<T> onValueChanged;
        T Value { get; }
    }

    public static class IObservableExt
    {
        public static void Observe<T>(this IObservable<T> observable, ObservableEvent.ValueChanged<T> valueGetter)
        {
            observable.onValueChanged += valueGetter;
            valueGetter(observable.Value);
        }
    }
}