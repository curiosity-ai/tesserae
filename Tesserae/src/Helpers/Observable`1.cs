using static Retyped.dom;

namespace Tesserae
{
    /// <summary>
    /// Enables monitoring of changes for a variable of type T (this class is for listeners only, if updating the value is required then the SettableObserver should be used)
    /// </summary>
    /// <typeparam name="T">An immutable type to be observed. Be careful with non-imutable types, as they may be changed in ways that will not be repoted here</typeparam>
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
                RaiseOnChanged();
            }
        }

        public void Observe(ValueChanged onChange)
        {
            OnValueChanged += onChange;
            onChange(_value);
        }

        public void ObserveLazy(ValueChanged onChange) => OnValueChanged += onChange; // TODO [2020-03-05 DWR]: Why does this method exist if we already have an event that could be listened to (even if it is currently private, couldn't it be public)?

        public void Unobserve(ValueChanged onChange) => OnValueChanged -= onChange; // TODO [2020-03-05 DWR]: Why does this method exist if we already have an event that could be listened to (even if it is currently private, couldn't it be public)?
    }
}