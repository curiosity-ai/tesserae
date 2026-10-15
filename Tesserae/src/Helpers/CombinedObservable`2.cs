using System;

namespace Tesserae
{
    /// <summary>
    /// Combines two or more <see cref="IObservable{T}"/> instances into a single observable that emits a tuple of
    /// their latest values. It observes its sources until it is disposed: dispose one that is shorter-lived than
    /// its sources, or each one created leaves a subscriber on them.
    /// </summary>
    [Transpose.Name("tss.CombinedObservableT2")]
    public sealed class CombinedObservable<T1, T2> : IObservable<(T1 first, T2 second)>, IDisposable
    {
        private readonly IObservable<T1> _first;
        private readonly IObservable<T2> _second;

        private readonly CombinedObservableCore<(T1 first, T2 second)> _core;

        /// <summary>
        /// Gets the component's current value tuple.
        /// </summary>
        public (T1 first, T2 second) Value => (_first.Value, _second.Value);

        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        public CombinedObservable(IObservable<T1> o1, IObservable<T2> o2)
        {
            _first  = o1;
            _second = o2;

            _core = new CombinedObservableCore<(T1 first, T2 second)>(() => Value);
            _core.Watch(o1);
            _core.Watch(o2);
        }

        /// <summary>
        /// Configures the component to observe.
        /// </summary>
        public void Observe(ObservableEvent.ValueChanged<(T1 first, T2 second)> valueGetter) => _core.Observe(valueGetter, callbackImmediately: true);

        /// <summary>
        /// Subscribes the given callback so it fires on every future change to the observed value.
        /// </summary>
        public void ObserveFutureChanges(ObservableEvent.ValueChanged<(T1 first, T2 second)> valueGetter) => _core.Observe(valueGetter, callbackImmediately: false);

        /// <summary>
        /// Stops a previously-registered callback from receiving further change notifications.
        /// </summary>
        public void StopObserving(ObservableEvent.ValueChanged<(T1 first, T2 second)> valueGetter) => _core.StopObserving(valueGetter);

        /// <summary>
        /// Registers the callback for value changes and returns an IDisposable that, when disposed, unregisters
        /// the callback.
        /// </summary>
        public IDisposable Subscribe(Action<(T1 first, T2 second)> callback, bool fireImmediately = true) => _core.Subscribe(callback, fireImmediately);

        /// <summary>
        /// Stops observing the source observables, so this instance no longer holds a subscriber on them and
        /// raises no further changes. Idempotent.
        /// </summary>
        public void Dispose() => _core.Dispose();
    }
}
