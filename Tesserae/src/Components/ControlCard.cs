using System;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    [H5.Name("tss.ControlCard")]
    public class ControlCard : ComponentBase<ControlCard, HTMLDivElement>
    {
        private readonly Card _card;
        private readonly Stack _mainStack;
        private readonly Stack _textStack;
        private readonly HTMLDivElement _controlContainer;
        private readonly TextBlock _label;
        private readonly TextBlock _description;
        private IComponent _control;
        private bool _showAsSelectedWhenChecked = true;

        public ControlCard(IComponent control, string label = null, string description = null)
        {
            _control = control;
            _controlContainer = Div(_("tss-controlcard-control"));
            if (control != null)
            {
                _controlContainer.appendChild(control.Render());
            }

            _label = TextBlock(label).MediumPlus().SemiBold();
            _description = TextBlock(description).Small().Foreground(Theme.Secondary.Foreground);

            _textStack = VStack().Class("tss-controlcard-text").JustifyContent(ItemJustify.Center);
            if (!string.IsNullOrEmpty(label)) _textStack.Children(_label);
            if (!string.IsNullOrEmpty(description)) _textStack.Children(_description);

            _mainStack = HStack().AlignCenter().Class("tss-controlcard");
            _mainStack.Render().style.gap = "16px";
            _mainStack.Render().style.width = "100%";
            _mainStack.Children(Raw(_controlContainer), _textStack);

            _card = Card(_mainStack).Padding("16px");

            _card.OnClick((s, e) =>
            {
                if (e.target is HTMLInputElement) return; // if clicked directly on input, browser handles it

                if (_control is CheckBox cb)
                {
                    cb.IsChecked = !cb.IsChecked;
                    UpdateSelectionState(cb.IsChecked);
                }
                else if (_control is Toggle toggle)
                {
                    toggle.IsChecked = !toggle.IsChecked;
                    UpdateSelectionState(toggle.IsChecked);
                }
            });

            if (_control is CheckBox checkbox)
            {
                checkbox.OnChange((s, e) => UpdateSelectionState(checkbox.IsChecked));
            }
            else if (_control is Toggle tog)
            {
                tog.OnChange((s, e) => UpdateSelectionState(tog.IsChecked));
            }

            InnerElement = Div(_("tss-controlcard-wrapper", styles: s => s.cursor = "pointer"), _card.Render());
        }

        private void UpdateSelectionState(bool isChecked)
        {
            if (_showAsSelectedWhenChecked)
            {
                if (isChecked)
                {
                    _card.Render().style.border = "1px solid var(--tss-primary-background-color)";
                    _card.Render().style.backgroundColor = "rgba(var(--tss-primary-background-color-rgb), 0.05)";
                }
                else
                {
                    _card.Render().style.border = "";
                    _card.Render().style.backgroundColor = "";
                }
            }
        }

        public ControlCard Label(string label)
        {
            _label.Text = label;
            if (string.IsNullOrEmpty(label))
            {
                _textStack.Remove(_label);
            }
            else
            {
                _textStack.Remove(_label);
                _textStack.InsertBefore(_label, _description);
            }
            return this;
        }

        public ControlCard Description(string description)
        {
            _description.Text = description;
            if (string.IsNullOrEmpty(description))
            {
                _textStack.Remove(_description);
            }
            else
            {
                _textStack.Remove(_description);
                _textStack.Children(_description);
            }
            return this;
        }

        public ControlCard ShowAsSelectedWhenChecked(bool show)
        {
            _showAsSelectedWhenChecked = show;

            bool isChecked = false;
            if (_control is CheckBox cb) isChecked = cb.IsChecked;
            else if (_control is Toggle toggle) isChecked = toggle.IsChecked;

            UpdateSelectionState(isChecked);
            return this;
        }

        public override HTMLElement Render()
        {
            return InnerElement;
        }
    }
}