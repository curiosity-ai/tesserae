using H5;
using System;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// A Stack is a container-type component that abstracts the implementation of a flexbox in order to define the layout of its children components.
    /// </summary>
    [H5.Name("tss.S")]
    public class Stack : IContainer<Stack, IComponent>, IHasBackgroundColor, IHasMarginPadding, ISpecialCaseStyling, ICanWrap
    {
        /// <summary>
        /// Gets or sets the stack orientation.
        /// </summary>
        public Orientation StackOrientation
        {
            get
            {
                switch (InnerElement.style.flexDirection)
                {
                    case "row":            return Orientation.Horizontal;
                    case "column":         return Orientation.Vertical;
                    case "row-reverse":    return Orientation.HorizontalReverse;
                    case "column-reverse": return Orientation.VerticalReverse;
                }

                return Orientation.Vertical;
            }
            set
            {
                switch (value)
                {
                    case Orientation.Horizontal:
                        InnerElement.style.flexDirection = "row";
                        break;
                    case Orientation.Vertical:
                        InnerElement.style.flexDirection = "column";
                        break;
                    case Orientation.HorizontalReverse:
                        InnerElement.style.flexDirection = "row-reverse";
                        break;
                    case Orientation.VerticalReverse:
                        InnerElement.style.flexDirection = "column-reverse";
                        break;
                }
            }
        }

        /// <summary>
        /// Gets or sets whether the stack can wrap its children.
        /// </summary>
        public bool CanWrap
        {
            get => InnerElement.style.flexWrap != "nowrap";
            set => InnerElement.style.flexWrap = value ? "wrap" : "nowrap";
        }

        /// <summary>
        /// Gets or sets whether the stack is displayed as an inline-flex.
        /// </summary>
        public bool IsInline
        {
            get => InnerElement.style.display == "inline-flex";
            set => InnerElement.style.display = value ? "inline-flex" : "";
        }

        /// <summary>Gets the inner element.</summary>
        public HTMLElement InnerElement { get;                                  private set; }
        /// <summary>Gets or sets the background color.</summary>
        public string      Background   { get => InnerElement.style.background; set => InnerElement.style.background = value; }
        /// <summary>Gets or sets the margin.</summary>
        public string      Margin       { get => InnerElement.style.margin;     set => InnerElement.style.margin = value; }
        /// <summary>Gets or sets the padding.</summary>
        public string      Padding      { get => InnerElement.style.padding;    set => InnerElement.style.padding = value; }

        /// <summary>Gets the styling container.</summary>
        public HTMLElement StylingContainer => InnerElement;

        /// <summary>Gets or sets whether to propagate styling to the stack item parent.</summary>
        public bool PropagateToStackItemParent { get; private set; } = true;

        /// <summary>
        /// Sets the alignment for a component within a stack.
        /// </summary>
        /// <param name="component">The component.</param>
        /// <param name="align">The alignment.</param>
        public static void SetAlign(IComponent component, ItemAlign align)
        {
            var (item, remember) = GetCorrectItemToApplyStyle(component);
            var cssAlign = align.ToString();

            if (cssAlign == "end" || cssAlign == "start")
            {
                cssAlign = $"flex-{cssAlign}";
            }

            item.style.alignSelf = cssAlign;

            if (remember)
            {
                item.setAttribute("tss-stk-as", "");

                if (component.HasOwnProperty("StackItem"))
                {
                    component["StackItem"].As<HTMLElement>().style.alignSelf = item.style.alignSelf;
                }
                else if (component.HasOwnProperty("GridItem"))
                {
                    component["GridItem"].As<HTMLElement>().style.justifySelf = item.style.justifySelf;
                }
            }
        }

        /// <summary>
        /// Sets the justification for a component within a stack.
        /// </summary>
        /// <param name="component">The component.</param>
        /// <param name="align">The justification.</param>
        public static void SetJustify(IComponent component, ItemJustify align)
        {
            var (item, remember) = GetCorrectItemToApplyStyle(component);
            var cssAlign = align.ToString();

            if (cssAlign == "end" || cssAlign == "start")
            {
                cssAlign = $"flex-{cssAlign}";
            }

            item.style.justifySelf = cssAlign;

            if (remember)
            {
                item.setAttribute("tss-stk-js", "");

                if (component.HasOwnProperty("StackItem"))
                {
                    component["StackItem"].As<HTMLElement>().style.justifySelf = item.style.justifySelf;
                }
                else if (component.HasOwnProperty("GridItem"))
                {
                    component["GridItem"].As<HTMLElement>().style.justifySelf = item.style.justifySelf;
                }
            }
        }

        /// <summary>
        /// Sets the align-items CSS property for this stack.
        /// </summary>
        /// <param name="align">The alignment.</param>
        /// <returns>The current instance.</returns>
        public Stack AlignItems(ItemAlign align)
        {
            string cssAlign                                        = align.ToString();
            if (cssAlign == "end" || cssAlign == "start") cssAlign = $"flex-{cssAlign}";
            InnerElement.style.alignItems = cssAlign;
            return this;
        }

        /// <summary>
        /// Sets the align-items CSS property for this stack to 'center'.
        /// </summary>
        /// <returns>The current instance.</returns>
        public Stack AlignItemsCenter() => AlignItems(ItemAlign.Center);

        /// <summary>
        /// Make this stack relative (i.e. position:relative).
        /// </summary>
        /// <returns>The current instance.</returns>
        public Stack Relative()
        {
            InnerElement.classList.add("tss-relative");
            return this;
        }

        /// <summary>
        /// Sets the align-content CSS property for this stack.
        /// </summary>
        /// <param name="align">The alignment.</param>
        /// <returns>The current instance.</returns>
        public Stack AlignContent(ItemAlign align)
        {
            string cssAlign                                        = align.ToString();
            if (cssAlign == "end" || cssAlign == "start") cssAlign = $"flex-{cssAlign}";
            InnerElement.style.alignContent = cssAlign;
            return this;
        }

        /// <summary>
        /// Sets the justify-content CSS property for this stack.
        /// </summary>
        /// <param name="justify">The justification.</param>
        /// <returns>The current instance.</returns>
        public Stack JustifyContent(ItemJustify justify)
        {
            string cssJustify                                                                           = justify.ToString();
            if (cssJustify == "end"     || cssJustify == "start") cssJustify                            = $"flex-{cssJustify}";
            if (cssJustify == "between" || cssJustify == "around" || cssJustify == "evenly") cssJustify = $"space-{cssJustify}";
            InnerElement.style.justifyContent = cssJustify;
            return this;
        }

        /// <summary>
        /// Sets the justify-items CSS property for this stack.
        /// </summary>
        /// <param name="justify">The justification.</param>
        /// <returns>The current instance.</returns>
        public Stack JustifyItems(ItemJustify justify)
        {
            string cssJustify                                                                           = justify.ToString();
            if (cssJustify == "end"     || cssJustify == "start") cssJustify                            = $"flex-{cssJustify}";
            if (cssJustify == "between" || cssJustify == "around" || cssJustify == "evenly") cssJustify = $"space-{cssJustify}";
            InnerElement.style.justifyItems = cssJustify;
            return this;
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

        /// <summary>
        /// Disables propagation of styling to the stack item parent.
        /// </summary>
        /// <returns>The current instance.</returns>
        public Stack RemovePropagation()
        {
            PropagateToStackItemParent = false;
            return this;
        }

        /// <summary>Sets the width of a component within a stack.</summary>
        public static void SetWidth(IComponent component, UnitSize unitSize)
        {
            var (item, remember) = GetCorrectItemToApplyStyle(component);
            item.style.width     = unitSize.ToString();

            if (remember)
            {
                item.setAttribute("tss-stk-w", "");

                if (component.HasOwnProperty("StackItem"))
                {
                    component["StackItem"].As<HTMLElement>().style.width = item.style.width;
                }
            }
        }

        /// <summary>Sets the minimum width of a component within a stack.</summary>
        public static void SetMinWidth(IComponent component, UnitSize unitSize)
        {
            var (item, remember) = GetCorrectItemToApplyStyle(component);
            item.style.minWidth  = unitSize.ToString();

            if (remember)
            {
                item.setAttribute("tss-stk-mw", "");

                if (component.HasOwnProperty("StackItem"))
                {
                    component["StackItem"].As<HTMLElement>().style.minWidth = item.style.minWidth;
                }
            }
        }

        /// <summary>Sets the maximum width of a component within a stack.</summary>
        public static void SetMaxWidth(IComponent component, UnitSize unitSize)
        {
            var (item, remember) = GetCorrectItemToApplyStyle(component);
            item.style.maxWidth  = unitSize.ToString();

            if (remember)
            {
                item.setAttribute("tss-stk-mxw", "");

                if (component.HasOwnProperty("StackItem"))
                {
                    component["StackItem"].As<HTMLElement>().style.maxWidth = item.style.maxWidth;
                }
            }
        }

        /// <summary>Sets the height of a component within a stack.</summary>
        public static void SetHeight(IComponent component, UnitSize unitSize)
        {
            var (item, remember) = GetCorrectItemToApplyStyle(component);
            item.style.height    = unitSize.ToString();

            if (remember)
            {
                item.setAttribute("tss-stk-h", "");

                if (component.HasOwnProperty("StackItem"))
                {
                    component["StackItem"].As<HTMLElement>().style.height = item.style.height;
                }
            }
        }

        /// <summary>Sets the minimum height of a component within a stack.</summary>
        public static void SetMinHeight(IComponent component, UnitSize unitSize)
        {
            var (item, remember) = GetCorrectItemToApplyStyle(component);
            item.style.minHeight = unitSize.ToString();

            if (remember)
            {
                item.setAttribute("tss-stk-mh", "");

                if (component.HasOwnProperty("StackItem"))
                {
                    component["StackItem"].As<HTMLElement>().style.minHeight = item.style.minHeight;
                }
            }
        }

        /// <summary>Sets the maximum height of a component within a stack.</summary>
        public static void SetMaxHeight(IComponent component, UnitSize unitSize)
        {
            var (item, remember) = GetCorrectItemToApplyStyle(component);
            item.style.maxHeight = unitSize.ToString();

            if (remember)
            {
                item.setAttribute("tss-stk-mxh", "");

                if (component.HasOwnProperty("StackItem"))
                {
                    component["StackItem"].As<HTMLElement>().style.maxHeight = item.style.maxHeight;
                }
            }
        }

        /// <summary>Sets the left margin of a component within a stack.</summary>
        public static void SetMarginLeft(IComponent component, UnitSize unitSize)
        {
            var (item, remember)  = GetCorrectItemToApplyStyle(component);
            item.style.marginLeft = unitSize.ToString();

            if (remember)
            {
                item.setAttribute("tss-stk-m", "");

                if (component.HasOwnProperty("StackItem"))
                {
                    component["StackItem"].As<HTMLElement>().style.marginLeft = item.style.marginLeft;
                }
            }
        }

        /// <summary>Sets the right margin of a component within a stack.</summary>
        public static void SetMarginRight(IComponent component, UnitSize unitSize)
        {
            var (item, remember)   = GetCorrectItemToApplyStyle(component);
            item.style.marginRight = unitSize.ToString();

            if (remember)
            {
                item.setAttribute("tss-stk-m", "");

                if (component.HasOwnProperty("StackItem"))
                {
                    component["StackItem"].As<HTMLElement>().style.marginRight = item.style.marginRight;
                }
            }
        }

        /// <summary>Sets the top margin of a component within a stack.</summary>
        public static void SetMarginTop(IComponent component, UnitSize unitSize)
        {
            var (item, remember) = GetCorrectItemToApplyStyle(component);
            item.style.marginTop = unitSize.ToString();

            if (remember)
            {
                item.setAttribute("tss-stk-m", "");

                if (component.HasOwnProperty("StackItem"))
                {
                    component["StackItem"].As<HTMLElement>().style.marginTop = item.style.marginTop;
                }
            }
        }

        /// <summary>Sets the bottom margin of a component within a stack.</summary>
        public static void SetMarginBottom(IComponent component, UnitSize unitSize)
        {
            var (item, remember)    = GetCorrectItemToApplyStyle(component);
            item.style.marginBottom = unitSize.ToString();

            if (remember)
            {
                item.setAttribute("tss-stk-m", "");

                if (component.HasOwnProperty("StackItem"))
                {
                    component["StackItem"].As<HTMLElement>().style.marginBottom = item.style.marginBottom;
                }
            }
        }

        /// <summary>Sets the left padding of a component within a stack.</summary>
        public static void SetPaddingLeft(IComponent component, UnitSize unitSize)
        {
            var (item, remember)   = GetCorrectItemToApplyStyle(component);
            item.style.paddingLeft = unitSize.ToString();

            if (remember)
            {
                item.setAttribute("tss-stk-p", "");

                if (component.HasOwnProperty("StackItem"))
                {
                    component["StackItem"].As<HTMLElement>().style.paddingLeft = item.style.paddingLeft;
                }
            }
        }

        /// <summary>Sets the right padding of a component within a stack.</summary>
        public static void SetPaddingRight(IComponent component, UnitSize unitSize)
        {
            var (item, remember)    = GetCorrectItemToApplyStyle(component);
            item.style.paddingRight = unitSize.ToString();

            if (remember)
            {
                item.setAttribute("tss-stk-p", "");

                if (component.HasOwnProperty("StackItem"))
                {
                    component["StackItem"].As<HTMLElement>().style.paddingRight = item.style.paddingRight;
                }
            }
        }

        /// <summary>Sets the top padding of a component within a stack.</summary>
        public static void SetPaddingTop(IComponent component, UnitSize unitSize)
        {
            var (item, remember)  = GetCorrectItemToApplyStyle(component);
            item.style.paddingTop = unitSize.ToString();

            if (remember)
            {
                item.setAttribute("tss-stk-p", "");

                if (component.HasOwnProperty("StackItem"))
                {
                    component["StackItem"].As<HTMLElement>().style.paddingTop = item.style.paddingTop;
                }
            }

        }

        /// <summary>Sets the bottom padding of a component within a stack.</summary>
        public static void SetPaddingBottom(IComponent component, UnitSize unitSize)
        {
            var (item, remember)     = GetCorrectItemToApplyStyle(component);
            item.style.paddingBottom = unitSize.ToString();

            if (remember)
            {
                item.setAttribute("tss-stk-p", "");

                if (component.HasOwnProperty("StackItem"))
                {
                    component["StackItem"].As<HTMLElement>().style.paddingBottom = item.style.paddingBottom;
                }
            }

        }

        /// <summary>Sets the flex-grow of a component within a stack.</summary>
        public static void SetGrow(IComponent component, int grow)
        {
            var (item, remember) = GetCorrectItemToApplyStyle(component);
            item.style.flexGrow  = grow.ToString();

            if (remember)
            {
                item.setAttribute("tss-stk-fg", "");

                if (component.HasOwnProperty("StackItem"))
                {
                    component["StackItem"].As<HTMLElement>().style.flexGrow = item.style.flexGrow;
                }
            }
        }

        /// <summary>Sets the flex-shrink of a component within a stack.</summary>
        public static void SetShrink(IComponent component, bool shrink)
        {
            var (item, remember)  = GetCorrectItemToApplyStyle(component);
            item.style.flexShrink = shrink ? "1" : "0";

            if (remember)
            {
                item.setAttribute("tss-stk-fs", "");

                if (component.HasOwnProperty("StackItem"))
                {
                    component["StackItem"].As<HTMLElement>().style.flexShrink = item.style.flexShrink;
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Stack"/> class.
        /// </summary>
        /// <param name="orientation">The stack orientation.</param>
        public Stack(Orientation orientation = Orientation.Vertical)
        {
            InnerElement     = Div(_("tss-stack"));
            StackOrientation = orientation;
        }
        private event ComponentEventHandler<Stack, Event> MouseOver;
        private event ComponentEventHandler<Stack, Event> MouseOut;

        private void RaiseMouseOver(Event ev) => MouseOver?.Invoke((Stack)this, ev);
        private void RaiseMouseOut(Event  ev) => MouseOut?.Invoke((Stack)this, ev);

        /// <summary>
        /// Attaches a handler to the mouse over event.
        /// </summary>
        /// <param name="onMouseOver">The handler.</param>
        /// <returns>The current instance.</returns>
        public Stack OnMouseOver(ComponentEventHandler<Stack, Event> onMouseOver)
        {
            if (!(InnerElement.onmouseover is object))
            {
                InnerElement.onmouseover += s => RaiseMouseOver(s);
            }

            MouseOver += onMouseOver;
            return (Stack)this;
        }

        /// <summary>
        /// Attaches a handler to the mouse out event.
        /// </summary>
        /// <param name="onMouseOut">The handler.</param>
        /// <returns>The current instance.</returns>
        public Stack OnMouseOut(ComponentEventHandler<Stack, Event> onMouseOut)
        {
            if (!(InnerElement.onmouseout is object))
            {
                InnerElement.onmouseout += s => RaiseMouseOut(s);
            }

            MouseOut += onMouseOut;
            return (Stack)this;
        }

        /// <summary>
        /// Adds a component to the stack.
        /// </summary>
        /// <param name="component">The component.</param>
        public void Add(IComponent component) => InnerElement.appendChild(GetItem(component, true));

        /// <summary>
        /// Adds a component to the beginning of the stack.
        /// </summary>
        /// <param name="component">The component.</param>
        public void Prepend(IComponent component)
        {
            var container = InnerElement;

            if (container.childElementCount > 0)
            {
                container.insertBefore(GetItem(component, true), container.firstElementChild);
            }
            else
            {
                container.appendChild(GetItem(component, true));
            }
        }

        /// <summary>
        /// Inserts a component before another component in the stack.
        /// </summary>
        /// <param name="component">The component to insert.</param>
        /// <param name="componentToInsertBefore">The reference component.</param>
        public void InsertBefore(IComponent component, IComponent componentToInsertBefore)
        {
            var container = InnerElement;

            var element               = GetItem(component,               true);
            var elementToInsertBefore = GetItem(componentToInsertBefore, true);

            if (!container.contains(elementToInsertBefore))
            {
                throw new Exception(nameof(componentToInsertBefore) + "is not a child of this stack");
            }

            container.insertBefore(element, elementToInsertBefore);
        }

        /// <summary>
        /// Inserts a component after another component in the stack.
        /// </summary>
        /// <param name="component">The component to insert.</param>
        /// <param name="componentToInsertBefore">The reference component.</param>
        public void InsertAfter(IComponent component, IComponent componentToInsertBefore)
        {
            var container = InnerElement;

            var element               = GetItem(component,               true);
            var elementToInsertBefore = GetItem(componentToInsertBefore, true);

            if (!container.contains(elementToInsertBefore))
            {
                throw new Exception(nameof(componentToInsertBefore) + "is not a child of this stack");
            }

            container.insertBefore(element, elementToInsertBefore.nextSibling);
        }

        /// <summary>
        /// Clears all children from the stack.
        /// </summary>
        public virtual void Clear() => ClearChildren(InnerElement);

        /// <summary>
        /// Replaces a component in the stack.
        /// </summary>
        /// <param name="newComponent">The new component.</param>
        /// <param name="oldComponent">The old component.</param>
        public void Replace(IComponent newComponent, IComponent oldComponent) => InnerElement.replaceChild(GetItem(newComponent), GetItem(oldComponent));

        /// <summary>
        /// Removes a component from the stack.
        /// </summary>
        /// <param name="component">The component.</param>
        public void Remove(IComponent component) => TryRemoveChild(InnerElement, GetItem(component));

        /// <summary>
        /// Renders the component.
        /// </summary>
        /// <returns>The rendered HTML element.</returns>
        public virtual HTMLElement Render() => InnerElement;

        /// <summary>Sets the stack orientation to horizontal.</summary>
        public Stack Horizontal()
        {
            StackOrientation = Stack.Orientation.Horizontal;
            return this;
        }

        /// <summary>Sets the stack orientation to vertical.</summary>
        public Stack Vertical()
        {
            StackOrientation = Stack.Orientation.Vertical;
            return this;
        }

        /// <summary>Sets the stack orientation to horizontal reverse.</summary>
        public Stack HorizontalReverse()
        {
            StackOrientation = Stack.Orientation.HorizontalReverse;
            return this;
        }

        /// <summary>Sets the stack orientation to vertical reverse.</summary>
        public Stack VerticalReverse()
        {
            StackOrientation = Stack.Orientation.VerticalReverse;
            return this;
        }

        /// <summary>Enables wrapping.</summary>
        public Stack Wrap()
        {
            CanWrap = true;
            return this;
        }

        /// <summary>Sets the stack to be inline.</summary>
        public Stack Inline()
        {
            IsInline = true;
            return this;
        }

        /// <summary>Disables wrapping.</summary>
        public Stack NoWrap()
        {
            CanWrap = false;
            return this;
        }

        /// <summary>Sets the overflow to hidden.</summary>
        public Stack OverflowHidden()
        {
            InnerElement.style.overflow = "hidden";
            return this;
        }

        /// <summary>Removes the default component margin.</summary>
        public Stack NoDefaultMargin()
        {
            InnerElement.classList.add("tss-default-component-no-margin");
            return this;
        }

        internal static HTMLElement GetItem(IComponent component, bool forceAdd = false)
        {
            HTMLElement item = null;

            if (component.HasOwnProperty("SectionStackItem"))
            {
                if (forceAdd)
                {
                    H5.Script.Delete(component["SectionStackItem"]);
                }
                else
                {
                    item = component["SectionStackItem"] as HTMLElement;
                }
            }

            if (item is null && component.HasOwnProperty("StackItem"))
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
                        s.alignSelf  = "auto";
                        s.width      = "auto";
                        s.height     = "auto";
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

            if (has("tss-stk-w"))
            {
                ts.width = fs.width;
                fs.width = "100%";
            }

            if (has("tss-stk-h"))
            {
                ts.height = fs.height;
                fs.height = "100%";
            }

            if (has("tss-stk-mw"))
            {
                ts.minWidth = fs.minWidth;
                fs.minWidth = fs.minWidth.Contains("%") ? "100%" : "inherit";
            }

            if (has("tss-stk-mxw"))
            {
                ts.maxWidth = fs.maxWidth;
                fs.maxWidth = fs.maxWidth.Contains("%") ? "100%" : "inherit";
            }

            if (has("tss-stk-mh"))
            {
                ts.minHeight = fs.minHeight;
                fs.minHeight = fs.minHeight.Contains("%") ? "100%" : "inherit";
            }

            if (has("tss-stk-mxh"))
            {
                ts.maxHeight = fs.maxHeight;
                fs.maxHeight = fs.maxHeight.Contains("%") ? "100%" : "inherit";
            }

            if (has("tss-stk-m"))
            {
                ts.marginLeft   = fs.marginLeft;
                ts.marginTop    = fs.marginTop;
                ts.marginRight  = fs.marginRight;
                ts.marginBottom = fs.marginBottom;
                fs.marginLeft   = fs.marginTop = fs.marginRight = fs.marginBottom = "";
            }

            if (has("tss-stk-p"))
            {
                ts.paddingLeft   = fs.paddingLeft;
                ts.paddingTop    = fs.paddingTop;
                ts.paddingRight  = fs.paddingRight;
                ts.paddingBottom = fs.paddingBottom;
                fs.paddingLeft   = fs.paddingTop = fs.paddingRight = fs.paddingBottom = "";
            }

            //TODO: check if should clear this here:
            if (has("tss-stk-fg")) { ts.flexGrow = fs.flexGrow; /*fs.flexGrow = ""; */ }

            if (has("tss-stk-fs")) { ts.flexShrink = fs.flexShrink; /*fs.flexShrink = ""; */ }

            if (has("tss-stk-as")) { ts.alignSelf = fs.alignSelf; /*fs.alignSelf = "";*/ }
            if (has("tss-stk-js")) { ts.justifySelf= fs.justifySelf; /*fs.justifySelf = "";*/ }

            //We need to propagate some styles otherwise they don't work if they were applied before adding to the stack
            foreach (var s in _stylesToPropagate)
            {
                if (from.classList.contains(s))
                {
                    from.classList.remove(s);
                    to.classList.add(s);
                }
            }
        }

        private static readonly string[] _stylesToPropagate = new[] { "tss-default-component-margin", "tss-collapse", "tss-fade-light", "tss-fade", "tss-show" };

        /// <summary>
        /// Represents the orientation of a stack.
        /// </summary>
        public enum Orientation
        {
            /// <summary>Vertical.</summary>
            Vertical,
            /// <summary>Horizontal.</summary>
            Horizontal,
            /// <summary>Vertical reverse.</summary>
            VerticalReverse,
            /// <summary>Horizontal reverse.</summary>
            HorizontalReverse,
        }

        /// <summary>
        /// Represents the size of a stack item.
        /// </summary>
        public struct ItemSize
        {
            /// <summary>Gets or sets the unit type.</summary>
            public Unit  Type  { get; set; }
            /// <summary>Gets or sets the value.</summary>
            public float Value { get; set; }
        }

        /// <summary>Sets the skeleton state.</summary>
        public IComponent Skeleton(bool enabled = true)
        {
            if (enabled)
            {
                InnerElement.classList.add("tss-skeleton");
            }
            else
            {
                InnerElement.classList.remove("tss-skeleton");
            }

            return this;
        }
    }

    /// <summary>
    /// Represents the alignment of an item.
    /// </summary>
    [Name("tss.ItemAlign")]
    [Enum(Emit.StringName)]
    public enum ItemAlign
    {
        /// <summary>Auto.</summary>
        [Name("auto")]       Auto,
        /// <summary>Stretch.</summary>
        [Name("stretch")]    Stretch,
        /// <summary>Baseline.</summary>
        [Name("baseline")]   Baseline,
        /// <summary>Start.</summary>
        [Name("flex-start")] Start,
        /// <summary>Center.</summary>
        [Name("center")]     Center,
        /// <summary>End.</summary>
        [Name("flex-end")]   End
    }

    /// <summary>
    /// Represents the justification of an item.
    /// </summary>
    [Name("tss.ItemJustify")]
    [Enum(Emit.StringName)]
    public enum ItemJustify
    {
        /// <summary>Space between.</summary>
        [Name("space-between")] Between,
        /// <summary>Space around.</summary>
        [Name("space-around")]  Around,
        /// <summary>Space evenly.</summary>
        [Name("space-evenly")]  Evenly,
        /// <summary>Start.</summary>
        [Name("flex-start")]    Start,
        /// <summary>Center.</summary>
        [Name("center")]        Center,
        /// <summary>End.</summary>
        [Name("flex-end")]      End
    }
}