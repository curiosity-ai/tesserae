using System;

namespace Tesserae
{

    /// <summary>
    /// Encapsulates a variable of type T, and enables monitoring for changes
    /// </summary>
    /// <typeparam name="T">An immutable type to be observed. Be careful with non-imutable types, as you can change them in ways that will not be visible here</typeparam>
    public class Observable<T> : IObservable<T>
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

        private event ValueChanged onChanged;

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
                    onChanged?.Invoke(_value);
                }
            }
        }

        public void Observe(ValueChanged valueGetter)
        {
            onChanged += valueGetter;
            valueGetter(_value);
        }

        public void ObserveLazy(ValueChanged valueGetter)
        {
            onChanged += valueGetter;
        }
    }
}