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

        public delegate void ValueChanged<T>(T value);
        private event ValueChanged<T> onChanged;

        public T Value
        {
            set
            {
                _value = value;
                onChanged?.Invoke(_value);
            }
        }

        public void Get(ValueChanged<T> valueGetter)
        {
            onChanged += valueGetter;
            valueGetter(_value);
        }
    }
}
