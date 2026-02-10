using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{

    [H5.Name("tss.Image")]
    public class Image : ComponentBase<Image, HTMLImageElement>, ISpecialCaseStyling, IHasBackgroundColor
    {
        public HTMLElement StylingContainer => InnerElement;

        public bool PropagateToStackItemParent { get; private set; }

        public Image(string source, string fallback = null)
        {
            if (!string.IsNullOrEmpty(fallback))
            {
                InnerElement = UI.Image(_("tss-image", src: fallback));

                InnerElement.onerror = _ => //Need to be hooked before setting src
                {
                    if (InnerElement.src != fallback)
                    {
                        InnerElement.src = fallback;
                    }

                };

                InnerElement.src = source;
            }
            else
            {
                InnerElement = UI.Image(_("tss-image", src: source));
            }

            PropagateToStackItemParent = true;
            AttachClick();
            AttachContextMenu();
        }

        public string Source
        {
            get => InnerElement.src;
            set => InnerElement.src = value;
        }

        public string Cursor
        {
            get => InnerElement.style.cursor;
            set => InnerElement.style.cursor = value;
        }

        public override HTMLElement Render()
        {
            return InnerElement;
        }

        public Image Position(string objectPosition)
        {
            InnerElement.style.objectPosition = objectPosition;
            return this;
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

        public Image Circle()
        {
            InnerElement.style.borderRadius = "50%";
            PropagateToStackItemParent      = false;
            return this;
        }

        public Image Circle(int pixels)
        {
            InnerElement.style.borderRadius = $"{pixels}px";
            return this;
        }
        public string Background
        {
            get => InnerElement.style.background;
            set => InnerElement.style.background = value;
        }
    }
}