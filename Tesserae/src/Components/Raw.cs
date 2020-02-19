using System;
using static Retyped.dom;
using static Tesserae.UI;

namespace Tesserae.Components
{
    public class Raw : IComponent, IHasMarginPadding, IHasBackgroundColor
    {
        private HTMLElement InnerElement;

        public Raw(HTMLElement content = null)
        {
            InnerElement = content ?? DIV();
        }

        public Raw Content(IComponent component) => Content(component.Render());

        public Raw Content(HTMLElement element)
        {
            if (InnerElement != element)
            {
                if (InnerElement.parentElement is object)
                {
                    InnerElement.parentElement.replaceChild(element, InnerElement);
                }
                else
                {
                    InnerElement = element;
                }
            }
            return this;
        }


        public string Background { get => InnerElement.style.background; set => InnerElement.style.background = value; }
        public string Margin { get => InnerElement.style.margin; set => InnerElement.style.margin = value; }
        public string Padding { get => InnerElement.style.padding; set => InnerElement.style.padding = value; }
        public string Width { get => InnerElement.style.width; set => InnerElement.style.width = value; }
        public string Height { get => InnerElement.style.height; set => InnerElement.style.height = value; }

        public HTMLElement Render()
        {
            return InnerElement;
        }
    }
}