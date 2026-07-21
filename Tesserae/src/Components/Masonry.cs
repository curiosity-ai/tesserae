using Transpose;
using System.Linq;
using static Transpose.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// A masonry (Pinterest-style) layout that flows items of varying heights into columns of equal width.
    /// </summary>
    [Transpose.Name("tss.Masonry")]
    public class Masonry : IContainer<Masonry, IComponent>, IHasBackgroundColor, IHasMarginPadding, ISpecialCaseStyling
    {
        private readonly string      _percent;
        private readonly HTMLElement _masonry;
        private readonly object      _masonryObj;
        private readonly int         _gutter;

        /// <summary>
        /// Gets or sets the CSS background of the component.
        /// </summary>
        public string Background { get => _masonry.style.background; set => _masonry.style.background = value; }
        /// <summary>
        /// Gets or sets the CSS margin of the component.
        /// </summary>
        public string Margin     { get => _masonry.style.margin;     set => _masonry.style.margin = value; }
        /// <summary>
        /// Gets or sets the CSS padding of the component.
        /// </summary>
        public string Padding    { get => _masonry.style.padding;    set => _masonry.style.padding = value; }

        /// <summary>
        /// Gets or sets the styling container.
        /// </summary>
        public  HTMLElement StylingContainer => _masonry;
        private double      _timeout;
        /// <summary>
        /// Gets or sets the propagate to stack item parent.
        /// </summary>
        public  bool        PropagateToStackItemParent => false;

        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        public Masonry(int columns, int gutter = 10)
        {
            _percent    = $"calc({(100f / columns):0.00}% - {gutter}px)";
            _masonry    = Div(Att("tss-masonry"));
            _masonryObj = Script.Write<object>("new Masonry({0}, { itemSelector: '.tss-masonry-item', columnWidth: '.tss-masonry-item', gutter: {1}, percentPosition: true })", _masonry, gutter);
            _gutter     = gutter;
            DomObserver.WhenMounted(_masonry, () => Layout());
        }

        /// <summary>
        /// Adds the given item to the component.
        /// </summary>
        public void Add(IComponent component)
        {
            var el = GetItem(component, true);
            el.style.marginBottom = _gutter + "px";
            _masonry.appendChild(el);
            Script.Write("{0}.appended({1})", _masonryObj, el);
            Layout();
        }

        private void Layout()
        {
            if (_masonry.IsMounted())
            {
                window.clearTimeout(_timeout);

                _timeout = window.setTimeout((_) =>
                {
                    Script.Write("{0}.layout()", _masonryObj);
                }, 16);
            }
        }

        internal HTMLElement GetItem(IComponent component, bool forceAdd = false)
        {
            HTMLElement item = null;

            if (component.HasOwnProperty("MasonryItem"))
            {
                item = component["MasonryItem"] as HTMLElement;
            }

            if (item is null)
            {
                var rendered = component.Render();

                if (forceAdd || (rendered.parentElement is object))
                {
                    item = Div(Att("tss-masonry-item", styles: s =>
                    {
                        s.alignSelf  = "auto";
                        s.width      = _percent;
                        s.height     = "auto";
                        s.flexShrink = "1";
                    }), component.Render());

                    component["MasonryItem"] = item;

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
            //Copy base-styles using same method from Stack
            Stack.CopyStylesDefinedWithExtension(from, to);

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
        }

        /// <summary>
        /// Clears the component's current state.
        /// </summary>
        public virtual void Clear()
        {
            ClearChildren(_masonry);
            Layout();
        }

        /// <summary>
        /// Replaces an existing item with a new one.
        /// </summary>
        public void Replace(IComponent newComponent, IComponent oldComponent)
        {
            _masonry.replaceChild(GetItem(newComponent), GetItem(oldComponent));
            Layout();
        }

        /// <summary>
        /// Removes the given item from the component.
        /// </summary>
        public void Remove(IComponent component)
        {
            Script.Write("{0}.remove({1})", _masonryObj, GetItem(component));
            Layout();
        }

        /// <summary>
        /// Renders the component's root HTML element.
        /// </summary>
        public virtual HTMLElement Render() => _masonry;
    }
}