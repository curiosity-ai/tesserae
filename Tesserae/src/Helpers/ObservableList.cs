using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using static H5.Core.dom;

namespace Tesserae
{
    [H5.Name("tss.ObservableList")]
    public sealed class ObservableList<T> : IList<T>, ICollection<T>, IObservable<IReadOnlyList<T>>
    {
        private event ObservableEvent.ValueChanged<IReadOnlyList<T>> ValueChanged;

        private readonly List<T> _list;
        private readonly bool _shouldHookNestedObservables;
        private double _refreshTimeout;
        private double _refreshDelay = 1;
        public double Delay { get => _refreshDelay; set => _refreshDelay = value; }
        public ObservableList(params T[] initialValues) : this(shouldHook: true, initialValues: initialValues) { }
        public ObservableList(bool shouldHook, params T[] initialValues)
        {
            _list = initialValues.ToList();
            _shouldHookNestedObservables = shouldHook && PossibleObservableHelpers.IsObservable(typeof(T));

            // Note: The HookValue method will also check these conditions but we can save ourselve the enumeration if we know that we don't care about hooking nested lists
            if (_shouldHookNestedObservables)
            {
                foreach (var i in _list)
                   HookValue(i);
            }
        }

        public void Observe(ObservableEvent.ValueChanged<IReadOnlyList<T>> valueGetter) => Observe(valueGetter, callbackImmediately: true);
        public void ObserveFutureChanges(ObservableEvent.ValueChanged<IReadOnlyList<T>> valueGetter) => Observe(valueGetter, callbackImmediately: false);
        private void Observe(ObservableEvent.ValueChanged<IReadOnlyList<T>> valueGetter, bool callbackImmediately)
        {
            ValueChanged += valueGetter;
            if (callbackImmediately)
                valueGetter(Value);
        }

        public void StopObserving(ObservableEvent.ValueChanged<IReadOnlyList<T>> valueGetter) => ValueChanged -= valueGetter;

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
                RaiseOnValueChanged();
            }
        }

        private void RaiseOnValueChanged()
        {
            window.clearTimeout(_refreshTimeout);
            _refreshTimeout = window.setTimeout(
                _ => ValueChanged?.Invoke(_list),
                _refreshDelay
            );
        }

        public int Count => _list.Count;
        public bool IsReadOnly => false;

        public IReadOnlyList<T> Value => _list;

        public void Add(T item)
        {
            _list.Add(item);
            HookValue(item);
            RaiseOnValueChanged();
        }

        public void AddRange(IEnumerable<T> enumerable)
        {
            foreach (var item in enumerable)
            {
                _list.Add(item);
                HookValue(item);
                RaiseOnValueChanged();
            }
        }

        public void Clear()
        {
            if (_shouldHookNestedObservables)
            {
                foreach (var i in _list)
                {
                    UnhookValue(i);
                }
            }
            _list.Clear();
            RaiseOnValueChanged();
        }

        public bool Contains(T item) => _list.Contains(item);

        public void CopyTo(T[] array, int arrayIndex) => _list.CopyTo(array, arrayIndex);

        public int IndexOf(T item) => _list.IndexOf(item);

        public void Insert(int index, T item)
        {
            if (_list.Count > index)
            {
                UnhookValue(_list[index]);
            }

            _list.Insert(index, item);
            HookValue(item);
            RaiseOnValueChanged();
        }
        
        public void Update(int index, T item)
        {
            RemoveAt(index);
            Insert(index, item);
        }
        
        public int RemoveAll(Func<T, bool> match)
        {
            var toRemove = _list.Where(match).ToArray();
            foreach(var r in toRemove)
            {
                Remove(r);
            }
            return toRemove.Length;
        }

        public bool Remove(T item)
        {
            var removed = _list.Remove(item);
            if (removed)
            {
                UnhookValue(item);
                RaiseOnValueChanged();
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
            RaiseOnValueChanged();
        }

        private void HookValue(T v)
        {
            if (_shouldHookNestedObservables)
                PossibleObservableHelpers.ObserveFutureChangesIfObservable(v, RaiseOnValueChanged);
        }

        private void UnhookValue(T v)
        {
            if (_shouldHookNestedObservables)
                PossibleObservableHelpers.StopObservingIfObservable(v, RaiseOnValueChanged);
        }

        public IEnumerator<T> GetEnumerator() => _list.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}