using System;
using System.Threading.Tasks;

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
        /// <param name="closeable">Whether the tab shows a close button.</param>
        /// <param name="onClosed">An action to perform once the tab is closed.</param>
        /// <param name="onBeforeClose">
        /// Optional guard awaited when the user clicks the tab's close button. The tab is
        /// only removed if it resolves <c>true</c>; resolve <c>false</c> to keep it open
        /// (e.g. after the user cancels an "unsaved changes" confirmation).
        /// </param>
        /// <returns>The pivot component itself, for chaining.</returns>
        public static Pivot Pivot(this Pivot pivot, string id, Func<IComponent> titleCreator, Func<IComponent> contentCreator, bool cached = false, bool closeable = false, Action onClosed = null, Func<Task<bool>> onBeforeClose = null)
        {
            return pivot.Add(new Pivot.Tab(id, titleCreator, contentCreator, cached, closeable, onClosed, onBeforeClose));
        }

        /// <summary>
        /// Adds a new tab to the pivot that hosts an embedded Modal.
        /// </summary>
        /// <param name="pivot">The pivot to add the tab to.</param>
        /// <param name="modal">The modal to embed.</param>
        /// <param name="id">The unique identifier for the tab.</param>
        /// <param name="titleCreator">A function that creates the title component for the tab.</param>
        /// <param name="closeable">Whether the tab is closeable. Defaults to true for hosted modals.</param>
        /// <param name="onClosed">An action to perform when the tab is closed.</param>
        /// <returns>The pivot component itself, for chaining.</returns>
        public static Pivot Host(this Pivot pivot, Modal modal, string id, Func<IComponent> titleCreator, bool closeable = true, Action onClosed = null)
        {
            return pivot.Pivot(id, titleCreator, () => modal.ShowEmbedded(), cached: true, closeable: closeable, onClosed: () =>
            {
                modal.Hide();
                onClosed?.Invoke();
            });
        }
    }
}