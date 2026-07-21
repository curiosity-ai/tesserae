using Transpose;
using System;
using System.Collections.Generic;
using System.Linq;
using static Transpose.Core.dom;
using static Tesserae.UI;
using Orientation = Tesserae.Stack.Orientation;

namespace Tesserae
{
    /// <summary>
    /// A Stack whose children are driven by an <see cref="ObservableList{T}"/> of
    /// <see cref="IComponentWithID"/>. On every change it performs a keyed reconcile against the live
    /// DOM rather than rebuilding: existing children are matched by their string <c>Identifier</c>, a
    /// matched child is re-rendered (<c>replaceChild</c>) only when its <c>ContentHash</c> changes,
    /// surviving children are reordered to match the new sequence, dropped items are removed and new
    /// items inserted. Changes are debounced by default.
    /// </summary>
    /// <remarks>
    /// Use this when your items are (or can cheaply be) rendered components that each expose a stable
    /// key and a content hash, and you want reordering and/or content-hash-driven replacement (e.g. a
    /// server-driven or streaming list such as <c>Chat</c>).
    ///
    /// For the complementary case, see <see cref="ObservableStack{T}"/>: it keys data models by
    /// reference identity, builds each row lazily via a factory, and never replaces a matched row (rows
    /// refresh their own content via observation). Prefer that when items are data objects identified by
    /// reference and each row is a self-managing component you do not want rebuilt. Note that it only
    /// diffs a common prefix/suffix, so a reorder of interior rows rebuilds that span, whereas
    /// <see cref="KeyedObservableStack"/> handles arbitrary reorders.
    /// </remarks>
    [Transpose.Name("tss.KOS")]
    public class KeyedObservableStack : IComponent, IHasBackgroundColor, IHasMarginPadding, ISpecialCaseStyling, ICanWrap
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
        /// Gets or sets a value indicating whether the component's text can wrap onto multiple lines.
        /// </summary>
        public bool CanWrap
        {
            get => InnerElement.style.flexWrap != "nowrap";
            set => InnerElement.style.flexWrap = value ? "wrap" : "nowrap";
        }

        /// <summary>
        /// Returns a value indicating whether the component is inline.
        /// </summary>
        public bool IsInline
        {
            get => InnerElement.style.display == "inline-flex";
            set => InnerElement.style.display = value ? "inline-flex" : "";
        }

        /// <summary>
        /// Gets the underlying DOM element backing this component.
        /// </summary>
        public HTMLElement InnerElement { get;                                  private set; }
        /// <summary>
        /// Gets or sets the CSS background of the component.
        /// </summary>
        public string      Background   { get => InnerElement.style.background; set => InnerElement.style.background = value; }
        /// <summary>
        /// Gets or sets the CSS margin of the component.
        /// </summary>
        public string      Margin       { get => InnerElement.style.margin;     set => InnerElement.style.margin = value; }
        /// <summary>
        /// Gets or sets the CSS padding of the component.
        /// </summary>
        public string      Padding      { get => InnerElement.style.padding;    set => InnerElement.style.padding = value; }

        /// <summary>
        /// Gets or sets the styling container.
        /// </summary>
        public HTMLElement StylingContainer => InnerElement;

        /// <summary>
        /// Gets or sets the propagate to stack item parent.
        /// </summary>
        public bool PropagateToStackItemParent { get; private set; } = true;


        /// <summary>
        /// Sets the align-items css property for this stack
        /// </summary>
        /// <param name="align"></param>
        /// <returns></returns>
        public KeyedObservableStack AlignItems(ItemAlign align)
        {
            string cssAlign                                        = align.ToString();
            if (cssAlign == "end" || cssAlign == "start") cssAlign = $"flex-{cssAlign}";
            InnerElement.style.alignItems = cssAlign;
            return this;
        }

        /// <summary>
        /// Sets the align-items css property for this stack to 'center'
        /// </summary>
        /// <param name="align"></param>
        /// <returns></returns>
        public KeyedObservableStack AlignItemsCenter() => AlignItems(ItemAlign.Center);

        /// <summary>
        /// Make this stack relative (i.e. position:relative)
        /// </summary>
        /// <returns></returns>
        public KeyedObservableStack Relative()
        {
            InnerElement.classList.add("tss-relative");
            return this;
        }

        /// <summary>
        /// Sets the align-content CSS property for this stack.
        /// </summary>
        /// <param name="align">The alignment.</param>
        /// <returns>The current instance.</returns>
        public KeyedObservableStack AlignContent(ItemAlign align)
        {
            string cssAlign                                        = align.ToString().ToLower();
            if (cssAlign == "end" || cssAlign == "start") cssAlign = $"flex-{cssAlign}";
            InnerElement.style.alignContent = cssAlign;
            return this;
        }

        /// <summary>
        /// Sets the justify-content CSS property for this stack.
        /// </summary>
        /// <param name="justify">The justification.</param>
        /// <returns>The current instance.</returns>
        public KeyedObservableStack JustifyContent(ItemJustify justify)
        {
            string cssJustify                                                                           = justify.ToString().ToLower();
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
        public KeyedObservableStack JustifyItems(ItemJustify justify)
        {
            string cssJustify                                                                           = justify.ToString().ToLower();
            if (cssJustify == "end"     || cssJustify == "start") cssJustify                            = $"flex-{cssJustify}";
            if (cssJustify == "between" || cssJustify == "around" || cssJustify == "evenly") cssJustify = $"space-{cssJustify}";
            InnerElement.style.justifyItems = cssJustify;
            return this;
        }

        /// <summary>
        /// Removes the given propagation from the component.
        /// </summary>
        public KeyedObservableStack RemovePropagation()
        {
            PropagateToStackItemParent = false;
            return this;
        }

        private ObservableList<IComponentWithID> _observableList;

        private class ExistingStackElement
        {
            /// <summary>
            /// Gets or sets the identifier.
            /// </summary>
            public string      Identifier      { get; set; }
            /// <summary>
            /// Gets or sets the content hash.
            /// </summary>
            public string      ContentHash     { get; set; }
            /// <summary>
            /// Gets or sets the rendered element.
            /// </summary>
            public HTMLElement RenderedElement { get; set; }
            /// <summary>
            /// Gets or sets the index.
            /// </summary>
            public int         Index           { get; set; }
        }

        private void ReconcileChildren(IReadOnlyList<IComponentWithID> newChildren)
        {
            var parent = InnerElement;

            var selectedElement = document.activeElement;
            if (selectedElement is object && !parent.contains(selectedElement))
            {
                selectedElement = null;
            }

            var currentKeyMap = new Dictionary<string, ExistingStackElement>();

            var index                = 0;
            var currentChildrenArray = parent.querySelectorAll(":scope > [data-tssid]").ToArray();

            foreach (var renderedElement in currentChildrenArray)
            {
                var dataset     = renderedElement.As<HTMLElement>().dataset;
                var identifier  = dataset["tssid"].As<string>();
                var contentHash = dataset["tsshash"].As<string>();

                currentKeyMap.Add(identifier, new ExistingStackElement()
                {
                    Identifier      = identifier,
                    ContentHash     = contentHash,
                    RenderedElement = renderedElement.As<HTMLElement>(),
                    Index           = index,
                });

                index++;
            }

            int lastIndex            = 0;
            var processedIdentifiers = new HashSet<string>();

            var addedElementsToInsert = new List<(int position, IComponentWithID component)>();

            for (int newIdx = 0; newIdx < newChildren.Count; newIdx++)
            {
                IComponentWithID newChild   = newChildren[newIdx];
                string           identifier = newChild.Identifier;

                if (currentKeyMap.TryGetValue(identifier, out var existingChild))
                {
                    processedIdentifiers.Add(identifier);

                    if (existingChild.ContentHash != newChild.ContentHash)
                    {
                        var newItem = GetItem(newChild);

                        parent.replaceChild(newItem, existingChild.RenderedElement);
                        existingChild.RenderedElement = newItem;
                    }

                    int oldIndex = currentKeyMap[identifier].Index;

                    if (oldIndex < lastIndex)
                    {
                        if (lastIndex >= parent.children.length)
                        {
                            parent.appendChild(existingChild.RenderedElement);
                        }
                        else
                        {
                            parent.insertBefore(existingChild.RenderedElement, parent.children[(uint)lastIndex + 1]);
                        }
                    }
                    else
                    {
                        lastIndex = oldIndex;
                    }
                }
                else
                {
                    addedElementsToInsert.Add((newIdx, newChild));
                }
            }

            foreach (var entry in currentKeyMap)
            {
                if (!processedIdentifiers.Contains(entry.Key))
                {
                    parent.removeChild(entry.Value.RenderedElement);
                }
            }

            foreach (var (position, component) in addedElementsToInsert.OrderBy(x => x.position))
            {
                var newItem = GetItem(component);

                var currentNodeAt = parent.children[(uint)position].As<HTMLElement>();

                if (currentNodeAt != null)
                {
                    parent.insertBefore(newItem, currentNodeAt);
                }
                else
                {
                    parent.appendChild(newItem);
                }
            }

            if (selectedElement is object && parent.contains(selectedElement))
            {
                selectedElement.As<HTMLElement>().focus();
            }
        }
        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        public KeyedObservableStack(ObservableList<IComponentWithID> observableList, Orientation orientation = Orientation.Vertical, bool debounce = true)
        {
            InnerElement     = Div(_("tss-stack"));
            StackOrientation = orientation;
            _observableList  = observableList;

            if (debounce)
            {
                var debouncer = new DebouncerWithMaxDelay(() =>
                {
                    ReconcileChildren(_observableList.Value);
                }, delayInMs: 16, maxDelayInMs: 300);

                _observableList.Observe(currentValues =>
                {
                    debouncer.RaiseOnValueChanged();
                });
            }
            else
            {
                _observableList.Observe(currentValues =>
                {
                    ReconcileChildren(currentValues);
                });
            }
        }

        private event ComponentEventHandler<KeyedObservableStack, Event> MouseOver;
        private event ComponentEventHandler<KeyedObservableStack, Event> MouseOut;

        private void RaiseMouseOver(Event ev) => MouseOver?.Invoke((KeyedObservableStack)this, ev);
        private void RaiseMouseOut(Event  ev) => MouseOut?.Invoke((KeyedObservableStack)this, ev);

        /// <summary>
        /// Registers a callback invoked when the mouse over event fires.
        /// </summary>
        public KeyedObservableStack OnMouseOver(ComponentEventHandler<KeyedObservableStack, Event> onMouseOver)
        {
            if (!(InnerElement.onmouseover is object))
            {
                InnerElement.onmouseover += s => RaiseMouseOver(s);
            }

            MouseOver += onMouseOver;
            return (KeyedObservableStack)this;
        }

        /// <summary>
        /// Registers a callback invoked when the mouse out event fires.
        /// </summary>
        public KeyedObservableStack OnMouseOut(ComponentEventHandler<KeyedObservableStack, Event> onMouseOut)
        {
            if (!(InnerElement.onmouseout is object))
            {
                InnerElement.onmouseout += s => RaiseMouseOut(s);
            }

            MouseOut += onMouseOut;
            return (KeyedObservableStack)this;
        }


        /// <summary>
        /// Clears the component's current state.
        /// </summary>
        public virtual void Clear() => ClearChildren(InnerElement);

        /// <summary>
        /// Renders the component's root HTML element.
        /// </summary>
        public virtual HTMLElement Render() => InnerElement;

        /// <summary>
        /// Configures the component to horizontal.
        /// </summary>
        public KeyedObservableStack Horizontal()
        {
            StackOrientation = Stack.Orientation.Horizontal;
            return this;
        }

        /// <summary>
        /// Configures the component to vertical.
        /// </summary>
        public KeyedObservableStack Vertical()
        {
            StackOrientation = Stack.Orientation.Vertical;
            return this;
        }

        /// <summary>
        /// Configures the horizontal reverse on the component.
        /// </summary>
        public KeyedObservableStack HorizontalReverse()
        {
            StackOrientation = Stack.Orientation.HorizontalReverse;
            return this;
        }

        /// <summary>
        /// Configures the vertical reverse on the component.
        /// </summary>
        public KeyedObservableStack VerticalReverse()
        {
            StackOrientation = Stack.Orientation.VerticalReverse;
            return this;
        }

        /// <summary>
        /// Allows the component's content to wrap onto multiple lines.
        /// </summary>
        public KeyedObservableStack Wrap()
        {
            CanWrap = true;
            return this;
        }

        /// <summary>
        /// Renders the component inline.
        /// </summary>
        public KeyedObservableStack Inline()
        {
            IsInline = true;
            return this;
        }

        /// <summary>
        /// Removes / disables the wrap on the component.
        /// </summary>
        public KeyedObservableStack NoWrap()
        {
            CanWrap = false;
            return this;
        }

        /// <summary>
        /// Hides any content that overflows the component's bounds.
        /// </summary>
        public KeyedObservableStack OverflowHidden()
        {
            InnerElement.style.overflow = "hidden";
            return this;
        }
        /// <summary>
        /// Removes / disables the default margin on the component.
        /// </summary>
        public KeyedObservableStack NoDefaultMargin()
        {
            InnerElement.classList.add("tss-default-component-no-margin");
            return this;
        }

        internal static HTMLElement GetItem(IComponentWithID component)
        {
            var rendered = component.Render();

            var item = Div(_("tss-stack-item", styles: s =>
            {
                s.alignSelf  = "auto";
                s.width      = "auto";
                s.height     = "auto";
                s.flexShrink = "1";
            }), rendered);


            item.dataset["tssid"]   = component.Identifier;
            item.dataset["tsshash"] = component.ContentHash;

            CopyStylesDefinedWithExtension(rendered, item);
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
            if (has("tss-stk-js")) { ts.justifySelf = fs.justifySelf; /*fs.alignSelf = "";*/ }

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
        /// Configures the component to skeleton.
        /// </summary>
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

    [Transpose.Name("tss.ICID")]
    public interface IComponentWithID : IComponent
    {
        string Identifier  { get; }
        string ContentHash { get; }
    }
}