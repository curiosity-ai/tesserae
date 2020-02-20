namespace Tesserae
{
    public interface IObservable<T>
    {
        public delegate void ValueChanged(T value);

        T Value { get; }
        void Observe(ValueChanged valueGetter);
        void ObserveLazy(ValueChanged valueGetter);
    }
}