using static Tesserae.UI;
using static Retyped.dom;

namespace Tesserae.Components
{
    public class CheckBox : ComponentBase<CheckBox, HTMLInputElement>
    {
        private readonly HTMLSpanElement _checkSpan;
        private readonly HTMLLabelElement _label;

        public CheckBox(string text = string.Empty)
        {
            InnerElement = CheckBox(_("tss-checkbox"));
            _checkSpan = Span(_("tss-checkbox-mark"));
            _label = Label(_("tss-checkbox-container", text: text), InnerElement, _checkSpan);
            AttachClick();
            AttachChange();
            AttachFocus();
            AttachBlur();
        }

        /// <summary>
        /// Gets or sets button text
        /// </summary>
        public string Text
        {
            get { return _label.innerText; }
            set { _label.innerText = value; }
        }

        /// <summary>
        /// Gets or sets whenever CheckBox is enabled
        /// </summary>
        public bool IsEnabled
        {
            get { return !_label.classList.contains("disabled"); }
            set
            {
                if (value != IsEnabled)
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

        public override HTMLElement Render()
        {
            return _label;
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
