using static Tesserae.HTML.HtmlUtil;
using static Tesserae.HTML.HtmlAttributes;
using static Retyped.dom;

namespace Tesserae.Components
{
    public class TextBox : ComponentBase<TextBox, HTMLInputElement>
    {
        #region Properties

        public bool IsEnabled
        {
            get { return !InnerElement.classList.contains("disabled"); }
            set
            {
                if (value != IsEnabled)
                {
                    if (value)
                    {
                        InnerElement.classList.remove("disabled");
                    }
                    else
                    {
                        InnerElement.classList.add("disabled");
                    }
                }
            }
        }
        public string Text
        {
            get { return InnerElement.value; }
            set
            {
                if (InnerElement.value != value)
                {
                    InnerElement.value = value;
                    RaiseOnInput(null);
                }
            }
        }

        public bool IsInvalid
        {
            get { return InnerElement.classList.contains("invalid"); }
            set
            {
                if (value != IsInvalid)
                {
                    if (value)
                    {
                        InnerElement.classList.add("invalid");
                    }
                    else
                    {
                        InnerElement.classList.remove("invalid");
                    }
                }
            }
        }

        #endregion

        public TextBox(string text = string.Empty)
        {
            InnerElement = TextBox(_("mss-textBox", type: "text", value: text));
            AttachChange();
            AttachInput();
            AttachFocus();
            AttachBlur();
        }

        public override HTMLElement Render()
        {
            return InnerElement;
        }
    }

    public static class TextBoxExtensions
    {
        public static TextBox Text(this TextBox textBox, string text)
        {
            textBox.Text = text;
            return textBox;
        }

        public static TextBox Disabled(this TextBox textBox)
        {
            textBox.IsEnabled = false;
            return textBox;
        }

        public static TextBox Invalid(this TextBox textBox)
        {
            textBox.IsInvalid = true;
            return textBox;
        }
    }
}
