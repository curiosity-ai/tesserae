using static Tesserae.HTML.HtmlUtil;
using static Tesserae.HTML.HtmlAttributes;
using static Retyped.dom;

namespace Tesserae.Components
{
    public class Button : ComponentBase<Button, HTMLButtonElement>
    {
        #region Fields

        private HTMLSpanElement textSpan;
        private HTMLElement iconSpan;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets button text
        /// </summary>
        public string Text
        {
            get { return textSpan.innerText; }
            set { textSpan.innerText = value; }
        }

        /// <summary>
        /// Gets or sets button icon (icon class)
        /// </summary>
        public string Icon
        {
            get { return iconSpan?.className; }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    if (iconSpan != null)
                    {
                        InnerElement.removeChild(iconSpan);
                        iconSpan = null;
                        textSpan.classList.remove("ml-2");
                    }

                    return;
                }

                if (iconSpan == null)
                {
                    iconSpan = I(_());
                    InnerElement.insertBefore(iconSpan, textSpan);
                    textSpan.classList.add("ml-2");
                }

                iconSpan.className = value;
            }
        }

        /// <summary>
        /// Gets or set whenever button is primary 
        /// </summary>
        public bool IsPrimary
        {
            get { return InnerElement.classList.contains("mss-btn-primary"); }
            set
            {
                if (value != IsPrimary)
                {
                    if (value)
                    {
                        InnerElement.classList.add("mss-btn-primary");
                        InnerElement.classList.remove("mss-btn-default");
                    }
                    else
                    {
                        InnerElement.classList.remove("mss-btn-primary");
                        InnerElement.classList.add("mss-btn-default");
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets whenever button is enabled
        /// </summary>
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

        //public bool IsToggle
        //{
        //    get { return (string)InnerElement.dataset["toggle"] == "button"; }
        //    set { InnerElement.dataset["toggle"] = value ? "button" : ""; }
        //}

        //public bool IsChecked
        //{
        //    get { return InnerElement.classList.contains("active"); }
        //    set
        //    {
        //        if (value != IsChecked)
        //        {
        //            if (value) InnerElement.classList.add("active");
        //            else InnerElement.classList.remove("active");
        //        }
        //    }
        //}

        #endregion

        public Button(string text = string.Empty)
        {
            textSpan = Span(_(text: text));
            InnerElement = Button(_("m-1 mss-btn mss-btn-default"), textSpan);
            AttachClick();
            AttachFocus();
            AttachBlur();
        }

        public override HTMLElement Render()
        {
            return InnerElement;
        }
    }

    public static class ButtonExtensions
    {
        public static Button Text(this Button button, string text)
        {
            button.Text = text;
            return button;
        }
        public static Button Icon(this Button button, string icon)
        {
            button.Icon = icon;
            return button;
        }

        public static Button Primary(this Button button)
        {
            button.IsPrimary = true;
            return button;
        }

        //public static Button Toggle(this Button button)
        //{
        //    button.IsToggle = true;
        //    return button;
        //}

        //public static Button Checked(this Button button)
        //{
        //    button.IsChecked = true;
        //    return button;
        //}

        public static Button Disabled(this Button button)
        {
            button.IsEnabled = false;
            return button;
        }
    }
}
