using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Tesserae
{
    public class ObservableList<T>: IList<T>, ICollection<T>, IObservable<IReadOnlyList<T>>
    {
        private readonly List<T> _list;
        private event Observable<IReadOnlyList<T>>.ValueChanged onChanged;

        public ObservableList()
        {
            _list = new List<T>();
        }

        public ObservableList(params T[] initialValues)
        {
            _list = initialValues.ToList();
        }

        public void Observe(Observable<IReadOnlyList<T>>.ValueChanged valueGetter)
        {
            onChanged += valueGetter;
            valueGetter(_list);
        }

        public void ObserveLazy(Observable<IReadOnlyList<T>>.ValueChanged valueGetter)
        {
            onChanged += valueGetter;
        }

        public T this[int index] { get => _list[index]; set { _list[index] = value; onChanged?.Invoke(_list); } }

        public int Count => _list.Count;

        public bool IsReadOnly => false;

        public IReadOnlyList<T> Value => _list;
        
        public void Add(T item)
        {
            _list.Add(item);
            onChanged?.Invoke(_list);
        }

        public void Clear()
        {
            _list.Clear();
            onChanged?.Invoke(_list);
        }

        public bool Contains(T item)
        {
            return _list.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _list.CopyTo(array, arrayIndex);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        public int IndexOf(T item)
        {
            return _list.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            _list.Insert(index, item);
            onChanged?.Invoke(_list);
        }

        public bool Remove(T item)
        {
            var removed = _list.Remove(item);
            if (removed)
            {
                onChanged?.Invoke(_list);
            }
            return removed;
        }

        public void RemoveAt(int index)
        {
            _list.RemoveAt(index);
            onChanged?.Invoke(_list);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _list.GetEnumerator();
        }
    }
}