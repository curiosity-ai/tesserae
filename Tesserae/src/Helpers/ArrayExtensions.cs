using System;
using System.Collections.Generic;

namespace Tesserae
{
    /// <summary>
    /// Provides extension methods for arrays.
    /// </summary>
    [H5.Name("tss.arX")]
    public static class ArrayExtensions
    {
        /// <summary>Wraps an array into a ReadOnlyArray.</summary>
        public static ReadOnlyArray<T> AsReadOnlyArray<T>(this T[] source) => source;

        /// <summary>
        /// Like IndexOf, but returns int.MaxValue if an item is not found instead of -1
        /// </summary>
        /// <typeparam name="T">The type of the items.</typeparam>
        /// <param name="array">The array.</param>
        /// <param name="value">The value to find.</param>
        /// <returns>The index of the item, or int.MaxValue if not found.</returns>
        public static int IndexOrder<T>(this T[] array, T value)
        {
            var index = Array.IndexOf(array, value);
            return index < 0 ? int.MaxValue : index;
        }

        /// <summary>
        /// Removes an item at the "from" index and inserts it at the "to" index.
        /// </summary>
        /// <typeparam name="T">The type of the items.</typeparam>
        /// <param name="arr">The array.</param>
        /// <param name="fromIndex">The source index.</param>
        /// <param name="toIndex">The destination index.</param>
        public static void MoveItem<T>(this T[] arr, int fromIndex, int toIndex)
        {
            var element = arr[fromIndex];
            arr.Splice(fromIndex, 1);
            arr.Splice(toIndex,   0, element);
        }
        /// <summary>
        /// Gets the first item that matches if it exists.
        /// </summary>
        /// <typeparam name="T">The type of the items.</typeparam>
        /// <param name="arr">The source enumerable.</param>
        /// <param name="match">The predicate function.</param>
        /// <param name="value">The found item, if any.</param>
        /// <returns>True if a match was found, false otherwise.</returns>
        public static bool TryGetFirst<T>(this IEnumerable<T> arr, Func<T, bool> match, out T value)
        {
            foreach (var item in arr)
            {
                if (match(item))
                {
                    value = item;
                    return true;
                }
            }
            value = default;
            return false;
        }
    }
}