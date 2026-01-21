using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using static H5.Core.dom;

namespace Tesserae
{
    /// <summary>
    /// Represents a list of items that can be observed for changes.
    /// </summary>
    /// <typeparam name="T">The type of the items in the list.</typeparam>
    [H5.Name("tss.ObservableList")]
    public sealed class ObservableList<T> : IList<T>, ICollection<T>, IObservable<IReadOnlyList<T>>
    {
        private event ObservableEvent.ValueChanged<IReadOnlyList<T>> ValueChanged;

        private readonly List<T> _list;
        private readonly bool    _shouldHookNestedObservables;

        private DebouncerWithMaxDelay _debouncer;

        /// <summary>Gets or sets the debounce delay in milliseconds for raising value changed events.</summary>
        public int Delay
        {
            get
            {
                return _debouncer?.DelayInMs ?? 1;
            }
            set
            {
                _debouncer = new DebouncerWithMaxDelay(() => ValueChanged?.Invoke(Value), delayInMs: value);
            }
        }
        /// <summary>Initializes a new instance of the ObservableList class.</summary>
        public ObservableList(params T[] initialValues) : this(shouldHook: true, initialValues: initialValues) { }
        /// <summary>Initializes a new instance of the ObservableList class.</summary>
        /// <param name="shouldHook">Whether to hook into nested observables if T is an observable type.</param>
        /// <param name="initialValues">The initial set of values.</param>
        public ObservableList(bool shouldHook, params T[] initialValues)
        {
            _list                        = initialValues.ToList();
            _shouldHookNestedObservables = shouldHook && PossibleObservableHelpers.IsObservable(typeof(T));

            // Note: The HookValue method will also check these conditions but we can save ourselve the enumeration if we know that we don't care about hooking nested lists
            if (_shouldHookNestedObservables)
            {
                foreach (var i in _list)
                    HookValue(i);
            }

            _debouncer = new DebouncerWithMaxDelay(() => ValueChanged?.Invoke(_list), delayInMs: 1);
        }

        /// <summary>Registers a callback for changes, and executes it immediately with the current value.</summary>
        public void Observe(ObservableEvent.ValueChanged<IReadOnlyList<T>>              valueGetter) => Observe(valueGetter, callbackImmediately: true);
        /// <summary>Registers a callback for future changes.</summary>
        public void ObserveFutureChanges(ObservableEvent.ValueChanged<IReadOnlyList<T>> valueGetter) => Observe(valueGetter, callbackImmediately: false);
        private void Observe(ObservableEvent.ValueChanged<IReadOnlyList<T>> valueGetter, bool callbackImmediately)
        {
            ValueChanged += valueGetter;

            if (callbackImmediately)
                valueGetter(Value);
        }

        /// <summary>Unregisters a change callback.</summary>
        public void StopObserving(ObservableEvent.ValueChanged<IReadOnlyList<T>> valueGetter) => ValueChanged -= valueGetter;

        /// <summary>Gets or sets the item at the specified index.</summary>
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
            _debouncer.RaiseOnValueChanged();
        }

        /// <summary>Gets the number of items in the list.</summary>
        public int  Count      => _list.Count;
        /// <summary>Gets whether the list is read-only.</summary>
        public bool IsReadOnly => false;

        /// <summary>Gets the current list of items.</summary>
        public IReadOnlyList<T> Value => _list;

        /// <summary>Adds an item to the list.</summary>
        public void Add(T item)
        {
            _list.Add(item);
            HookValue(item);
            RaiseOnValueChanged();
        }

        /// <summary>Adds a range of items to the list.</summary>
        public void AddRange(IEnumerable<T> enumerable)
        {
            foreach (var item in enumerable)
            {
                _list.Add(item);
                HookValue(item);
            }
            RaiseOnValueChanged();
        }
        
        /// <summary>Clears the list and adds the specified items.</summary>
        public void ReplaceAll(IEnumerable<T> enumerable)
        {
            if (_shouldHookNestedObservables)
            {
                foreach (var i in _list)
                {
                    UnhookValue(i);
                }
            }
            _list.Clear();
            foreach (var item in enumerable)
            {
                _list.Add(item);
                HookValue(item);
            }
            RaiseOnValueChanged();
        }

        /// <summary>Clears the list.</summary>
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

        /// <summary>Determines whether the list contains the specified item.</summary>
        public bool Contains(T item) => _list.Contains(item);

        /// <summary>Copies the list items to an array.</summary>
        public void CopyTo(T[] array, int arrayIndex) => _list.CopyTo(array, arrayIndex);

        /// <summary>Gets the index of the specified item.</summary>
        public int IndexOf(T item) => _list.IndexOf(item);

        /// <summary>Inserts an item at the specified index.</summary>
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

        /// <summary>Updates an item at the specified index.</summary>
        public void Update(int index, T item)
        {
            if (_list.Count > index)
            {
                UnhookValue(_list[index]);
                _list.RemoveAt(index);
            }
            _list.Insert(index, item);
            HookValue(item);
            RaiseOnValueChanged();
        }

        /// <summary>Removes all items that match the specified predicate.</summary>
        public int RemoveAll(Func<T, bool> match)
        {
            var toRemove = _list.Where(match).ToArray();

            foreach (var r in toRemove)
            {
                Remove(r);
            }
            return toRemove.Length;
        }

        /// <summary>Removes the first occurrence of the specified item.</summary>
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

        /// <summary>Removes the item at the specified index.</summary>
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

        public IEnumerator<T>   GetEnumerator() => _list.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    }
}