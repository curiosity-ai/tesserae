using static Transpose.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{

    /// <summary>
    /// A responsive image element with optional sizing, fit-mode and lazy-loading support.
    /// </summary>
    [Transpose.Name("tss.Image")]
    public class Image : ComponentBase<Image, HTMLImageElement>, ISpecialCaseStyling, IHasBackgroundColor
    {
        /// <summary>
        /// Gets or sets the styling container.
        /// </summary>
        public HTMLElement StylingContainer => InnerElement;

        /// <summary>
        /// Gets or sets the propagate to stack item parent.
        /// </summary>
        public bool PropagateToStackItemParent { get; private set; }

        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        public Image(string source, string fallback = null)
        {
            if (!string.IsNullOrEmpty(fallback))
            {
                InnerElement = UI.Image(Att("tss-image", src: fallback));

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
                InnerElement = UI.Image(Att("tss-image", src: source));
            }

            PropagateToStackItemParent = true;
            AttachClick();
            AttachContextMenu();
        }

        /// <summary>
        /// Gets or sets the source.
        /// </summary>
        public string Source
        {
            get => InnerElement.src;
            set => InnerElement.src = value;
        }

        /// <summary>
        /// Gets or sets the CSS cursor of the component.
        /// </summary>
        public string Cursor
        {
            get => InnerElement.style.cursor;
            set => InnerElement.style.cursor = value;
        }

        /// <summary>
        /// Renders the component's root HTML element.
        /// </summary>
        public override HTMLElement Render()
        {
            return InnerElement;
        }

        /// <summary>
        /// Configures the component to position.
        /// </summary>
        public Image Position(string objectPosition)
        {
            InnerElement.style.objectPosition = objectPosition;
            return this;
        }

        /// <summary>
        /// Configures the component to contain.
        /// </summary>
        public Image Contain()
        {
            InnerElement.style.objectFit = "contain";
            return this;
        }

        /// <summary>
        /// Configures the component to cover.
        /// </summary>
        public Image Cover()
        {
            InnerElement.style.objectFit = "cover";
            return this;
        }

        /// <summary>
        /// Configures the component to fill.
        /// </summary>
        public Image Fill()
        {
            InnerElement.style.objectFit = "fill";
            return this;
        }

        /// <summary>
        /// Removes / disables the fit on the component.
        /// </summary>
        public Image NoFit()
        {
            InnerElement.style.objectFit = "none";
            return this;
        }

        /// <summary>
        /// Configures the scale down on the component.
        /// </summary>
        public Image ScaleDown()
        {
            InnerElement.style.objectFit = "scale-down";
            return this;
        }

        /// <summary>
        /// Configures the component to circle.
        /// </summary>
        public Image Circle()
        {
            InnerElement.style.borderRadius = "50%";
            PropagateToStackItemParent      = false;
            return this;
        }

        /// <summary>
        /// Configures the component to circle.
        /// </summary>
        public Image Circle(int pixels)
        {
            InnerElement.style.borderRadius = $"{pixels}px";
            return this;
        }
        /// <summary>
        /// Gets or sets the CSS background of the component.
        /// </summary>
        public string Background
        {
            get => InnerElement.style.background;
            set => InnerElement.style.background = value;
        }
    }
}