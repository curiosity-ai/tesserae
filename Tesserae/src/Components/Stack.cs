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

    public enum StackItemSize/*StackItemSizeType*/
    {
        Auto,
        Stretch
        // Percents,
        // Pixels
    }

    //public struct StackItemSize
    //{
    //    public StackItemSizeType Type { get; set; }
    //    public double Value { get; set; }
    //}

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
            switch (item.style.width)
            {
                case "auto": return StackItemSize.Auto;
                case "100%": return StackItemSize.Stretch;
            }
            throw new Exception("Incorrect Stack item width.");
        }
        public static void SetWidth(IComponent component, StackItemSize size)
        {
            var item = GetItem(component);
            item.style.width = size == StackItemSize.Auto ? "auto" : "100%";
        }

        public static StackItemSize GetHeight(IComponent component)
        {
            var item = GetItem(component);
            switch (item.style.height)
            {
                case "auto": return StackItemSize.Auto;
                case "100%": return StackItemSize.Stretch;
            }
            throw new Exception("Incorrect Stack item height.");
        }
        public static void SetHeight(IComponent component, StackItemSize size)
        {
            var item = GetItem(component);
            item.style.height = size == StackItemSize.Auto ? "auto" : "100%";
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
            Stack.SetWidth(component, StackItemSize.Auto);
            return component;
        }

        public static IComponent WidthStretch(this IComponent component)
        {
            Stack.SetWidth(component, StackItemSize.Stretch);
            return component;
        }

        public static IComponent HeightAuto(this IComponent component)
        {
            Stack.SetHeight(component, StackItemSize.Auto);
            return component;
        }

        public static IComponent HeightStretch(this IComponent component)
        {
            Stack.SetHeight(component, StackItemSize.Stretch);
            return component;
        }

        #endregion
    }
}
