using System;
using System.Threading.Tasks;
using Tesserae;
using static H5.Core.dom;

namespace Tesserae
{
    [H5.Name("tss.tippy")]
    public static class Tippy
    {
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

        public static void ShowFor(IComponent hostComponent, IComponent tooltip, out Action hide, TooltipAnimation animation = TooltipAnimation.None, TooltipPlacement placement = TooltipPlacement.Top, int delayShow = 0, int delayHide = 0, int maxWidth = 350, bool arrow = false, string theme = null, bool hideOnClick = true, Action onHiddenCallback = null, Func<bool> onHide = null)
        {
            var rendered = hostComponent.Render();

            if (!rendered.IsMounted())
            {
                hide = () => { };
                return; //Only show tooltips for mounted objects
            }

            var appendTo = GetAppendToTarget(rendered);

            var renderedTooltip = UI.DIV(tooltip.Render());
            renderedTooltip.style.display = "block";
            renderedTooltip.style.overflow = "hidden";
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

            if (animation == TooltipAnimation.None)
            {
                H5.Script.Write("tippy({0}, { content: {1}, interactive: true, interactiveBorder: 8, placement: {2}, appendTo: {3}, maxWidth: {4}, onHidden: {5}, delay: [{6},{7}], arrow: {8}, theme: {9}, hideOnClick: {10}, onHide: {11} });",
                    element, renderedTooltip, placement.ToString(), appendTo.As<object>(), maxWidth, onHiddenInternal, delayShow, delayHide, arrow, theme, hideOnClick, onHide);
            }
            else
            {
                H5.Script.Write("tippy({0}, { content: {1}, interactive: true, interactiveBorder: 8, placement: {2}, animation: {3},  appendTo: {4}, maxWidth: {5}, onHidden: {6}, delay: [{7},{8}], arrow: {9}, theme: {10}, hideOnClick : {11}, onHide: {12} });", element, renderedTooltip, placement.ToString(), animation.ToString(), appendTo.As<object>(), maxWidth, onHiddenInternal, delayShow, delayHide, arrow, theme, hideOnClick, onHide);
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
                    case TooltipPlacement.Top: placement = TooltipPlacement.Bottom; break;
                    case TooltipPlacement.TopStart: placement = TooltipPlacement.BottomStart; break;
                    case TooltipPlacement.TopEnd: placement = TooltipPlacement.BottomEnd; break;
                }
            }

            return placement;
        }

        public static void ShowFor(HTMLElement hostElement, HTMLElement tooltip, out Action hide, TooltipAnimation animation = TooltipAnimation.None, TooltipPlacement placement = TooltipPlacement.Top, int delayShow = 0, int delayHide = 0, int maxWidth = 350, bool arrow = false, string theme = null, bool hideOnClick = true, Action onHiddenCallback = null, Func<bool> onHide = null)
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

            if (animation == TooltipAnimation.None)
            {
                H5.Script.Write("tippy({0}, { content: {1}, interactive: true, interactiveBorder: 8, placement: {2}, appendTo: {3}, maxWidth: {4}, onHidden: {5}, delay: [{6},{7}], arrow: {8}, theme: {9}, hideOnClick: {10}, onHide: {11} });", hostElement, tooltip, placement.ToString(), appendTo.As<object>(), maxWidth, onHiddenInternal, delayShow, delayHide, arrow, theme, hideOnClick, onHide);
            }
            else
            {
                H5.Script.Write("tippy({0}, { content: {1}, interactive: true, interactiveBorder: 8, placement: {2}, animation: {3},  appendTo: {4}, maxWidth: {5}, onHidden: {6}, delay: [{7},{8}], arrow: {9}, theme: {10}, hideOnClick : {11}, onHide: {12} });", hostElement, tooltip, placement.ToString(), animation.ToString(), appendTo.As<object>(), maxWidth, onHiddenInternal, delayShow, delayHide, arrow, theme, hideOnClick, onHide);
            }

            H5.Script.Write("{0}._tippy.show();", hostElement);

            // 2020-10-05 DWR: Sometimes a tooltip will be attached to an element that is removed from the DOM and then the tooltip is left hanging, orphaned. 
            DomObserver.WhenRemoved(hostElement, () =>
            {
                onHiddenInternal();
            });
        }

        public static void HideAll()
        {
            H5.Script.Write("tippy.hideAll()");
        }
    }
}