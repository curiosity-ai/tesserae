using static Retyped.dom;

namespace Tesserae
{

    /// <summary>
    /// Encapsulates a variable of type T, and enables monitoring for changes
    /// </summary>
    /// <typeparam name="T">An immutable type to be observed. Be careful with non-imutable types, as you can change them in ways that will not be visible here</typeparam>
    public class Observable<T> : Observable, IObservable<T>
    {
        private T _value;
        private double _refreshTimeout;

        public Observable(T value = default) => _value = value;

        public delegate void ValueChanged(T value);

        private event ValueChanged OnValueChanged;

        public T Value
        {
            get => _value;
            set
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
                RaiseOnChanged();
            }
        }

        public void Observe(ValueChanged onChange)
        {
            OnValueChanged += onChange;
            onChange(_value);
        }

        public void ObserveLazy(ValueChanged onChange) => OnValueChanged += onChange; // TODO [2020-03-05 DWR]: Why does this method exist if we already have a public event that can be listened to?

        public void Unobserve(ValueChanged onChange) => OnValueChanged -= onChange; // TODO [2020-03-05 DWR]: Why does this method exist if we already have a public event that listeners can be removed from?
    }
}