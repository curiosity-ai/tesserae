using System;
using System.Linq;

namespace Tesserae
{
    public class TextBox : Input<TextBox>, ITextFormating, IHasBackgroundColor, IHasForegroundColor
    {
        public TextBox(string text = string.Empty) : base("text", text) 
        {
            InnerElement.classList.add("tss-fontsize-small");
            InnerElement.classList.add("tss-fontweight-regular");
        }

        public string Placeholder
        {
            get => InnerElement.placeholder;
            set => InnerElement.placeholder = value;
        }

        public bool IsReadOnly
        {
            get => InnerElement.hasAttribute("readonly");
            set
            {
                if (value)
                {
                    InnerElement.setAttribute("readonly", string.Empty);
                }
                else
                {
                    InnerElement.removeAttribute("readonly");
                }
            }
        }

        // Could have a separate type for Password?
        public bool IsPassword
        {
            get => InnerElement.type == "password";
            set
            {
                if (value)
                {
                    InnerElement.type = "password";
                }
                else
                {
                    InnerElement.type = string.Empty;
                }
            }
        }

        public virtual TextSize Size
        {
            get => ITextFormatingExtensions.FromClassList(InnerElement, TextSize.Small);
            set
            {
                InnerElement.classList.remove(Size.ToString());
                InnerElement.classList.add(value.ToString());
            }
        }

        public virtual TextWeight Weight
        {
            get => ITextFormatingExtensions.FromClassList(InnerElement, TextWeight.Regular);
            set
            {
                InnerElement.classList.remove(Weight.ToString());
                InnerElement.classList.add(value.ToString());
            }
        }

        public TextAlign TextAlign
        {
            get
            {
                return ITextFormatingExtensions.FromClassList(InnerElement, TextAlign.Left);
            }
            set
            {
                InnerElement.classList.remove(TextAlign.ToString());
                InnerElement.classList.add(value.ToString());
            }
        }

        public string Background
        {
            get => InnerElement.style.background;
            set => InnerElement.style.background = value;
        }

        public string Foreground
        {
            get => InnerElement.style.color;
            set => InnerElement.style.color = value;
        }

        public TextBox SetPlaceholder(string placeholder)
        {
            Placeholder = placeholder;
            return this;
        }

        public TextBox ReadOnly()
        {
            IsReadOnly = true;
            return this;
        }

        public TextBox Password()
        {
            IsPassword = true;
            return this;
        }

        public TextBox NoBorder()
        {
            InnerElement.classList.add("tss-textbox-noborder");
            return this;
        }

        public TextBox UnlockHeight()
        {
            InnerElement.classList.add("tss-textbox-h100");
            return this;
        }
    }
}