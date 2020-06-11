namespace Tesserae
{
    public static class IObservableExtensions
    {
        public static void Observe<T>(this IObservable<T> observable, ObservableEvent.ValueChanged<T> valueGetter)
        {
            observable.onValueChanged += valueGetter;
            valueGetter(observable.Value);
        }
    }
}