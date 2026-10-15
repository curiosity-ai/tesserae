using System;
using System.Collections.Generic;

namespace Tesserae
{
    /// <summary>
    /// The shared body of the <c>CombinedObservable</c> classes: it watches each source for changes, raises one
    /// debounced notification with the combined value, and stops watching the sources when disposed.
    /// </summary>
    [Transpose.Name("tss.CombinedObservableCore")]
    internal sealed class CombinedObservableCore<T> : IDisposable
    {
        private readonly Func<T>               _value;
        private readonly DebouncerWithMaxDelay _debouncer;
        private readonly List<IDisposable>     _sources = new List<IDisposable>();

        private event ObservableEvent.ValueChanged<T> ValueChanged;

        internal CombinedObservableCore(Func<T> value)
        {
            _value     = value;
            _debouncer = new DebouncerWithMaxDelay(() => ValueChanged?.Invoke(_value()));
        }

        internal void Watch<TSource>(IObservable<TSource> source) => _sources.Add(Subscription.OnFutureChanges(source, _debouncer.RaiseOnValueChanged));

        internal void Observe(ObservableEvent.ValueChanged<T> valueGetter, bool callbackImmediately)
        {
            ValueChanged += valueGetter;

            if (callbackImmediately)
                valueGetter(_value());
        }

        internal void StopObserving(ObservableEvent.ValueChanged<T> valueGetter) => ValueChanged -= valueGetter;

        internal IDisposable Subscribe(Action<T> callback, bool fireImmediately)
        {
            ObservableEvent.ValueChanged<T> handler = v => callback(v);
            ValueChanged += handler;
            if (fireImmediately) callback(_value());
            return new Subscription(() => ValueChanged -= handler);
        }

        public void Dispose()
        {
            foreach (var source in _sources)
            {
                source.Dispose();
            }

            _sources.Clear();
        }
    }
}
