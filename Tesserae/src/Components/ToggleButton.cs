using static H5.Core.dom;

namespace Tesserae
{
    [H5.Name("tss.ToggleButton")]
    public class ToggleButton : IComponent
    {
        private Button _button;

        protected event ComponentEventHandler<ToggleButton, Event> Changed;

        public bool IsChecked
        {
            get
            {
                return !_button.Render().classList.contains("tss-toggle-btn-unchecked");
            }
            set
            {
                var current = IsChecked;

                if (value)
                {
                    _button.Render().classList.remove("tss-toggle-btn-unchecked");
                }
                else
                {
                    _button.Render().classList.add("tss-toggle-btn-unchecked");
                }
            }
        }

        public ToggleButton(Button button)
        {
            _button = button;
            _button.OnClick(() =>
            {
                IsChecked = !IsChecked;
                Changed?.Invoke(this, null);
            });

            IsChecked = false;
        }

        public ToggleButton OnChange(ComponentEventHandler<ToggleButton, Event> onChange)
        {
            Changed += onChange;
            return this;
        }

        public HTMLElement Render()
        {
            return ((IComponent) _button).Render();
        }

        public ToggleButton Disabled(bool value = true)
        {
            IsEnabled = !value;
            return this;
        }

        public ToggleButton Checked(bool value = true)
        {
            IsChecked = value;
            return this;
        }

        public bool IsEnabled
        {
            get => _button.IsEnabled;
            set => _button.IsEnabled = value;
        }
    }
}