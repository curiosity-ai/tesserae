using System;
using System.Collections.Generic;
using static H5.Core.dom;

namespace Tesserae
{
    /// <summary>
    /// Enables monitoring of changes for a variable of type T (this class is for listeners only, if updating the value is required then the SettableObserver should be used)
    /// </summary>
    /// <typeparam name="T">An immutable type to be observed. Be careful with non-imutable types, as they may be changed in ways that will not be repoted here</typeparam>
    [H5.Name("tss.ReadOnlyObservableT")]
    public abstract class ReadOnlyObservable<T> : IObservable<T> // 2020-07-01 DWR: This is an abstract class because it doesn't make sense to have an Observable that can never be updated, so it should always be derived from in order to be useful
    {
        private T                    _value;
        private IEqualityComparer<T> _comparer;

        private DebouncerWithMaxDelay _debouncer;

        /// <summary>
        /// Gets or sets the delay.
        /// </summary>
        public int Delay
        {
            get
            {
                return _debouncer?.DelayInMs ?? 1;
            }
            set
            {
                _debouncer = new DebouncerWithMaxDelay(() => ValueChanged?.Invoke(_value), delayInMs: value);
            }
        }

        protected ReadOnlyObservable(T value = default, IEqualityComparer<T> comparer = null)
        {
            _value    = value;
            _comparer = comparer ?? EqualityComparer<T>.Default;

            _debouncer = new DebouncerWithMaxDelay(() => ValueChanged?.Invoke(_value));
        }

        private event ObservableEvent.ValueChanged<T> ValueChanged;

        /// <summary>
        /// Configures the component to observe.
        /// </summary>
        public void Observe(ObservableEvent.ValueChanged<T>              valueGetter) => Observe(valueGetter, callbackImmediately: true);
        /// <summary>
        /// Subscribes the given callback so it fires on every future change to the observed value.
        /// </summary>
        public void ObserveFutureChanges(ObservableEvent.ValueChanged<T> valueGetter) => Observe(valueGetter, callbackImmediately: false);
        private void Observe(ObservableEvent.ValueChanged<T> valueGetter, bool callbackImmediately)
        {
            ValueChanged += valueGetter;

            if (callbackImmediately)
            {
                valueGetter(Value);
            }
        }

        /// <summary>
        /// Stops a previously-registered callback from receiving further change notifications.
        /// </summary>
        public void StopObserving(ObservableEvent.ValueChanged<T> valueGetter) => ValueChanged -= valueGetter;

        /// <summary>
        /// Registers the callback for value changes and returns an IDisposable that, when disposed, unregisters
        /// the callback.
        /// </summary>
        public IDisposable Subscribe(Action<T> callback, bool fireImmediately = true)
        {
            ObservableEvent.ValueChanged<T> handler = v => callback(v);
            ValueChanged += handler;
            if (fireImmediately) callback(Value);
            return new Subscription(() => ValueChanged -= handler);
        }

        /// <summary>
        /// Gets or sets the current value of the component.
        /// </summary>
        public T Value
        {
            get => _value;
            protected set
            {
                if (!_comparer.Equals(_value, value))
                {
                    _value = value;
                    RaiseOnValueChanged();
                }
            }
        }

        protected void RaiseOnValueChanged()
        {
            _debouncer.RaiseOnValueChanged();
        }
    }
}