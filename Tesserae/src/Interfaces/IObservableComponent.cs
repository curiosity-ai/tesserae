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
}