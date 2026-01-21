using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    [H5.Name("tss.Shimmer")]
    public class Shimmer : ComponentBase<Shimmer, HTMLDivElement>
    {
        public Shimmer()
        {
            InnerElement = Div(_("tss-shimmer tss-shimmer-square"));
            Width = "100%";
            Height = "16px";
        }

        public string Width
        {
            get => InnerElement.style.width;
            set => InnerElement.style.width = value;
        }

        public string Height
        {
            get => InnerElement.style.height;
            set => InnerElement.style.height = value;
        }

        public Shimmer Circle()
        {
            InnerElement.classList.remove("tss-shimmer-square");
            InnerElement.classList.add("tss-shimmer-circle");
            return this;
        }

        public Shimmer Square()
        {
            InnerElement.classList.remove("tss-shimmer-circle");
            InnerElement.classList.add("tss-shimmer-square");
            return this;
        }

        public override HTMLElement Render()
        {
            return InnerElement;
        }
    }
}
