using System;
using System.Collections.Generic;
using static Tesserae.HTML.HtmlUtil;
using static Tesserae.HTML.HtmlAttributes;
using static Retyped.dom;

namespace Tesserae.Components
{
    public enum StackOrientation
    {
        Vertical,
        Horizontal,
        VerticalReverse,
        HorizontalReverse,
    }

    public enum StackItemAlign
    {
        Auto,
        Stretch,
        Baseline,
        Start,
        Center,
        End
    }

    public enum StackItemSizeType
    {
        Auto,
        Percents,
        Pixels
    }

    public struct StackItemSize
    {
        public StackItemSizeType Type { get; set; }
        public double Value { get; set; }
    }

    public class Stack : IContainer<Stack>
    {
        #region Properties

        public StackOrientation Orientation
        {
            get
            {
                switch (InnerElement.style.flexDirection)
                {
                    case "row": return StackOrientation.Horizontal;
                    case "column": return StackOrientation.Vertical;
                    case "row-reverse": return StackOrientation.HorizontalReverse;
                    case "column-reverse": return StackOrientation.VerticalReverse;
                }

                return StackOrientation.Vertical;
            }
            set
            {
                if (value != Orientation)
                {
                    switch (value)
                    {
                        case StackOrientation.Horizontal: InnerElement.style.flexDirection = "row"; break;
                        case StackOrientation.Vertical: InnerElement.style.flexDirection = "column"; break;
                        case StackOrientation.HorizontalReverse: InnerElement.style.flexDirection = "row-reverse"; break;
                        case StackOrientation.VerticalReverse: InnerElement.style.flexDirection = "column-reverse"; break;
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

        #endregion

        #region Static

        private static Dictionary<IComponent, HTMLDivElement> _ItemsDictionary = new Dictionary<IComponent, HTMLDivElement>();

        private static HTMLDivElement GetItem(IComponent component)
        {
            if (_ItemsDictionary.ContainsKey(component)) return _ItemsDictionary[component];
            else
            {
                var item = Div(_("mss-stack-item", styles: s =>
                {
                    s.alignSelf = "auto";
                    s.width = "auto";
                    s.height = "auto";
                    s.flexShrink = "1";
                }), component.Render());
                _ItemsDictionary.Add(component, item);
                return item;
            }
        }

        #region Items Properties

        public static StackItemAlign GetAlign(IComponent component)
        {
            var item = GetItem(component);
            if (Enum.TryParse<StackItemAlign>(item.style.alignSelf.Replace("flex-", ""), true, out var result)) return result;
            throw new Exception("Incorrect Stack item align.");
        }

        public static void SetAlign(IComponent component, StackItemAlign align)
        {
            var item = GetItem(component);
            string cssAlign = align.ToString().ToLower();
            if (cssAlign == "end" || cssAlign == "start") cssAlign = $"flex-{cssAlign}";
            item.style.alignSelf = cssAlign;
        }

        public static StackItemSize GetWidth(IComponent component)
        {
            var item = GetItem(component);
            if (item.style.width == "auto") return new StackItemSize() { Type = StackItemSizeType.Auto };
            if (item.style.width.EndsWith("px")) return new StackItemSize() { Type = StackItemSizeType.Pixels, Value = double.Parse(item.style.width.Substring(item.style.width.Length - 2)) };
            if (item.style.width.EndsWith("%")) return new StackItemSize() { Type = StackItemSizeType.Percents, Value = double.Parse(item.style.width.Substring(item.style.width.Length - 1)) };

            throw new Exception("Incorrect Stack item width.");
        }
        public static void SetWidth(IComponent component, StackItemSizeType sizeType, double size = 0)
        {
            var item = GetItem(component);
            switch (sizeType)
            {
                case StackItemSizeType.Auto: item.style.width = "auto"; break;
                case StackItemSizeType.Pixels: item.style.width = $"{size:0.####}px"; break;
                case StackItemSizeType.Percents: item.style.width = $"{size:0.####}%"; break;
            }
        }

        public static StackItemSize GetHeight(IComponent component)
        {
            var item = GetItem(component);
            if (item.style.height == "auto") return new StackItemSize() { Type = StackItemSizeType.Auto };
            if (item.style.height.EndsWith("px")) return new StackItemSize() { Type = StackItemSizeType.Pixels, Value = double.Parse(item.style.height.Substring(item.style.height.Length - 2)) };
            if (item.style.height.EndsWith("%")) return new StackItemSize() { Type = StackItemSizeType.Percents, Value = double.Parse(item.style.height.Substring(item.style.height.Length - 1)) };

            throw new Exception("Incorrect Stack item height.");
        }
        public static void SetHeight(IComponent component, StackItemSizeType sizeType, double size = 0)
        {
            var item = GetItem(component);
            switch (sizeType)
            {
                case StackItemSizeType.Auto: item.style.height = "auto"; break;
                case StackItemSizeType.Pixels: item.style.height = $"{size:0.####}px"; break;
                case StackItemSizeType.Percents: item.style.height = $"{size:0.####}%"; break;
            }
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

        public Stack(StackOrientation orientation = StackOrientation.Vertical)
        {
            InnerElement = Div(_("mss-stack"));
            Orientation = orientation;
        }

        public void Add(IComponent component)
        {
            InnerElement.appendChild(GetItem(component));
        }

        public void Clear()
        {
            ClearChildren(InnerElement);
        }

        public void Replace(IComponent newComponent, IComponent oldComponent)
        {
            InnerElement.replaceChild(GetItem(newComponent), GetItem(oldComponent));
        }

        public HTMLElement Render()
        {
            return InnerElement;
        }
    }

    public static class StackExtensions
    {
        public static Stack Horizontal(this Stack stack)
        {
            stack.Orientation = StackOrientation.Horizontal;
            return stack;
        }

        public static Stack Vertical(this Stack stack)
        {
            stack.Orientation = StackOrientation.Vertical;
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

        public static IComponent AlignAuto(this IComponent component)
        {
            Stack.SetAlign(component, StackItemAlign.Auto);
            return component;
        }

        public static IComponent AlignStretch(this IComponent component)
        {
            Stack.SetAlign(component, StackItemAlign.Stretch);
            return component;
        }
        public static IComponent AlignBaseline(this IComponent component)
        {
            Stack.SetAlign(component, StackItemAlign.Baseline);
            return component;
        }
        public static IComponent AlignStart(this IComponent component)
        {
            Stack.SetAlign(component, StackItemAlign.Start);
            return component;
        }
        public static IComponent AlignCenter(this IComponent component)
        {
            Stack.SetAlign(component, StackItemAlign.Center);
            return component;
        }
        public static IComponent AlignEnd(this IComponent component)
        {
            Stack.SetAlign(component, StackItemAlign.End);
            return component;
        }

        public static IComponent WidthAuto(this IComponent component)
        {
            Stack.SetWidth(component, StackItemSizeType.Auto);
            return component;
        }

        public static IComponent WidthPixels(this IComponent component, double size)
        {
            Stack.SetWidth(component, StackItemSizeType.Pixels, size);
            return component;
        }
        public static IComponent WidthPercents(this IComponent component, double size)
        {
            Stack.SetWidth(component, StackItemSizeType.Percents, size);
            return component;
        }

        public static IComponent WidthStretch(this IComponent component)
        {
            Stack.SetWidth(component, StackItemSizeType.Percents, 100);
            return component;
        }

        public static IComponent HeightAuto(this IComponent component)
        {
            Stack.SetHeight(component, StackItemSizeType.Auto);
            return component;
        }
        public static IComponent HeightPixels(this IComponent component, double size)
        {
            Stack.SetHeight(component, StackItemSizeType.Pixels, size);
            return component;
        }
        public static IComponent HeightPercents(this IComponent component, double size)
        {
            Stack.SetHeight(component, StackItemSizeType.Percents, size);
            return component;
        }
        public static IComponent HeightStretch(this IComponent component)
        {
            Stack.SetHeight(component, StackItemSizeType.Percents, 100);
            return component;
        }

        public static IComponent Grow(this IComponent component, int grow)
        {
            Stack.SetGrow(component, grow);
            return component;
        }

        public static IComponent Shrink(this IComponent component)
        {
            Stack.SetShrink(component, true);
            return component;
        }

        public static IComponent NoShrink(this IComponent component)
        {
            Stack.SetShrink(component, false);
            return component;
        }

        #endregion
    }
}
