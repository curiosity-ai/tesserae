using System;
using static Retyped.dom;
using static Tesserae.HTML.HtmlAttributes;
using static Tesserae.HTML.HtmlUtil;

namespace Tesserae.Components
{
    public class TextBox : ComponentBase<TextBox, HTMLInputElement>, ICanValidate<TextBox>
    {
        #region Fields

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

        public bool IsReadOnly
        {
            get { return InnerElement.hasAttribute("readonly"); }
            set
            {
                if (IsReadOnly != value)
                {
                    if (value) InnerElement.removeAttribute("readonly");
                    else InnerElement.setAttribute("readonly", "");
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

        public string Placeholder
        {
            get { return InnerElement.placeholder; }
            set { InnerElement.placeholder = value; }
        }

        public string Error
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
            get { return container.classList.contains("tss-required"); }
            set
            {
                if (value != IsInvalid)
                {
                    if (value)
                    {
                        container.classList.add("tss-required");
                    }
                    else
                    {
                        container.classList.remove("tss-required");
                    }
                }
            }
        }
        #endregion

        public TextBox(string text = string.Empty)
        {
            InnerElement = TextBox(_("tss-textBox", type: "text", value: text));
            errorSpan = Span(_("tss-textBox-error"));
            container = Div(_("tss-textBox-container"), InnerElement, errorSpan);
            AttachChange();
            AttachInput();
            AttachFocus();
            AttachBlur();
        }

        public override HTMLElement Render()
        {
            return container;
        }

        public void Attach(EventHandler<TextBox> handler, Validation.Mode mode)
        {
            if (mode == Validation.Mode.OnBlur)
            {
                OnChange += handler;
            }
            else
            {
                OnInput += handler;
            }
        }
    }

    public static class TextBoxExtensions
    {
        public static TextBox Text(this TextBox textBox, string text)
        {
            textBox.Text = text;
            return textBox;
        }

        public static TextBox Placeholder(this TextBox textBox, string error)
        {
            textBox.Placeholder = error;
            return textBox;
        }

        public static TextBox Disabled(this TextBox textBox)
        {
            textBox.IsEnabled = false;
            return textBox;
        }

        public static TextBox ReadOnly(this TextBox textBox)
        {
            textBox.IsReadOnly = true;
            return textBox;
        }

        public static TextBox Required(this TextBox textBox)
        {
            textBox.IsRequired = true;
            return textBox;
        }
    }
}
