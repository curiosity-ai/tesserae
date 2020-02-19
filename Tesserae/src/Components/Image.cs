using Tesserae.HTML;
using static Retyped.dom;

namespace Tesserae.Components
{
    public class Image : ComponentBase<Image, HTMLImageElement>
    {
        public Image(Attributes attributes)
        {
            InnerElement = UI.Image(attributes);
        }

        public override HTMLElement Render()
        {
            return InnerElement;
        }
    }
}
