using System;
using System.Linq;

namespace Tesserae
{
    /// <summary>
    /// A single-line text input component.
    /// </summary>
    [H5.Name("tss.TextBox")]
    public class TextBox : Input<TextBox>, ITextFormating, IHasBackgroundColor, IHasForegroundColor, IRoundedStyle
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TextBox"/> class.
        /// </summary>
        /// <param name="text">The initial text.</param>
        public TextBox(string text = string.Empty) : base("text", text)
        {
            InnerElement.classList.add("tss-fontsize-small");
            InnerElement.classList.add("tss-fontweight-regular");
        }

        /// <summary>Gets or sets the placeholder text.</summary>
        public string Placeholder
        {
            get => InnerElement.placeholder;
            set => InnerElement.placeholder = value;
        }

        /// <summary>Gets or sets whether the text box is read-only.</summary>
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

        /// <summary>Gets or sets the maximum length of the text.</summary>
        public int MaxLength
        {
            get => InnerElement.maxLength;
            set => InnerElement.maxLength = value;
        }

        /// <summary>Gets or sets whether the text box is in password mode.</summary>
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

        /// <summary>Gets or sets the text size.</summary>
        public virtual TextSize Size
        {
            get => ITextFormatingExtensions.FromClassList(InnerElement, TextSize.Small);
            set
            {
                InnerElement.classList.remove(Size.ToString());
                InnerElement.classList.add(value.ToString());
            }
        }

        /// <summary>Gets or sets the text weight.</summary>
        public virtual TextWeight Weight
        {
            get => ITextFormatingExtensions.FromClassList(InnerElement, TextWeight.Regular);
            set
            {
                InnerElement.classList.remove(Weight.ToString());
                InnerElement.classList.add(value.ToString());
            }
        }

        /// <summary>Gets or sets the text alignment.</summary>
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

        /// <summary>Gets or sets the background color.</summary>
        public string Background
        {
            get => InnerElement.style.background;
            set => InnerElement.style.background = value;
        }

        /// <summary>Gets or sets the foreground color.</summary>
        public string Foreground
        {
            get => InnerElement.style.color;
            set => InnerElement.style.color = value;
        }

        /// <summary>
        /// Sets the placeholder text.
        /// </summary>
        /// <param name="placeholder">The placeholder text.</param>
        /// <returns>The current instance.</returns>
        public TextBox SetPlaceholder(string placeholder)
        {
            Placeholder = placeholder;
            return this;
        }

        /// <summary>Sets the text box as read-only.</summary>
        public TextBox ReadOnly()
        {
            IsReadOnly = true;
            return this;
        }

        /// <summary>Sets the text box to password mode.</summary>
        public TextBox Password()
        {
            IsPassword = true;
            return this;
        }

        /// <summary>Removes the border from the text box.</summary>
        public TextBox NoBorder()
        {
            InnerElement.classList.add("tss-textbox-noborder");
            return this;
        }

        /// <summary>Removes the default minimum width.</summary>
        public TextBox NoMinWidth()
        {
            InnerElement.size = 1; //Remove the input min-width derived from the size attribute, see https://stackoverflow.com/questions/29470676/why-doesnt-the-input-element-respect-min-width
            return this;
        }

        /// <summary>Unlocks the height restriction.</summary>
        public TextBox UnlockHeight()
        {
            InnerElement.classList.add("tss-textbox-h100");
            return this;
        }
    }
}