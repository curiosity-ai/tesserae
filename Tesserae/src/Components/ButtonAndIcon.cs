using System;
using static Transpose.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// A composite button that pairs a label with a leading icon, useful for command-bar and toolbar items.
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

        // The tone and the enabled state belong to the whole split button, so each one is carried to the
        // arrow half as well. The base constructor can set them before the arrow exists, hence the null checks.

        /// <summary>
        /// Gets or set whenever button is primary
        /// </summary>
        public override bool IsPrimary
        {
            get => base.IsPrimary;
            set
            {
                base.IsPrimary = value;
                if (_arrowButton is object) _arrowButton.IsPrimary = value;
            }
        }

        /// <summary>
        /// Gets or set whenever button is success
        /// </summary>
        public override bool IsSuccess
        {
            get => base.IsSuccess;
            set
            {
                base.IsSuccess = value;
                if (_arrowButton is object) _arrowButton.IsSuccess = value;
            }
        }

        /// <summary>
        /// Gets or set whenever button is danger
        /// </summary>
        public override bool IsDanger
        {
            get => base.IsDanger;
            set
            {
                base.IsDanger = value;
                if (_arrowButton is object) _arrowButton.IsDanger = value;
            }
        }

        /// <summary>
        /// Gets or sets whenever button is enabled
        /// </summary>
        public override bool IsEnabled
        {
            get => base.IsEnabled;
            set
            {
                base.IsEnabled = value;
                if (_arrowButton is object) _arrowButton.IsEnabled = value;
            }
        }

        /// <summary>
        /// Gets the button that renders the secondary (arrow) half, e.g. to anchor a menu or add a tooltip to it.
        /// </summary>
        public Button IconButton => _arrowButton;

        /// <summary>
        /// Sets the secondary icon of the component.
        /// </summary>
        public ButtonAndIcon SetSecondaryIcon(UIcons icon,string color = "", TextSize size = TextSize.Small, UIconsWeight weight = UIconsWeight.Regular)
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