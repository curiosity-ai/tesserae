using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// A Toggle component that allows users to switch between two states (on/off).
    /// </summary>
    [H5.Name("tss.Toggle")]
    public class Toggle : ComponentBase<Toggle, HTMLInputElement>, IObservableComponent<bool>, IRoundedStyle, ITextFormating
    {
        private readonly HTMLElement              _checkElement;
        private readonly HTMLElement              _onOffSpan;
        private readonly HTMLElement              _container;
        private readonly IComponent               _offText;
        private readonly IComponent               _onText;
        private readonly SettableObservable<bool> _observable;

        /// <summary>
        /// Initializes a new instance of the Toggle class.
        /// </summary>
        /// <param name="onText">The text to display when the toggle is on.</param>
        /// <param name="offText">The text to display when the toggle is off.</param>
        public Toggle(IComponent onText = null, IComponent offText = null)
        {
            _onText       = onText  ?? TextBlock("On");
            _offText      = offText ?? TextBlock("Off");
            InnerElement  = CheckBox(_("tss-checkbox"));
            InnerElement.setAttribute("role", "switch");
            _checkElement = Div(_("tss-toggle-mark"));
            _onOffSpan    = Div(_("tss-toggle-text"),                                   _offText.Render());
            _container    = Div(_("tss-toggle-container tss-default-component-margin " + TextSize.Small.ToString() + " " + TextWeight.Regular.ToString()), InnerElement, _checkElement, _onOffSpan);

            _observable = new SettableObservable<bool>();

            _container.onclick += (e) =>
            {
                StopEvent(e);
                IsChecked = !IsChecked;
                OnToggleChanged();
                RaiseOnChange(ev: null);
            };

            OnChange((s, e) => OnToggleChanged());
            AttachClick();
            AttachChange();
            AttachFocus();
            AttachBlur();
        }

        /// <summary>
        /// Gets or sets toggle text
        /// </summary>
        public string Text
        {
            get => _container.innerText;
            set
            {
                _container.innerText = value;

                if (string.IsNullOrEmpty(value)) _onOffSpan.style.display = "";
                else _onOffSpan.style.display                             = "none";
            }
        }

        /// <summary>
        /// Gets or sets whenever Toggle is enabled
        /// </summary>
        public bool IsEnabled
        {
            get => !_container.classList.contains("tss-disabled");
            set
            {
                if (value)
                {
                    _container.classList.remove("tss-disabled");
                }
                else
                {
                    _container.classList.add("tss-disabled");
                }
            }
        }

        /// <summary>
        /// Gets or sets whenever Toggle is checked
        /// </summary>
        public bool IsChecked
        {
            get => InnerElement.@checked;
            set
            {
                InnerElement.@checked = value;
                _observable.Value     = value;
                ClearChildren(_onOffSpan);

                if (value)
                {
                    _onOffSpan.appendChild(_onText.Render());
                }
                else
                {
                    _onOffSpan.appendChild(_offText.Render());
                }
            }
        }

        /// <summary>
        /// Renders the toggle component.
        /// </summary>
        /// <returns>The rendered HTMLElement.</returns>
        public override HTMLElement Render()
        {
            return _container;
        }

        private void OnToggleChanged()
        {
            ClearChildren(_onOffSpan);

            if (IsChecked)
            {
                _onOffSpan.appendChild(_onText.Render());
            }
            else
            {
                _onOffSpan.appendChild(_offText.Render());
            }
            _observable.Value = IsChecked;
        }

        /// <summary>
        /// Sets the text of the toggle.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>The current instance of the type.</returns>
        public Toggle SetText(string text)
        {
            Text = text;
            return this;
        }

        /// <summary>
        /// Sets whether the toggle is disabled.
        /// </summary>
        /// <param name="value">Whether to disable the toggle.</param>
        /// <returns>The current instance of the type.</returns>
        public Toggle Disabled(bool value = true)
        {
            IsEnabled = !value;
            return this;
        }

        /// <summary>
        /// Sets whether the toggle is checked.
        /// </summary>
        /// <param name="value">Whether to check the toggle.</param>
        /// <returns>The current instance of the type.</returns>
        public Toggle Checked(bool value = true)
        {
            IsChecked = value;
            return this;
        }

        /// <summary>
        /// Returns an observable that tracks the checked state of the toggle.
        /// </summary>
        /// <returns>An observable.</returns>
        public IObservable<bool> AsObservable()
        {
            return _observable;
        }

        /// <summary>Gets or sets the text size.</summary>
        public TextSize Size
        {
            get => ITextFormatingExtensions.FromClassList(_container, TextSize.Small);
            set
            {
                _container.classList.remove(Size.ToString());
                _container.classList.add(value.ToString());
            }
        }

        /// <summary>Gets or sets the text weight.</summary>
        public TextWeight Weight
        {
            get => ITextFormatingExtensions.FromClassList(_container, TextWeight.Regular);
            set
            {
                _container.classList.remove(Weight.ToString());
                _container.classList.add(value.ToString());
            }
        }

        /// <summary>Gets or sets the text alignment.</summary>
        public TextAlign TextAlign
        {
            get => ITextFormatingExtensions.FromClassList(_container, TextAlign.Left);
            set
            {
                _container.classList.remove(TextAlign.ToString());
                _container.classList.add(value.ToString());
            }
        }
    }
}