using System;
using System.Collections.Generic;
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
        public HTMLElement popper { get; }
        public HTMLElement reference { get; }
        public void setProps(object props) { }
        public void setContent(HTMLElement content) { }

        public TippyInstanceState state { get; }
    }

    [H5.Name("tss.tippy")]
    public static class Tippy
    {
        public static int DeadZoneTop = 0;
        private const string ShowForMarkerProperty = "_tss_show_for_tippy";
        private static readonly HashSet<int> _pinnedShowForTippyIds = new HashSet<int>();
        private static readonly HashSet<int> _allowPinnedHideTippyIds = new HashSet<int>();

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

        private static bool IsPinned(TippyInstance instance)
        {
            return instance is object && _pinnedShowForTippyIds.Contains(instance.id);
        }

        private static bool IsPinnedHideAllowed(TippyInstance instance)
        {
            return instance is object && _allowPinnedHideTippyIds.Contains(instance.id);
        }

        private static void SetPinnedHideAllowed(TippyInstance instance, bool value)
        {
            if (instance is null)
            {
                return;
            }

            if (value)
            {
                _allowPinnedHideTippyIds.Add(instance.id);
            }
            else
            {
                _allowPinnedHideTippyIds.Remove(instance.id);
            }
        }

        private static void MarkShowForTippy(TippyInstance instance)
        {
            if (instance is null)
            {
                return;
            }

            _pinnedShowForTippyIds.Remove(instance.id);
            _allowPinnedHideTippyIds.Remove(instance.id);

            if (instance?.reference is object)
            {
                instance.reference[ShowForMarkerProperty] = true;
            }

            if (instance?.popper is object)
            {
                instance.popper[ShowForMarkerProperty] = true;
            }
        }

        private static bool HasShowForMarker(HTMLElement element)
        {
            return element?[ShowForMarkerProperty].As<bool>() == true;
        }

        private static Func<bool> BuildOnHideHandler(Func<bool> onHide, Func<TippyInstance> getInstance, string logPrefix)
        {
            return () =>
            {
                var instance = getInstance();
                var pinned = IsPinned(instance);
                var allowPinnedHide = IsPinnedHideAllowed(instance);

                console.log($"{logPrefix} onHide", new
                {
                    reference = instance?.reference,
                    pinned,
                    allowPinnedHide
                });

                if (pinned && !allowPinnedHide)
                {
                    console.log($"{logPrefix} onHide prevented because tippy is pinned");
                    return false;
                }

                var allowHide = onHide?.Invoke() ?? true;

                if (pinned)
                {
                    SetPinnedHideAllowed(instance, false);
                }

                console.log($"{logPrefix} onHide result", new
                {
                    allowHide
                });

                return allowHide;
            };
        }

        private static Action<TippyInstance, MouseEvent> BuildOnClickOutsideHandler(Func<bool> onHide, Action<TippyInstance, MouseEvent> onClickOutside)
        {
            return (instance, ev) =>
            {
                console.log("[tippy] onClickOutside", new
                {
                    reference = instance?.reference,
                    target = ev?.target,
                    pinned = IsPinned(instance)
                });

                onClickOutside?.Invoke(instance, ev);

                if (IsPinned(instance) && (onHide?.Invoke() ?? true))
                {
                    SetPinnedHideAllowed(instance, true);
                    console.log("[tippy] pinned instance hiding from outside click");
                    instance.hide();
                }
            };
        }

        internal static void PinOwningTippy(HTMLElement container)
        {
            if (!(container?.isConnected ?? false)) return;

            var parent = container;

            while (parent is object)
            {
                if (parent.HasOwnProperty("_tippy"))
                {
                    var instance = H5.Script.Write<TippyInstance>("{0}._tippy", parent);
                    var isShowFor = HasShowForMarker(parent) ||
                                    HasShowForMarker(instance?.reference) ||
                                    HasShowForMarker(instance?.popper);

                    console.log("[tippy] PinOwningTippy found owner", new
                    {
                        owner = parent,
                        reference = instance?.reference,
                        pinned = IsPinned(instance),
                        isShowFor
                    });

                    if (isShowFor && !IsPinned(instance))
                    {
                        _pinnedShowForTippyIds.Add(instance.id);
                        _allowPinnedHideTippyIds.Remove(instance.id);

                        instance.clearDelayTimeouts();
                        instance.setProps(new
                        {
                            trigger = "manual"
                        });
                        console.log("[tippy] PinOwningTippy switched trigger to manual");
                        instance.show();
                    }

                    break;
                }

                parent = parent.parentElement;
            }
        }

        private static Action<TippyInstance, MouseEvent> _doNothing = (_,__) => { };
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
                console.log("[tippy] ShowFor(IComponent) onHiddenInternal", new
                {
                    element,
                    hasTippy = element.HasOwnProperty("_tippy")
                });
                var instance = H5.Script.Write<TippyInstance>("{0}._tippy", element);
                if (instance is object)
                {
                    _pinnedShowForTippyIds.Remove(instance.id);
                    _allowPinnedHideTippyIds.Remove(instance.id);
                }
                if (element.HasOwnProperty("_tippy"))
                {
                    H5.Script.Write("{0}._tippy.destroy();", element);
                }
                onHiddenCallback?.Invoke();
            };

            hide = onHiddenInternal;

            //Remove previous tooltips
            if (element.HasOwnProperty("_tippy"))
            {
                H5.Script.Write("{0}._tippy.destroy();", element);
            }

            placement = CheckDeadZone(placement, appendTo);

            if (onHide is null) onHide = () => true;

            Func<TippyInstance> getInstance = () => H5.Script.Write<TippyInstance>("{0}._tippy", element);
            var onHideInternal = BuildOnHideHandler(onHide, getInstance, "[tippy] ShowFor(IComponent)");
            var onClickOutsideInternal = BuildOnClickOutsideHandler(onHide, onClickOutside ?? _doNothing);

            if (animation == TooltipAnimation.None)
            {
                H5.Script.Write("tippy({0}, { content: {1}, interactive: true, interactiveBorder: 8, placement: {2}, appendTo: {3}, maxWidth: {4}, onHidden: {5}, delay: [{6},{7}], arrow: {8}, theme: {9}, hideOnClick: {10}, onHide: {11}, onClickOutside: {12} });",
                                        element, renderedTooltip, placement.ToString(), appendTo.As<object>(), maxWidth, onHiddenInternal, delayShow, delayHide, arrow, theme, hideOnClick, onHideInternal, onClickOutsideInternal);
            }
            else
            {
                H5.Script.Write("tippy({0}, { content: {1}, interactive: true, interactiveBorder: 8, placement: {2}, animation: {3},  appendTo: {4}, maxWidth: {5}, onHidden: {6}, delay: [{7},{8}], arrow: {9}, theme: {10}, hideOnClick : {11}, onHide: {12}, onClickOutside: {13} });", 
                                        element, renderedTooltip, placement.ToString(), animation.ToString(), appendTo.As<object>(), maxWidth, onHiddenInternal, delayShow, delayHide, arrow, theme, hideOnClick, onHideInternal, onClickOutsideInternal);
            }

            H5.Script.Write("{0}._tippy.show();", element);
            var shownInstance = H5.Script.Write<TippyInstance>("{0}._tippy", element);
            MarkShowForTippy(shownInstance);
            console.log("[tippy] ShowFor(IComponent) shown", new
            {
                element,
                appendTo,
                placement = placement.ToString(),
                delayShow,
                delayHide,
                hideOnClick,
                theme,
                popper = shownInstance?.popper,
                reference = shownInstance?.reference
            });

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
                console.log("[tippy] ShowFor(HTMLElement) onHiddenInternal", new
                {
                    hostElement,
                    hasTippy = hostElement.HasOwnProperty("_tippy")
                });
                var instance = H5.Script.Write<TippyInstance>("{0}._tippy", hostElement);
                if (instance is object)
                {
                    _pinnedShowForTippyIds.Remove(instance.id);
                    _allowPinnedHideTippyIds.Remove(instance.id);
                }
                if (hostElement.HasOwnProperty("_tippy"))
                {
                    H5.Script.Write("{0}._tippy.destroy();", hostElement);
                }
                onHiddenCallback?.Invoke();
            };

            hide = onHiddenInternal;

            //Remove previous tooltips
            if (hostElement.HasOwnProperty("_tippy"))
            {
                H5.Script.Write("{0}._tippy.destroy();", hostElement);
            }

            placement = CheckDeadZone(placement, hostElement);

            if (onHide is null) onHide = () => true;

            Func<TippyInstance> getInstance = () => H5.Script.Write<TippyInstance>("{0}._tippy", hostElement);
            var onHideInternal = BuildOnHideHandler(onHide, getInstance, "[tippy] ShowFor(HTMLElement)");
            var onClickOutsideInternal = BuildOnClickOutsideHandler(onHide, onClickOutside ?? _doNothing);

            if (animation == TooltipAnimation.None)
            {
                H5.Script.Write("tippy({0}, { content: {1}, interactive: true, interactiveBorder: 8, placement: {2}, appendTo: {3}, maxWidth: {4}, onHidden: {5}, delay: [{6},{7}], arrow: {8}, theme: {9}, hideOnClick: {10}, onHide: {11}, onClickOutside: {12} });",
                                hostElement, tooltip, placement.ToString(), appendTo.As<object>(), maxWidth, onHiddenInternal, delayShow, delayHide, arrow, theme, hideOnClick, onHideInternal, onClickOutsideInternal);
            }
            else
            {
                H5.Script.Write("tippy({0}, { content: {1}, interactive: true, interactiveBorder: 8, placement: {2}, animation: {3},  appendTo: {4}, maxWidth: {5}, onHidden: {6}, delay: [{7},{8}], arrow: {9}, theme: {10}, hideOnClick : {11}, onHide: {12}, onClickOutside: {13} });", 
                                hostElement, tooltip, placement.ToString(), animation.ToString(), appendTo.As<object>(), maxWidth, onHiddenInternal, delayShow, delayHide, arrow, theme, hideOnClick, onHideInternal, onClickOutsideInternal);
            }

            H5.Script.Write("{0}._tippy.show();", hostElement);
            var shownInstance = H5.Script.Write<TippyInstance>("{0}._tippy", hostElement);
            MarkShowForTippy(shownInstance);
            console.log("[tippy] ShowFor(HTMLElement) shown", new
            {
                hostElement,
                appendTo,
                placement = placement.ToString(),
                delayShow,
                delayHide,
                hideOnClick,
                theme,
                popper = shownInstance?.popper,
                reference = shownInstance?.reference
            });

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
