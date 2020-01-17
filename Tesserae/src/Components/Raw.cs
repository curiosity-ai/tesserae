using Retyped;

namespace Tesserae.Components
{
    public class Raw :IComponent, IHasMarginPadding, IHasBackgroundColor
    {
        private dom.HTMLElement InnerElement;

        public Raw(dom.HTMLElement element)
        {
            InnerElement = element;
        }

        public string Background { get => InnerElement.style.background; set => InnerElement.style.background = value; }
        public string Margin { get => InnerElement.style.margin; set => InnerElement.style.margin = value; }
        public string Padding { get => InnerElement.style.padding; set => InnerElement.style.padding = value; }

        public dom.HTMLElement Render()
        {
            return InnerElement;
        }
    }
}