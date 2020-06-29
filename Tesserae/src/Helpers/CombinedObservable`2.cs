using static H5.Core.dom;

namespace Tesserae
{
    public class CombinedObservable<T1, T2> : IObservable<(T1 first, T2 second)>
    {
        private readonly IObservable<T1> _first;
        private readonly IObservable<T2> _second;
        private double _refreshTimeout;

        public (T1 first, T2 second) Value => (_first.Value, _second.Value);

        private event ObservableEvent.ValueChanged<(T1 first, T2 second)> OnValueChanged;

        public CombinedObservable(IObservable<T1> o1, IObservable<T2> o2)
        {
            o1.ObserveFutureChanges(_ => RaiseOnValueChanged());
            o2.ObserveFutureChanges(_ => RaiseOnValueChanged());
            _first = o1;
            _second = o2;
        }

        public void Observe(ObservableEvent.ValueChanged<(T1 first, T2 second)> valueGetter) => Observe(valueGetter, callbackImmediately: true);
        public void ObserveFutureChanges(ObservableEvent.ValueChanged<(T1 first, T2 second)> valueGetter) => Observe(valueGetter, callbackImmediately: false);
        private void Observe(ObservableEvent.ValueChanged<(T1 first, T2 second)> valueGetter, bool callbackImmediately)
        {
            OnValueChanged += valueGetter;
            if (callbackImmediately)
                valueGetter(Value);
        }

        public void StopObserving(ObservableEvent.ValueChanged<(T1 first, T2 second)> valueGetter) => OnValueChanged -= valueGetter;

        private void RaiseOnValueChanged()
        {
            window.clearTimeout(_refreshTimeout);
            _refreshTimeout = window.setTimeout(
                _ => OnValueChanged?.Invoke(Value),
                1
            );
        }
    }
}