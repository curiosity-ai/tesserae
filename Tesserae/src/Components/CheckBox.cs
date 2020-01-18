using static Tesserae.UI;
using static Retyped.dom;

namespace Tesserae.Components
{
    public class CheckBox : ComponentBase<CheckBox, HTMLInputElement>
    {
        #region Fields

        private HTMLSpanElement _CheckSpan;
        private HTMLLabelElement _Label;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets button text
        /// </summary>
        public string Text
        {
            get { return _Label.innerText; }
            set { _Label.innerText = value; }
        }

        /// <summary>
        /// Gets or sets whenever CheckBox is enabled
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
            InnerElement = CheckBox(_("tss-checkbox"));
            _CheckSpan = Span(_("tss-checkbox-mark"));
            _Label = Label(_("tss-checkbox-container", text: text), InnerElement, _CheckSpan);
            AttachClick();
            AttachChange();
            AttachFocus();
            AttachBlur();
        }

        public override HTMLElement Render()
        {
            return _Label;
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
