using static Tesserae.HTML.HtmlUtil;
using static Tesserae.HTML.HtmlAttributes;
using static Retyped.dom;

namespace Tesserae.Components
{
    public class Toggle : ComponentBase<Toggle, HTMLInputElement>
    {
        #region Fields

        private HTMLSpanElement checkSpan;
        private HTMLSpanElement onOffSpan;
        private HTMLLabelElement label;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets toggle text
        /// </summary>
        public string Text
        {
            get { return label.innerText; }
            set
            {
                label.innerText = value;
                if (string.IsNullOrEmpty(value)) onOffSpan.style.display = "";
                else onOffSpan.style.display = "none";
            }
        }

        /// <summary>
        /// Gets or sets whenever Toggle is enabled
        /// </summary>
        public bool IsEnabled
        {
            get { return !label.classList.contains("disabled"); }
            set
            {
                if (value != IsEnabled)
                {
                    if (value)
                    {
                        label.classList.remove("disabled");
                    }
                    else
                    {
                        label.classList.add("disabled");
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
                }
            }
        }

        #endregion

        public Toggle(string text = string.Empty)
        {
            InnerElement = CheckBox(_("mss-checkbox"));
            checkSpan = Span(_("mss-toggle-mark"));
            onOffSpan = Span(_(text: "Off"));
            if (!string.IsNullOrEmpty(text)) onOffSpan.style.display = "none";
            label = Label(_("m-1 mss-toggle-container", text: text), InnerElement, checkSpan, onOffSpan);
            OnChange += OnChanged;
            AttachClick();
            AttachChange();
            AttachFocus();
            AttachBlur();
        }

        private void OnChanged(object sender, Toggle e)
        {
            if (e.IsChecked) onOffSpan.innerText = "On";
            else onOffSpan.innerText = "Off";
        }

        public override HTMLElement Render()
        {
            return label;
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
