using System;

namespace Tesserae
{
    public class Observable<T>
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
                if(!_value.Equals(value))
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