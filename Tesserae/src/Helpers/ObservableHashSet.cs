using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Tesserae
{
    /// <summary>
    /// Represents a hash set of items that can be observed for changes.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the hash set.</typeparam>
    [Transpose.Name("tss.ObservableHashSet")]
    public sealed class ObservableHashSet<T> : ISet<T>, ICollection<T>, IObservable<IReadOnlyCollection<T>>
    {
        private event ObservableEvent.ValueChanged<IReadOnlyCollection<T>> ValueChanged;

        private readonly HashSet<T> _set;
        private readonly bool _shouldHookNestedObservables;

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

        /// <summary>Initializes a new instance of the ObservableHashSet class.</summary>
        public ObservableHashSet(params T[] initialValues) : this(shouldHook: true, initialValues: initialValues) { }

        /// <summary>Initializes a new instance of the ObservableHashSet class.</summary>
        /// <param name="shouldHook">Whether to hook into nested observables if T is an observable type.</param>
        /// <param name="initialValues">The initial set of values.</param>
        public ObservableHashSet(bool shouldHook, params T[] initialValues)
        {
            _set = new HashSet<T>(initialValues);
            _shouldHookNestedObservables = shouldHook && PossibleObservableHelpers.IsObservable(typeof(T));

            if (_shouldHookNestedObservables)
            {
                foreach (var i in _set)
                    HookValue(i);
            }

            _debouncer = new DebouncerWithMaxDelay(() => ValueChanged?.Invoke(_set), delayInMs: 1);
        }

        /// <summary>Registers a callback for changes, and executes it immediately with the current value.</summary>
        public void Observe(ObservableEvent.ValueChanged<IReadOnlyCollection<T>> valueGetter) => Observe(valueGetter, callbackImmediately: true);

        /// <summary>Registers a callback for future changes.</summary>
        public void ObserveFutureChanges(ObservableEvent.ValueChanged<IReadOnlyCollection<T>> valueGetter) => Observe(valueGetter, callbackImmediately: false);

        private void Observe(ObservableEvent.ValueChanged<IReadOnlyCollection<T>> valueGetter, bool callbackImmediately)
        {
            ValueChanged += valueGetter;

            if (callbackImmediately)
                valueGetter(Value);
        }

        /// <summary>Unregisters a change callback.</summary>
        public void StopObserving(ObservableEvent.ValueChanged<IReadOnlyCollection<T>> valueGetter) => ValueChanged -= valueGetter;

        /// <summary>
        /// Registers the callback for value changes and returns an IDisposable that, when disposed, unregisters
        /// the callback.
        /// </summary>
        public IDisposable Subscribe(Action<IReadOnlyCollection<T>> callback, bool fireImmediately = true)
        {
            ObservableEvent.ValueChanged<IReadOnlyCollection<T>> handler = v => callback(v);
            ValueChanged += handler;
            if (fireImmediately) callback(Value);
            return new Subscription(() => ValueChanged -= handler);
        }

        private void RaiseOnValueChanged()
        {
            _debouncer.RaiseOnValueChanged();
        }

        /// <summary>Gets the number of elements that are contained in a set.</summary>
        public int Count => _set.Count;

        /// <summary>Gets a value indicating whether the collection is read-only.</summary>
        public bool IsReadOnly => false;

        /// <summary>Gets the current set of items.</summary>
        public IReadOnlyCollection<T> Value => _set;

        /// <summary>Adds the specified element to a set.</summary>
        public bool Add(T item)
        {
            if (_set.Add(item))
            {
                HookValue(item);
                RaiseOnValueChanged();
                return true;
            }
            return false;
        }

        void ICollection<T>.Add(T item) => Add(item);

        /// <summary>Removes all elements from a set.</summary>
        public void Clear()
        {
            if (_shouldHookNestedObservables)
            {
                foreach (var i in _set)
                {
                    UnhookValue(i);
                }
            }
            _set.Clear();
            RaiseOnValueChanged();
        }

        /// <summary>Determines whether a set contains the specified element.</summary>
        public bool Contains(T item) => _set.Contains(item);

        /// <summary>Copies the elements of a set to an array, starting at the specified array index.</summary>
        public void CopyTo(T[] array, int arrayIndex) => _set.CopyTo(array, arrayIndex);

        /// <summary>Removes the specified element from a set.</summary>
        public bool Remove(T item)
        {
            if (_set.Remove(item))
            {
                UnhookValue(item);
                RaiseOnValueChanged();
                return true;
            }
            return false;
        }

        /// <summary>Modifies the current set so that it contains all elements that are present in the current set, in the specified collection, or in both.</summary>
        public void UnionWith(IEnumerable<T> other)
        {
            var addedAny = false;
            foreach (var item in other)
            {
                if (_set.Add(item))
                {
                    HookValue(item);
                    addedAny = true;
                }
            }
            if (addedAny)
            {
                RaiseOnValueChanged();
            }
        }

        /// <summary>Modifies the current set so that it contains only elements that are also in a specified collection.</summary>
        public void IntersectWith(IEnumerable<T> other)
        {
            var otherSet = other as HashSet<T> ?? new HashSet<T>(other);
            var toRemove = _set.Where(item => !otherSet.Contains(item)).ToList();

            if (toRemove.Count > 0)
            {
                foreach (var item in toRemove)
                {
                    _set.Remove(item);
                    UnhookValue(item);
                }
                RaiseOnValueChanged();
            }
        }

        /// <summary>Removes all elements in the specified collection from the current set.</summary>
        public void ExceptWith(IEnumerable<T> other)
        {
            var removedAny = false;
            foreach (var item in other)
            {
                if (_set.Remove(item))
                {
                    UnhookValue(item);
                    removedAny = true;
                }
            }
            if (removedAny)
            {
                RaiseOnValueChanged();
            }
        }

        /// <summary>Modifies the current set so that it contains only elements that are present either in the current set or in the specified collection, but not both.</summary>
        public void SymmetricExceptWith(IEnumerable<T> other)
        {
            var otherSet = new HashSet<T>(other);
            var changed = false;
            foreach (var item in otherSet)
            {
                if (_set.Remove(item))
                {
                    UnhookValue(item);
                    changed = true;
                }
                else if (_set.Add(item))
                {
                    HookValue(item);
                    changed = true;
                }
            }
            if (changed)
            {
                RaiseOnValueChanged();
            }
        }

        /// <summary>Determines whether a set is a subset of a specified collection.</summary>
        public bool IsSubsetOf(IEnumerable<T> other) => _set.IsSubsetOf(other);

        /// <summary>Determines whether the current set is a proper (strict) subset of a specified collection.</summary>
        public bool IsProperSubsetOf(IEnumerable<T> other) => _set.IsProperSubsetOf(other);

        /// <summary>Determines whether a set is a superset of a specified collection.</summary>
        public bool IsSupersetOf(IEnumerable<T> other) => _set.IsSupersetOf(other);

        /// <summary>Determines whether the current set is a proper (strict) superset of a specified collection.</summary>
        public bool IsProperSupersetOf(IEnumerable<T> other) => _set.IsProperSupersetOf(other);

        /// <summary>Determines whether the current set overlaps with the specified collection.</summary>
        public bool Overlaps(IEnumerable<T> other) => _set.Overlaps(other);

        /// <summary>Determines whether the current set and the specified collection contain the same elements.</summary>
        public bool SetEquals(IEnumerable<T> other) => _set.SetEquals(other);

        /// <summary>Removes all elements that match the conditions defined by the specified predicate from a set.</summary>
        public int RemoveWhere(Predicate<T> match)
        {
            var toRemove = _set.Where(item => match(item)).ToList();
            if (toRemove.Count > 0)
            {
                foreach (var item in toRemove)
                {
                    _set.Remove(item);
                    UnhookValue(item);
                }
                RaiseOnValueChanged();
            }
            return toRemove.Count;
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

        /// <summary>
        /// Returns the enumerator of the component.
        /// </summary>
        public IEnumerator<T> GetEnumerator() => _set.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}