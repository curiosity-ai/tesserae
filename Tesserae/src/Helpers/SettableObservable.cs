using System.Collections.Generic;

namespace Tesserae
{
    /// <summary>
    /// Provides a static factory class for creating SettableObservable instances.
    /// </summary>
    [H5.Name("tss.SettableObservable")]
    public static class SettableObservable
    {
        /// <summary>
        /// This is a static factory method that lets us leverage type inference (so you can create a SettableObservable from an item without having to repeat the type name of the item when creating the instance)
        /// </summary>
        /// <typeparam name="T">The type of the value to observe.</typeparam>
        /// <param name="value">The initial value.</param>
        /// <param name="comparer">An optional equality comparer.</param>
        /// <returns>A new SettableObservable instance.</returns>
        public static SettableObservable<T> For<T>(T value, IEqualityComparer<T> comparer = null) => new SettableObservable<T>(value, comparer);
    }
}