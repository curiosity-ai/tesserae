using System;
using Transpose;
using static Transpose.Core.dom;

namespace Tesserae
{
    /// <summary>
    /// Fits a growing text area to its content. A text area that is not rendered - inside a <c>display: none</c>
    /// ancestor, such as a closed DockPanel pane, a collapsed section or a hidden tab - reads a scrollHeight of 0, and
    /// writing that as its height leaves it invisible once it is shown. So while it has no box this leaves the height to
    /// the stylesheet and measures again the first time it gets one.
    /// </summary>
    internal static class ContentHeight
    {
        private const string AwaitingBox = "tss-fit-awaiting-box";

        /// <summary>
        /// Runs <paramref name="fit"/> now if <paramref name="element"/> is rendered. Otherwise clears its inline height
        /// and runs <paramref name="fit"/> once the element is laid out. Returns whether it ran now.
        /// </summary>
        public static bool FitWhenRendered(HTMLElement element, Action fit)
        {
            if (HasBox(element))
            {
                fit();
                return true;
            }

            element.style.height = "";
            if (element.hasAttribute(AwaitingBox)) return false;

            element.setAttribute(AwaitingBox, "");
            var observer = new ResizeObserver((entries, obs) =>
            {
                if (!HasBox(element)) return;
                obs.disconnect();
                element.removeAttribute(AwaitingBox);
                fit();
            });
            observer.observe(element);
            return false;
        }

        // No client rects: the element, or an ancestor, is display: none (or it is detached).
        private static bool HasBox(HTMLElement element) => element.getClientRects().As<DOMRectList>().length > 0;
    }
}
