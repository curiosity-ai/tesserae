using System;

namespace Tesserae
{
    /// <summary>
    /// Extension methods for the <see cref="Pivot"/> component.
    /// </summary>
    [H5.Name("tss.PivotX")]
    public static class PivotExtensions
    {
        /// <summary>
        /// Adds a new tab to the pivot.
        /// </summary>
        /// <param name="pivot">The pivot to add the tab to.</param>
        /// <param name="id">The unique identifier for the tab.</param>
        /// <param name="titleCreator">A function that creates the title component for the tab.</param>
        /// <param name="contentCreator">A function that creates the content component for the tab.</param>
        /// <param name="cached">Whether to cache the content component after it is first created.</param>
        /// <returns>The pivot component itself, for chaining.</returns>
        public static Pivot Pivot(this Pivot pivot, string id, Func<IComponent> titleCreator, Func<IComponent> contentCreator, bool cached = false)
        {
            return pivot.Add(new Pivot.Tab(id, titleCreator, contentCreator, cached));
        }
    }
}