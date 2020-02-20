using System.Collections;
using System.Collections.Generic;

namespace Tesserae
{
    public class ObservableDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IObservable<IReadOnlyDictionary<TKey, TValue>>
    {
        private readonly Dictionary<TKey, TValue> _dictionary;
        private event Observable<IReadOnlyDictionary<TKey, TValue>>.ValueChanged onChanged;
        public IReadOnlyDictionary<TKey, TValue> Value => _dictionary;

        public void Observe(Observable<IReadOnlyDictionary<TKey, TValue>>.ValueChanged valueGetter)
        {
            onChanged += valueGetter;
            valueGetter(_dictionary);
        }

        public void ObserveLazy(Observable<IReadOnlyDictionary<TKey, TValue>>.ValueChanged valueGetter)
        {
            onChanged += valueGetter;
        }

        public TValue this[TKey key] { get => _dictionary[key]; set { _dictionary[key] = value; onChanged?.Invoke(_dictionary); } }

        public ICollection<TKey> Keys => _dictionary.Keys;

        public ICollection<TValue> Values => _dictionary.Values;

        public int Count => _dictionary.Count;

        public bool IsReadOnly => false;

        public void Add(TKey key, TValue value)
        {
            _dictionary.Add(key, value);
            onChanged?.Invoke(_dictionary);
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            _dictionary.Add(item.Key, item.Value);
            onChanged?.Invoke(_dictionary);
        }

        public void Clear()
        {
            _dictionary.Clear();
            onChanged?.Invoke(_dictionary);
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return ((IDictionary<TKey, TValue>)_dictionary).Contains(item);
        }

        public bool ContainsKey(TKey key)
        {
            return _dictionary.ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            ((IDictionary<TKey, TValue>)_dictionary).CopyTo(array, arrayIndex);
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return _dictionary.GetEnumerator();
        }

        public bool Remove(TKey key)
        {
            var removed = _dictionary.Remove(key);
            if (removed)
            {
                onChanged?.Invoke(_dictionary);
            }
            return removed;
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            var removed =  ((IDictionary<TKey, TValue>)_dictionary).Remove(item);
            if(removed)
            {
                onChanged?.Invoke(_dictionary);
            }
            return removed;
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return _dictionary.TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _dictionary.GetEnumerator();
        }
    }
}