using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// A Stack is a container-type component that abstracts the implementation of a flexbox in order to define the layout of its children components.
    /// </summary>
    public class Stack : IContainer<Stack, IComponent>, IHasBackgroundColor, IHasMarginPadding, ISpecialCaseStyling, IHaveTextWrappingOptions
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
                switch (value)
                {
                    case Orientation.Horizontal: InnerElement.style.flexDirection = "row"; break;
                    case Orientation.Vertical: InnerElement.style.flexDirection = "column"; break;
                    case Orientation.HorizontalReverse: InnerElement.style.flexDirection = "row-reverse"; break;
                    case Orientation.VerticalReverse: InnerElement.style.flexDirection = "column-reverse"; break;
                }
            }
        }

        public bool CanWrap
        {
            get => InnerElement.style.flexWrap != "nowrap";
            set => InnerElement.style.flexWrap = value ? "wrap" : "nowrap";
        }

        public bool IsInline
        {
            get => InnerElement.style.display == "inline-flex";
            set => InnerElement.style.display = value ? "inline-flex" : "";
        }

        public HTMLElement InnerElement { get; private set; }
        public string Background { get => InnerElement.style.background; set => InnerElement.style.background = value; }
        public string Margin { get => InnerElement.style.margin; set => InnerElement.style.margin = value; }
        public string Padding { get => InnerElement.style.padding; set => InnerElement.style.padding = value; }

        public HTMLElement StylingContainer => InnerElement;

        public bool PropagateToStackItemParent { get; private set; } = true;

        public static void SetAlign(IComponent component, ItemAlign align)
        {
            var (item, remember) = GetCorrectItemToApplyStyle(component);
            var cssAlign = align.ToString().ToLower();
            if (cssAlign == "end" || cssAlign == "start")
                cssAlign = $"flex-{cssAlign}";
            item.style.alignSelf = cssAlign;
            if (remember)
                item.setAttribute("tss-stk-as", "");
        }

        /// <summary>
        /// Sets the align-items css property for this stack
        /// </summary>
        /// <param name="align"></param>
        /// <returns></returns>
        public Stack AlignItems(ItemAlign align)
        {
            string cssAlign = align.ToString().ToLower();
            if (cssAlign == "end" || cssAlign == "start") cssAlign = $"flex-{cssAlign}";
            InnerElement.style.alignItems = cssAlign;
            return this;
        }

        /// <summary>
        /// Sets the align-items css property for this stack to 'center'
        /// </summary>
        /// <param name="align"></param>
        /// <returns></returns>
        public Stack AlignItemsCenter() => AlignItems(ItemAlign.Center);
        
        /// <summary>
        /// Make this stack relative (i.e. position:relative)
        /// </summary>
        /// <returns></returns>
        public Stack Relative()
        {
            InnerElement.classList.add("tss-relative");
            return this;
        }

        /// <summary>
        /// Sets the align-items css property for this stack
        /// </summary>
        /// <param name="align"></param>
        /// <returns></returns>
        public Stack AlignContent(ItemAlign align)
        {
            string cssAlign = align.ToString().ToLower();
            if (cssAlign == "end" || cssAlign == "start") cssAlign = $"flex-{cssAlign}";
            InnerElement.style.alignContent = cssAlign;
            return this;
        }

        /// <summary>
        /// Sets the justify-content css property for this stack
        /// </summary>
        /// <param name="justify"></param>
        /// <returns></returns>
        public Stack JustifyContent(ItemJustify justify)
        {
            string cssJustify = justify.ToString().ToLower();
            if (cssJustify == "end" || cssJustify == "start") cssJustify = $"flex-{cssJustify}";
            if (cssJustify == "between" || cssJustify == "around" || cssJustify == "evenly") cssJustify = $"space-{cssJustify}";
            InnerElement.style.justifyContent = cssJustify;
            return this;
        }

        /// <summary>
        /// Sets the justify-content css property for this stack
        /// </summary>
        /// <param name="justify"></param>
        /// <returns></returns>
        public Stack JustifyItems(ItemJustify justify)
        {
            string cssJustify = justify.ToString().ToLower();
            if (cssJustify == "end" || cssJustify == "start") cssJustify = $"flex-{cssJustify}";
            if (cssJustify == "between" || cssJustify == "around" || cssJustify == "evenly") cssJustify = $"space-{cssJustify}";
            InnerElement.style.justifyItems = cssJustify;
            return this;
        }

        public static void SetWidth(IComponent component, UnitSize unitSize)
        {
            var (item, remember) = GetCorrectItemToApplyStyle(component);
            item.style.width = unitSize.ToString();
            if (remember) item.setAttribute("tss-stk-w", "");
        }

        internal static (HTMLElement item, bool remember) GetCorrectItemToApplyStyle(IComponent component)
        {
            if (component is ISpecialCaseStyling specialCase)
            {
                return (specialCase.StylingContainer, specialCase.PropagateToStackItemParent);
            }
            else
            {
                return (GetItem(component), true);
            }
        }

        public Stack RemovePropagation()
        {
            PropagateToStackItemParent = false;
            return this;
        }

        public static void SetMinWidth(IComponent component, UnitSize unitSize)
        {
            var (item, remember) = GetCorrectItemToApplyStyle(component);
            item.style.minWidth = unitSize.ToString();
            if (remember) item.setAttribute("tss-stk-mw", "");
        }

        public static void SetMaxWidth(IComponent component, UnitSize unitSize)
        {
            var (item, remember) = GetCorrectItemToApplyStyle(component);
            item.style.maxWidth = unitSize.ToString();
            if (remember) item.setAttribute("tss-stk-mxw", "");
        }

        public static void SetHeight(IComponent component, UnitSize unitSize)
        {
            var (item, remember) = GetCorrectItemToApplyStyle(component);
            item.style.height = unitSize.ToString();
            if (remember) item.setAttribute("tss-stk-h", "");
        }

        public static void SetMinHeight(IComponent component, UnitSize unitSize)
        {
            var (item, remember) = GetCorrectItemToApplyStyle(component);
            item.style.minHeight = unitSize.ToString();
            if (remember) item.setAttribute("tss-stk-mh", "");
        }

        public static void SetMaxHeight(IComponent component, UnitSize unitSize)
        {
            var (item, remember) = GetCorrectItemToApplyStyle(component);
            item.style.maxHeight = unitSize.ToString();
            if (remember) item.setAttribute("tss-stk-mxh", "");
        }

        public static void SetMarginLeft(IComponent component, UnitSize unitSize)
        {
            var (item, remember) = GetCorrectItemToApplyStyle(component);
            item.style.marginLeft = unitSize.ToString();
            if (remember) item.setAttribute("tss-stk-m", "");
        }

        public static void SetMarginRight(IComponent component, UnitSize unitSize)
        {
            var (item, remember) = GetCorrectItemToApplyStyle(component);
            item.style.marginRight = unitSize.ToString();
            if (remember) item.setAttribute("tss-stk-m", "");
        }

        public static void SetMarginTop(IComponent component, UnitSize unitSize)
        {
            var (item, remember) = GetCorrectItemToApplyStyle(component);
            item.style.marginTop = unitSize.ToString();
            if (remember) item.setAttribute("tss-stk-m", "");
        }

        public static void SetMarginBottom(IComponent component, UnitSize unitSize)
        {
            var (item, remember) = GetCorrectItemToApplyStyle(component);
            item.style.marginBottom = unitSize.ToString();
            if (remember) item.setAttribute("tss-stk-m", "");
        }

        public static void SetPaddingLeft(IComponent component, UnitSize unitSize)
        {
            var (item, remember) = GetCorrectItemToApplyStyle(component);
            item.style.paddingLeft = unitSize.ToString();
            if (remember) item.setAttribute("tss-stk-p", "");
        }

        public static void SetPaddingRight(IComponent component, UnitSize unitSize)
        {
            var (item, remember) = GetCorrectItemToApplyStyle(component);
            item.style.paddingRight = unitSize.ToString();
            if (remember) item.setAttribute("tss-stk-p", "");
        }

        public static void SetPaddingTop(IComponent component, UnitSize unitSize)
        {
            var (item, remember) = GetCorrectItemToApplyStyle(component);
            item.style.paddingTop = unitSize.ToString();
            if (remember) item.setAttribute("tss-stk-p", "");
        }

        public static void SetPaddingBottom(IComponent component, UnitSize unitSize)
        {
            var (item, remember) = GetCorrectItemToApplyStyle(component);
            item.style.paddingBottom = unitSize.ToString();
            if (remember) item.setAttribute("tss-stk-p", "");
        }

        public static void SetGrow(IComponent component, int grow)
        {
            var (item, remember) = GetCorrectItemToApplyStyle(component);
            item.style.flexGrow = grow.ToString();
            if (remember) item.setAttribute("tss-stk-fg", "");
        }

        public static void SetShrink(IComponent component, bool shrink)
        {
            var (item, remember) = GetCorrectItemToApplyStyle(component);
            item.style.flexShrink = shrink ? "1" : "0";
            if (remember) item.setAttribute("tss-stk-fs", "");
        }

        public Stack(Orientation orientation = Orientation.Vertical)
        {
            InnerElement = Div(_("tss-stack"));
            StackOrientation = orientation;
        }
        private event ComponentEventHandler<Stack, Event> MouseOver;
        private event ComponentEventHandler<Stack, Event> MouseOut;

        private void RaiseMouseOver(Event ev) => MouseOver?.Invoke((Stack) this, ev);
        private void RaiseMouseOut(Event  ev) => MouseOut?.Invoke((Stack) this, ev);

        public Stack OnMouseOver(ComponentEventHandler<Stack, Event> onMouseOver)
        {
            if (!(InnerElement.onmouseover is object))
            {
                InnerElement.onmouseover += s => RaiseMouseOver(s);
            }

            MouseOver += onMouseOver;
            return (Stack) this;
        }

        public Stack OnMouseOut(ComponentEventHandler<Stack, Event> onMouseOut)
        {
            if (!(InnerElement.onmouseout is object))
            {
                InnerElement.onmouseout += s => RaiseMouseOut(s);
            }

            MouseOut += onMouseOut;
            return (Stack) this;
        }

        public void Add(IComponent component) => ScrollBar.GetCorrectContainer(InnerElement).appendChild(GetItem(component, true));

        public void Prepend(IComponent component)
        {
            var container = ScrollBar.GetCorrectContainer(InnerElement);
            
            if(container.childElementCount > 0)
            {
                container.insertBefore(GetItem(component, true), container.firstElementChild);
            }
            else
            {
                container.appendChild(GetItem(component, true));
            }
        }

        public virtual void Clear() => ClearChildren(ScrollBar.GetCorrectContainer(InnerElement));

        public void Replace(IComponent newComponent, IComponent oldComponent) => ScrollBar.GetCorrectContainer(InnerElement).replaceChild(GetItem(newComponent), GetItem(oldComponent));
        public void Remove(IComponent component) => ScrollBar.GetCorrectContainer(InnerElement).removeChild(GetItem(component));

        public virtual HTMLElement Render() => InnerElement;

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

        public Stack Inline()
        {
            IsInline = true;
            return this;
        }

        public Stack NoWrap()
        {
            CanWrap = false;
            return this;
        }

        public Stack OverflowHidden()
        {
            InnerElement.style.overflow = "hidden";
            return this;
        }

        internal static HTMLElement GetItem(IComponent component, bool forceAdd = false)
        {
            HTMLElement item = null;
            if(component.HasOwnProperty("StackItem"))
            {
                item = component["StackItem"] as HTMLElement;
            }

            if (item is null)
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
                    component["StackItem"] = item;

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

        internal static void CopyStylesDefinedWithExtension(HTMLElement from, HTMLElement to)
        {
            // RFO: this class does some magic to move any styles applied to an element using the extensions methods like Width, etc... to the actual StackItem HTML element
            // so that they're relevant on the flex-box and not only inside of each child item of the flexbox

            var fs = from.style;
            var ts = to.style;

            bool has(string att)
            {
                bool ha = from.hasAttribute(att);
                if (ha)
                {
                    from.removeAttribute(att);
                }
                return ha;
            }

            if (has("tss-stk-w")) { ts.width = fs.width; fs.width = "100%"; }
            if (has("tss-stk-mw")) { ts.minWidth = fs.minWidth; fs.minWidth = "100%"; }
            if (has("tss-stk-mxw")) { ts.maxWidth = fs.maxWidth; fs.maxWidth = "100%"; }
            if (has("tss-stk-h")) { ts.height = fs.height; fs.height = "100%"; }
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

            if (from.classList.contains("tss-default-component-margin"))
            {
                from.classList.remove("tss-default-component-margin");
                to.classList.add("tss-default-component-margin");
            }
        }

        public enum Orientation
        {
            Vertical,
            Horizontal,
            VerticalReverse,
            HorizontalReverse,
        }

        public struct ItemSize
        {
            public Unit Type { get; set; }
            public float Value { get; set; }
        }
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

    public enum ItemJustify
    {
        Between,
        Around,
        Evenly,
        Start,
        Center,
        End
    }
}
