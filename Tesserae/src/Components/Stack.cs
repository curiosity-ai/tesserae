using System;
using System.Collections.Generic;
using Retyped;
using static Tesserae.UI;
using static Retyped.dom;

namespace Tesserae.Components
{
    public class Stack : IContainer<Stack, IComponent>, IHasBackgroundColor, IHasMarginPadding
    {
        public Orientation StackOrientation
        {
            get
            {
                switch (InnerElement.style.flexDirection)
                {
                    case "row": return Orientation.Horizontal;
                    case "column": return Orientation.Vertical;
                    case "row-reverse": return Orientation.HorizontalReverse;
                    case "column-reverse": return Orientation.VerticalReverse;
                }

                return Orientation.Vertical;
            }
            set
            {
                if (value != StackOrientation)
                {
                    switch (value)
                    {
                        case Orientation.Horizontal: InnerElement.style.flexDirection = "row"; break;
                        case Orientation.Vertical: InnerElement.style.flexDirection = "column"; break;
                        case Orientation.HorizontalReverse: InnerElement.style.flexDirection = "row-reverse"; break;
                        case Orientation.VerticalReverse: InnerElement.style.flexDirection = "column-reverse"; break;
                    }
                }
            }
        }

        public bool CanWrap
        {
            get { return InnerElement.style.flexWrap != "nowrap"; }
            set
            {
                if (value != CanWrap)
                {
                    InnerElement.style.flexWrap = value ? "wrap" : "nowrap";
                }
            }
        }

        public HTMLElement InnerElement { get; private set; }
        public string Background { get => InnerElement.style.background; set => InnerElement.style.background = value; }
        public string Margin { get => InnerElement.style.margin; set => InnerElement.style.margin = value; }
        public string Padding { get => InnerElement.style.padding; set => InnerElement.style.padding = value; }

        public static ItemAlign GetAlign(IComponent component)
        {
            var item = GetItem(component);
            if (Enum.TryParse<ItemAlign>(item.style.alignSelf.Replace("flex-", ""), true, out var result)) return result;
            throw new Exception("Incorrect Stack item align.");
        }

        public static void SetAlign(IComponent component, ItemAlign align)
        {
            var item = GetItem(component);
            string cssAlign = align.ToString().ToLower();
            if (cssAlign == "end" || cssAlign == "start") cssAlign = $"flex-{cssAlign}";
            item.style.alignSelf = cssAlign;
            item.setAttribute("tss-stk-as", "");
        }

        /// <summary>
        /// Sets the align-items css property for this stack
        /// </summary>
        /// <param name="align"></param>
        /// <returns></returns>
        public Stack VerticalAlígn(ItemAlign align)
        {
            string cssAlign = align.ToString().ToLower();
            if (cssAlign == "end" || cssAlign == "start") cssAlign = $"flex-{cssAlign}";
            InnerElement.style.alignItems = cssAlign;
            return this;
        }

        /// <summary>
        /// Sets the justify-content css property for this stack
        /// </summary>
        /// <param name="align"></param>
        /// <returns></returns>
        public Stack HorizontalAlígn(JustifyContent justify)
        {
            string cssJustify = justify.ToString().ToLower();
            if (cssJustify == "end" || cssJustify == "start") cssJustify = $"flex-{cssJustify}";
            if (cssJustify == "between" || cssJustify == "around" || cssJustify == "evenly") cssJustify = $"space-{cssJustify}";
            InnerElement.style.justifyItems = cssJustify;
            return this;
        }

        public static ItemSize GetWidth(IComponent component)
        {
            var item = GetItem(component);

            if (item.style.width == "auto") return new ItemSize() { Type = Unit.Auto };
            if (item.style.width.EndsWith("px")) return new ItemSize() { Type = Unit.Pixels, Value = float.Parse(item.style.width.Substring(0,item.style.width.Length - 2)) };
            if (item.style.width.EndsWith("%")) return new ItemSize() { Type = Unit.Percents, Value = float.Parse(item.style.width.Substring(0, item.style.width.Length - 1)) };
            if (item.style.width.EndsWith("vw")) return new ItemSize() { Type = Unit.Viewport, Value = float.Parse(item.style.width.Substring(0, item.style.width.Length - 2)) };

            throw new Exception("Incorrect Stack item width.");
        }

        public static void SetWidth(IComponent component, Unit sizeType, float size = 0)
        {
            var item = GetItem(component);
            switch (sizeType)
            {
                case Unit.Auto: item.style.width = "auto"; break;
                case Unit.Pixels: item.style.width = $"{size:0.####}px"; break;
                case Unit.Percents: item.style.width = $"{size:0.####}%"; break;
                case Unit.Viewport: item.style.width = $"{size:0.####}vw"; break;
            }
            item.setAttribute("tss-stk-w","");
        }

        private static HTMLElement GetCorrectItemToApplyStyle(IComponent component)
        {
            HTMLElement item;
            if (component is Stack stack)
            {
                item = stack.InnerElement;
            }
            else if (component is Modal modal)
            {
                item = modal._modal;
            }
            else
            {
                item = GetItem(component);
            }
            return item;
        }

        public static void SetMinWidth(IComponent component, Unit sizeType, float size = 0)
        {
            var item = GetCorrectItemToApplyStyle(component);
            switch (sizeType)
            {
                case Unit.Auto: item.style.minWidth = "auto"; break;
                case Unit.Pixels: item.style.minWidth = $"{size:0.####}px"; break;
                case Unit.Percents: item.style.minWidth = $"{size:0.####}%"; break;
                case Unit.Viewport: item.style.minWidth = $"{size:0.####}vw"; break;
            }
            item.setAttribute("tss-stk-mw", "");
        }

        public static void SetMaxWidth(IComponent component, Unit sizeType, float size = 0)
        {
            var item = GetCorrectItemToApplyStyle(component);
            switch (sizeType)
            {
                case Unit.Auto: item.style.maxWidth = "auto"; break;
                case Unit.Pixels: item.style.maxWidth = $"{size:0.####}px"; break;
                case Unit.Percents: item.style.maxWidth = $"{size:0.####}%"; break;
                case Unit.Viewport: item.style.maxWidth = $"{size:0.####}vw"; break;
            }
            item.setAttribute("tss-stk-mxw", "");
        }

        public static ItemSize GetHeight(IComponent component)
        {
            var item = GetItem(component);
            if (item.style.height == "auto") return new ItemSize() { Type = Unit.Auto };
            if (item.style.height.EndsWith("px")) return new ItemSize() { Type = Unit.Pixels, Value = float.Parse(item.style.height.Substring(0, item.style.height.Length - 2)) };
            if (item.style.height.EndsWith("%")) return new ItemSize() { Type = Unit.Percents, Value = float.Parse(item.style.height.Substring(0, item.style.height.Length - 1)) };
            if (item.style.height.EndsWith("vh")) return new ItemSize() { Type = Unit.Viewport, Value = float.Parse(item.style.height.Substring(0, item.style.height.Length - 2)) };

            throw new Exception("Incorrect Stack item height.");
        }

        public static void SetHeight(IComponent component, Unit sizeType, float size = 0)
        {
            var item = GetItem(component);
            switch (sizeType)
            {
                case Unit.Auto: item.style.height = "auto"; break;
                case Unit.Pixels: item.style.height = $"{size:0.####}px"; break;
                case Unit.Percents: item.style.height = $"{size:0.####}%"; break;
                case Unit.Viewport: item.style.height = $"{size:0.####}vh"; break;
            }
            item.setAttribute("tss-stk-h", "");
        }

        public static void SetMinHeight(IComponent component, Unit sizeType, float size = 0)
        {
            var item = GetCorrectItemToApplyStyle(component);
            item.style.minHeight = Units.Translate(sizeType, size);
            item.setAttribute("tss-stk-mh", "");
        }

        public static void SetMaxHeight(IComponent component, Unit sizeType, float size = 0)
        {
            var item = GetCorrectItemToApplyStyle(component);
            item.style.maxHeight = Units.Translate(sizeType, size);
            item.setAttribute("tss-stk-mxh", "");
        }

        public static void SetMarginLeft(IComponent component, Unit sizeType, float size = 0)
        {
            var item = GetCorrectItemToApplyStyle(component);
            item.style.marginLeft = Units.Translate(sizeType, size);
            item.setAttribute("tss-stk-m", "");
        }

        public static void SetMarginRight(IComponent component, Unit sizeType, float size = 0)
        {
            var item = GetCorrectItemToApplyStyle(component);
            item.style.marginRight = Units.Translate(sizeType, size);
            item.setAttribute("tss-stk-m", "");
        }

        public static void SetMarginTop(IComponent component, Unit sizeType, float size = 0)
        {
            var item = GetCorrectItemToApplyStyle(component);
            item.style.marginTop= Units.Translate(sizeType, size);
            item.setAttribute("tss-stk-m", "");
        }

        public static void SetMarginBottom(IComponent component, Unit sizeType, float size = 0)
        {
            var item = GetCorrectItemToApplyStyle(component);
            item.style.marginBottom = Units.Translate(sizeType, size);
            item.setAttribute("tss-stk-m", "");
        }


        public static void SetPaddingLeft(IComponent component, Unit sizeType, float size = 0)
        {
            var item = GetCorrectItemToApplyStyle(component);
            item.style.paddingLeft = Units.Translate(sizeType, size);
            item.setAttribute("tss-stk-p", "");
        }

        public static void SetPaddingRight(IComponent component, Unit sizeType, float size = 0)
        {
            var item = GetCorrectItemToApplyStyle(component);
            item.style.paddingRight = Units.Translate(sizeType, size);
            item.setAttribute("tss-stk-p", "");
        }

        public static void SetPaddingTop(IComponent component, Unit sizeType, float size = 0)
        {
            var item = GetCorrectItemToApplyStyle(component);
            item.style.paddingTop = Units.Translate(sizeType, size);
            item.setAttribute("tss-stk-p", "");
        }

        public static void SetPaddingBottom(IComponent component, Unit sizeType, float size = 0)
        {
            var item = GetCorrectItemToApplyStyle(component);
            item.style.paddingBottom = Units.Translate(sizeType, size);
            item.setAttribute("tss-stk-p", "");
        }

        public static int GetGrow(IComponent component)
        {
            var item = GetItem(component);
            return int.Parse(item.style.flexGrow);
        }

        public static void SetGrow(IComponent component, int grow)
        {
            var item = GetItem(component);
            item.style.flexGrow = grow.ToString();
            item.setAttribute("tss-stk-fg", "");
        }

        public static bool GetShrink(IComponent component)
        {
            var item = GetItem(component);
            return item.style.flexShrink == "1";
        }
        public static void SetShrink(IComponent component, bool shrink)
        {
            var item = GetItem(component);
            item.style.flexShrink = shrink ? "1" : "0";
            item.setAttribute("tss-stk-fs", "");
        }

        public Stack(Orientation orientation = Orientation.Vertical)
        {
            InnerElement = Div(_("tss-stack"));
            this.StackOrientation = orientation;
        }

        public void Add(IComponent component)
        {
            InnerElement.appendChild(GetItem(component, true));
        }

        public virtual void Clear()
        {
            ClearChildren(InnerElement);
        }

        public void Replace(IComponent newComponent, IComponent oldComponent)
        {
            InnerElement.replaceChild(GetItem(newComponent), GetItem(oldComponent));
        }
        
        public virtual HTMLElement Render()
        {
            return InnerElement;
        }

        public Stack Horizontal()
        {
            StackOrientation = Stack.Orientation.Horizontal;
            return this;
        }

        public Stack Vertical()
        {
            StackOrientation = Stack.Orientation.Vertical;
            return this;
        }

        public Stack HorizontalReverse()
        {
            StackOrientation = Stack.Orientation.HorizontalReverse;
            return this;
        }

        public Stack VerticalReverse()
        {
            StackOrientation = Stack.Orientation.VerticalReverse;
            return this;
        }

        public Stack Wrap()
        {
            CanWrap = true;
            return this;
        }

        public Stack NoWrap()
        {
            CanWrap = false;
            return this;
        }

        private static HTMLElement GetItem(IComponent component, bool forceAdd = false)
        {
            if (!((component as dynamic).StackItem is HTMLElement item))
            {
                var rendered = component.Render();
                if (forceAdd || (rendered.parentElement is object && rendered.parentElement.classList.contains("tss-stack")))
                {
                    item = Div(_("tss-stack-item", styles: s =>
                    {
                        s.alignSelf = "auto";
                        s.width = "auto";
                        s.height = "auto";
                        s.flexShrink = "1";
                    }), component.Render());
                    (component as dynamic).StackItem = item;

                    if (forceAdd)
                    {
                        CopyStylesDefinedWithExtension(rendered, item);
                    }

                }
                else
                {
                    item = rendered;
                }
            }
            return item;
        }

        private static void CopyStylesDefinedWithExtension(HTMLElement from, HTMLElement to)
        {
            var fs = from.style;
            var ts = to.style;

            bool has(string att) => from.hasAttribute(att);

            if (has("tss-stk-w"))  { ts.width     = fs.width;  fs.width = "100%"; }
            if (has("tss-stk-mw")) { ts.minWidth = fs.minWidth; fs.minWidth = "100%"; }
            if (has("tss-stk-mxw")) { ts.maxWidth = fs.maxWidth; fs.maxWidth = "100%"; }
            if (has("tss-stk-h"))  { ts.height    = fs.height; fs.height = "100%"; }
            if (has("tss-stk-mh")) { ts.minHeight = fs.minHeight; fs.minHeight = "100%"; }
            if (has("tss-stk-mxh")) { ts.maxHeight = fs.maxHeight; fs.maxHeight = "100%"; }

            if (has("tss-stk-m"))
            {
                ts.marginLeft = fs.marginLeft;
                ts.marginTop = fs.marginTop;
                ts.marginRight = fs.marginRight;
                ts.marginBottom = fs.marginBottom;
                fs.marginLeft = fs.marginTop = fs.marginRight = fs.marginBottom = "";
            }

            if (has("tss-stk-p"))
            {
                ts.paddingLeft = fs.paddingLeft;
                ts.paddingTop = fs.paddingTop;
                ts.paddingRight = fs.paddingRight;
                ts.paddingBottom = fs.paddingBottom;
                fs.paddingLeft = fs.paddingTop = fs.paddingRight = fs.paddingBottom = "";
            }

            //TODO: check if should clear this here:
            if (has("tss-stk-fg")) { ts.flexGrow = fs.flexGrow;  /*fs.flexGrow = ""; */}
            if (has("tss-stk-fs")) { ts.flexShrink = fs.flexShrink;  /*fs.flexShrink = ""; */}
            if (has("tss-stk-as")) { ts.alignSelf = fs.alignSelf; /*fs.alignSelf = "";*/ }
        }

        public enum Orientation
        {
            Vertical,
            Horizontal,
            VerticalReverse,
            HorizontalReverse,
        }

        public enum ItemAlign
        {
            Auto,
            Stretch,
            Baseline,
            Start,
            Center,
            End
        }

        public enum JustifyContent
        {
            Between,
            Around,
            Evenly,
            Start,
            Center,
            End
        }

        public struct ItemSize
        {
            public Unit Type { get; set; }
            public float Value { get; set; }
        }
    }

    public static class StackExtensions
    {
        public static T AlignAuto<T>(this T component) where T : IComponent
        {
            Stack.SetAlign(component, Stack.ItemAlign.Auto);
            return component;
        }

        public static T AlignStretch<T>(this T component) where T : IComponent
        {
            Stack.SetAlign(component, Stack.ItemAlign.Stretch);
            return component;
        }
        public static T AlignBaseline<T>(this T component) where T : IComponent
        {
            Stack.SetAlign(component, Stack.ItemAlign.Baseline);
            return component;
        }
        public static T AlignStart<T>(this T component) where T : IComponent
        {
            Stack.SetAlign(component, Stack.ItemAlign.Start);
            return component;
        }
        public static T AlignCenter<T>(this T component) where T : IComponent
        {
            Stack.SetAlign(component, Stack.ItemAlign.Center);
            return component;
        }
        public static T AlignEnd<T>(this T component) where T : IComponent
        {
            Stack.SetAlign(component, Stack.ItemAlign.End);
            return component;
        }

        public static T Margin<T>(this T component, Unit unit, float margin = 0) where T : IComponent
        {
            Stack.SetMarginLeft(component, unit, margin);
            Stack.SetMarginRight(component, unit, margin);
            Stack.SetMarginTop(component, unit, margin);
            Stack.SetMarginBottom(component, unit, margin);
            return component;
        }

        public static T MarginLeft<T>(this T component, Unit unit, float margin = 0) where T : IComponent
        {
            Stack.SetMarginLeft(component, unit, margin);
            return component;
        }

        public static T MarginRight<T>(this T component, Unit unit, float margin = 0) where T : IComponent
        {
            Stack.SetMarginRight(component, unit, margin);
            return component;
        }

        public static T MarginTop<T>(this T component, Unit unit, float margin = 0) where T : IComponent
        {
            Stack.SetMarginTop(component, unit, margin);
            return component;
        }

        public static T MarginBottom<T>(this T component, Unit unit, float margin = 0) where T : IComponent
        {
            Stack.SetMarginBottom(component, unit, margin);
            return component;
        }

        public static T Padding<T>(this T component, Unit unit, float padding = 0) where T : IComponent
        {
            Stack.SetPaddingLeft(component, unit, padding);
            Stack.SetPaddingRight(component, unit, padding);
            Stack.SetPaddingTop(component, unit, padding);
            Stack.SetPaddingBottom(component, unit, padding);
            return component;
        }

        public static T PaddingLeft<T>(this T component, Unit unit, float padding = 0) where T : IComponent
        {
            Stack.SetPaddingLeft(component, unit, padding);
            return component;
        }

        public static T PaddingRight<T>(this T component, Unit unit, float padding = 0) where T : IComponent
        {
            Stack.SetPaddingRight(component, unit, padding);
            return component;
        }

        public static T PaddingTop<T>(this T component, Unit unit, float padding = 0) where T : IComponent
        {
            Stack.SetPaddingTop(component, unit, padding);
            return component;
        }

        public static T PaddingBottom<T>(this T component, Unit unit, float padding = 0) where T : IComponent
        {
            Stack.SetPaddingBottom(component, unit, padding);
            return component;
        }

        public static T WidthAuto<T>(this T component) where T : IComponent
        {
            Stack.SetWidth(component, Unit.Auto);
            return component;
        }

        public static T Width<T>(this T component, float value, Unit unit) where T : IComponent
        {
            Stack.SetWidth(component, unit, value);
            return component;
        }

        public static T MinWidth<T>(this T component, float value, Unit unit) where T : IComponent
        {
            Stack.SetMinWidth(component, unit, value);
            return component;
        }

        public static T MaxWidth<T>(this T component, float value, Unit unit) where T : IComponent
        {
            Stack.SetMaxWidth(component, unit, value);
            return component;
        }

        public static T WidthStretch<T>(this T component) where T : IComponent
        {
            Stack.SetWidth(component, Unit.Percents, 100);
            return component;
        }

        public static T HeightAuto<T>(this T component) where T : IComponent
        {
            Stack.SetHeight(component, Unit.Auto);
            return component;
        }

        public static T Height<T>(this T component, float value, Unit unit) where T : IComponent
        {
            Stack.SetHeight(component, unit, value);
            return component;
        }
        
        public static T MinHeight<T>(this T component, float value, Unit unit) where T : IComponent
        {
            Stack.SetMinHeight(component, unit, value);
            return component;
        }

        public static T MaxHeight<T>(this T component, float value, Unit unit) where T : IComponent
        {
            Stack.SetMaxHeight(component, unit, value);
            return component;
        }

        public static T HeightStretch<T>(this T component) where T : IComponent
        {
            Stack.SetHeight(component, Unit.Percents, 100);
            return component;
        }

        public static T MinHeightStretch<T>(this T component) where T : IComponent
        {
            Stack.SetMinHeight(component, Unit.Percents, 100);
            return component;
        }
        
        public static T Grow<T>(this T component, int grow) where T : IComponent
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
    }

    public enum Unit
    {
        Auto,
        Percents,
        Viewport,
        Pixels
    }

    public static class Units
    {
        public static string Translate(Unit unit, double value)
        {
            switch (unit)
            {
                case Unit.Auto: return "auto";
                case Unit.Pixels: return $"{value:0.####}px";
                case Unit.Percents: return $"{value:0.####}%";
                case Unit.Viewport: return $"{value:0.####}vh";
            }

            throw new NotSupportedException();
        }
    }
}
