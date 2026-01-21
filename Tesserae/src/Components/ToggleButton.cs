using static H5.Core.dom;

namespace Tesserae
{
    /// <summary>
    /// A ToggleButton component that behaves like a button but maintains a checked state.
    /// </summary>
    [H5.Name("tss.ToggleButton")]
    public class ToggleButton : IComponent
    {
        private Button _button;

        /// <summary>
        /// Event fired when the checked state of the toggle button changes.
        /// </summary>
        protected event ComponentEventHandler<ToggleButton, Event> Changed;

        /// <summary>
        /// Gets or sets whether the toggle button is checked.
        /// </summary>
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

        /// <summary>
        /// Initializes a new instance of the ToggleButton class.
        /// </summary>
        /// <param name="button">The button to use as the base for the toggle button.</param>
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

        /// <summary>
        /// Adds a change event handler to the toggle button.
        /// </summary>
        /// <param name="onChange">The event handler.</param>
        /// <returns>The current instance of the type.</returns>
        public ToggleButton OnChange(ComponentEventHandler<ToggleButton, Event> onChange)
        {
            Changed += onChange;
            return this;
        }

        /// <summary>
        /// Renders the toggle button.
        /// </summary>
        /// <returns>The rendered HTMLElement.</returns>
        public HTMLElement Render()
        {
            return ((IComponent)_button).Render();
        }

        /// <summary>
        /// Sets whether the toggle button is disabled.
        /// </summary>
        /// <param name="value">Whether to disable the toggle button.</param>
        /// <returns>The current instance of the type.</returns>
        public ToggleButton Disabled(bool value = true)
        {
            IsEnabled = !value;
            return this;
        }

        /// <summary>
        /// Sets whether the toggle button is checked.
        /// </summary>
        /// <param name="value">Whether to check the toggle button.</param>
        /// <returns>The current instance of the type.</returns>
        public ToggleButton Checked(bool value = true)
        {
            IsChecked = value;
            return this;
        }

        /// <summary>
        /// Gets or sets whether the toggle button is enabled.
        /// </summary>
        public bool IsEnabled
        {
            get => _button.IsEnabled;
            set => _button.IsEnabled = value;
        }
    }
}