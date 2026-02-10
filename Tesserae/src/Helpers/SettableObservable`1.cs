using System;
using System.Collections.Generic;

namespace Tesserae
{
    // Note.. mainly to myself [2020-07-01 DWR]: This is derived from ReadOnlyObservable and can be passed into a method that wants an observable that it will only read from but it also implements the logic for updating that value so that
    // whatever holds the reference to the instance as this type can update the value

    /// <summary>
    /// Encapsulates a variable of type T, and enables monitoring for changes as well as the ability to update that value (which will trigger a ValueChanged event)
    /// </summary>
    /// <typeparam name="T">An immutable type to be observed. Be careful with non-imutable types, as you can change them in ways that will not be visible here</typeparam>
    [H5.Name("tss.SettableObservableT")]
    public sealed class SettableObservable<T> : ReadOnlyObservable<T>
    {
        public SettableObservable(T value = default, IEqualityComparer<T> comparer = null) : base(value, comparer) { }

        public new T Value
        {
            get => base.Value;
            set => base.Value = value;
        }

        public void Update(Action<T> action)
        {
            action(Value);
            RaiseOnValueChanged(); //Must raise this here, as obj == obj is always true and wont trigger the change
        }
    }
}