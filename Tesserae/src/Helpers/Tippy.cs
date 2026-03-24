using System;
using System.Threading.Tasks;
using Tesserae;
using static H5.Core.dom;

namespace Tesserae
{
    [H5.ObjectLiteral]
    public class TippyInstanceState
    {
        public bool isEnabled;
        public bool isVisible;
        public bool isDestroyed;
        public bool isMounted;
        public bool isShown;
    }

    [H5.ObjectLiteral]
    public class TippyInstance
    {
        public void clearDelayTimeouts() { }
        public void destroy() { }
        public void disable() { }
        public void enable() { }
        public void hide() { }
        public void show() { }
        public void hideWithInteractivity() { }
        public int id { get; }
        public void unmount() { }
        public HTMLElement reference { get; }
        public void setContent(HTMLElement content) { }

        public TippyInstanceState state { get; }
    }

    public class TippyConfig
    {
        public IComponent Header { get; set; }
        public IComponent Content { get; set; }
        public IComponent Footer { get; set; }

        public TooltipAnimation Animation { get; set; } = TooltipAnimation.None;
        public TooltipPlacement Placement { get; set; } = TooltipPlacement.Top;
        public int DelayShow { get; set; } = 0;
        public int DelayHide { get; set; } = 0;
        public int MaxWidth { get; set; } = 350;
        public bool Arrow { get; set; } = false;
        public string Theme { get; set; } = null;
        public bool Interactive { get; set; } = true;
        public bool HideOnClick { get; set; } = true;
        public Action OnHiddenCallback { get; set; } = null;
        public Func<bool> OnHide { get; set; } = null;
        public Action<TippyInstance, MouseEvent> OnClickOutside { get; set; } = null;
        public bool AppendToBody { get; set; } = true;
        public bool FollowCursor { get; set; } = false;

        public TippyConfig() {}
    }

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

        private static Action<TippyInstance, MouseEvent> _doNothing = (_,__) => { };

        public static void ShowFor(IComponent hostComponent, TippyConfig config, out Action hide)
        {
            var rendered = hostComponent.Render();

            if (!rendered.IsMounted())
            {
                hide = () => { };
                return; //Only show tooltips for mounted objects
            }

            var appendTo = config.AppendToBody ? document.body : GetAppendToTarget(rendered);

            var renderedTooltip = UI.DIV();
            renderedTooltip.classList.add("tss-tippy-container");

            if (config.Header != null)
            {
                var headerDiv = UI.DIV(config.Header.Render());
                headerDiv.classList.add("tss-tippy-header");
                renderedTooltip.appendChild(headerDiv);
            }
            if (config.Content != null)
            {
                var contentDiv = UI.DIV(config.Content.Render());
                contentDiv.classList.add("tss-tippy-content-inner");
                renderedTooltip.appendChild(contentDiv);
            }
            if (config.Footer != null)
            {
                var footerDiv = UI.DIV(config.Footer.Render());
                footerDiv.classList.add("tss-tippy-footer");
                renderedTooltip.appendChild(footerDiv);
            }

            renderedTooltip.style.display      = "block";
            renderedTooltip.style.overflow     = "hidden";
            renderedTooltip.style.textOverflow = "ellipsis";

            if (config.AppendToBody)
            {
                document.body.appendChild(renderedTooltip);
            }

            var (element, _) = Stack.GetCorrectItemToApplyStyle(hostComponent);

            Action onHiddenInternal = () =>
            {
                if (element.HasOwnProperty("_tippy"))
                {
                    H5.Script.Write("{0}._tippy.destroy();", element);
                }
                config.OnHiddenCallback?.Invoke();
            };

            var onHide = config.OnHide ?? (() => true);
            hide = onHiddenInternal;

            //Remove previous tooltips
            if (element.HasOwnProperty("_tippy"))
            {
                H5.Script.Write("{0}._tippy.destroy();", element);
            }

            var placement = CheckDeadZone(config.Placement, appendTo);
            var onClickOutside = config.OnClickOutside ?? _doNothing;

            if (config.Animation == TooltipAnimation.None)
            {
                H5.Script.Write("tippy({0}, { content: {1}, interactive: {2}, interactiveBorder: 8, placement: {3}, appendTo: {4}, maxWidth: {5}, onHidden: {6}, delay: [{7},{8}], arrow: {9}, theme: {10}, hideOnClick: {11}, onHide: {12}, onClickOutside: {13}, followCursor: {14} });",
                                        element, renderedTooltip, config.Interactive, placement.ToString(), config.AppendToBody ? document.body.As<object>() : appendTo.As<object>(), config.MaxWidth, onHiddenInternal, config.DelayShow, config.DelayHide, config.Arrow, config.Theme, config.HideOnClick, onHide, onClickOutside, config.FollowCursor);
            }
            else
            {
                H5.Script.Write("tippy({0}, { content: {1}, interactive: {2}, interactiveBorder: 8, placement: {3}, animation: {4},  appendTo: {5}, maxWidth: {6}, onHidden: {7}, delay: [{8},{9}], arrow: {10}, theme: {11}, hideOnClick : {12}, onHide: {13}, onClickOutside: {14}, followCursor: {15} });",
                                        element, renderedTooltip, config.Interactive, placement.ToString(), config.Animation.ToString(), config.AppendToBody ? document.body.As<object>() : appendTo.As<object>(), config.MaxWidth, onHiddenInternal, config.DelayShow, config.DelayHide, config.Arrow, config.Theme, config.HideOnClick, onHide, onClickOutside, config.FollowCursor);
            }

            H5.Script.Write("{0}._tippy.show();", element);

            hostComponent.WhenRemoved(() =>
            {
                onHiddenInternal();
            });
        }

        public static void ShowFor(IComponent hostComponent, IComponent tooltip, out Action hide, TooltipAnimation animation = TooltipAnimation.None, TooltipPlacement placement = TooltipPlacement.Top, int delayShow = 0, int delayHide = 0, int maxWidth = 350, bool arrow = false, string theme = null, bool hideOnClick = true, Action onHiddenCallback = null, Func<bool> onHide = null, Action<TippyInstance, MouseEvent> onClickOutside = null)
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

            if (animation == TooltipAnimation.None)
            {
                H5.Script.Write("tippy({0}, { content: {1}, interactive: true, interactiveBorder: 8, placement: {2}, appendTo: {3}, maxWidth: {4}, onHidden: {5}, delay: [{6},{7}], arrow: {8}, theme: {9}, hideOnClick: {10}, onHide: {11}, onClickOutside: {12} });",
                                        element, renderedTooltip, placement.ToString(), appendTo.As<object>(), maxWidth, onHiddenInternal, delayShow, delayHide, arrow, theme, hideOnClick, onHide, onClickOutside ?? _doNothing);
            }
            else
            {
                H5.Script.Write("tippy({0}, { content: {1}, interactive: true, interactiveBorder: 8, placement: {2}, animation: {3},  appendTo: {4}, maxWidth: {5}, onHidden: {6}, delay: [{7},{8}], arrow: {9}, theme: {10}, hideOnClick : {11}, onHide: {12}, onClickOutside: {13} });", 
                                        element, renderedTooltip, placement.ToString(), animation.ToString(), appendTo.As<object>(), maxWidth, onHiddenInternal, delayShow, delayHide, arrow, theme, hideOnClick, onHide, onClickOutside ?? _doNothing);
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

        public static void ShowFor(HTMLElement hostElement, HTMLElement tooltip, out Action hide, TooltipAnimation animation = TooltipAnimation.None, TooltipPlacement placement = TooltipPlacement.Top, int delayShow = 0, int delayHide = 0, int maxWidth = 350, bool arrow = false, string theme = null, bool hideOnClick = true, Action onHiddenCallback = null, Func<bool> onHide = null, Action<TippyInstance, MouseEvent> onClickOutside = null)
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
                H5.Script.Write("tippy({0}, { content: {1}, interactive: true, interactiveBorder: 8, placement: {2}, appendTo: {3}, maxWidth: {4}, onHidden: {5}, delay: [{6},{7}], arrow: {8}, theme: {9}, hideOnClick: {10}, onHide: {11}, onClickOutside: {12} });",
                                hostElement, tooltip, placement.ToString(), appendTo.As<object>(), maxWidth, onHiddenInternal, delayShow, delayHide, arrow, theme, hideOnClick, onHide, onClickOutside ?? _doNothing);
            }
            else
            {
                H5.Script.Write("tippy({0}, { content: {1}, interactive: true, interactiveBorder: 8, placement: {2}, animation: {3},  appendTo: {4}, maxWidth: {5}, onHidden: {6}, delay: [{7},{8}], arrow: {9}, theme: {10}, hideOnClick : {11}, onHide: {12}, onClickOutside: {13} });", 
                                hostElement, tooltip, placement.ToString(), animation.ToString(), appendTo.As<object>(), maxWidth, onHiddenInternal, delayShow, delayHide, arrow, theme, hideOnClick, onHide, onClickOutside ?? _doNothing);
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