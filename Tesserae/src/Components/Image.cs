using Tesserae.HTML;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    public class Image : ComponentBase<Image, HTMLImageElement>
    {
        public Image(string source)
        {
            InnerElement = UI.Image(_(src:source));
        }

        public override HTMLElement Render()
        {
            return InnerElement;
        }

        public Image Contain()
        {
            InnerElement.style.objectFit = "contain";
            return this;
        }

        public Image Cover()
        {
            InnerElement.style.objectFit = "cover";
            return this;
        }

        public Image Fill()
        {
            InnerElement.style.objectFit = "fill";
            return this;
        }

        public Image NoFit()
        {
            InnerElement.style.objectFit = "none";
            return this;
        }

        public Image ScaleDown()
        {
            InnerElement.style.objectFit = "scale-down";
            return this;
        }
    }
}
