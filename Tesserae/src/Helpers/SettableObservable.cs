using System.Collections.Generic;

namespace Tesserae
{
    [H5.Name("tss.SettableObservable")]
    public static class SettableObservable
    {
        /// <summary>
        /// This is a static factory method that lets us leverage type inference (so you can create a SettableObservable from an item without having to repeat the type name of the item when creating the instance)
        /// </summary>
        public static SettableObservable<T> For<T>(T value, IEqualityComparer<T> comparer = null) => new SettableObservable<T>(value, comparer);
    }
}