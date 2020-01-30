﻿using static Tesserae.UI;
using static Retyped.dom;
using System.Linq;
using System;

namespace Tesserae.Components
{
    public class Button : ComponentBase<Button, HTMLButtonElement>, IHasTextSize
    {
        private readonly HTMLSpanElement _textSpan;
        private HTMLElement _iconSpan;

        public Button(string text = string.Empty)
        {
            _textSpan = Span(_(text: text));
            InnerElement = Button(_("tss-btn tss-btn-default"), _textSpan);
            Weight = TextWeight.SemiBold;
            Size = TextSize.Small;
            AttachClick();
            AttachFocus();
            AttachBlur();
        }

        /// <summary>
        /// Gets or sets button text
        /// </summary>
        public string Text
        {
            get { return _textSpan.innerText; }
            set { _textSpan.innerText = value; }
        }

        /// <summary>
        /// Gets or sets button icon (icon class)
        /// </summary>
        public string Icon
        {
            get { return _iconSpan?.className; }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    if (_iconSpan != null)
                    {
                        InnerElement.removeChild(_iconSpan);
                        _iconSpan = null;
                    }

                    return;
                }

                if (_iconSpan == null)
                {
                    _iconSpan = I(_());
                    InnerElement.insertBefore(_iconSpan, _textSpan);
                }

                _iconSpan.className = value;
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
                        InnerElement.classList.remove("tss-btn-success");
                        InnerElement.classList.remove("tss-btn-danger");
                    }
                    else
                    {
                        InnerElement.classList.remove("tss-btn-default");
                        InnerElement.classList.remove("tss-btn-success");
                        InnerElement.classList.remove("tss-btn-danger");
                        InnerElement.classList.remove("tss-btn-primary");
                        InnerElement.classList.add("tss-btn-default");
                    }
                }
            }
        }

        /// <summary>
        /// Gets or set whenever button is primary 
        /// </summary>
        public bool IsSuccess
        {
            get { return InnerElement.classList.contains("tss-btn-success"); }
            set
            {
                if (value != IsSuccess)
                {
                    if (value)
                    {
                        InnerElement.classList.add("tss-btn-success");
                        InnerElement.classList.remove("tss-btn-default");
                        InnerElement.classList.remove("tss-btn-primary");
                        InnerElement.classList.remove("tss-btn-danger");
                    }
                    else
                    {
                        InnerElement.classList.remove("tss-btn-default");
                        InnerElement.classList.remove("tss-btn-success");
                        InnerElement.classList.remove("tss-btn-danger");
                        InnerElement.classList.remove("tss-btn-primary");
                        InnerElement.classList.add("tss-btn-default");
                    }
                }
            }
        }

        /// <summary>
        /// Gets or set whenever button is primary 
        /// </summary>
        public bool IsDanger
        {
            get { return InnerElement.classList.contains("tss-btn-danger"); }
            set
            {
                if (value != IsDanger)
                {
                    if (value)
                    {
                        InnerElement.classList.add("tss-btn-danger");
                        InnerElement.classList.remove("tss-btn-default");
                        InnerElement.classList.remove("tss-btn-primary");
                        InnerElement.classList.remove("tss-btn-success");
                    }
                    else
                    {
                        InnerElement.classList.remove("tss-btn-default");
                        InnerElement.classList.remove("tss-btn-success");
                        InnerElement.classList.remove("tss-btn-danger");
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
                var curFontSize = InnerElement.classList.FirstOrDefault(t => t.StartsWith("tss-fontsize-"));
                if (curFontSize is object && Enum.TryParse<TextSize>(curFontSize.Substring("tss-fontsize-".Length), true, out var result))
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
                var curFontSize = InnerElement.classList.FirstOrDefault(t => t.StartsWith("tss-fontsize-"));
                if (curFontSize is object)
                {
                    InnerElement.classList.remove(curFontSize);
                }
                InnerElement.classList.add($"tss-fontsize-{value.ToString().ToLower()}");
            }
        }

        public TextWeight Weight
        {
            get
            {
                var curFontSize = InnerElement.classList.FirstOrDefault(t => t.StartsWith("tss-fontweight-"));
                if (curFontSize is object && Enum.TryParse<TextWeight>(curFontSize.Substring("tss-fontweight-".Length), true, out var result))
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
                var curFontSize = InnerElement.classList.FirstOrDefault(t => t.StartsWith("tss-fontweight-"));
                if (curFontSize is object)
                {
                    InnerElement.classList.remove(curFontSize);
                }
                InnerElement.classList.add($"tss-fontweight-{value.ToString().ToLower()}");
            }
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

        public static Button Success(this Button button)
        {
            button.IsSuccess = true;
            return button;
        }

        public static Button Danger(this Button button)
        {
            button.IsDanger = true;
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

        public static Button NoBackground(this Button button)
        {
            button.InnerElement.classList.add("tss-btn-nobg");
            return button;
        }

        public static Button Color(this Button button, string background, string textColor = "white", string borderColor = "white")
        {
            button.InnerElement.classList.add("tss-btn-nobg");
            button.InnerElement.style.background = background;
            button.InnerElement.style.color = textColor;
            button.InnerElement.style.borderColor = borderColor;
            return button;
        }
    }
}
