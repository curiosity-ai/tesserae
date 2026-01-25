using System;

namespace Tesserae
{
    /// <summary>
    /// Extension methods for the <see cref="PivotSelector"/> component.
    /// </summary>
    [H5.Name("tss.PivotSelectorX")]
    public static class PivotSelectorExtensions
    {
        /// <summary>
        /// Adds a new tab to the pivot selector.
        /// </summary>
        /// <param name="pivot">The pivot selector to add the tab to.</param>
        /// <param name="id">The unique identifier for the tab.</param>
        /// <param name="title">The title for the tab.</param>
        /// <param name="contentCreator">A function that creates the content component for the tab.</param>
        /// <param name="cached">Whether to cache the content component after it is first created.</param>
        /// <returns>The pivot selector component itself, for chaining.</returns>
        public static PivotSelector Pivot(this PivotSelector pivot, string id, string title, Func<IComponent> contentCreator, bool cached = false)
        {
            return pivot.Add(new PivotSelector.Tab(id, () => UI.TextBlock(title), contentCreator, cached));
        }

        /// <summary>
        /// Adds a new tab to the pivot selector.
        /// </summary>
        /// <param name="pivot">The pivot selector to add the tab to.</param>
        /// <param name="id">The unique identifier for the tab.</param>
        /// <param name="titleCreator">A function that creates the title component for the tab.</param>
        /// <param name="contentCreator">A function that creates the content component for the tab.</param>
        /// <param name="cached">Whether to cache the content component after it is first created.</param>
        /// <returns>The pivot selector component itself, for chaining.</returns>
        public static PivotSelector Pivot(this PivotSelector pivot, string id, Func<IComponent> titleCreator, Func<IComponent> contentCreator, bool cached = false)
        {
            return pivot.Add(new PivotSelector.Tab(id, titleCreator, contentCreator, cached));
        }

        /// <summary>
        /// Adds multiple tabs to the pivot selector.
        /// </summary>
        /// <param name="pivot">The pivot selector to add the tabs to.</param>
        /// <param name="tabs">An array of tab definitions.</param>
        /// <returns>The pivot selector component itself, for chaining.</returns>
        public static PivotSelector Pivot(this PivotSelector pivot, params (string id, string title, Func<IComponent> contentCreator)[] tabs)
        {
            foreach (var tab in tabs)
            {
                pivot.Pivot(tab.id, tab.title, tab.contentCreator);
            }
            return pivot;
        }
    }
}
