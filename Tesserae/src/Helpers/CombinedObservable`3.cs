using static H5.Core.dom;

namespace Tesserae
{
    [H5.Name("tss.CombinedObservableT3")]
    public sealed class CombinedObservable<T1, T2, T3> : IObservable<(T1 first, T2 second, T3 third)>
    {
        private readonly IObservable<T1>       _first;
        private readonly IObservable<T2>       _second;
        private readonly IObservable<T3>       _third;
        private          DebouncerWithMaxDelay _debouncer;


        public (T1 first, T2 second, T3 third) Value => (_first.Value, _second.Value, _third.Value);

        private event ObservableEvent.ValueChanged<(T1 first, T2 second, T3 thrid)> ValueChanged;

        public CombinedObservable(IObservable<T1> o1, IObservable<T2> o2, IObservable<T3> o3)
        {
            o1.ObserveFutureChanges(_ => RaiseOnValueChanged());
            o2.ObserveFutureChanges(_ => RaiseOnValueChanged());
            o3.ObserveFutureChanges(_ => RaiseOnValueChanged());
            _first  = o1;
            _second = o2;
            _third  = o3;

            _debouncer = new DebouncerWithMaxDelay(() => ValueChanged?.Invoke(Value));
        }

        public void Observe(ObservableEvent.ValueChanged<(T1 first, T2 second, T3 third)>              valueGetter) => Observe(valueGetter, callbackImmediately: true);
        public void ObserveFutureChanges(ObservableEvent.ValueChanged<(T1 first, T2 second, T3 third)> valueGetter) => Observe(valueGetter, callbackImmediately: false);
        private void Observe(ObservableEvent.ValueChanged<(T1 first, T2 second, T3 third)> valueGetter, bool callbackImmediately)
        {
            ValueChanged += valueGetter;

            if (callbackImmediately)
                valueGetter(Value);
        }

        public void StopObserving(ObservableEvent.ValueChanged<(T1 first, T2 second, T3 third)> valueGetter) => ValueChanged -= valueGetter;

        private void RaiseOnValueChanged()
        {
            _debouncer.RaiseOnValueChanged();
        }
    }
}