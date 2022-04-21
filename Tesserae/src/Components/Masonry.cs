using H5;
using System.Linq;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    [H5.Name("tss.Masonry")]
    public class Masonry : IContainer<Masonry, IComponent>, IHasBackgroundColor, IHasMarginPadding, ISpecialCaseStyling
    {
        private readonly string _percent;
        private readonly HTMLElement _masonry;
        private readonly object _masonryObj;
        private readonly int _gutter;

        public string Background { get => _masonry.style.background; set => _masonry.style.background = value; }
        public string Margin     { get => _masonry.style.margin;     set => _masonry.style.margin = value; }
        public string Padding    { get => _masonry.style.padding;    set => _masonry.style.padding = value; }

        public HTMLElement StylingContainer => _masonry;
        private double _timeout;
        public bool PropagateToStackItemParent => false;

        public Masonry(int columns, int gutter = 10)
        {
            _percent = $"calc({(100f / columns):0.00}% - {gutter}px)";
            _masonry = Div(_("tss-masonry"));
            _masonryObj = Script.Write<object>("new Masonry({0}, { itemSelector: '.tss-masonry-item', columnWidth: '.tss-masonry-item', gutter: {1}, percentPosition: true })", _masonry, gutter);
            _gutter = gutter;
            DomObserver.WhenMounted(_masonry, () => Layout());
        }

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
                    console.log("layouted");
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
                    item = Div(_("tss-masonry-item", styles: s =>
                    {
                        s.alignSelf = "auto";
                        s.width = _percent;
                        s.height = "auto";
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

        public virtual void Clear()
        {
            ClearChildren(_masonry);
            Layout();
        }

        public void Replace(IComponent newComponent, IComponent oldComponent)
        {
            _masonry.replaceChild(GetItem(newComponent), GetItem(oldComponent));
            Layout();
        }

        public void Remove(IComponent component)
        {
            Script.Write("{0}.remove({1})", _masonryObj, GetItem(component));
            Layout();
        }

        public virtual HTMLElement Render() => _masonry;
    }
}