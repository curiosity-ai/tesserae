using System;

namespace Tesserae
{
    /// <summary>
    /// Combines two or more <see cref="IObservable{T}"/> instances into a single observable that emits a tuple of
    /// their latest values. It observes its sources until it is disposed: dispose one that is shorter-lived than
    /// its sources, or each one created leaves a subscriber on them.
    /// </summary>
    [Transpose.Name("tss.CombinedObservableT4")]
    public sealed class CombinedObservable<T1, T2, T3, T4> : IObservable<(T1 first, T2 second, T3 third, T4 forth)>, IDisposable
    {
        private readonly IObservable<T1> _first;
        private readonly IObservable<T2> _second;
        private readonly IObservable<T3> _third;
        private readonly IObservable<T4> _forth;

        private readonly CombinedObservableCore<(T1 first, T2 second, T3 third, T4 forth)> _core;

        /// <summary>
        /// Gets the component's current value tuple.
        /// </summary>
        public (T1 first, T2 second, T3 third, T4 forth) Value => (_first.Value, _second.Value, _third.Value, _forth.Value);

        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        public CombinedObservable(IObservable<T1> o1, IObservable<T2> o2, IObservable<T3> o3, IObservable<T4> o4)
        {
            _first  = o1;
            _second = o2;
            _third  = o3;
            _forth  = o4;

            _core = new CombinedObservableCore<(T1 first, T2 second, T3 third, T4 forth)>(() => Value);
            _core.Watch(o1);
            _core.Watch(o2);
            _core.Watch(o3);
            _core.Watch(o4);
        }

        /// <summary>
        /// Configures the component to observe.
        /// </summary>
        public void Observe(ObservableEvent.ValueChanged<(T1 first, T2 second, T3 third, T4 forth)> valueGetter) => _core.Observe(valueGetter, callbackImmediately: true);

        /// <summary>
        /// Subscribes the given callback so it fires on every future change to the observed value.
        /// </summary>
        public void ObserveFutureChanges(ObservableEvent.ValueChanged<(T1 first, T2 second, T3 third, T4 forth)> valueGetter) => _core.Observe(valueGetter, callbackImmediately: false);

        /// <summary>
        /// Stops a previously-registered callback from receiving further change notifications.
        /// </summary>
        public void StopObserving(ObservableEvent.ValueChanged<(T1 first, T2 second, T3 third, T4 forth)> valueGetter) => _core.StopObserving(valueGetter);

        /// <summary>
        /// Registers the callback for value changes and returns an IDisposable that, when disposed, unregisters
        /// the callback.
        /// </summary>
        public IDisposable Subscribe(Action<(T1 first, T2 second, T3 third, T4 forth)> callback, bool fireImmediately = true) => _core.Subscribe(callback, fireImmediately);

        /// <summary>
        /// Stops observing the source observables, so this instance no longer holds a subscriber on them and
        /// raises no further changes. Idempotent.
        /// </summary>
        public void Dispose() => _core.Dispose();
    }
}
