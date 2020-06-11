namespace Tesserae
{
    public interface IObservable<T> : IObservable
    {
        event ObservableEvent.ValueChanged<T> onValueChanged;
        T Value { get; }
    }
}