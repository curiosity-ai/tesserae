using static Tesserae.HTML.HtmlUtil;
using static Tesserae.HTML.HtmlAttributes;
using static Retyped.dom;

namespace Tesserae.Components
{
    public class CheckBox : ComponentBase<CheckBox, HTMLInputElement>
    {
        #region Fields

        private HTMLSpanElement checkSpan;
        private HTMLLabelElement label;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets button text
        /// </summary>
        public string Text
        {
            get { return label.innerText; }
            set { label.innerText = value; }
        }

        /// <summary>
        /// Gets or sets whenever CheckBox is enabled
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
        /// Gets or sets whenever CheckBox is checked
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

        public CheckBox(string text = string.Empty)
        {
            InnerElement = CheckBox(_("mss-checkbox"));
            checkSpan = Span(_("mss-checkbox-mark"));
            label = Label(_("m-1 mss-checkbox-container", text: text), InnerElement, checkSpan);
            AttachClick();
            AttachChange();
            AttachFocus();
            AttachBlur();
        }

        public override HTMLElement Render()
        {
            return label;
        }
    }

    public static class CheckBoxExtensions
    {
        public static CheckBox Text(this CheckBox checkBox, string text)
        {
            checkBox.Text = text;
            return checkBox;
        }

        public static CheckBox Disabled(this CheckBox checkBox)
        {
            checkBox.IsEnabled = false;
            return checkBox;
        }

        public static CheckBox Checked(this CheckBox checkBox)
        {
            checkBox.IsChecked = true;
            return checkBox;
        }
    }
}
