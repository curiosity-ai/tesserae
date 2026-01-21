using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    [H5.Name("tss.Badge")]
    public class Badge : ComponentBase<Badge, HTMLSpanElement>, ITextFormating, IHasBackgroundColor, IHasForegroundColor
    {
        public Badge(string text = string.Empty)
        {
            InnerElement = Span(_("tss-badge tss-badge-primary", text: text));
            AttachClick();
        }

        public string Text
        {
            get => InnerElement.innerText;
            set => InnerElement.innerText = value;
        }

        public string Background
        {
            get => InnerElement.style.background;
            set => InnerElement.style.background = value;
        }

        public string Foreground
        {
            get => InnerElement.style.color;
            set => InnerElement.style.color = value;
        }

        public TextSize Size
        {
            get => ITextFormatingExtensions.FromClassList(InnerElement, TextSize.Small);
            set
            {
                InnerElement.classList.remove(Size.ToString());
                InnerElement.classList.add(value.ToString());
            }
        }

        public TextWeight Weight
        {
            get => ITextFormatingExtensions.FromClassList(InnerElement, TextWeight.Regular);
            set
            {
                InnerElement.classList.remove(Weight.ToString());
                InnerElement.classList.add(value.ToString());
            }
        }

        public TextAlign TextAlign
        {
            get => ITextFormatingExtensions.FromClassList(InnerElement, TextAlign.Center);
            set
            {
                InnerElement.classList.remove(TextAlign.ToString());
                InnerElement.classList.add(value.ToString());
            }
        }

        public Badge Primary()
        {
            InnerElement.classList.add("tss-badge-primary");
            InnerElement.classList.remove("tss-badge-success", "tss-badge-danger", "tss-badge-warning", "tss-badge-info");
            return this;
        }

        public Badge Success()
        {
            InnerElement.classList.add("tss-badge-success");
            InnerElement.classList.remove("tss-badge-primary", "tss-badge-danger", "tss-badge-warning", "tss-badge-info");
            return this;
        }

        public Badge Danger()
        {
            InnerElement.classList.add("tss-badge-danger");
            InnerElement.classList.remove("tss-badge-primary", "tss-badge-success", "tss-badge-warning", "tss-badge-info");
            return this;
        }

        public Badge Warning()
        {
            InnerElement.classList.add("tss-badge-warning");
            InnerElement.classList.remove("tss-badge-primary", "tss-badge-success", "tss-badge-danger", "tss-badge-info");
            return this;
        }

        public Badge Info()
        {
            InnerElement.classList.add("tss-badge-info");
            InnerElement.classList.remove("tss-badge-primary", "tss-badge-success", "tss-badge-danger", "tss-badge-warning");
            return this;
        }

        public Badge Small()
        {
            InnerElement.classList.add("tss-badge-small");
            return this;
        }

        public override HTMLElement Render()
        {
            return InnerElement;
        }
    }
}
