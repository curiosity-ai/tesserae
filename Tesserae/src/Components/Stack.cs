using static Tesserae.HTML.HtmlUtil;
using static Tesserae.HTML.HtmlAttributes;
using static Retyped.dom;

namespace Tesserae.Components
{
    public enum StackOrientation
    {
        Vertical,
        Horizontal
    }

    public class Stack : IContainer<Stack>
    {
        public StackOrientation Orientation
        {
            get { return InnerElement.style.flexDirection == "row" ? StackOrientation.Horizontal : StackOrientation.Vertical; }
            set { InnerElement.style.flexDirection = value == StackOrientation.Horizontal ? "row" : "column"; }
        }

        public HTMLElement InnerElement { get; private set; }

        public Stack(StackOrientation orientation = StackOrientation.Vertical)
        {
            InnerElement = Div(_(/*"container"*/"m-1"));
            InnerElement.style.display = "flex";
            if (orientation == StackOrientation.Horizontal) InnerElement.style.flexDirection = "row";
            else InnerElement.style.flexDirection = "column";
        }

        public void Add(IComponent component)
        {
            InnerElement.appendChild(component.Render());
        }

        public void Clear()
        {
            ClearChildren(InnerElement);
        }

        public void Replace(IComponent newComponent, IComponent oldComponent)
        {
            InnerElement.replaceChild(newComponent.Render(), oldComponent.Render());
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
    }
}
