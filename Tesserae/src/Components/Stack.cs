using System;
using System.Collections.Generic;
using Retyped;
using static Tesserae.UI;
using static Retyped.dom;

namespace Tesserae.Components
{
    public class Stack : IContainer<Stack, IComponent>, IHasBackgroundColor, IHasMarginPadding
    {
        #region Properties

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

        #endregion

        #region Static

        private static HTMLElement GetItem(IComponent component, bool forceAdd = false)
        {
            if (!((component as dynamic).StackItem is HTMLElement item))
            {
                var rendered = component.Render();
                if (forceAdd  || (rendered.parentElement is object && rendered.parentElement.classList.contains("tss-stack")))
                {
                    item = Div(_("tss-stack-item", styles: s =>
                    {
                        s.alignSelf = "auto";
                        s.width = "auto";
                        s.height = "auto";
                        s.flexShrink = "1";
                        s.overflow = "hidden";
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

        #region Items Properties

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

        public static Stack.ItemSize GetWidth(IComponent component)
        {
            var item = GetItem(component);
            if (item.style.width == "auto") return new Stack.ItemSize() { Type = ItemSizeType.Auto };
            if (item.style.width.EndsWith("px")) return new Stack.ItemSize() { Type = ItemSizeType.Pixels, Value = double.Parse(item.style.width.Substring(item.style.width.Length - 2)) };
            if (item.style.width.EndsWith("%")) return new Stack.ItemSize() { Type = ItemSizeType.Percents, Value = double.Parse(item.style.width.Substring(item.style.width.Length - 1)) };

            throw new Exception("Incorrect Stack item width.");
        }

        public static void SetWidth(IComponent component, Stack.ItemSizeType sizeType, double size = 0)
        {
            var item = GetItem(component);
            switch (sizeType)
            {
                case ItemSizeType.Auto: item.style.width = "auto"; break;
                case ItemSizeType.Pixels: item.style.width = $"{size:0.####}px"; break;
                case ItemSizeType.Percents: item.style.width = $"{size:0.####}%"; break;
            }
        }

        public static Stack.ItemSize GetHeight(IComponent component)
        {
            var item = GetItem(component);
            if (item.style.height == "auto") return new Stack.ItemSize() { Type = ItemSizeType.Auto };
            if (item.style.height.EndsWith("px")) return new Stack.ItemSize() { Type = ItemSizeType.Pixels, Value = double.Parse(item.style.height.Substring(item.style.height.Length - 2)) };
            if (item.style.height.EndsWith("%")) return new Stack.ItemSize() { Type = ItemSizeType.Percents, Value = double.Parse(item.style.height.Substring(item.style.height.Length - 1)) };

            throw new Exception("Incorrect Stack item height.");
        }
        public static void SetHeight(IComponent component, Stack.ItemSizeType sizeType, double size = 0)
        {
            var item = GetItem(component);
            switch (sizeType)
            {
                case ItemSizeType.Auto: item.style.height = "auto"; break;
                case ItemSizeType.Pixels: item.style.height = $"{size:0.####}px"; break;
                case ItemSizeType.Percents: item.style.height = $"{size:0.####}%"; break;
            }
        }

        public static void SetMinHeight(IComponent component, ItemSizeType sizeType, double size = 0)
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
                case ItemSizeType.Auto: item.style.minHeight = "auto"; break;
                case ItemSizeType.Pixels: item.style.minHeight = $"{size:0.####}px"; break;
                case ItemSizeType.Percents: item.style.minHeight = $"{size:0.####}%"; break;
            }
        }

        public static ItemMargin GetMargin(IComponent component)
        {
            var item = GetItem(component);
            ItemMargin result = new ItemMargin();
            if (item.style.marginLeft.EndsWith("px")) result.Left = double.Parse(item.style.marginLeft.Substring(0, item.style.marginLeft.Length - 2));
            if (item.style.marginTop.EndsWith("px")) result.Top = double.Parse(item.style.marginTop.Substring(0, item.style.marginTop.Length - 2));
            if (item.style.marginRight.EndsWith("px")) result.Right = double.Parse(item.style.marginRight.Substring(0, item.style.marginRight.Length - 2));
            if (item.style.marginBottom.EndsWith("px")) result.Bottom = double.Parse(item.style.marginBottom.Substring(0, item.style.marginBottom.Length - 2));

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

        #endregion

        #endregion

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

        public enum ItemSizeType
        {
            Auto,
            Percents,
            Pixels
        }

        public struct ItemSize
        {
            public ItemSizeType Type { get; set; }
            public double Value { get; set; }
        }

        public struct ItemMargin
        {
            public double Left { get; set; }
            public double Top { get; set; }
            public double Right { get; set; }
            public double Bottom { get; set; }

            public ItemMargin(double margin = 0)
            {
                Left = Top = Right = Bottom = margin;
            }

            public ItemMargin(double leftRight, double topBottom)
            {
                Left = Right = leftRight;
                Top = Bottom = topBottom;
            }

            public ItemMargin(double left, double top, double right, double bottom)
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
        public static Stack Horizontal(this Stack stack)
        {
            stack.StackOrientation = Stack.Orientation.Horizontal;
            return stack;
        }

        public static Stack Vertical(this Stack stack)
        {
            stack.StackOrientation = Stack.Orientation.Vertical;
            return stack;
        }

        public static Stack HorizontalReverse(this Stack stack)
        {
            stack.StackOrientation = Stack.Orientation.HorizontalReverse;
            return stack;
        }

        public static Stack VerticalReverse(this Stack stack)
        {
            stack.StackOrientation = Stack.Orientation.VerticalReverse;
            return stack;
        }

        public static Stack Wrap(this Stack stack)
        {
            stack.CanWrap = true;
            return stack;
        }

        public static Stack NoWrap(this Stack stack)
        {
            stack.CanWrap = false;
            return stack;
        }

        #region Item Properties

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

        public static T Margin<T>(this T component, double margin) where T : IComponent
        {
            Stack.SetMargin(component, new Stack.ItemMargin(margin));
            return component;
        }

        public static T Margin<T>(this T component, double leftRight, double topBottom) where T : IComponent
        {
            Stack.SetMargin(component, new Stack.ItemMargin(leftRight, topBottom));
            return component;
        }

        public static T Margin<T>(this T component, double left, double top, double right, double bottom) where T : IComponent
        {
            Stack.SetMargin(component, new Stack.ItemMargin(left, top, right, bottom));
            return component;
        }


        public static T Padding<T>(this T component, Stack.ItemMargin margin) where T : IComponent
        {
            Stack.SetPadding(component, margin);
            return component;
        }

        public static T Padding<T>(this T component, double margin) where T : IComponent
        {
            Stack.SetPadding(component, new Stack.ItemMargin(margin));
            return component;
        }

        public static T Padding<T>(this T component, double leftRight, double topBottom) where T : IComponent
        {
            Stack.SetPadding(component, new Stack.ItemMargin(leftRight, topBottom));
            return component;
        }

        public static T Padding<T>(this T component, double left, double top, double right, double bottom) where T : IComponent
        {
            Stack.SetPadding(component, new Stack.ItemMargin(left, top, right, bottom));
            return component;
        }

        public static T WidthAuto<T>(this T component) where T : IComponent
        {
            Stack.SetWidth(component, Stack.ItemSizeType.Auto);
            return component;
        }

        public static T WidthPixels<T>(this T component, double size) where T : IComponent
        {
            Stack.SetWidth(component, Stack.ItemSizeType.Pixels, size);
            return component;
        }

        public static T WidthPercents<T>(this T component, double size) where T : IComponent
        {
            Stack.SetWidth(component, Stack.ItemSizeType.Percents, size);
            return component;
        }

        public static T WidthStretch<T>(this T component) where T : IComponent
        {
            Stack.SetWidth(component, Stack.ItemSizeType.Percents, 100);
            return component;
        }

        public static T HeightAuto<T>(this T component) where T : IComponent
        {
            Stack.SetHeight(component, Stack.ItemSizeType.Auto);
            return component;
        }

        public static T HeightPixels<T>(this T component, double size) where T : IComponent
        {
            Stack.SetHeight(component, Stack.ItemSizeType.Pixels, size);
            return component;
        }

        public static T HeightPercents<T>(this T component, double size) where T : IComponent
        {
            Stack.SetHeight(component, Stack.ItemSizeType.Percents, size);
            return component;
        }

        public static T HeightStretch<T>(this T component) where T : IComponent
        {
            Stack.SetHeight(component, Stack.ItemSizeType.Percents, 100);
            return component;
        }

        public static T MinHeightStretch<T>(this T component) where T : IComponent
        {
            Stack.SetMinHeight(component, Stack.ItemSizeType.Percents, 100);
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

        #endregion
    }
}
