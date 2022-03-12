using System.Collections.Generic;
using static H5.Core.dom;

namespace Tesserae
{
    /// <summary>
    /// Enables monitoring of changes for a variable of type T (this class is for listeners only, if updating the value is required then the SettableObserver should be used)
    /// </summary>
    /// <typeparam name="T">An immutable type to be observed. Be careful with non-imutable types, as they may be changed in ways that will not be repoted here</typeparam>
    [H5.Name("tss.ConstantObservableT")]
    public class ConstantObservable<T> : IObservable<T> 
    {
        private T _value;

        protected ConstantObservable(T value)
        {
            _value = value;
        }

        public void Observe(ObservableEvent.ValueChanged<T> valueGetter) { valueGetter(Value); }
        public void ObserveFutureChanges(ObservableEvent.ValueChanged<T> valueGetter) { }
        public void StopObserving(ObservableEvent.ValueChanged<T> valueGetter) { }

        public T Value => _value;
    }
}