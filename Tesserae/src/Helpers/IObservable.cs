namespace Tesserae
{
    public interface IObservable<T> : IObservable
    {
        T Value { get; }
        void Observe(Observable<T>.ValueChanged onChange);
        void ObserveLazy(Observable<T>.ValueChanged onChange);
        void Unobserve(Observable<T>.ValueChanged onChange);
    }

    public interface IObservable
    {
        void OnChange(Observable.Changed changed);
        void Unobserve(Observable.Changed changed);
    }

    public class Observable
    {
        public delegate void Changed();
        private event Changed onChanged;
        
        public void OnChange(Changed changed)
        {
            onChanged += changed;
        }

        public void Unobserve(Changed changed)
        {
            onChanged -= changed;
        }

        protected void RaiseOnChanged()
        {
            onChanged?.Invoke();
        }
    }
}