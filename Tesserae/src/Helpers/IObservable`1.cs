namespace Tesserae
{
    public interface IObservable<T> : IObservable
    {
        T Value { get; }
        void Observe(Observable<T>.ValueChanged onChange);
        void ObserveLazy(Observable<T>.ValueChanged onChange);
        void Unobserve(Observable<T>.ValueChanged onChange);
    }
}