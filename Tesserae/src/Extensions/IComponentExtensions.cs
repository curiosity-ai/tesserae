using System;
using System.Threading.Tasks;
using Tesserae.HTML;
using static H5.Core.dom;

namespace Tesserae.Components
{

    public static class IComponentExtensions
    {
        public static T WhenMounted<T>(this T component, Action callback) where T : IComponent
        {
            if (component.Render().IsMounted())
            {
                // 2020-08-13 DWR: In case this is called after the component has already been mounted, call this immediately instead of adding a callback to the DomObserver.WhenMounted queue (that will likely never be cleared because the DomObserver will
                // never see the component as having been mounted if it's ALREADY mounted)
                callback();
            }
            else
            {
                DomObserver.WhenMounted(component.Render(), callback);
            }
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
            Stack.GetCorrectItemToApplyStyle(component).item.classList.add("tss-collapse");
            return component;
        }

        public static T FadeThenCollapse<T>(this T component) where T : IComponent => Fade(component,async () => { await Task.Delay(1000); Collapse(component); } );


        public static T Fade<T>(this T component) where T : IComponent => Fade(component, () => { });
        public static T Fade<T>(this T component, Func<Task> andThen = null) where T : IComponent => Fade(component, andThen is object ? (Action)(() => andThen.Invoke().FireAndForget()) : null);

        public static T Fade<T>(this T component, Action andThen = null) where T : IComponent
        {
            var (el, _) = Stack.GetCorrectItemToApplyStyle(component);
            el.classList.add("tss-fade");
            el.classList.remove("tss-fade-light", "tss-show", "tss-fade-light-clickable");
            component.Render().classList.remove("tss-fade-light", "tss-show", "tss-fade-light-clickable"); //Need to remove from component as well, because it could have been set before it was added to a stack
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
            el.classList.add("tss-fade-light");
            el.classList.remove("tss-fade", "tss-show", "tss-fade-light-clickable");
            component.Render().classList.remove("tss-fade", "tss-show", "tss-fade-light-clickable"); //Need to remove from component as well, because it could have been set before it was added to a stack
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

        public static T LightFadeClickable<T>(this T component) where T : IComponent
        {
            var (el, _) = Stack.GetCorrectItemToApplyStyle(component);
            el.classList.add("tss-fade-light-clickable");
            el.classList.remove("tss-fade", "tss-show", "tss-fade-light");
            component.Render().classList.remove("tss-fade", "tss-show", "tss-fade-light"); //Need to remove from component as well, because it could have been set before it was added to a stack
            return component;
        }

        public static T Show<T>(this T component) where T : IComponent
        {
            var (el, _) = Stack.GetCorrectItemToApplyStyle(component);
            el.classList.add("tss-fade", "tss-show");
            el.classList.remove("tss-fade-light", "tss-collapse", "tss-fade-light-clickable");
            component.Render().classList.remove("tss-fade-light", "tss-collapse", "tss-fade-light-clickable", "tss-fade"); //Need to remove all from component as well, because it could have been set before it was added to a stack
            return component;
        }

        public static T Tooltip<T>(this T component, string text, TooltipPosition position = TooltipPosition.Top) where T : IComponent
        {
            var element = component.Render();
            element.dataset["tooltip"] = text;
            element.dataset["flow"] = position.ToString();
            return component;
        }

        //Shortcuts:

        /// <summary>Width</summary>
        public static T W<T>(this T component, UnitSize unitSize) where T : IComponent => Width(component, unitSize);

        /// <summary>Height</summary>
        public static T H<T>(this T component, UnitSize unitSize) where T : IComponent => Height(component, unitSize);

        /// <summary>Stretch</summary>
        public static T S<T>(this T component) where T : IComponent => Stretch(component);

        /// <summary>WidthStretch</summary>
        public static T WS<T>(this T component) where T : IComponent => WidthStretch(component);

        /// <summary>HeightStretch</summary>
        public static T HS<T>(this T component) where T : IComponent => HeightStretch(component);

        /// <summary>MarginLeft</summary>
        public static T ML<T>(this T component, UnitSize unitSize) where T : IComponent => MarginLeft(component, unitSize);

        /// <summary>MarginRight</summary>
        public static T MR<T>(this T component, UnitSize unitSize) where T : IComponent => MarginRight(component, unitSize);

        /// <summary>MarginTop</summary>
        public static T MT<T>(this T component, UnitSize unitSize) where T : IComponent => MarginTop(component, unitSize);

        /// <summary>MarginBottom</summary>
        public static T MB<T>(this T component, UnitSize unitSize) where T : IComponent => MarginBottom(component, unitSize);

        /// <summary>PaddingLeft</summary>
        public static T PL<T>(this T component, UnitSize unitSize) where T : IComponent => PaddingLeft(component, unitSize);

        /// <summary>PaddingRight</summary>
        public static T PR<T>(this T component, UnitSize unitSize) where T : IComponent => PaddingRight(component, unitSize);

        /// <summary>PaddingTop</summary>
        public static T PT<T>(this T component, UnitSize unitSize) where T : IComponent => PaddingTop(component, unitSize);

        /// <summary>PaddingBottom</summary>
        public static T PB<T>(this T component, UnitSize unitSize) where T : IComponent => PaddingBottom(component, unitSize);
    }
}
