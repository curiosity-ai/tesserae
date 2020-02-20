using System;

namespace Tesserae
{

    /// <summary>
    /// Encapsulates a variable of type T, and enables monitoring for changes
    /// </summary>
    /// <typeparam name="T">An immutable type to be observed. Be careful with non-imutable types, as you can change them in ways that will not be visible here</typeparam>
    public class Observable<T> : Observable, IObservable<T>
    {
        private T _value;

        public Observable(T value)
        {
            _value = value;
        }

        public Observable()
        {
            _value = default(T);
        }

        public delegate void ValueChanged(T value);

        private event ValueChanged onValueChanged;

        public T Value
        {
            get
            {
                return _value;
            }
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
            onValueChanged?.Invoke(_value);
            RaiseOnChanged();
        }

        public void Observe(ValueChanged onChange)
        {
            onValueChanged += onChange;
            onChange(_value);
        }

        public void ObserveLazy(ValueChanged onChange)
        {
            onValueChanged += onChange;
        }

        public void Unobserve(ValueChanged onChange)
        {
            onValueChanged -= onChange;
        }
    }
}