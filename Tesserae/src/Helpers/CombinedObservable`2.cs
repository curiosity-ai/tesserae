using static H5.Core.dom;

namespace Tesserae
{
    public class CombinedObservable<T1, T2> : IObservable<(T1 first, T2 second)>
    {
        private readonly IObservable<T1> _first;
        private readonly IObservable<T2> _second;
        private double _refreshTimeout;

        public (T1 first, T2 second) Value => (_first.Value, _second.Value);

        public event ObservableEvent.ValueChanged<(T1 first, T2 second)> onValueChanged;

        public CombinedObservable(IObservable<T1> o1, IObservable<T2> o2)
        {
            o1.onValueChanged += FirstValueChanged;
            o2.onValueChanged += SecondValueChanged;
            _first = o1;
            _second = o2;
        }

        private void FirstValueChanged(T1 value)
        {
            RaiseOnValueChanged();
        }

        private void SecondValueChanged(T2 value)
        {
            RaiseOnValueChanged();
        }

        private void RaiseOnValueChanged()
        {
            window.clearTimeout(_refreshTimeout);
            _refreshTimeout = window.setTimeout(raise, 1);
            void raise(object t)
            {
                onValueChanged?.Invoke(Value);
            }
        }
    }
}