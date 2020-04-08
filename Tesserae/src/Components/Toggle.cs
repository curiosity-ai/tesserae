using static Retyped.dom;
using static Tesserae.UI;

namespace Tesserae.Components
{
    public class Toggle : ComponentBase<Toggle, HTMLInputElement>, IObservableComponent<bool>
    {
        private HTMLSpanElement _checkSpan;
        private HTMLSpanElement _onOffSpan;
        private HTMLLabelElement _label;
        private string _offText;
        private string _onText;
        private readonly SettableObservable<bool> _observable = new SettableObservable<bool>();

        public Toggle(string text = null, string onText = null, string offText = null)
        {
            _onText = onText ?? "On";
            _offText = offText ?? "Off";
            InnerElement = CheckBox(_("tss-checkbox"));
            _checkSpan = Span(_("tss-toggle-mark"));
            _onOffSpan = Span(_("tss-text-ellipsis", text: _offText));
            if (!string.IsNullOrEmpty(text)) _onOffSpan.style.display = "none";
            _label = Label(_("tss-toggle-container", text: text), InnerElement, _checkSpan, _onOffSpan);
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
            get => !_label.classList.contains("disabled");
            set
            {
                if (value)
                {
                    _label.classList.remove("disabled");
                }
                else
                {
                    _label.classList.add("disabled");
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
                if (value) _onOffSpan.innerText = _onText;
                else _onOffSpan.innerText = _offText;
            }
        }

        public override HTMLElement Render()
        {
            return _label;
        }

        private void OnToggleChanged()
        {
            _onOffSpan.innerText = IsChecked ? _onText : _offText;
            _observable.Value = IsChecked;
        }
        public Toggle SetText(string text)
        {
            Text = text;
            return this;
        }

        public Toggle Disabled()
        {
            IsEnabled = false;
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
