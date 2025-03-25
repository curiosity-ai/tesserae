using H5;
using System;
using System.Collections.Generic;
using System.Linq;
using static H5.Core.dom;
using static Tesserae.UI;
using Orientation = Tesserae.Stack.Orientation;

namespace Tesserae
{
    /// <summary>
    /// A Stack is a container-type component that abstracts the implementation of a flexbox in order to define the layout of its children components.
    /// </summary>
    [H5.Name("tss.OS")]
    public class ObservableStack<T> : IComponent, IHasBackgroundColor, IHasMarginPadding, ISpecialCaseStyling, ICanWrap where T : ObservableStack<T>.IObservableStackItem
    {
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

        public HTMLElement InnerElement { get;                                  private set; }
        public string      Background   { get => InnerElement.style.background; set => InnerElement.style.background = value; }
        public string      Margin       { get => InnerElement.style.margin;     set => InnerElement.style.margin = value; }
        public string      Padding      { get => InnerElement.style.padding;    set => InnerElement.style.padding = value; }

        public HTMLElement StylingContainer => InnerElement;

        public bool PropagateToStackItemParent { get; private set; } = true;


        /// <summary>
        /// Sets the align-items css property for this stack
        /// </summary>
        /// <param name="align"></param>
        /// <returns></returns>
        public ObservableStack<T> AlignItems(ItemAlign align)
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
        public ObservableStack<T> AlignItemsCenter() => AlignItems(ItemAlign.Center);

        /// <summary>
        /// Make this stack relative (i.e. position:relative)
        /// </summary>
        /// <returns></returns>
        public ObservableStack<T> Relative()
        {
            InnerElement.classList.add("tss-relative");
            return this;
        }

        /// <summary>
        /// Sets the align-items css property for this stack
        /// </summary>
        /// <param name="align"></param>
        /// <returns></returns>
        public ObservableStack<T> AlignContent(ItemAlign align)
        {
            string cssAlign                                        = align.ToString().ToLower();
            if (cssAlign == "end" || cssAlign == "start") cssAlign = $"flex-{cssAlign}";
            InnerElement.style.alignContent = cssAlign;
            return this;
        }

        /// <summary>
        /// Sets the justify-content css property for this stack
        /// </summary>
        /// <param name="justify"></param>
        /// <returns></returns>
        public ObservableStack<T> JustifyContent(ItemJustify justify)
        {
            string cssJustify                                                                           = justify.ToString().ToLower();
            if (cssJustify == "end"     || cssJustify == "start") cssJustify                            = $"flex-{cssJustify}";
            if (cssJustify == "between" || cssJustify == "around" || cssJustify == "evenly") cssJustify = $"space-{cssJustify}";
            InnerElement.style.justifyContent = cssJustify;
            return this;
        }

        /// <summary>
        /// Sets the justify-content css property for this stack
        /// </summary>
        /// <param name="justify"></param>
        /// <returns></returns>
        public ObservableStack<T> JustifyItems(ItemJustify justify)
        {
            string cssJustify                                                                           = justify.ToString().ToLower();
            if (cssJustify == "end"     || cssJustify == "start") cssJustify                            = $"flex-{cssJustify}";
            if (cssJustify == "between" || cssJustify == "around" || cssJustify == "evenly") cssJustify = $"space-{cssJustify}";
            InnerElement.style.justifyItems = cssJustify;
            return this;
        }

        public ObservableStack<T> RemovePropagation()
        {
            PropagateToStackItemParent = false;
            return this;
        }

        private ObservableList<T> _observableList;

        private class ExistingStackElement
        {
            public string      StackID         { get; set; }
            public string      StackHash       { get; set; }
            public HTMLElement RenderedElement { get; set; }
        }

        private void ReconcileChildren(T[] newChildren)
        {
            var parent = ScrollBar.GetCorrectContainer(InnerElement);

            var currentKeyMap   = new Dictionary<string, ExistingStackElement>();
            var currentIndexMap = new Dictionary<string, int>();

            var index                = 0;
            var currentChildrenArray = parent.querySelectorAll($"[data-stackid]").ToArray();

            foreach (var renderedElement in currentChildrenArray)
            {
                var dataset   = renderedElement.As<HTMLElement>().dataset;
                var stackId   = dataset["stackid"].As<string>();
                var stackHash = dataset["stackhash"].As<string>();

                currentKeyMap.Add(stackId, new ExistingStackElement()
                {
                    StackID         = stackId,
                    StackHash       = stackHash,
                    RenderedElement = renderedElement.As<HTMLElement>(),
                });
                currentIndexMap.Add(stackId, index);

                index++;
            }

            int             lastIndex     = 0;
            HashSet<string> processedKeys = new HashSet<string>();

            int newIdx = 0;

            while (newIdx < newChildren.Length)
            {
                IObservableStackItem newChild = newChildren[newIdx];
                string               key      = newChild.GetId();

                HTMLElement currentNodeAtPosition = newIdx < currentChildrenArray.Length ? currentChildrenArray[newIdx].As<HTMLElement>() : null;

                if (currentKeyMap.TryGetValue(key, out var existingChild))
                {
                    processedKeys.Add(key);

                    if (existingChild.StackHash != newChild.GetHash())
                    {
                        var newItem = GetItem(newChild);

                        parent.replaceChild(newItem, existingChild.RenderedElement);
                    }

                    int oldIndex = currentIndexMap[key];

                    if (oldIndex < lastIndex)
                    {
                        HTMLElement nextSibling = newIdx + 1 < newChildren.Length ? parent.childNodes[newIdx + 1].As<HTMLElement>() : null;
                        parent.insertBefore(existingChild.RenderedElement, nextSibling);
                    }
                    else
                    {
                        lastIndex = oldIndex;
                    }
                }
                else
                {
                    var newItem = GetItem(newChild);
                    parent.insertBefore(newItem, currentNodeAtPosition);
                }

                newIdx++;
            }

            foreach (var entry in currentKeyMap)
            {
                if (!processedKeys.Contains(entry.Key))
                {
                    parent.removeChild(entry.Value.RenderedElement);
                }
            }
        }

        public ObservableStack(ObservableList<T> observableList, Orientation orientation = Orientation.Vertical)
        {
            InnerElement     = Div(_("tss-stack"));
            StackOrientation = orientation;
            _observableList  = observableList;

            _observableList.Observe(currentValues =>
            {
#if DEBUG
                if (currentValues.Count != currentValues.Select(v => v.GetId()).Distinct().Count())
                {
                    console.error("Values in ObservableStack don't have unique ids ", currentValues);
                }
                if (currentValues.Count != currentValues.Select(v => v.GetHash()).Distinct().Count())
                {
                    console.error("Values in ObservableStack don't have unique hashes ", currentValues);
                }
#endif

                ReconcileChildren(currentValues.ToArray());
            });
        }

        private event ComponentEventHandler<ObservableStack<T>, Event> MouseOver;
        private event ComponentEventHandler<ObservableStack<T>, Event> MouseOut;

        private void RaiseMouseOver(Event ev) => MouseOver?.Invoke((ObservableStack<T>)this, ev);
        private void RaiseMouseOut(Event  ev) => MouseOut?.Invoke((ObservableStack<T>)this, ev);

        public ObservableStack<T> OnMouseOver(ComponentEventHandler<ObservableStack<T>, Event> onMouseOver)
        {
            if (!(InnerElement.onmouseover is object))
            {
                InnerElement.onmouseover += s => RaiseMouseOver(s);
            }

            MouseOver += onMouseOver;
            return (ObservableStack<T>)this;
        }

        public ObservableStack<T> OnMouseOut(ComponentEventHandler<ObservableStack<T>, Event> onMouseOut)
        {
            if (!(InnerElement.onmouseout is object))
            {
                InnerElement.onmouseout += s => RaiseMouseOut(s);
            }

            MouseOut += onMouseOut;
            return (ObservableStack<T>)this;
        }


        public virtual void Clear() => ClearChildren(ScrollBar.GetCorrectContainer(InnerElement));

        public virtual HTMLElement Render() => InnerElement;

        public ObservableStack<T> Horizontal()
        {
            StackOrientation = Stack.Orientation.Horizontal;
            return this;
        }

        public ObservableStack<T> Vertical()
        {
            StackOrientation = Stack.Orientation.Vertical;
            return this;
        }

        public ObservableStack<T> HorizontalReverse()
        {
            StackOrientation = Stack.Orientation.HorizontalReverse;
            return this;
        }

        public ObservableStack<T> VerticalReverse()
        {
            StackOrientation = Stack.Orientation.VerticalReverse;
            return this;
        }

        public ObservableStack<T> Wrap()
        {
            CanWrap = true;
            return this;
        }

        public ObservableStack<T> Inline()
        {
            IsInline = true;
            return this;
        }

        public ObservableStack<T> NoWrap()
        {
            CanWrap = false;
            return this;
        }

        public ObservableStack<T> OverflowHidden()
        {
            InnerElement.style.overflow = "hidden";
            return this;
        }
        public ObservableStack<T> NoDefaultMargin()
        {
            InnerElement.classList.add("tss-default-component-no-margin");
            return this;
        }

        internal static HTMLElement GetItem(IObservableStackItem component)
        {
            var rendered = component.Render();

            var item = Div(_("tss-stack-item", styles: s =>
            {
                s.alignSelf  = "auto";
                s.width      = "auto";
                s.height     = "auto";
                s.flexShrink = "1";
            }), rendered);


            item.dataset["stackid"]   = component.GetId();
            item.dataset["stackhash"] = component.GetHash();

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

        public class StackItemHelper
        {
            public static int Fnv1aHash(string value)
            {
                var valArray = value.ToCharArray();

                var hash = 0x811c9dc5; // FNV offset basis

                for (var i = 0; i < valArray.Length; i++)
                {
                    hash ^= valArray[i];
                    hash += (hash << 1) + (hash << 4) + (hash << 7) + (hash << 8) + (hash << 24);
                }
                return Script.Write<int>("{0} >>> 0", hash); // Convert to unsigned 32-bit integer
            }
        }

        public interface IObservableStackItem : IComponent
        {
            string GetId();
            string GetHash();
        }
    }
}