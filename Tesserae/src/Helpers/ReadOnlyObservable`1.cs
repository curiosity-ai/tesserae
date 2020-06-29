using static H5.Core.dom;

namespace Tesserae
{
    /// <summary>
    /// Enables monitoring of changes for a variable of type T (this class is for listeners only, if updating the value is required then the SettableObserver should be used)
    /// </summary>
    /// <typeparam name="T">An immutable type to be observed. Be careful with non-imutable types, as they may be changed in ways that will not be repoted here</typeparam>
    public class ReadOnlyObservable<T> : IObservable<T>
    {
        private T _value;
        private double _refreshTimeout;

        public ReadOnlyObservable(T value = default) => _value = value;

        private event ObservableEvent.ValueChanged<T> OnValueChanged;

        public void Observe(ObservableEvent.ValueChanged<T> valueGetter) => Observe(valueGetter, callbackImmediately: true);
        public void ObserveFutureChanges(ObservableEvent.ValueChanged<T> valueGetter) => Observe(valueGetter, callbackImmediately: false);
        private void Observe(ObservableEvent.ValueChanged<T> valueGetter, bool callbackImmediately)
        {
            OnValueChanged += valueGetter;
            if (callbackImmediately)
                valueGetter(Value);
        }

        public void StopObserving(ObservableEvent.ValueChanged<T> valueGetter) => OnValueChanged -= valueGetter;

        public T Value
        {
            get => _value;
            protected set
            {
                if (!_value.Equals(value))
                {
                    _value = value;
                    RaiseOnValueChanged();
                }
            }
        }

        private void RaiseOnValueChanged()
        {
            window.clearTimeout(_refreshTimeout);
            _refreshTimeout = window.setTimeout(raise, 1);
            void raise(object t)
            {
                OnValueChanged?.Invoke(_value);
            }
        }
    }
}