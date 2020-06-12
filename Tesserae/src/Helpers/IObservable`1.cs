namespace Tesserae
{
    public interface IObservable<T> : IObservable
    {
        /// <summary>
        /// This will execute the callback immediately with the current value AND for any future changes (unless the callback is passed to StopObserving first)
        /// </summary>
        void Observe(ObservableEvent.ValueChanged<T> valueGetter);

        /// <summary>
        /// This will execute the callback for any future changes (unless the callback is passed to StopObserving first) but it will NOT execute it immediately, with the current value (which is how this method differs to Observing)
        /// </summary>
        void ObserveFutureChanges(ObservableEvent.ValueChanged<T> valueGetter);

        void StopObserving(ObservableEvent.ValueChanged<T> valueGetter);

        T Value { get; }
    }
}