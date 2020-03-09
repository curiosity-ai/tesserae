namespace Tesserae
{
    public interface IObservable<T> : IObservable
    {
        T Value { get; }

        /// <summary>
        /// This will register a callback that will be executed when the value is changed and it will immediately execute that callback now (this is useful when initialising an object that relies upon an Observable, it means that
        /// that there is less code required because calling this Observe method will immediately execute that callback and update the object according to the Observable's current state - otherwise, the initialisation code would
        /// have to create the dependent object and then call Observe AND THEN set the initial state). If it is not desirable for the callback to be made immediately then use ObserveLazy instead of Observe.
        /// </summary>
        void Observe(Observable<T>.ValueChanged onChange);

        /// <summary>
        /// See the summary comments on Observe to find out the differences between Observe and ObserveLazy
        /// </summary>
        void ObserveLazy(Observable<T>.ValueChanged onChange);

        void Unobserve(Observable<T>.ValueChanged onChange);
    }
}