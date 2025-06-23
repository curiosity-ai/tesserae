using System;
using System.Threading.Tasks;
using static H5.Core.dom;

namespace Tesserae
{
    [H5.Name("tss.ICX")]
    public static class IComponentExtensions
    {
        public static T WhenMounted<T>(this T component, Action callback) where T : IComponent
        {
            //No need to double check if already mounted here, as teh DomObserver already do it
            DomObserver.WhenMounted(component.Render(), callback);
            return component;
        }

        public static T WhenMountedDelayed<T>(this T component, TimeSpan delay, Action callback, bool onlyIfStillMounted = true) where T : IComponent
        {
            //No need to double check if already mounted here, as teh DomObserver already do it
            DomObserver.WhenMounted(component.Render(), () =>
            {
                window.setTimeout(_ =>
                {
                    if (component.IsMounted())
                    {
                        callback();
                    }
                }, delay.TotalMilliseconds);
            });
            return component;
        }

        public static T WhenRemoved<T>(this T component, Action callback) where T : IComponent
        {
            DomObserver.WhenRemoved(component.Render(), callback);
            return component;
        }

        // The WhenMountedOrRemoved method shouldn't be used, as it would leak the component memory on the DomObserver forever (because whenever it's mounted it's registered with WhenRemoved and whenever it's removed it's re-registered with WhenMounted)
        // We left the code here as a reminder.
        //     public static T WhenMountedOrRemoved<T>(T component, Action onMounted, Action onRemoved) where T : IComponent
        //     {
        //         void Mounted()
        //         {
        //             onMounted?.Invoke();
        //             DomObserver.WhenRemoved(component.Render(), Removed);
        //         }

        //         void Removed()
        //         {
        //             onRemoved?.Invoke();
        //             DomObserver.WhenMounted(component.Render(), Mounted);
        //         }

        //         DomObserver.WhenMounted(component.Render(), Mounted);
        //         return component;
        //     }

        public static T AlignAuto<T>(this T component) where T : IComponent
        {
            Stack.SetAlign(component, ItemAlign.Auto);
            return component;
        }

        public static T AlignStretch<T>(this T component) where T : IComponent
        {
            Stack.SetAlign(component, ItemAlign.Stretch);
            return component;
        }

        public static T AlignBaseline<T>(this T component) where T : IComponent
        {
            Stack.SetAlign(component, ItemAlign.Baseline);
            return component;
        }

        public static T AlignStart<T>(this T component) where T : IComponent
        {
            Stack.SetAlign(component, ItemAlign.Start);
            return component;
        }

        public static T AlignCenter<T>(this T component) where T : IComponent
        {
            Stack.SetAlign(component, ItemAlign.Center);
            return component;
        }

        public static T AlignEnd<T>(this T component) where T : IComponent
        {
            Stack.SetAlign(component, ItemAlign.End);
            return component;
        }

        public static T JustifyStart<T>(this T component) where T : IComponent
        {
            Stack.SetJustify(component, ItemJustify.Start);
            return component;
        }

        public static T JustifyCenter<T>(this T component) where T : IComponent
        {
            Stack.SetJustify(component, ItemJustify.Center);
            return component;
        }

        public static T JustifyEnd<T>(this T component) where T : IComponent
        {
            Stack.SetJustify(component, ItemJustify.End);
            return component;
        }

        public static T Margin<T>(this T component, UnitSize unitSize) where T : IComponent
        {
            Stack.SetMarginLeft(component, unitSize);
            Stack.SetMarginRight(component, unitSize);
            Stack.SetMarginTop(component, unitSize);
            Stack.SetMarginBottom(component, unitSize);
            return component;
        }

        public static T MarginLeft<T>(this T component, UnitSize unitSize) where T : IComponent
        {
            Stack.SetMarginLeft(component, unitSize);
            return component;
        }

        public static T MarginRight<T>(this T component, UnitSize unitSize) where T : IComponent
        {
            Stack.SetMarginRight(component, unitSize);
            return component;
        }

        public static T MarginTop<T>(this T component, UnitSize unitSize) where T : IComponent
        {
            Stack.SetMarginTop(component, unitSize);
            return component;
        }

        public static T MarginBottom<T>(this T component, UnitSize unitSize) where T : IComponent
        {
            Stack.SetMarginBottom(component, unitSize);
            return component;
        }

        public static T Padding<T>(this T component, UnitSize unitSize) where T : IComponent
        {
            Stack.SetPaddingLeft(component, unitSize);
            Stack.SetPaddingRight(component, unitSize);
            Stack.SetPaddingTop(component, unitSize);
            Stack.SetPaddingBottom(component, unitSize);
            return component;
        }

        public static T PaddingLeft<T>(this T component, UnitSize unitSize) where T : IComponent
        {
            Stack.SetPaddingLeft(component, unitSize);
            return component;
        }

        public static T PaddingRight<T>(this T component, UnitSize unitSize) where T : IComponent
        {
            Stack.SetPaddingRight(component, unitSize);
            return component;
        }

        public static T PaddingTop<T>(this T component, UnitSize unitSize) where T : IComponent
        {
            Stack.SetPaddingTop(component, unitSize);
            return component;
        }

        public static T PaddingBottom<T>(this T component, UnitSize unitSize) where T : IComponent
        {
            Stack.SetPaddingBottom(component, unitSize);
            return component;
        }

        public static T WidthAuto<T>(this T component) where T : IComponent
        {
            Stack.SetWidth(component, UnitSize.Auto());
            return component;
        }

        public static T Width<T>(this T component, UnitSize unitSize) where T : IComponent
        {
            Stack.SetWidth(component, unitSize);
            return component;
        }

        public static T MinWidth<T>(this T component, UnitSize unitSize) where T : IComponent
        {
            Stack.SetMinWidth(component, unitSize);
            return component;
        }

        public static T MaxWidth<T>(this T component, UnitSize unitSize) where T : IComponent
        {
            Stack.SetMaxWidth(component, unitSize);
            return component;
        }

        public static T WidthStretch<T>(this T component) where T : IComponent
        {
            Stack.SetWidth(component, 100.percent());
            return component;
        }

        public static T HeightAuto<T>(this T component) where T : IComponent
        {
            Stack.SetHeight(component, UnitSize.Auto());
            return component;
        }

        public static T Height<T>(this T component, UnitSize unitSize) where T : IComponent
        {
            Stack.SetHeight(component, unitSize);
            return component;
        }

        public static T MinHeight<T>(this T component, UnitSize unitSize) where T : IComponent
        {
            Stack.SetMinHeight(component, unitSize);
            return component;
        }

        public static T MaxHeight<T>(this T component, UnitSize unitSize) where T : IComponent
        {
            Stack.SetMaxHeight(component, unitSize);
            return component;
        }

        public static T HeightStretch<T>(this T component) where T : IComponent
        {
            Stack.SetHeight(component, 100.percent());
            return component;
        }

        public static T MinHeightStretch<T>(this T component) where T : IComponent
        {
            Stack.SetMinHeight(component, 100.percent());
            return component;
        }

        public static T Stretch<T>(this T component) where T : IComponent
        {
            Stack.SetWidth(component, 100.percent());
            Stack.SetHeight(component, 100.percent());
            return component;
        }

        public static T Grow<T>(this T component, int grow = 1) where T : IComponent
        {
            Stack.SetGrow(component, grow);
            return component;
        }

        public static T Shrink<T>(this T component) where T : IComponent
        {
            Stack.SetShrink(component, true);
            return component;
        }

        public static T NoShrink<T>(this T component) where T : IComponent
        {
            Stack.SetShrink(component, false);
            return component;
        }

        public static T GridColumn<T>(this T component, int start, int end) where T : IComponent
        {
            Grid.SetGridColumn(component, start, end);
            return component;
        }

        public static T GridColumnStretch<T>(this T component) where T : IComponent
        {
            Grid.SetGridColumn(component, 1, -1);
            return component;
        }

        public static T GridRow<T>(this T component, int start, int end) where T : IComponent
        {
            Grid.SetGridRow(component, start, end);
            return component;
        }

        public static T GridRowStretch<T>(this T component) where T : IComponent
        {
            Grid.SetGridRow(component, 1, -1);
            return component;
        }

        public static T Collapse<T>(this T component) where T : IComponent
        {
            var (el, _) = Stack.GetCorrectItemToApplyStyle(component);
            el          = TryGetParentStackItem(el);
            el.classList.add("tss-collapse");
            return component;
        }

        public static T FadeThenCollapse<T>(this T component) where T : IComponent => Fade(component, async () =>
        {
            await Task.Delay(1000);
            Collapse(component);
        });


        public static T Fade<T>(this T component) where T : IComponent                            => Fade(component, () => { });
        public static T Fade<T>(this T component, Func<Task> andThen = null) where T : IComponent => Fade(component, andThen is object ? (Action)(() => andThen.Invoke().FireAndForget()) : null);

        public static T Fade<T>(this T component, Action andThen = null) where T : IComponent
        {
            var (el, _) = Stack.GetCorrectItemToApplyStyle(component);
            el          = TryGetParentStackItem(el);
            el.classList.add("tss-fade");
            el.classList.remove("tss-fade-light", "tss-show");
            component.Render().classList.remove("tss-fade-light", "tss-show"); //Need to remove from component as well, because it could have been set before it was added to a stack

            if (andThen is object)
            {
                // The opacity transition time on "tss-fade" is 0.15s, so this is the amount of time after which we want to make the optional "andThen" callback
                setTimeout(
                    _ => andThen(),
                    150
                );
            }
            return component;
        }

        public static T LightFade<T>(this T component) where T : IComponent => LightFade(component, () => { });

        public static T LightFade<T>(this T component, Func<Task> andThen = null) where T : IComponent => LightFade(component, andThen is object ? (Action)(() => andThen.Invoke().FireAndForget()) : null);

        public static T LightFade<T>(this T component, Action andThen = null) where T : IComponent
        {
            var (el, _) = Stack.GetCorrectItemToApplyStyle(component);
            el          = TryGetParentStackItem(el);
            el.classList.add("tss-fade-light");
            el.classList.remove("tss-fade", "tss-show");
            component.Render().classList.remove("tss-fade", "tss-show"); //Need to remove from component as well, because it could have been set before it was added to a stack

            if (andThen is object)
            {
                // The opacity transition time on "tss-fade" is 0.15s, so this is the amount of time after which we want to make the optional "andThen" callback
                setTimeout(
                    _ => andThen(),
                    150
                );
            }
            return component;
        }

        public static T Show<T>(this T component) where T : IComponent
        {
            var (el, _) = Stack.GetCorrectItemToApplyStyle(component);
            el          = TryGetParentStackItem(el);
            el.classList.add("tss-fade", "tss-show");
            el.classList.remove("tss-fade-light", "tss-collapse");
            component.Render().classList.remove("tss-fade-light", "tss-collapse", "tss-fade"); //Need to remove all from component as well, because it could have been set before it was added to a stack
            return component;
        }

        private static HTMLElement TryGetParentStackItem(HTMLElement el)
        {
            if (el.parentElement is object && el.parentElement.classList.contains("tss-stack-item"))
            {
                return el.parentElement;
            }
            return el;
        }

        public static T Tooltip<T>(this T component, string tooltipHtml, TooltipAnimation animation = TooltipAnimation.None, TooltipPlacement placement = TooltipPlacement.Top, int delayShow = 250, int delayHide = 0, bool followCursor = false, int maxWidth = 350, bool arrow = false, string theme = null, IComponent parent = null) where T : IComponent
        {
            if (string.IsNullOrWhiteSpace(tooltipHtml))
                return component;

            return component.Tooltip(
                new Raw(UI.Raw(tooltipHtml)),
                animation: animation,
                placement: placement,
                delayShow: delayShow,
                delayHide: delayHide,
                followCursor: followCursor,
                maxWidth: maxWidth,
                arrow: arrow,
                parent: parent,
                theme: theme
            );
        }

        public static T RemoveTooltip<T>(this T component) where T : IComponent
        {
            var (element, _) = Stack.GetCorrectItemToApplyStyle(component);

            if (element.HasOwnProperty("_tippy"))
            {
                H5.Script.Write("{0}._tippy.destroy();", element);
            }

            var rendered = component.Render();

            rendered.onmouseenter = null;

            if (rendered.HasOwnProperty("tooltipMarker"))
            {
                rendered["tooltipMarker"] = null;
            }

            return component;
        }

        public static T Tooltip<T>(this T component, IComponent tooltip, bool interactive = false, TooltipAnimation animation = TooltipAnimation.None, TooltipPlacement placement = TooltipPlacement.Top, int delayShow = 250, int delayHide = 0, bool appendToBody = true, bool followCursor = false, int maxWidth = 350, bool hideOnClick = true, bool arrow = false, string theme = null, IComponent parent = null) where T : IComponent
        {
            if (tooltip is null)
                return component;

            var rendered = component.Render();

            var marker = new object();

            rendered["tooltipMarker"] = marker;

            rendered.onmouseenter += AttachTooltip;

            void AttachTooltip(MouseEvent e)
            {
                rendered.onmouseenter -= AttachTooltip;

                if (rendered["tooltipMarker"] != marker) return;

                var renderedTooltip = UI.DIV(tooltip.Render());
                renderedTooltip.style.display      = "block";
                renderedTooltip.style.overflow     = "hidden";
                renderedTooltip.style.textOverflow = "ellipsis";
                document.body.appendChild(renderedTooltip);

                var (element, _) = Stack.GetCorrectItemToApplyStyle(component);

                if (element.HasOwnProperty("_tippy"))
                {
                    H5.Script.Write("{0}._tippy.destroy();", element);
                }

                if (animation == TooltipAnimation.None)
                {
                    H5.Script.Write("tippy({0}, { content: {1}, interactive: {2}, placement: {3}, delay: [{4},{5}], appendTo: {6}, followCursor: {7}, maxWidth: {8}, hideOnClick:{9}, arrow: {10}, theme: {11} });", element, renderedTooltip, interactive, placement.ToString(), delayShow, delayHide, appendToBody ? document.body.As<object>() : "parent".As<object>(), followCursor, maxWidth, hideOnClick, arrow, theme);
                }
                else
                {
                    H5.Script.Write("tippy({0}, { content: {1}, interactive: {2}, placement: {3},  animation: {4}, delay: [{5},{6}], appendTo: {7}, followCursor: {8}, maxWidth: {9}, hideOnClick: {10}, arrow: {11}, theme: {12} });", element, renderedTooltip, interactive, placement.ToString(), animation.ToString(), delayShow, delayHide, appendToBody ? document.body.As<object>() : "parent".As<object>(), followCursor, maxWidth, hideOnClick, arrow, theme);
                }

                H5.Script.Write("{0}._tippy.show();", element); //Shows it imediatelly, as the mouse is hovering the element

                var currentTippy = H5.Script.Write<object>("{0}._tippy", element);

                // 2020-10-05 DWR: Sometimes a tooltip will be attached to an element that is removed from the DOM and then the tooltip is left hanging, orphaned. 
                if (parent is null) parent = component;

                parent.WhenRemoved(() =>
                {
                    // 2020-10-05 DWR: I presume that have to check this property before trying to kill it in case it's already been tidied up
                    if (element.HasOwnProperty("_tippy"))
                    {
                        if (currentTippy == H5.Script.Write<object>("{0}._tippy", element))
                        {
                            H5.Script.Write("{0}._tippy.destroy();", element);
                        }
                    }
                    if (rendered["tooltipMarker"] != marker) return;
                    rendered.onmouseenter += AttachTooltip; //Add mount again as needed
                });
            }
            return component;
        }


        public static T TabIndex<T>(this T component, int tabIndex) where T : IComponent
        {
            if (component is ITabIndex hasTabIndex)
            {
                hasTabIndex.TabIndex = tabIndex;
                return component;
            }
            else
            {
                var rendered = component.Render();
                rendered.tabIndex = tabIndex;
                return component;
            }
        }

        public static T Style<T>(this T component, Action<CSSStyleDeclaration> style) where T : IComponent
        {
            style(component.Render().style);
            return component;
        }


        public static T SkipTab<T>(this T component) where T : IComponent => TabIndex(component, -1);

        //Shortcuts:

        /// <summary>Width</summary>
        public static T W<T>(this T component, UnitSize unitSize) where T : IComponent => Width(component, unitSize);

        /// <summary>Height</summary>
        public static T H<T>(this T component, UnitSize unitSize) where T : IComponent => Height(component, unitSize);

        /// <summary>Width</summary>
        public static T W<T>(this T component, int pixels) where T : IComponent => Width(component, pixels.px());

        /// <summary>Height</summary>
        public static T H<T>(this T component, int pixels) where T : IComponent => Height(component, pixels.px());

        /// <summary>Stretch</summary>
        public static T S<T>(this T component) where T : IComponent => Stretch(component);

        /// <summary>WidthStretch</summary>
        public static T WS<T>(this T component) where T : IComponent => WidthStretch(component);

        /// <summary>HeightStretch</summary>
        public static T HS<T>(this T component) where T : IComponent => HeightStretch(component);

        /// <summary>Margin</summary>
        public static T M<T>(this T component, UnitSize unitSize) where T : IComponent => Margin(component, unitSize);

        /// <summary>MarginLeft</summary>
        public static T ML<T>(this T component, UnitSize unitSize) where T : IComponent => MarginLeft(component, unitSize);

        /// <summary>MarginRight</summary>
        public static T MR<T>(this T component, UnitSize unitSize) where T : IComponent => MarginRight(component, unitSize);

        /// <summary>MarginTop</summary>
        public static T MT<T>(this T component, UnitSize unitSize) where T : IComponent => MarginTop(component, unitSize);

        /// <summary>MarginBottom</summary>
        public static T MB<T>(this T component, UnitSize unitSize) where T : IComponent => MarginBottom(component, unitSize);

        /// <summary>Padding</summary>
        public static T P<T>(this T component, UnitSize unitSize) where T : IComponent => Padding(component, unitSize);

        /// <summary>PaddingLeft</summary>
        public static T PL<T>(this T component, UnitSize unitSize) where T : IComponent => PaddingLeft(component, unitSize);

        /// <summary>PaddingRight</summary>
        public static T PR<T>(this T component, UnitSize unitSize) where T : IComponent => PaddingRight(component, unitSize);

        /// <summary>PaddingTop</summary>
        public static T PT<T>(this T component, UnitSize unitSize) where T : IComponent => PaddingTop(component, unitSize);

        /// <summary>PaddingBottom</summary>
        public static T PB<T>(this T component, UnitSize unitSize) where T : IComponent => PaddingBottom(component, unitSize);

        /// <summary>Margin</summary>
        public static T M<T>(this T component, int pixels) where T : IComponent => Margin(component, pixels.px());

        /// <summary>MarginLeft</summary>
        public static T ML<T>(this T component, int pixels) where T : IComponent => MarginLeft(component, pixels.px());

        /// <summary>MarginRight</summary>
        public static T MR<T>(this T component, int pixels) where T : IComponent => MarginRight(component, pixels.px());

        /// <summary>MarginTop</summary>
        public static T MT<T>(this T component, int pixels) where T : IComponent => MarginTop(component, pixels.px());

        /// <summary>MarginBottom</summary>
        public static T MB<T>(this T component, int pixels) where T : IComponent => MarginBottom(component, pixels.px());

        /// <summary>Padding</summary>
        public static T P<T>(this T component, int pixels) where T : IComponent => Padding(component, pixels.px());

        /// <summary>PaddingLeft</summary>
        public static T PL<T>(this T component, int pixels) where T : IComponent => PaddingLeft(component, pixels.px());

        /// <summary>PaddingRight</summary>
        public static T PR<T>(this T component, int pixels) where T : IComponent => PaddingRight(component, pixels.px());

        /// <summary>PaddingTop</summary>
        public static T PT<T>(this T component, int pixels) where T : IComponent => PaddingTop(component, pixels.px());

        /// <summary>PaddingBottom</summary>
        public static T PB<T>(this T component, int pixels) where T : IComponent => PaddingBottom(component, pixels.px());
    }
}