using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// Predefined shape presets used by the <see cref="Skeleton"/> placeholder.
    /// </summary>
    public enum SkeletonType
    {
        Line,
        Circle,
        Rect
    }

    [H5.Name("tss.Skeleton")]
    public sealed class Skeleton : ComponentBase<Skeleton, HTMLElement>
    {
        private SkeletonType _type;

        public Skeleton(SkeletonType type = SkeletonType.Line)
        {
            InnerElement = Div(_("tss-skeleton"));
            Type(type);
            Animated();
        }

        public SkeletonType TypeValue
        {
            get => _type;
            set => Type(value);
        }

        public bool IsAnimated
        {
            get => InnerElement.classList.contains("tss-skeleton-animated");
            set => InnerElement.UpdateClassIf(value, "tss-skeleton-animated");
        }

        public Skeleton Type(SkeletonType type)
        {
            _type = type;
            InnerElement.classList.remove("tss-skeleton-line", "tss-skeleton-circle", "tss-skeleton-rect");

            switch (type)
            {
                case SkeletonType.Circle:
                    InnerElement.classList.add("tss-skeleton-circle");
                    break;
                case SkeletonType.Rect:
                    InnerElement.classList.add("tss-skeleton-rect");
                    break;
                default:
                    InnerElement.classList.add("tss-skeleton-line");
                    break;
            }

            return this;
        }

        public Skeleton Animated(bool value = true)
        {
            IsAnimated = value;
            return this;
        }

        /// <summary>
        /// Gets or sets the CSS background of the component.
        /// </summary>
        public Skeleton Background(string color)
        {
            InnerElement.style.backgroundColor = color;
            return this;
        }

        /// <summary>
        /// Renders the component's root HTML element.
        /// </summary>
        public override HTMLElement Render() => InnerElement;
    }
}
