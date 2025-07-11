using System;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    [H5.Name("tss.ButtonAndIcon")]
    public class ButtonAndIcon : Button
    {
        public delegate void IconClickHandler(ButtonAndIcon mainButton, Button iconButton, MouseEvent e);

        private readonly HTMLElement _parent;
        private readonly Button _arrowButton;
        public ButtonAndIcon(string text, IconClickHandler onIconClick, UIcons mainIcon = UIcons.Circle, UIcons secondaryIcon = UIcons.AngleDown)
        {
            SetIcon(mainIcon);
            Ellipsis();
            _parent = Div(_("tss-btn-and-icon tss-default-component-no-margin"));
            _arrowButton = Button().SetIcon(secondaryIcon).Class("tss-btn-and-icon-icon").OnClick((b, e) =>
            {
                StopEvent(e);
                onIconClick(this, b, e);
            });
            _parent.appendChild(InnerElement);
            _parent.appendChild(_arrowButton.Render());
        }

        public ButtonAndIcon SetSecondaryIcon(UIcons icon, string color = "", TextSize size = TextSize.Small, UIconsWeight weight = UIconsWeight.Regular)
        {
            _arrowButton.SetIcon(icon, color, size, weight);
            return this;
        }
        public ButtonAndIcon SetSecondaryIcon(Emoji icon)
        {
            _arrowButton.SetIcon(icon);
            return this;
        }

        public ButtonAndIcon MaxWidth(UnitSize maxWidth)
        {
            InnerElement.style.maxWidth = maxWidth.ToString();
            return this;
        }
        public ButtonAndIcon MinWidth(UnitSize minWidth)
        {
            InnerElement.style.minWidth = minWidth.ToString();
            return this;
        }

        public override HTMLElement Render() => _parent;
    }
}