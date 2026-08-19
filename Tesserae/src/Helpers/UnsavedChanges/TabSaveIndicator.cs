using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Transpose;
using static Transpose.Core.dom;

namespace Tesserae
{
    /// <summary>
    /// Toggles a CSS class on a <see cref="Pivot"/> tab title element so it can show an
    /// "unsaved changes" marker: a dot standing in for the close cross on a closeable tab,
    /// or beside the label on a tab without one. Both are drawn from <c>tss.pivot.css</c>.
    /// </summary>
    /// <remarks>
    /// An editor hosted as a pivot tab calls <see cref="MarkDirty"/> / <see cref="MarkClean"/>
    /// from inside its own validation / save handling. The host gives the tab title element an
    /// <c>id</c> (any string that is stable for as long as the tab is open) and the editor
    /// references that same id, so the host and the editor don't need to pass tab/component
    /// references back and forth.
    ///
    /// An editor also registers its save handler here via <see cref="OnSave"/>, which is what
    /// lets <see cref="UnsavedChangesGuard"/> offer to save the tab instead of only offering to
    /// discard it.
    /// </remarks>
    [Transpose.Name("tss.TabSaveIndicator")]
    public static class TabSaveIndicator
    {
        public const string NeedsSavingClass = "tss-pivot-tab-needs-saving";

        // Save handler per tab indicator id. Registered by the editor hosted in
        // the tab, dropped by the host when the tab closes (see Forget).
        private static readonly Dictionary<string, Func<Task<bool>>> _saveHandlers = new Dictionary<string, Func<Task<bool>>>();

        /// <summary>Builds a stable tab indicator id from an item type and uid, e.g. <c>tab-endpoint-{uid}</c>.</summary>
        public static string TabId(string itemType, object uid) => $"tab-{itemType}-{uid}";

        /// <summary>
        /// A pivot tab title that carries <paramref name="tabIndicatorId"/>, so
        /// <see cref="MarkDirty"/> can find it. Same styling as <see cref="UI.PivotTitle(string)"/>
        /// — a title built from anything else (a bare <c>TextBlock</c>, say) gets none of the
        /// tab strip's padding and sits wrong in the strip.
        /// </summary>
        public static Func<IComponent> Title(string tabIndicatorId, string text) => () => UI.Button(text).NoBackground().Regular().Id(tabIndicatorId);

        /// <summary>
        /// A pivot tab title with an icon that carries <paramref name="tabIndicatorId"/>.
        /// See <see cref="Title(string, string)"/>.
        /// </summary>
        public static Func<IComponent> Title(string tabIndicatorId, string text, UIcons icon) => () => UI.Button(text).NoBackground().Regular().SetIcon(icon).Id(tabIndicatorId);

        public static void MarkDirty(string tabIndicatorId)
        {
            if (string.IsNullOrEmpty(tabIndicatorId)) return;
            var el = document.getElementById(tabIndicatorId);
            el?.classList.add(NeedsSavingClass);
        }

        public static void MarkClean(string tabIndicatorId)
        {
            if (string.IsNullOrEmpty(tabIndicatorId)) return;
            var el = document.getElementById(tabIndicatorId);
            el?.classList.remove(NeedsSavingClass);
        }

        /// <summary>Whether the tab currently shows the unsaved-changes indicator.</summary>
        public static bool IsDirty(string tabIndicatorId)
        {
            if (string.IsNullOrEmpty(tabIndicatorId)) return false;
            var el = document.getElementById(tabIndicatorId);
            return el is object && el.classList.contains(NeedsSavingClass);
        }

        /// <summary>
        /// Registers the editor's save handler for <paramref name="tabIndicatorId"/>.
        /// It must return whether the save actually succeeded — an editor that
        /// refuses to save (incomplete form, compile error, …) returns false and
        /// is expected to tell the user why.
        /// </summary>
        public static void OnSave(string tabIndicatorId, Func<Task<bool>> saveAsync)
        {
            if (string.IsNullOrEmpty(tabIndicatorId) || saveAsync is null) return;
            _saveHandlers[tabIndicatorId] = saveAsync;
        }

        /// <summary>Drops the registered save handler — call this when the tab closes.</summary>
        public static void Forget(string tabIndicatorId)
        {
            if (string.IsNullOrEmpty(tabIndicatorId)) return;
            _saveHandlers.Remove(tabIndicatorId);
        }

        /// <summary>Whether an editor registered a save handler for this tab.</summary>
        public static bool CanSave(string tabIndicatorId)
        {
            return !string.IsNullOrEmpty(tabIndicatorId) && _saveHandlers.ContainsKey(tabIndicatorId);
        }

        /// <summary>
        /// Runs the tab's registered save handler. Returns false when the tab has
        /// no handler or the editor reported the save didn't go through.
        /// </summary>
        public static Task<bool> SaveAsync(string tabIndicatorId)
        {
            if (string.IsNullOrEmpty(tabIndicatorId) || !_saveHandlers.TryGetValue(tabIndicatorId, out var save)) return Task.FromResult(false);
            return save();
        }

        /// <summary>
        /// The ids of every tab currently showing the unsaved-changes indicator.
        /// Reading them back off the DOM (rather than a registry we'd have to keep
        /// in sync) means a tab that was torn down can't leave a stale entry behind.
        /// </summary>
        public static string[] DirtyTabIds()
        {
            var ids = new List<string>();

            foreach (var el in (IEnumerable<HTMLElement>)document.querySelectorAll<HTMLElement>("." + NeedsSavingClass))
            {
                if (!string.IsNullOrEmpty(el.id)) ids.Add(el.id);
            }

            return ids.ToArray();
        }

        /// <summary>The tab's visible title, used when telling the user which editors are unsaved.</summary>
        public static string TitleOf(string tabIndicatorId)
        {
            if (string.IsNullOrEmpty(tabIndicatorId)) return null;
            var el = document.getElementById(tabIndicatorId);
            return el is object ? el.textContent?.Trim() : null;
        }
    }
}
