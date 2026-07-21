using System;
using static Transpose.Core.dom;

namespace Tesserae
{
    /// <summary>
    /// Combines two or more <see cref="IObservable{T}"/> instances into a single observable that emits a tuple of
    /// their latest values.
    /// </summary>
    [Transpose.Name("tss.CombinedObservableT3")]
    public sealed class CombinedObservable<T1, T2, T3> : IObservable<(T1 first, T2 second, T3 third)>
    {
        private readonly IObservable<T1>       _first;
        private readonly IObservable<T2>       _second;
        private readonly IObservable<T3>       _third;
        private          DebouncerWithMaxDelay _debouncer;


        /// <summary>
        /// Gets the component's current value tuple.
        /// </summary>
        public (T1 first, T2 second, T3 third) Value => (_first.Value, _second.Value, _third.Value);

        private event ObservableEvent.ValueChanged<(T1 first, T2 second, T3 thrid)> ValueChanged;

        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        public CombinedObservable(IObservable<T1> o1, IObservable<T2> o2, IObservable<T3> o3)
        {
            o1.ObserveFutureChanges(_ => RaiseOnValueChanged());
            o2.ObserveFutureChanges(_ => RaiseOnValueChanged());
            o3.ObserveFutureChanges(_ => RaiseOnValueChanged());
            _first  = o1;
            _second = o2;
            _third  = o3;

            _debouncer = new DebouncerWithMaxDelay(() => ValueChanged?.Invoke(Value));
        }

        /// <summary>
        /// Configures the component to observe.
        /// </summary>
        public void Observe(ObservableEvent.ValueChanged<(T1 first, T2 second, T3 third)>              valueGetter) => Observe(valueGetter, callbackImmediately: true);
        /// <summary>
        /// Subscribes the given callback so it fires on every future change to the observed value.
        /// </summary>
        public void ObserveFutureChanges(ObservableEvent.ValueChanged<(T1 first, T2 second, T3 third)> valueGetter) => Observe(valueGetter, callbackImmediately: false);
        private void Observe(ObservableEvent.ValueChanged<(T1 first, T2 second, T3 third)> valueGetter, bool callbackImmediately)
        {
            ValueChanged += valueGetter;

            if (callbackImmediately)
                valueGetter(Value);
        }

        /// <summary>
        /// Stops a previously-registered callback from receiving further change notifications.
        /// </summary>
        public void StopObserving(ObservableEvent.ValueChanged<(T1 first, T2 second, T3 third)> valueGetter) => ValueChanged -= valueGetter;

        /// <summary>
        /// Registers the callback for value changes and returns an IDisposable that, when disposed, unregisters
        /// the callback.
        /// </summary>
        public IDisposable Subscribe(Action<(T1 first, T2 second, T3 third)> callback, bool fireImmediately = true)
        {
            ObservableEvent.ValueChanged<(T1 first, T2 second, T3 third)> handler = v => callback(v);
            ValueChanged += handler;
            if (fireImmediately) callback(Value);
            return new Subscription(() => ValueChanged -= handler);
        }

        private void RaiseOnValueChanged()
        {
            _debouncer.RaiseOnValueChanged();
        }
    }
}