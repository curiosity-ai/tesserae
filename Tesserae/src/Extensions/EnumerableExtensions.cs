using System.Collections.Generic;
using System.Linq;

namespace Tesserae
{
    /// <summary>
    /// Provides internal extension methods for IEnumerable.
    /// </summary>
    [H5.Name("tss.eeX")]
    internal static class EnumerableExtensions
    {
        /// <summary>
        /// Splits an enumerable into groups of a specified size.
        /// </summary>
        /// <typeparam name="T">The type of the items.</typeparam>
        /// <param name="source">The source enumerable.</param>
        /// <param name="groupSize">The size of each group.</param>
        /// <returns>A list of lists, where each inner list contains up to groupSize items.</returns>
        internal static List<List<T>> InGroupsOf<T>(
            this IEnumerable<T> source,
            int                 groupSize)
        {
            return source
               .Select((item, index) => new { Index = index, Item = item })
               .GroupBy(item => item.Index / groupSize)
               .Select(groupItem => groupItem.Select(item => item.Item).ToList())
               .ToList();
        }
    }
}