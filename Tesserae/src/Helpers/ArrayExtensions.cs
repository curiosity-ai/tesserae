using System;
using System.Collections.Generic;

namespace Tesserae
{
    [H5.Name("tss.arX")]
    public static class ArrayExtensions
    {
        public static ReadOnlyArray<T> AsReadOnlyArray<T>(this T[] source) => source;

        /// <summary>
        /// Like IndexOf, but returns int.MaxValue if an item is not found instead of -1
        /// </summary>
        public static int IndexOrder<T>(this T[] array, T value)
        {
            var index = Array.IndexOf(array, value);
            return index < 0 ? int.MaxValue : index;
        }

        /// <summary>
        /// Removes an item at the "from" index and inserts it at the "to" index
        /// </summary>
        public static void MoveItem<T>(this T[] arr, int fromIndex, int toIndex)
        {
            var element = arr[fromIndex];
            arr.Splice(fromIndex, 1);
            arr.Splice(toIndex,   0, element);
        }
        /// <summary>
        /// Gets the first item that matches if it exists
        /// </summary>
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