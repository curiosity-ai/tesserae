using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Tesserae
{
    public class ObservableList<T>: Observable, IList<T>, ICollection<T>, IObservable<IReadOnlyList<T>>
    {
        private readonly List<T> _list;
        private event Observable<IReadOnlyList<T>>.ValueChanged _onValueChanged;
        private bool _valueIsObservable;

        public ObservableList()
        {
            _list = new List<T>();
            _valueIsObservable = typeof(IObservable).IsAssignableFrom(typeof(T));
        }

        public ObservableList(params T[] initialValues)
        {
            _list = initialValues.ToList();
            _valueIsObservable = typeof(IObservable).IsAssignableFrom(typeof(T));
            if(_valueIsObservable)
            {
                foreach(var i in _list)
                {
                    HookValue(i);
                }
            }
        }

        private void HookValue(T v)
        {
            if (_valueIsObservable && v is object)
            {
                ((IObservable)v).OnChange(RaiseOnValueChanged);
            }
        }
        private void UnhookValue(T v)
        {
            if (_valueIsObservable && v is object)
            {
                ((IObservable)v).Unobserve(RaiseOnValueChanged);
            }
        }

        public void Observe(Observable<IReadOnlyList<T>>.ValueChanged valueGetter)
        {
            _onValueChanged += valueGetter;
            valueGetter(_list);
        }

        public void ObserveLazy(Observable<IReadOnlyList<T>>.ValueChanged valueGetter)
        {
            _onValueChanged += valueGetter;
        }

        public void Unobserve(Observable<IReadOnlyList<T>>.ValueChanged onChange)
        {
            _onValueChanged -= onChange;
        }

        public T this[int index] 
        {
            get => _list[index]; 
            set 
            { 
                if(_list.Count > index)
                {
                    UnhookValue(_list[index]);
                }
                _list[index] = value;
                RaiseOnValueChanged();
            }
        }

        private void RaiseOnValueChanged()
        {
            _onValueChanged?.Invoke(_list);
            RaiseOnChanged();
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
            RaiseOnValueChanged();
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
            if(_list.Count > index)
            {
                UnhookValue(_list[index]);
            }

            _list.Insert(index, item);
            HookValue(item);
            RaiseOnValueChanged();
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

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _list.GetEnumerator();
        }
    }
}