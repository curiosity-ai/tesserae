using static Retyped.dom;
using static Tesserae.UI;

namespace Tesserae.Components
{
    public class Toggle : ComponentBase<Toggle, HTMLInputElement>, IObservableComponent<bool>
    {
        private readonly HTMLSpanElement _checkSpan;
        private readonly HTMLSpanElement _onOffSpan;
        private readonly HTMLLabelElement _label;
        private readonly IComponent _offText;
        private readonly IComponent _onText;
        private readonly SettableObservable<bool> _observable = new SettableObservable<bool>();

        public Toggle(IComponent onText = null, IComponent offText = null)
        {
            _onText = onText ?? TextBlock("On");
            _offText = offText ?? TextBlock("Off");
            InnerElement = CheckBox(_("tss-checkbox"));
            _checkSpan = Span(_("tss-toggle-mark"));
            _onOffSpan = Span(_("tss-text-ellipsis"), _offText.Render());
            _label = Label(_("tss-toggle-container"), InnerElement, _checkSpan, _onOffSpan);
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
            get => _label.innerText;
            set
            {
                _label.innerText = value;
                if (string.IsNullOrEmpty(value)) _onOffSpan.style.display = "";
                else _onOffSpan.style.display = "none";
            }
        }

        /// <summary>
        /// Gets or sets whenever Toggle is enabled
        /// </summary>
        public bool IsEnabled
        {
            get => !_label.classList.contains("tss-disabled");
            set
            {
                if (value)
                {
                    _label.classList.remove("tss-disabled");
                }
                else
                {
                    _label.classList.add("tss-disabled");
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
                _observable.Value = value;
                ClearChildren(_onOffSpan);
                if (value)
                {
                    _onOffSpan.appendChild(_onText);
                }
                else
                {
                    _onOffSpan.appendChild(_offText);
                }
            }
        }

        public override HTMLElement Render()
        {
            return _label;
        }

        private void OnToggleChanged()
        {
            ClearChildren(_onOffSpan);
            if (IsChecked)
            {
                _onOffSpan.appendChild(_onText);
            }
            else
            {
                _onOffSpan.appendChild(_offText);
            }
            _observable.Value = IsChecked;
        }

        public Toggle SetText(string text)
        {
            Text = text;
            return this;
        }

        public Toggle Disabled(bool value = true)
        {
            IsEnabled = !value;
            return this;
        }

        public Toggle Checked(bool value = true)
        {
            IsChecked = value;
            return this;
        }

        public IObservable<bool> AsObservable()
        {
            return _observable;
        }
    }
}
