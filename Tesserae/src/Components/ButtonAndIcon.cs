using System;
using static Transpose.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// A split button: the main half runs the common action (<see cref="Button.OnClick(Action)"/>), the arrow half
    /// a complementary one, often a menu of alternatives anchored on it.
    /// </summary>
    [Transpose.Name("tss.ButtonAndIcon")]
    public class ButtonAndIcon : Button
    {
        public delegate void IconClickHandler(ButtonAndIcon mainButton, Button iconButton, MouseEvent e);

        private readonly HTMLElement _parent;
        private readonly Button _arrowButton;
        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        public ButtonAndIcon(string text, IconClickHandler onIconClick, UIcons mainIcon = UIcons.Circle, UIcons secondaryIcon = UIcons.AngleDown)
        {
            SetText(text);
            SetIcon(mainIcon);
            Ellipsis();
            _parent = Div(Att("tss-btn-and-icon tss-default-component-no-margin"));
            _arrowButton = Button().SetIcon(secondaryIcon).Class("tss-btn-and-icon-icon").OnClick((b, e) =>
            {
                StopEvent(e);
                onIconClick(this, b, e);
            });
            _parent.appendChild(InnerElement);
            _parent.appendChild(_arrowButton.Render());
        }

        // The tone and the enabled state belong to the whole split button, so each one is carried to the arrow half too.

        /// <summary>
        /// Gets or sets whether both halves use the primary tone
        /// </summary>
        public override bool IsPrimary
        {
            get => base.IsPrimary;
            set
            {
                base.IsPrimary = value;
                _arrowButton.IsPrimary = value;
            }
        }

        /// <summary>
        /// Gets or sets whether both halves use the success tone
        /// </summary>
        public override bool IsSuccess
        {
            get => base.IsSuccess;
            set
            {
                base.IsSuccess = value;
                _arrowButton.IsSuccess = value;
            }
        }

        /// <summary>
        /// Gets or sets whether both halves use the danger tone
        /// </summary>
        public override bool IsDanger
        {
            get => base.IsDanger;
            set
            {
                base.IsDanger = value;
                _arrowButton.IsDanger = value;
            }
        }

        /// <summary>
        /// Gets or sets whether both halves are enabled
        /// </summary>
        public override bool IsEnabled
        {
            get => base.IsEnabled;
            set
            {
                base.IsEnabled = value;
                _arrowButton.IsEnabled = value;
            }
        }

        /// <summary>
        /// Gets the button that renders the secondary (arrow) half, e.g. to anchor a menu or add a tooltip to it.
        /// </summary>
        public Button IconButton => _arrowButton;

        /// <summary>
        /// Sets the secondary icon of the component.
        /// </summary>
        public ButtonAndIcon SetSecondaryIcon(UIcons icon, string color = "", TextSize size = TextSize.Small, UIconsWeight weight = UIconsWeight.Regular)
        {
            _arrowButton.SetIcon(icon, color, size, weight);
            return this;
        }
        /// <summary>
        /// Sets the secondary icon of the component.
        /// </summary>
        public ButtonAndIcon SetSecondaryIcon(Emoji icon)
        {
            _arrowButton.SetIcon(icon);
            return this;
        }

        /// <summary>
        /// Gets or sets the CSS max-width of the component.
        /// </summary>
        public ButtonAndIcon MaxWidth(UnitSize maxWidth)
        {
            InnerElement.style.maxWidth = maxWidth.ToString();
            return this;
        }
        /// <summary>
        /// Gets or sets the CSS min-width of the component.
        /// </summary>
        public ButtonAndIcon MinWidth(UnitSize minWidth)
        {
            InnerElement.style.minWidth = minWidth.ToString();
            return this;
        }

        /// <summary>
        /// Renders the component's root HTML element.
        /// </summary>
        public override HTMLElement Render() => _parent;
    }
}