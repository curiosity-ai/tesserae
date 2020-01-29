using static Tesserae.UI;
using static Retyped.dom;

namespace Tesserae.Components
{
    public class Toggle : ComponentBase<Toggle, HTMLInputElement>
    {
        #region Fields

        private HTMLSpanElement _CheckSpan;
        private HTMLSpanElement _OnOffSpan;
        private HTMLLabelElement _Label;
        private string OffText;
        private string OnText;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets toggle text
        /// </summary>
        public string Text
        {
            get { return _Label.innerText; }
            set
            {
                _Label.innerText = value;
                if (string.IsNullOrEmpty(value)) _OnOffSpan.style.display = "";
                else _OnOffSpan.style.display = "none";
            }
        }

        /// <summary>
        /// Gets or sets whenever Toggle is enabled
        /// </summary>
        public bool IsEnabled
        {
            get { return !_Label.classList.contains("disabled"); }
            set
            {
                if (value != IsEnabled)
                {
                    if (value)
                    {
                        _Label.classList.remove("disabled");
                    }
                    else
                    {
                        _Label.classList.add("disabled");
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets whenever Toggle is checked
        /// </summary>
        public bool IsChecked
        {
            get { return InnerElement.@checked; }
            set
            {
                if (value != IsChecked)
                {
                    InnerElement.@checked = value;
                    if (value) _OnOffSpan.innerText = OnText;
                    else _OnOffSpan.innerText = OffText;
                }
            }
        }

        #endregion

        public Toggle(string text = null, string onText = null, string offText = null)
        {
            OnText = onText ?? "On";
            OffText = offText ?? "Off";
            InnerElement = CheckBox(_("tss-checkbox"));
            _CheckSpan = Span(_("tss-toggle-mark"));
            _OnOffSpan = Span(_(text: OffText));
            if (!string.IsNullOrEmpty(text)) _OnOffSpan.style.display = "none";
            _Label = Label(_("tss-toggle-container", text: text), InnerElement, _CheckSpan, _OnOffSpan);
            OnChange((s,e) => OnToggleChanged());
            AttachClick();
            AttachChange();
            AttachFocus();
            AttachBlur();
        }

        private void OnToggleChanged()
        {
            _OnOffSpan.innerText = IsChecked ? OnText : OffText;
        }

        public override HTMLElement Render()
        {
            return _Label;
        }
    }

    public static class ToggleExtensions
    {
        public static Toggle Text(this Toggle toggle, string text)
        {
            toggle.Text = text;
            return toggle;
        }

        public static Toggle Disabled(this Toggle toggle)
        {
            toggle.IsEnabled = false;
            return toggle;
        }

        public static Toggle Checked(this Toggle toggle, bool value = true)
        {
            toggle.IsChecked = value;
            return toggle;
        }
    }
}
