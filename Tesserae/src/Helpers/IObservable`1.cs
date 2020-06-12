namespace Tesserae
{
    public interface IObservable<T> : IObservable
    {
        /// <summary>
        /// If callbackImmediately is true then the valueGetter will be executed immediately with the observable's current value (this is the default behaviour) - if you only want to observe FUTURE changes then set it to false
        /// </summary>
        void Observe(ObservableEvent.ValueChanged<T> valueGetter, bool callbackImmediately = true);

        void StopObserving(ObservableEvent.ValueChanged<T> valueGetter);

        T Value { get; }
    }
}