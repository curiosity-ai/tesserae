using System.Collections.Generic;

namespace Tesserae
{
    /// <summary>
    /// Defines a component that exposes its value as an observable.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    [H5.Name("tss.IObservableComponent")]
    public interface IObservableComponent<T>
    {
        /// <summary>Gets an observable that tracks the component's value.</summary>
        /// <returns>An observable.</returns>
        IObservable<T> AsObservable();
    }

    /// <summary>
    /// Defines a component that exposes its list of values as an observable.
    /// </summary>
    /// <typeparam name="T">The type of the items in the list.</typeparam>
    [H5.Name("tss.IObservableListComponent")]
    public interface IObservableListComponent<T>
    {
        /// <summary>Gets an observable that tracks the component's list of values.</summary>
        /// <returns>An observable.</returns>
        IObservable<IReadOnlyList<T>> AsObservable();
    }

    /// <summary>
    /// Defines a component whose value can both be observed and programmatically set,
    /// enabling two-way binding against a SettableObservable&lt;T&gt; via the .Bind extension.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    [H5.Name("tss.IBindableComponent")]
    public interface IBindableComponent<T> : IObservableComponent<T>
    {
        /// <summary>
        /// Sets the component's value as the result of a binding push from a source observable.
        /// Implementations must update the visual/DOM state and the internal observable, but must
        /// not raise a user-interaction event that would echo back to the bound source.
        /// </summary>
        void SetBoundValue(T value);
    }
}