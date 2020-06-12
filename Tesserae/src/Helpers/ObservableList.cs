using System.Collections;
using System.Collections.Generic;
using System.Linq;
using static H5.Core.dom;

namespace Tesserae
{
    public class ObservableList<T> : IList<T>, ICollection<T>, IObservable<IReadOnlyList<T>>
    {
        private event ObservableEvent.ValueChanged<IReadOnlyList<T>> OnValueChanged;

        private readonly List<T> _list;
        private readonly bool _valueIsObservable;
        private double _refreshTimeout;

        public ObservableList()
        {
            _list = new List<T>();
            _valueIsObservable = typeof(IObservable).IsAssignableFrom(typeof(T));
        }

        public ObservableList(params T[] initialValues)
        {
            _list = initialValues.ToList();
            _valueIsObservable = typeof(IObservable).IsAssignableFrom(typeof(T));
            if (_valueIsObservable)
            {
                foreach (var i in _list)
                {
                    HookValue(i);
                }
            }
        }

        private void HookValue(T v)
        {
            if (_valueIsObservable && (v is IObservable<T> observableV))
                observableV.ObserveFutureChanges(RaiseOnValueChanged);
        }

        private void UnhookValue(T v)
        {
            if (_valueIsObservable && (v is IObservable<T> observableV))
                observableV.StopObserving(RaiseOnValueChanged);
        }

        public void Observe(ObservableEvent.ValueChanged<IReadOnlyList<T>> valueGetter) => Observe(valueGetter, callbackImmediately: true);
        public void ObserveFutureChanges(ObservableEvent.ValueChanged<IReadOnlyList<T>> valueGetter) => Observe(valueGetter, callbackImmediately: false);
        private void Observe(ObservableEvent.ValueChanged<IReadOnlyList<T>> valueGetter, bool callbackImmediately)
        {
            OnValueChanged += valueGetter;
            if (callbackImmediately)
                valueGetter(Value);
        }

        public void StopObserving(ObservableEvent.ValueChanged<IReadOnlyList<T>> valueGetter) => OnValueChanged -= valueGetter;

        public T this[int index]
        {
            get => _list[index];
            set
            {
                if (_list.Count > index)
                {
                    UnhookValue(_list[index]);
                }
                _list[index] = value;
                RaiseOnValueChanged(value);
            }
        }

        private void RaiseOnValueChanged(T value)
        {
            window.clearTimeout(_refreshTimeout);
            _refreshTimeout = window.setTimeout(raise, 1);
            void raise(object t)
            {
                OnValueChanged?.Invoke(_list);
            }
        }

        public int Count => _list.Count;

        public bool IsReadOnly => false;

        public IReadOnlyList<T> Value => _list;

        public void Add(T item)
        {
            _list.Add(item);
            HookValue(item);
            RaiseOnValueChanged(item);
        }

        public void AddRange(IEnumerable<T> enumerable)
        {
            foreach (var item in enumerable)
            {
                _list.Add(item);
                HookValue(item);
                RaiseOnValueChanged(item);
            }
        }

        public void Clear()
        {
            if (_valueIsObservable)
            {
                foreach (var i in _list)
                {
                    UnhookValue(i);
                }
            }
            _list.Clear();
            RaiseOnValueChanged(default);
        }

        public bool Contains(T item) => _list.Contains(item);

        public void CopyTo(T[] array, int arrayIndex) => _list.CopyTo(array, arrayIndex);

        public IEnumerator<T> GetEnumerator() => _list.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public int IndexOf(T item) => _list.IndexOf(item);

        public void Insert(int index, T item)
        {
            if (_list.Count > index)
            {
                UnhookValue(_list[index]);
            }

            _list.Insert(index, item);
            HookValue(item);
            RaiseOnValueChanged(item);
        }

        public bool Remove(T item)
        {
            var removed = _list.Remove(item);
            if (removed)
            {
                UnhookValue(item);
                RaiseOnValueChanged(item);
            }
            return removed;
        }

        public void RemoveAt(int index)
        {
            if (_list.Count > index)
            {
                UnhookValue(_list[index]);
            }

            _list.RemoveAt(index);
            RaiseOnValueChanged(default);
        }
    }
}