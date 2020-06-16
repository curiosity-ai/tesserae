using System.Collections;
using System.Collections.Generic;
using static H5.Core.dom;

namespace Tesserae
{
    public class ObservableDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IObservable<IReadOnlyDictionary<TKey, TValue>>
    {
        private event ObservableEvent.ValueChanged<IReadOnlyDictionary<TKey, TValue>> OnValueChanged;
        public IReadOnlyDictionary<TKey, TValue> Value => _dictionary;
        
        private readonly Dictionary<TKey, TValue> _dictionary;
        private bool _valueIsObservable;
        private double _refreshTimeout;

        public ObservableDictionary()
        {
            _dictionary = new Dictionary<TKey, TValue>();
            _valueIsObservable = typeof(TValue).IsObservable();
        }

        public ObservableDictionary(Dictionary<TKey, TValue> dictionary)
        {
            _dictionary = dictionary;
            _valueIsObservable = typeof(TValue).IsObservable();
            if (_valueIsObservable)
            {
                foreach (var kv in _dictionary)
                {
                    HookValue(kv.Value);
                }
            }
        }

        public void Observe(ObservableEvent.ValueChanged<IReadOnlyDictionary<TKey, TValue>> valueGetter) => Observe(valueGetter, callbackImmediately: true);
        public void ObserveFutureChanges(ObservableEvent.ValueChanged<IReadOnlyDictionary<TKey, TValue>> valueGetter) => Observe(valueGetter, callbackImmediately: false);
        private void Observe(ObservableEvent.ValueChanged<IReadOnlyDictionary<TKey, TValue>> valueGetter, bool callbackImmediately)
        {
            OnValueChanged += valueGetter;
            if (callbackImmediately)
                valueGetter(Value);
        }

        public void StopObserving(ObservableEvent.ValueChanged<IReadOnlyDictionary<TKey, TValue>> valueGetter) => OnValueChanged -= valueGetter;

        private void HookValue(TValue v)
        {
            if (_valueIsObservable && (v is IObservable<TValue> observable))
                observable.ObserveFutureChanges(RaiseOnValueChanged);
        }

        private void UnhookValue(TValue v)
        {
            if (_valueIsObservable && v is IObservable<TValue> observable)
                observable.StopObserving(RaiseOnValueChanged);
        }

        private void RaiseOnValueChanged(TValue value)
        {
            window.clearTimeout(_refreshTimeout);
            _refreshTimeout = window.setTimeout(raise, 1);
            void raise(object t)
            {
                OnValueChanged?.Invoke(_dictionary);
            }
        }

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
                RaiseOnValueChanged(value);
            }
        }

        public ICollection<TKey> Keys => _dictionary.Keys;
        public ICollection<TValue> Values => _dictionary.Values;
        public int Count => _dictionary.Count;
        public bool IsReadOnly => false;

        public void Add(TKey key, TValue value)
        {
            _dictionary.Add(key, value);
            HookValue(value);
            RaiseOnValueChanged(value);
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            _dictionary.Add(item.Key, item.Value);
            HookValue(item.Value);
            RaiseOnValueChanged(item.Value);
        }

        public void Clear()
        {
            if (_valueIsObservable)
            {
                foreach (var kv in _dictionary)
                {
                    UnhookValue(kv.Value);
                }
            }

            _dictionary.Clear();
            RaiseOnValueChanged(default);
        }

        public bool Contains(KeyValuePair<TKey, TValue> item) => ((IDictionary<TKey, TValue>)_dictionary).Contains(item);
        public bool ContainsKey(TKey key) => _dictionary.ContainsKey(key);

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) => ((IDictionary<TKey, TValue>)_dictionary).CopyTo(array, arrayIndex);

        public bool Remove(TKey key)
        {
            if (_dictionary.TryGetValue(key, out var prev))
            {
                _dictionary.Remove(key);
                UnhookValue(prev);
                RaiseOnValueChanged(default);
                return true;
            }
            return false;
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            if (_dictionary.TryGetValue(item.Key, out var prev) && Equals(item.Value, prev))
            {
                _dictionary.Remove(item.Key);
                UnhookValue(prev);
                RaiseOnValueChanged(default);
                return true;
            }
            return false;
        }

        public bool TryGetValue(TKey key, out TValue value) => _dictionary.TryGetValue(key, out value);

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => _dictionary.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}