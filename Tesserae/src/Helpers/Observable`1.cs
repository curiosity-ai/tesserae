using static Retyped.dom;

namespace Tesserae
{
    /// <summary>
    /// Enables monitoring of changes for a variable of type T (this class is for listeners only, if updating the value is required then the SettableObserver should be used)
    /// </summary>
    /// <typeparam name="T">An immutable type to be observed. Be careful with non-imutable types, as they may be changed in ways that will not be repoted here</typeparam>
    public class Observable<T> : Observable, IObservable<T>
    {
        private T _value;
        private double _refreshTimeout;

        public Observable(T value = default) => _value = value;

        public delegate void ValueChanged(T value);

        private event ValueChanged OnValueChanged;

        public T Value
        {
            get => _value;
            protected set
            {
                if (!_value.Equals(value))
                {
                    _value = value;
                    RaiseOnValueChanged();
                }
            }
        }

        private void RaiseOnValueChanged()
        {
            window.clearTimeout(_refreshTimeout);
            _refreshTimeout = window.setTimeout(raise, 1);
            void raise(object t)
            {
                OnValueChanged?.Invoke(_value);
                RaiseOnChanged();
            }
        }

        /// <summary>
        /// This will register a callback that will be executed when the value is changed and it will immediately execute that callback now (this is useful when initialising an object that relies upon an Observable, it means that
        /// that there is less code required because calling this Observe method will immediately execute that callback and update the object according to the Observable's current state - otherwise, the initialisation code would
        /// have to create the dependent object and then call Observe AND THEN set the initial state). If it is not desirable for the callback to be made immediately then use ObserveLazy instead of Observe.
        /// </summary>
        public void Observe(ValueChanged onChange)
        {
            OnValueChanged += onChange;
            onChange(_value);
        }

        /// <summary>
        /// See the summary comments on Observe to find out the differences between Observe and ObserveLazy
        /// </summary>
        public void ObserveLazy(ValueChanged onChange) => OnValueChanged += onChange;

        public void Unobserve(ValueChanged onChange) => OnValueChanged -= onChange;
    }
}