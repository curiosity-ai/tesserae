using System.Collections.Generic;
using static H5.Core.dom;

namespace Tesserae
{
    /// <summary>
    /// Enables monitoring of changes for a variable of type T (this class is for listeners only, if updating the value is required then the SettableObserver should be used)
    /// </summary>
    /// <typeparam name="T">An immutable type to be observed. Be careful with non-imutable types, as they may be changed in ways that will not be repoted here</typeparam>
    public abstract class ReadOnlyObservable<T> : IObservable<T> // 2020-07-01 DWR: This is an abstract class because it doesn't make sense to have an Observable that can never be updated, so it should always be derived from in order to be useful
    {
        private T _value;
        private IEqualityComparer<T> _comparer;
        private double _refreshTimeout;
        protected ReadOnlyObservable(T value = default, IEqualityComparer<T> comparer = null)
        {
            _value = value;
            _comparer = comparer ?? EqualityComparer<T>.Default;
        }

        private event ObservableEvent.ValueChanged<T> ValueChanged;

        public void Observe(ObservableEvent.ValueChanged<T> valueGetter) => Observe(valueGetter, callbackImmediately: true);
        public void ObserveFutureChanges(ObservableEvent.ValueChanged<T> valueGetter) => Observe(valueGetter, callbackImmediately: false);
        private void Observe(ObservableEvent.ValueChanged<T> valueGetter, bool callbackImmediately)
        {
            ValueChanged += valueGetter;
            if (callbackImmediately)
                valueGetter(Value);
        }

        public void StopObserving(ObservableEvent.ValueChanged<T> valueGetter) => ValueChanged -= valueGetter;

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
            window.clearTimeout(_refreshTimeout);
            _refreshTimeout = window.setTimeout(raise, 1);
            void raise(object t)
            {
                ValueChanged?.Invoke(_value);
            }
        }
    }
}