namespace Tesserae
{
    [H5.Name("tss.CombinedObservableT5")]
    public sealed class CombinedObservable<T1, T2, T3, T4, T5> : IObservable<(T1 first, T2 second, T3 third, T4 forth, T5 fifth)>
    {
        private readonly IObservable<T1> _first;
        private readonly IObservable<T2> _second;
        private readonly IObservable<T3> _third;
        private readonly IObservable<T4> _forth;
        private readonly IObservable<T5> _fifth;

        private DebouncerWithMaxDelay _debouncer;


        public (T1 first, T2 second, T3 third, T4 forth, T5 fifth) Value => (_first.Value, _second.Value, _third.Value, _forth.Value, _fifth.Value);

        private event ObservableEvent.ValueChanged<(T1 first, T2 second, T3 third, T4 forth, T5 fifth)> ValueChanged;

        public CombinedObservable(IObservable<T1> o1, IObservable<T2> o2, IObservable<T3> o3, IObservable<T4> o4, IObservable<T5> o5)
        {
            o1.ObserveFutureChanges(_ => RaiseOnValueChanged());
            o2.ObserveFutureChanges(_ => RaiseOnValueChanged());
            o3.ObserveFutureChanges(_ => RaiseOnValueChanged());
            o4.ObserveFutureChanges(_ => RaiseOnValueChanged());
            o5.ObserveFutureChanges(_ => RaiseOnValueChanged());
            _first  = o1;
            _second = o2;
            _third  = o3;
            _forth  = o4;
            _fifth  = o5;

            _debouncer = new DebouncerWithMaxDelay(() => ValueChanged?.Invoke(Value));
        }

        public void Observe(ObservableEvent.ValueChanged<(T1 first, T2 second, T3 third, T4 forth, T5 fifth)>              valueGetter) => Observe(valueGetter, callbackImmediately: true);
        public void ObserveFutureChanges(ObservableEvent.ValueChanged<(T1 first, T2 second, T3 third, T4 forth, T5 fifth)> valueGetter) => Observe(valueGetter, callbackImmediately: false);
        private void Observe(ObservableEvent.ValueChanged<(T1 first, T2 second, T3 third, T4 forth, T5 fifth)> valueGetter, bool callbackImmediately)
        {
            ValueChanged += valueGetter;

            if (callbackImmediately)
                valueGetter(Value);
        }

        public void StopObserving(ObservableEvent.ValueChanged<(T1 first, T2 second, T3 third, T4 forth, T5 fifth)> valueGetter) => ValueChanged -= valueGetter;

        private void RaiseOnValueChanged()
        {
            _debouncer.RaiseOnValueChanged();
        }
    }
}