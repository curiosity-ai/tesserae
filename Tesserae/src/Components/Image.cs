using Tesserae.HTML;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae.Components
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

        public Image Fit(ImageFit fit)
        {
            switch (fit)
            {
                case ImageFit.Contain:
                    InnerElement.style.objectFit = "contain";
                    break;
                case ImageFit.Cover:
                    InnerElement.style.objectFit = "cover";
                    break;
                case ImageFit.Fill:
                    InnerElement.style.objectFit = "fill";
                    break;
                case ImageFit.None:
                    InnerElement.style.objectFit = "none ";
                    break;
                case ImageFit.ScaleDown:
                    InnerElement.style.objectFit = "scale-down";
                    break;
            }

            return this;
        }
    }

    public enum ImageFit
    {
        Contain,
        Cover,
        Fill,
        None,
        ScaleDown
    }
}
