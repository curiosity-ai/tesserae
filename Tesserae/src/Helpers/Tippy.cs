using System;
using System.Threading.Tasks;
using Tesserae;
using static H5.Core.dom;

namespace Tesserae
{
    /// <summary>
    /// JavaScript-interop literal describing the current state of a Tippy popover instance.
    /// </summary>
    [H5.ObjectLiteral]
    public class TippyInstanceState
    {
        /// <summary>
        /// True when the Tippy instance is enabled and can be shown.
        /// </summary>
        public bool isEnabled;
        /// <summary>
        /// True when the Tippy popover is currently visible.
        /// </summary>
        public bool isVisible;
        /// <summary>
        /// True after the Tippy instance has been destroyed.
        /// </summary>
        public bool isDestroyed;
        /// <summary>
        /// True when the Tippy popper element is mounted in the DOM.
        /// </summary>
        public bool isMounted;
        /// <summary>
        /// True once the Tippy has finished its show animation.
        /// </summary>
        public bool isShown;
    }

    [H5.ObjectLiteral]
    public class TippyInstance
    {
        /// <summary>
        /// Clears any pending show/hide delay timeouts on this instance.
        /// </summary>
        public void clearDelayTimeouts() { }
        /// <summary>
        /// Permanently destroys this Tippy instance, releasing its DOM and listeners.
        /// </summary>
        public void destroy() { }
        /// <summary>
        /// Disables this Tippy instance so it can no longer be shown.
        /// </summary>
        public void disable() { }
        /// <summary>
        /// Enables this Tippy instance so it can be shown again.
        /// </summary>
        public void enable() { }
        /// <summary>
        /// Hides the popover immediately.
        /// </summary>
        public void hide() { }
        /// <summary>
        /// Shows the popover.
        /// </summary>
        public void show() { }
        /// <summary>
        /// Hides the popover with an interactive grace period that gives the user time to move the cursor into it before it disappears.
        /// </summary>
        public void hideWithInteractivity() { }
        /// <summary>
        /// Gets the unique numeric identifier assigned to this Tippy instance.
        /// </summary>
        public int id { get; }
        /// <summary>
        /// Removes the popper element from the DOM without destroying the instance.
        /// </summary>
        public void unmount() { }
        /// <summary>
        /// Gets the anchor element the popover is positioned against.
        /// </summary>
        public HTMLElement reference { get; }
        /// <summary>
        /// Replaces the content of the popover with the given element.
        /// </summary>
        public void setContent(HTMLElement content) { }

        /// <summary>
        /// Gets a snapshot of the current state (enabled / visible / mounted / shown / destroyed).
        /// </summary>
        public TippyInstanceState state { get; }
    }

    [H5.Name("tss.tippy")]
    public static class Tippy
    {
        /// <summary>
        /// Vertical pixel offset reserved at the top of the viewport. Tooltips anchored to elements above this offset are flipped to avoid being hidden behind a sticky header.
        /// </summary>
        public static int DeadZoneTop = 0;
        private static HTMLElement GetAppendToTarget(HTMLElement hostElement)
        {
            var child  = hostElement;
            var parent = child.parentElement;

            while (parent is object && !parent.classList.contains("tippy-content"))
            {
                child  = parent;
                parent = parent.parentElement;
            }

            if (parent is object && parent.classList.contains("tippy-content"))
            {
                return child;
            }

            return document.body;
        }

        private static Action<TippyInstance, MouseEvent> _doNothing = (_,__) => { };
        /// <summary>
        /// Shows a Tippy popover anchored to the given host element/component with the supplied content and configuration. Returns a delegate that can be invoked to hide the popover programmatically.
        /// </summary>
        public static void ShowFor(IComponent hostComponent, IComponent tooltip, out Action hide, TooltipAnimation animation = TooltipAnimation.None, TooltipPlacement placement = TooltipPlacement.Top, int delayShow = 0, int delayHide = 0, int maxWidth = 350, bool arrow = false, string theme = null, bool hideOnClick = true, Action onHiddenCallback = null, Func<bool> onHide = null, Action<TippyInstance, MouseEvent> onClickOutside = null, bool manualTrigger = false, int interactiveBorder = 8)
        {
            var rendered = hostComponent.Render();

            if (!rendered.IsMounted())
            {
                hide = () => { };
                return; //Only show tooltips for mounted objects
            }

            var appendTo = GetAppendToTarget(rendered);

            var renderedTooltip = UI.DIV(tooltip.Render());
            renderedTooltip.style.display      = "block";
            renderedTooltip.style.overflow     = "hidden";
            renderedTooltip.style.textOverflow = "ellipsis";
            document.body.appendChild(renderedTooltip);

            var (element, _) = Stack.GetCorrectItemToApplyStyle(hostComponent);

            Action onHiddenInternal = () =>
            {
                if (element.HasOwnProperty("_tippy"))
                {
                    H5.Script.Write("{0}._tippy.destroy();", element);
                }
                onHiddenCallback?.Invoke();
            };

            if (onHide is null) onHide = () => true;

            hide = onHiddenInternal;

            //Remove previous tooltips
            if (element.HasOwnProperty("_tippy"))
            {
                H5.Script.Write("{0}._tippy.destroy();", element);
            }

            placement = CheckDeadZone(placement, appendTo);

            var trigger = manualTrigger ? "manual" : "mouseenter focus";

            if (animation == TooltipAnimation.None)
            {
                H5.Script.Write("tippy({0}, { content: {1}, interactive: true, interactiveBorder: {13}, trigger: {14}, placement: {2}, appendTo: {3}, maxWidth: {4}, onHidden: {5}, delay: [{6},{7}], arrow: {8}, theme: {9}, hideOnClick: {10}, onHide: {11}, onClickOutside: {12} });",
                                        element, renderedTooltip, placement.ToString(), appendTo.As<object>(), maxWidth, onHiddenInternal, delayShow, delayHide, arrow, theme, hideOnClick, onHide, onClickOutside ?? _doNothing, interactiveBorder, trigger);
            }
            else
            {
                H5.Script.Write("tippy({0}, { content: {1}, interactive: true, interactiveBorder: {14}, trigger: {15}, placement: {2}, animation: {3},  appendTo: {4}, maxWidth: {5}, onHidden: {6}, delay: [{7},{8}], arrow: {9}, theme: {10}, hideOnClick : {11}, onHide: {12}, onClickOutside: {13} });",
                                        element, renderedTooltip, placement.ToString(), animation.ToString(), appendTo.As<object>(), maxWidth, onHiddenInternal, delayShow, delayHide, arrow, theme, hideOnClick, onHide, onClickOutside ?? _doNothing, interactiveBorder, trigger);
            }

            H5.Script.Write("{0}._tippy.show();", element);

            // 2020-10-05 DWR: Sometimes a tooltip will be attached to an element that is removed from the DOM and then the tooltip is left hanging, orphaned. 
            hostComponent.WhenRemoved(() =>
            {
                onHiddenInternal();
            });
        }

        private static TooltipPlacement CheckDeadZone(TooltipPlacement placement, HTMLElement appendTo)
        {
            var rect = appendTo.getBoundingClientRect().As<DOMRect>();

            if (rect.top <= DeadZoneTop)
            {
                switch (placement)
                {
                    case TooltipPlacement.Top:
                        placement = TooltipPlacement.Bottom;
                        break;
                    case TooltipPlacement.TopStart:
                        placement = TooltipPlacement.BottomStart;
                        break;
                    case TooltipPlacement.TopEnd:
                        placement = TooltipPlacement.BottomEnd;
                        break;
                }
            }

            return placement;
        }

        /// <summary>
        /// Shows a Tippy popover anchored to the given host element/component with the supplied content and configuration. Returns a delegate that can be invoked to hide the popover programmatically.
        /// </summary>
        /// <param name="manualTrigger">
        /// When <c>true</c>, the popover uses a manual trigger and Tippy installs no automatic show/hide listeners.
        /// In particular this disables the default mouseleave-hide on the anchor, which is the correct behaviour for
        /// imperatively-shown popovers (menus, comboboxes, etc.) where the user needs to be able to traverse the gap
        /// between the anchor and the popover surface without it closing under them.
        /// </param>
        public static void ShowFor(HTMLElement hostElement, HTMLElement tooltip, out Action hide, TooltipAnimation animation = TooltipAnimation.None, TooltipPlacement placement = TooltipPlacement.Top, int delayShow = 0, int delayHide = 0, int maxWidth = 350, bool arrow = false, string theme = null, bool hideOnClick = true, Action onHiddenCallback = null, Func<bool> onHide = null, Action<TippyInstance, MouseEvent> onClickOutside = null, bool manualTrigger = false, int interactiveBorder = 8)
        {
            if (!hostElement.IsMounted())
            {
                hide = () => { };
                return; //Only show tooltips for mounted objects
            }

            var appendTo = GetAppendToTarget(hostElement);

            document.body.appendChild(tooltip);

            Action onHiddenInternal = () =>
            {
                if (hostElement.HasOwnProperty("_tippy"))
                {
                    H5.Script.Write("{0}._tippy.destroy();", hostElement);
                }
                onHiddenCallback?.Invoke();
            };

            if (onHide is null) onHide = () => true;

            hide = onHiddenInternal;

            //Remove previous tooltips
            if (hostElement.HasOwnProperty("_tippy"))
            {
                H5.Script.Write("{0}._tippy.destroy();", hostElement);
            }

            placement = CheckDeadZone(placement, hostElement);

            var trigger = manualTrigger ? "manual" : "mouseenter focus";

            if (animation == TooltipAnimation.None)
            {
                H5.Script.Write("tippy({0}, { content: {1}, interactive: true, interactiveBorder: {13}, trigger: {14}, placement: {2}, appendTo: {3}, maxWidth: {4}, onHidden: {5}, delay: [{6},{7}], arrow: {8}, theme: {9}, hideOnClick: {10}, onHide: {11}, onClickOutside: {12} });",
                                hostElement, tooltip, placement.ToString(), appendTo.As<object>(), maxWidth, onHiddenInternal, delayShow, delayHide, arrow, theme, hideOnClick, onHide, onClickOutside ?? _doNothing, interactiveBorder, trigger);
            }
            else
            {
                H5.Script.Write("tippy({0}, { content: {1}, interactive: true, interactiveBorder: {14}, trigger: {15}, placement: {2}, animation: {3},  appendTo: {4}, maxWidth: {5}, onHidden: {6}, delay: [{7},{8}], arrow: {9}, theme: {10}, hideOnClick : {11}, onHide: {12}, onClickOutside: {13} });",
                                hostElement, tooltip, placement.ToString(), animation.ToString(), appendTo.As<object>(), maxWidth, onHiddenInternal, delayShow, delayHide, arrow, theme, hideOnClick, onHide, onClickOutside ?? _doNothing, interactiveBorder, trigger);
            }

            H5.Script.Write("{0}._tippy.show();", hostElement);

            // 2020-10-05 DWR: Sometimes a tooltip will be attached to an element that is removed from the DOM and then the tooltip is left hanging, orphaned. 
            DomObserver.WhenRemoved(hostElement, () =>
            {
                onHiddenInternal();
            });
        }

        /// <summary>
        /// Hides every currently visible Tippy instance in the document.
        /// </summary>
        public static void HideAll()
        {
            H5.Script.Write("tippy.hideAll()");
        }

        internal static void CheckRepositionNeeded(HTMLElement container)
        {
            if (!container.isConnected) return;
            var parent = container.parentElement;

            while (parent is object)
            {
                if (parent.HasOwnProperty("_tippy"))
                {
                    H5.Script.Write("{0}._tippy.popperInstance.update()", parent);
                    break;
                }
                parent = parent.parentElement;
            }
        }
    }
}