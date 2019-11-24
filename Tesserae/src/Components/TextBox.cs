using static Tesserae.HTML.HtmlUtil;
using static Tesserae.HTML.HtmlAttributes;
using static Retyped.dom;

namespace Tesserae.Components
{
    public class TextBox : ComponentBase<TextBox, HTMLInputElement>
    {
        #region Feilds

        private HTMLDivElement container;
        private HTMLSpanElement errorSpan;

        #endregion

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

        public string ErrorText
        {
            get { return errorSpan.innerText; }
            set
            {
                if (errorSpan.innerText != value)
                {
                    errorSpan.innerText = value;
                }
            }
        }

        public bool IsInvalid
        {
            get { return container.classList.contains("invalid"); }
            set
            {
                if (value != IsInvalid)
                {
                    if (value)
                    {
                        container.classList.add("invalid");
                    }
                    else
                    {
                        container.classList.remove("invalid");
                    }
                }
            }
        }
        public bool IsRequired
        {
            get { return container.classList.contains("mss-required"); }
            set
            {
                if (value != IsInvalid)
                {
                    if (value)
                    {
                        container.classList.add("mss-required");
                    }
                    else
                    {
                        container.classList.remove("mss-required");
                    }
                }
            }
        }
        #endregion

        public TextBox(string text = string.Empty)
        {
            InnerElement = TextBox(_("mss-textBox", type: "text", value: text));
            errorSpan = Span(_("mss-textBox-error"));
            container = Div(_("mss-textBox-container"), InnerElement, errorSpan);
            AttachChange();
            AttachInput();
            AttachFocus();
            AttachBlur();
        }

        public override HTMLElement Render()
        {
            return container;
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
        public static TextBox Required(this TextBox textBox)
        {
            textBox.IsRequired = true;
            return textBox;
        }
    }
}
