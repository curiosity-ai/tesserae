using System;

namespace Tesserae
{

    /// <summary>
    /// A simple observable contract used throughout the toolkit for reactive value propagation (Subscribe /
    /// ObserveFutureChanges).
    /// </summary>
    [H5.Name("tss.IOBS")]
    public interface IObservable<T>
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

        /// <summary>
        /// Registers the callback for value changes and returns an IDisposable that, when disposed, unregisters
        /// the callback. Dispose is idempotent. If <paramref name="fireImmediately"/> is true, the callback is
        /// invoked synchronously with the current value before this method returns; future changes are delivered
        /// on the observable's normal (typically debounced) schedule.
        /// </summary>
        IDisposable Subscribe(Action<T> callback, bool fireImmediately = true);

        T Value { get; }
    }
}
