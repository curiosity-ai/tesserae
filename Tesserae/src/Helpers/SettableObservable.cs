using System.Collections.Generic;

namespace Tesserae
{
    /// <summary>
    /// Provides a static factory class for creating SettableObservable instances.
    /// </summary>
    [Transpose.Name("tss.SettableObservable")]
    public static class SettableObservable
    {
        /// <summary>
        /// Creates a <see cref="SettableObservable{T}"/> with the type inferred from <paramref name="value"/>
        /// (e.g. <c>SettableObservable.Of("Ada")</c>), so the item type does not have to be repeated.
        /// </summary>
        /// <typeparam name="T">The type of the value to observe.</typeparam>
        /// <param name="value">The initial value.</param>
        /// <param name="comparer">An optional equality comparer.</param>
        /// <returns>A new SettableObservable instance.</returns>
        public static SettableObservable<T> Of<T>(T value, IEqualityComparer<T> comparer = null) => new SettableObservable<T>(value, comparer);

        /// <summary>
        /// Toggles the boolean value of the SettableObservable.
        /// </summary>
        /// <param name="observable">The observable boolean.</param>
        public static void Toggle(this SettableObservable<bool> observable)
        {
            observable.Value = !observable.Value;
        }
    }
}