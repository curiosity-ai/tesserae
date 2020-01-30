﻿using System;
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
            if (item.style.width.EndsWith("px")) return new ItemSize() { Type = Unit.Pixels, Value = float.Parse(item.style.width.Substring(item.style.width.Length - 2)) };
            if (item.style.width.EndsWith("%")) return new ItemSize() { Type = Unit.Percents, Value = float.Parse(item.style.width.Substring(item.style.width.Length - 1)) };
            if (item.style.width.EndsWith("vw")) return new ItemSize() { Type = Unit.Viewport, Value = float.Parse(item.style.width.Substring(item.style.width.Length - 2)) };

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
        }
        public static void SetMinWidth(IComponent component, Unit sizeType, float size = 0)
        {
            HTMLElement item;
            if (component is Stack stack)
            {
                item = stack.InnerElement;
            }
            else
            {
                item = GetItem(component);
            }
            switch (sizeType)
            {
                case Unit.Auto: item.style.minWidth = "auto"; break;
                case Unit.Pixels: item.style.minWidth = $"{size:0.####}px"; break;
                case Unit.Percents: item.style.minWidth = $"{size:0.####}%"; break;
                case Unit.Viewport: item.style.minWidth = $"{size:0.####}vw"; break;
            }
        }

        public static ItemSize GetHeight(IComponent component)
        {
            var item = GetItem(component);
            if (item.style.height == "auto") return new ItemSize() { Type = Unit.Auto };
            if (item.style.height.EndsWith("px")) return new ItemSize() { Type = Unit.Pixels, Value = float.Parse(item.style.height.Substring(item.style.height.Length - 2)) };
            if (item.style.height.EndsWith("%")) return new ItemSize() { Type = Unit.Percents, Value = float.Parse(item.style.height.Substring(item.style.height.Length - 1)) };
            if (item.style.height.EndsWith("vh")) return new ItemSize() { Type = Unit.Viewport, Value = float.Parse(item.style.height.Substring(item.style.height.Length - 2)) };

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
        }

        public static void SetMinHeight(IComponent component, Unit sizeType, float size = 0)
        {
            HTMLElement item;
            if (component is Stack stack)
            {
                item = stack.InnerElement;
            }
            else
            {
                item = GetItem(component);
            }
            switch (sizeType)
            {
                case Unit.Auto: item.style.minHeight = "auto"; break;
                case Unit.Pixels: item.style.minHeight = $"{size:0.####}px"; break;
                case Unit.Percents: item.style.minHeight = $"{size:0.####}%"; break;
                case Unit.Viewport: item.style.minHeight = $"{size:0.####}vh"; break;
            }
        }


        public static ItemMargin GetMargin(IComponent component)
        {
            var item = GetItem(component);
            ItemMargin result = new ItemMargin();
            if (item.style.marginLeft.EndsWith("px")) result.Left = float.Parse(item.style.marginLeft.Substring(0, item.style.marginLeft.Length - 2));
            if (item.style.marginTop.EndsWith("px")) result.Top = float.Parse(item.style.marginTop.Substring(0, item.style.marginTop.Length - 2));
            if (item.style.marginRight.EndsWith("px")) result.Right = float.Parse(item.style.marginRight.Substring(0, item.style.marginRight.Length - 2));
            if (item.style.marginBottom.EndsWith("px")) result.Bottom = float.Parse(item.style.marginBottom.Substring(0, item.style.marginBottom.Length - 2));

            return result;
        }

        public static void SetMargin(IComponent component, ItemMargin margin)
        {
            var item = GetItem(component);
            item.style.marginLeft = $"{margin.Left}px";
            item.style.marginTop = $"{margin.Top}px";
            item.style.marginRight = $"{margin.Right}px";
            item.style.marginBottom = $"{margin.Bottom}px";
        }

        public static void SetPadding(IComponent component, ItemMargin padding)
        {
            var item = GetItem(component);
            item.style.paddingLeft = $"{padding.Left}px";
            item.style.paddingTop = $"{padding.Top}px";
            item.style.paddingRight = $"{padding.Right}px";
            item.style.paddingBottom = $"{padding.Bottom}px";
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
                }
                else
                {
                    item = rendered;
                }
            }
            return item;
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

        public struct ItemMargin
        {
            public float Left { get; set; }
            public float Top { get; set; }
            public float Right { get; set; }
            public float Bottom { get; set; }

            public ItemMargin(float margin = 0)
            {
                Left = Top = Right = Bottom = margin;
            }

            public ItemMargin(float leftRight, float topBottom)
            {
                Left = Right = leftRight;
                Top = Bottom = topBottom;
            }

            public ItemMargin(float left, float top, float right, float bottom)
            {
                Left = left;
                Top = top;
                Right = right;
                Bottom = bottom;
            }
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

        public static T Margin<T>(this T component, Stack.ItemMargin margin) where T : IComponent
        {
            Stack.SetMargin(component, margin);
            return component;
        }

        public static T Margin<T>(this T component, float margin) where T : IComponent
        {
            Stack.SetMargin(component, new Stack.ItemMargin(margin));
            return component;
        }

        public static T Margin<T>(this T component, float leftRight, float topBottom) where T : IComponent
        {
            Stack.SetMargin(component, new Stack.ItemMargin(leftRight, topBottom));
            return component;
        }

        public static T Margin<T>(this T component, float left, float top, float right, float bottom) where T : IComponent
        {
            Stack.SetMargin(component, new Stack.ItemMargin(left, top, right, bottom));
            return component;
        }


        public static T Padding<T>(this T component, Stack.ItemMargin margin) where T : IComponent
        {
            Stack.SetPadding(component, margin);
            return component;
        }

        public static T Padding<T>(this T component, float margin) where T : IComponent
        {
            Stack.SetPadding(component, new Stack.ItemMargin(margin));
            return component;
        }

        public static T Padding<T>(this T component, float leftRight, float topBottom) where T : IComponent
        {
            Stack.SetPadding(component, new Stack.ItemMargin(leftRight, topBottom));
            return component;
        }

        public static T Padding<T>(this T component, float left, float top, float right, float bottom) where T : IComponent
        {
            Stack.SetPadding(component, new Stack.ItemMargin(left, top, right, bottom));
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
}
