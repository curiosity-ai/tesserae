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

        // A blocking overlay stops the page behind it scrolling. Which overlays are doing that is read off
        // the DOM, exactly as CurrentZIndex reads the z-indices: the marker class is the record, so nothing
        // has to be counted and nothing can get out of step. An element yanked out of the document without
        // a release simply stops being found, and the next release restores correctly.
        private const string PageScrollLockClass    = "tss-locks-page-scroll";
        private const string PageScrollLockSelector = ".tss-locks-page-scroll";

        // What the application had on the body before the first overlay locked it - restored when the last
        // one releases. Only written on the 0 -> 1 transition, so a shell that says "the body never scrolls"
        // gets that back rather than having it cleared.
        private static string _bodyOverflowBeforeLock;

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

        /// <summary>
        /// Stops the page behind <paramref name="element"/> scrolling while it is shown, remembering what the
        /// application had set so it can be put back. Safe to call again for an element already locking.
        /// <see cref="Layer{T}.LocksPageScroll"/> is how a layer opts in; anything that is not a layer -
        /// <see cref="ModalStack"/> - calls this directly.
        /// </summary>
        internal static void LockPageScroll(HTMLElement element)
        {
            if (element is null) return;

            if (!AnyPageScrollLock()) _bodyOverflowBeforeLock = document.body.style.overflowY;

            element.classList.add(PageScrollLockClass);
            document.body.style.overflowY = "hidden";
        }

        /// <summary>
        /// Releases <paramref name="element"/>'s hold on the page's scrolling, and puts the application's own
        /// value back once nothing else is holding it. Call it before the element leaves the document, so the
        /// answer does not depend on a removal that may be animated.
        /// </summary>
        internal static void ReleasePageScroll(HTMLElement element)
        {
            // Only an element that was actually holding the page can end the hold. Without this, a layer
            // that never locked - or one that gave up its lock earlier, by being made modeless while open -
            // would restore on the way out and write the saved value a second time, when it has already
            // been consumed: that puts an empty string on the body and is the very clobber this replaced.
            var wasLocking = element is object && element.classList.contains(PageScrollLockClass);

            element?.classList.remove(PageScrollLockClass);

            if (!wasLocking || AnyPageScrollLock()) return;

            document.body.style.overflowY = _bodyOverflowBeforeLock ?? "";
            _bodyOverflowBeforeLock       = null;
        }

        private static bool AnyPageScrollLock() => document.querySelectorAll(PageScrollLockSelector).length > 0;

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