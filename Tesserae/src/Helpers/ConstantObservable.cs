using System;
using System.Collections.Generic;
using static Transpose.Core.dom;

namespace Tesserae
{
    /// <summary>
    /// Enables monitoring of changes for a variable of type T (this class is for listeners only, if updating the value is required then the SettableObserver should be used)
    /// </summary>
    /// <typeparam name="T">An immutable type to be observed. Be careful with non-imutable types, as they may be changed in ways that will not be repoted here</typeparam>
    [Transpose.Name("tss.ConstantObservableT")]
    public class ConstantObservable<T> : IObservable<T>
    {
        private T _value;

        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        public ConstantObservable(T value)
        {
            _value = value;
        }

        /// <summary>
        /// Configures the component to observe.
        /// </summary>
        public void Observe(ObservableEvent.ValueChanged<T>              valueGetter) { valueGetter(Value); }
        /// <summary>
        /// Subscribes the given callback so it fires on every future change to the observed value.
        /// </summary>
        public void ObserveFutureChanges(ObservableEvent.ValueChanged<T> valueGetter) { }
        /// <summary>
        /// Stops a previously-registered callback from receiving further change notifications.
        /// </summary>
        public void StopObserving(ObservableEvent.ValueChanged<T>        valueGetter) { }

        /// <summary>
        /// Registers the callback for value changes. The value never changes, so the returned disposable is a
        /// no-op; the callback is invoked once immediately if <paramref name="fireImmediately"/> is true.
        /// </summary>
        public IDisposable Subscribe(Action<T> callback, bool fireImmediately = true)
        {
            if (fireImmediately) callback(_value);
            return Subscription.Empty;
        }

        /// <summary>
        /// Gets or sets the current value of the component.
        /// </summary>
        public T Value => _value;
    }
}