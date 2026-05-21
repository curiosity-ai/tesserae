using H5;
using static H5.Core.dom;

namespace Tesserae
{
    /// <summary>
    /// Application-wide registry that tracks the current z-index for stacked layers (modals, dialogs, toasts).
    /// </summary>
    [Name("tss.Layers")]
    public static class Layers
    {
        private const int BaseZIndex = 1000;
        /// <summary>
        /// Configures the push layer on the component.
        /// </summary>
        public static string PushLayer(HTMLElement element)
        {
            return (CurrentZIndex() + 10).ToString();
        }

        internal static int CurrentZIndex()
        {
            int maxIndex = BaseZIndex;

            foreach (HTMLElement htmlElement in document.querySelectorAll(".tss-layer"))
            {
                if (int.TryParse(htmlElement.style.zIndex, out var zIndex) && zIndex > maxIndex) maxIndex = zIndex;
            }
            // Imperatively-shown Tippy popovers (Popover / Menu / TreeCommand / SidebarCommand / Teaching)
            // also participate in the application z-index stack: when one of them is visible, any new
            // Layer (Dropdown, Modal, Panel, …) opened on top of it must sit above it visually, not
            // be hidden behind Tippy's hard-coded z-index. Including [data-tippy-root] in the scan
            // means PushLayer() naturally lifts subsequent layers above the popover.
            foreach (HTMLElement htmlElement in document.querySelectorAll("[data-tippy-root]"))
            {
                if (int.TryParse(htmlElement.style.zIndex, out var zIndex) && zIndex > maxIndex) maxIndex = zIndex;
            }
            return maxIndex;
        }

        /// <summary>
        /// Configures the above current on the component.
        /// </summary>
        public static string AboveCurrent()
        {
            return (CurrentZIndex() + 5).ToString();
        }
    }
}