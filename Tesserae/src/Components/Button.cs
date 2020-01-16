using static Tesserae.HTML.HtmlUtil;
using static Tesserae.HTML.HtmlAttributes;
using static Retyped.dom;
using System.Linq;
using System;

namespace Tesserae.Components
{
    public class Button : ComponentBase<Button, HTMLButtonElement>, IHasTextSize
    {
        #region Fields

        private HTMLSpanElement _TextSpan;
        private HTMLElement _IconSpan;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets button text
        /// </summary>
        public string Text
        {
            get { return _TextSpan.innerText; }
            set { _TextSpan.innerText = value; }
        }

        /// <summary>
        /// Gets or sets button icon (icon class)
        /// </summary>
        public string Icon
        {
            get { return _IconSpan?.className; }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    if (_IconSpan != null)
                    {
                        InnerElement.removeChild(_IconSpan);
                        _IconSpan = null;
                    }

                    return;
                }

                if (_IconSpan == null)
                {
                    _IconSpan = I(_());
                    InnerElement.insertBefore(_IconSpan, _TextSpan);
                }

                _IconSpan.className = value;
            }
        }

        /// <summary>
        /// Gets or set whenever button is primary 
        /// </summary>
        public bool IsPrimary
        {
            get { return InnerElement.classList.contains("tss-btn-primary"); }
            set
            {
                if (value != IsPrimary)
                {
                    if (value)
                    {
                        InnerElement.classList.add("tss-btn-primary");
                        InnerElement.classList.remove("tss-btn-default");
                    }
                    else
                    {
                        InnerElement.classList.remove("tss-btn-primary");
                        InnerElement.classList.add("tss-btn-default");
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

        public TextSize Size
        {
            get
            {
                var curFontSize = InnerElement.classList.FirstOrDefault(t => t.StartsWith("tss-fontSize-"));
                if (curFontSize is object && Enum.TryParse<TextSize>(curFontSize.Substring("tss-fontSize-".Length), true, out var result))
                {
                    return result;
                }
                else
                {
                    return TextSize.Small;
                }
            }
            set
            {
                var curFontSize = InnerElement.classList.FirstOrDefault(t => t.StartsWith("tss-fontSize-"));
                if (curFontSize is object)
                {
                    InnerElement.classList.remove(curFontSize);
                }
                InnerElement.classList.add($"tss-fontSize-{value.ToString().ToLower()}");
            }
        }

        public TextWeight Weight
        {
            get
            {
                var curFontSize = InnerElement.classList.FirstOrDefault(t => t.StartsWith("tss-fontWeight-"));
                if (curFontSize is object && Enum.TryParse<TextWeight>(curFontSize.Substring("tss-fontWeight-".Length), true, out var result))
                {
                    return result;
                }
                else
                {
                    return TextWeight.Regular;
                }
            }
            set
            {
                var curFontSize = InnerElement.classList.FirstOrDefault(t => t.StartsWith("tss-fontWeight-"));
                if (curFontSize is object)
                {
                    InnerElement.classList.remove(curFontSize);
                }
                InnerElement.classList.add($"tss-fontWeight-{value.ToString().ToLower()}");
            }
        }


        #endregion

        public Button(string text = string.Empty)
        {
            _TextSpan = Span(_(text: text));
            InnerElement = Button(_("tss-btn tss-btn-default"), _TextSpan);
            Weight = TextWeight.SemiBold;
            Size = TextSize.Small;
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

        public static Button Disabled(this Button button)
        {
            button.IsEnabled = false;
            return button;
        }

        public static Button NoBorder(this Button button)
        {
            button.InnerElement.classList.add("tss-btn-noborder");
            return button;
        }
    }
}
