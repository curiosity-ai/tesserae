using Transpose;
using static Transpose.Core.dom;

namespace Tesserae
{
    /// <summary>
    /// Application-wide registry that tracks the current z-index for stacked layers (modals, dialogs, toasts).
    /// </summary>
    [Name("tss.Layers")]
    public static class Layers
    {
        private const int BaseZIndex = 1000;

        // Elements pinned above the whole layer stack. A full-width banner is page chrome: it shrinks the
        // body and stays fixed to a viewport edge, so a modal opened after it must not cover it. Pushing a
        // layer lifts them again, and AboveCurrent() counts them so a popover opened from inside one still
        // lands in front of it.
        private const string AlwaysOnTopClass    = "tss-always-on-top";
        private const string AlwaysOnTopSelector = ".tss-always-on-top";

        // One selector rather than one query per kind: every caller here needs the maximum z-index
        // over the same union of elements, and each document.querySelectorAll walks the whole tree.
        // This is on the path of every tooltip that gets created, so the scans add up quickly.
        private const string LayerSelector           = ".tss-layer,[data-tippy-root]";
        private const string LayerOrAlwaysOnTopScope = ".tss-layer,[data-tippy-root],.tss-always-on-top";

        /// <summary>
        /// Configures the push layer on the component.
        /// </summary>
        public static string PushLayer(HTMLElement element)
        {
            int zIndex = CurrentZIndex() + 10;

            foreach (HTMLElement pinned in document.querySelectorAll(AlwaysOnTopSelector))
            {
                pinned.style.zIndex = (zIndex + 5).ToString();
            }

            return zIndex.ToString();
        }

        /// <summary>
        /// Pins an element above every layer and keeps it there: whatever is pushed afterwards lifts it
        /// again rather than covering it. For page chrome that lives outside the body box, such as an
        /// edge-to-edge banner - an ordinary overlay wants <see cref="PushLayer"/> instead.
        /// </summary>
        public static string PushAlwaysOnTop(HTMLElement element)
        {
            element.classList.add(AlwaysOnTopClass);
            return (CurrentZIndex() + 10).ToString();
        }

        // Imperatively-shown Tippy popovers (Popover / Menu / TreeCommand / SidebarCommand / Teaching)
        // also participate in the application z-index stack: when one of them is visible, any new
        // Layer (Dropdown, Modal, Panel, …) opened on top of it must sit above it visually, not
        // be hidden behind Tippy's hard-coded z-index. Including [data-tippy-root] in the scan
        // means PushLayer() naturally lifts subsequent layers above the popover.
        internal static int CurrentZIndex() => MaxZIndex(LayerSelector);

        /// <summary>
        /// Configures the above current on the component.
        /// </summary>
        public static string AboveCurrent() => (MaxZIndex(LayerOrAlwaysOnTopScope) + 5).ToString();

        private static int MaxZIndex(string selector)
        {
            int maxIndex = BaseZIndex;

            foreach (HTMLElement htmlElement in document.querySelectorAll(selector))
            {
                if (int.TryParse(htmlElement.style.zIndex, out var zIndex) && zIndex > maxIndex) maxIndex = zIndex;
            }

            return maxIndex;
        }
    }
}