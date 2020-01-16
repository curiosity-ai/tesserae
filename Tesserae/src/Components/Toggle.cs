using static Tesserae.HTML.HtmlUtil;
using static Tesserae.HTML.HtmlAttributes;
using static Retyped.dom;

namespace Tesserae.Components
{
    public class Toggle : ComponentBase<Toggle, HTMLInputElement>
    {
        #region Fields

        private HTMLSpanElement _CheckSpan;
        private HTMLSpanElement _OnOffSpan;
        private HTMLLabelElement _Label;

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
                    if (value) _OnOffSpan.innerText = "On";
                    else _OnOffSpan.innerText = "Off";
                }
            }
        }

        #endregion

        public Toggle(string text = string.Empty)
        {
            InnerElement = CheckBox(_("tss-checkbox"));
            _CheckSpan = Span(_("tss-toggle-mark"));
            _OnOffSpan = Span(_(text: "Off"));
            if (!string.IsNullOrEmpty(text)) _OnOffSpan.style.display = "none";
            _Label = Label(_("tss-toggle-container", text: text), InnerElement, _CheckSpan, _OnOffSpan);
            OnChange += OnChanged;
            AttachClick();
            AttachChange();
            AttachFocus();
            AttachBlur();
        }

        private void OnChanged(object sender, Toggle e)
        {
            if (e.IsChecked) _OnOffSpan.innerText = "On";
            else _OnOffSpan.innerText = "Off";
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

        public static Toggle Checked(this Toggle toggle)
        {
            toggle.IsChecked = true;
            return toggle;
        }
    }
}
