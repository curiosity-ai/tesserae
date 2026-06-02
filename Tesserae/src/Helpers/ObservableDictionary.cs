using System;
using System.Collections;
using System.Collections.Generic;
using static H5.Core.dom;

namespace Tesserae
{
    /// <summary>
    /// A dictionary whose contents are observable for reactive composition.
    /// </summary>
    [H5.Name("tss.ObservableDictionary")]
    public sealed class ObservableDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IObservable<IReadOnlyDictionary<TKey, TValue>>
    {
        private event ObservableEvent.ValueChanged<IReadOnlyDictionary<TKey, TValue>> ValueChanged;

        private readonly Dictionary<TKey, TValue> _dictionary;
        private readonly bool                     _shouldHookNestedObservables;

        private DebouncerWithMaxDelay _debouncer;

        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        public ObservableDictionary(IEqualityComparer<TKey>  keyComparer = null, bool shouldHook = true) : this(new Dictionary<TKey, TValue>(keyComparer), shouldHook) { }
        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        public ObservableDictionary(Dictionary<TKey, TValue> values,             bool shouldHook = true) : this(values, values?.Comparer, shouldHook) { }
        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        public ObservableDictionary(IEnumerable<KeyValuePair<TKey, TValue>> values, IEqualityComparer<TKey> keyComparer = null, bool shouldHook = true)
        {
            // 2020-6-17 DWR: We're cloning the input, like we do in ObservableList, rather than accepting a Dictionary reference directly and storing that (that whoever provided it to us could mutate without us being aware here)
            _dictionary                  = new Dictionary<TKey, TValue>(comparer: keyComparer);
            _shouldHookNestedObservables = shouldHook && PossibleObservableHelpers.IsObservable(typeof(TValue));

            foreach (var entry in values)
            {
                if (_dictionary.ContainsKey(entry.Key)) throw new ArgumentException("Key appears multiple times in input data - invalid: " + entry.Key);

                _dictionary.Add(entry.Key, entry.Value);
            }

            // Note: The HookValue method will also check these conditions but we can save ourselve the enumeration if we know that we don't care about hooking nested lists
            if (_shouldHookNestedObservables)
            {
                foreach (var kv in _dictionary) HookValue(kv.Value);
            }

            _debouncer = new DebouncerWithMaxDelay(() => ValueChanged?.Invoke(_dictionary), delayInMs: 1);
        }

        /// <summary>
        /// Configures the component to observe.
        /// </summary>
        public void Observe(ObservableEvent.ValueChanged<IReadOnlyDictionary<TKey, TValue>>              valueGetter) => Observe(valueGetter, callbackImmediately: true);
        /// <summary>
        /// Subscribes the given callback so it fires on every future change to the observed value.
        /// </summary>
        public void ObserveFutureChanges(ObservableEvent.ValueChanged<IReadOnlyDictionary<TKey, TValue>> valueGetter) => Observe(valueGetter, callbackImmediately: false);
        private void Observe(ObservableEvent.ValueChanged<IReadOnlyDictionary<TKey, TValue>> valueGetter, bool callbackImmediately)
        {
            ValueChanged += valueGetter;

            if (callbackImmediately)
                valueGetter(Value);
        }

        /// <summary>
        /// Stops a previously-registered callback from receiving further change notifications.
        /// </summary>
        public void StopObserving(ObservableEvent.ValueChanged<IReadOnlyDictionary<TKey, TValue>> valueGetter) => ValueChanged -= valueGetter;

        /// <summary>
        /// Registers the callback for value changes and returns an IDisposable that, when disposed, unregisters
        /// the callback.
        /// </summary>
        public IDisposable Subscribe(Action<IReadOnlyDictionary<TKey, TValue>> callback, bool fireImmediately = true)
        {
            ObservableEvent.ValueChanged<IReadOnlyDictionary<TKey, TValue>> handler = v => callback(v);
            ValueChanged += handler;
            if (fireImmediately) callback(Value);
            return new Subscription(() => ValueChanged -= handler);
        }

        /// <summary>
        /// Gets or sets the element at the specified index.
        /// </summary>
        public TValue this[TKey key]
        {
            get => _dictionary[key];
            set
            {
                if (_dictionary.TryGetValue(key, out var prev))
                {
                    UnhookValue(prev);
                }
                _dictionary[key] = value;
                HookValue(value);
                RaiseOnValueChanged();
            }
        }

        /// <summary>
        /// Gets or sets the keys.
        /// </summary>
        public ICollection<TKey>   Keys       => _dictionary.Keys;
        /// <summary>
        /// Gets or sets the values.
        /// </summary>
        public ICollection<TValue> Values     => _dictionary.Values;
        /// <summary>
        /// Gets the number of items in the component.
        /// </summary>
        public int                 Count      => _dictionary.Count;
        /// <summary>
        /// Returns a value indicating whether the component is read only.
        /// </summary>
        public bool                IsReadOnly => false;

        /// <summary>
        /// Gets or sets the current value of the component.
        /// </summary>
        public IReadOnlyDictionary<TKey, TValue> Value => _dictionary;

        /// <summary>
        /// Adds the given range to the component.
        /// </summary>
        public void AddRange(IEnumerable<KeyValuePair<TKey, TValue>> items)
        {
            foreach (var item in items)
            {
                _dictionary.Add(item.Key, item.Value);
                HookValue(item.Value);
            }
            RaiseOnValueChanged();
        }

        /// <summary>
        /// Adds the given item to the component.
        /// </summary>
        public void Add(TKey key, TValue value)
        {
            _dictionary.Add(key, value);
            HookValue(value);
            RaiseOnValueChanged();
        }

        /// <summary>
        /// Adds the given item to the component.
        /// </summary>
        public void Add(KeyValuePair<TKey, TValue> item)
        {
            _dictionary.Add(item.Key, item.Value);
            HookValue(item.Value);
            RaiseOnValueChanged();
        }

        /// <summary>
        /// Clears the component's current state.
        /// </summary>
        public void Clear()
        {
            if (_shouldHookNestedObservables)
            {
                foreach (var kv in _dictionary)
                {
                    UnhookValue(kv.Value);
                }
            }

            _dictionary.Clear();
            RaiseOnValueChanged();
        }

        /// <summary>
        /// Configures the component to contains.
        /// </summary>
        public bool Contains(KeyValuePair<TKey, TValue> item) => ((IDictionary<TKey, TValue>)_dictionary).Contains(item);
        /// <summary>
        /// Returns a value indicating whether the dictionary contains the given key.
        /// </summary>
        public bool ContainsKey(TKey                    key)  => _dictionary.ContainsKey(key);

        /// <summary>
        /// Copies the elements of this collection to the given array, starting at the supplied index.
        /// </summary>
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) => ((IDictionary<TKey, TValue>)_dictionary).CopyTo(array, arrayIndex);

        /// <summary>
        /// Removes the given item from the component.
        /// </summary>
        public bool Remove(TKey key)
        {
            if (_dictionary.TryGetValue(key, out var prev))
            {
                _dictionary.Remove(key);
                UnhookValue(prev);
                RaiseOnValueChanged();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Removes the given item from the component.
        /// </summary>
        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            if (_dictionary.TryGetValue(item.Key, out var prev) && Equals(item.Value, prev))
            {
                _dictionary.Remove(item.Key);
                UnhookValue(prev);
                RaiseOnValueChanged();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Attempts to get value, returning a value indicating success.
        /// </summary>
        public bool TryGetValue(TKey key, out TValue value) => _dictionary.TryGetValue(key, out value);

        private void HookValue(TValue v)
        {
            if (_shouldHookNestedObservables) PossibleObservableHelpers.ObserveFutureChangesIfObservable(v, RaiseOnValueChanged);
        }

        private void UnhookValue(TValue v)
        {
            if (_shouldHookNestedObservables) PossibleObservableHelpers.StopObservingIfObservable(v, RaiseOnValueChanged);
        }

        private void RaiseOnValueChanged()
        {
            _debouncer.RaiseOnValueChanged();
        }

        /// <summary>
        /// Returns the enumerator of the component.
        /// </summary>
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => _dictionary.GetEnumerator();
        IEnumerator IEnumerable.                       GetEnumerator() => GetEnumerator();
    }
}