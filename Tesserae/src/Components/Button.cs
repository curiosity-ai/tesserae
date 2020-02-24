using static Tesserae.UI;
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
            if (string.IsNullOrEmpty(text))
            {
                InnerElement.style.minWidth = "unset";
            }
        }

        /// <summary>
        /// Gets or sets button text
        /// </summary>
        public string Text
        {
            get { return _textSpan.innerText; }
            set 
            {
                _textSpan.innerText = value;
                if (string.IsNullOrEmpty(value))
                {
                    InnerElement.style.minWidth = "unset";
                }
                else
                {
                    InnerElement.style.minWidth = "";
                }
            }
        }

        /// <summary>
        /// Gets or sets button title
        /// </summary>
        public string Title
        {
            get { return InnerElement.title; }
            set { InnerElement.title = value; }
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
        /// Gets or set whenever button is rendered like a link 
        /// </summary>
        public bool IsLink
        {
            get { return InnerElement.classList.contains("tss-btn-link"); }
            set
            {
                if (value != IsLink)
                {
                    if (value)
                    {
                        InnerElement.classList.add("tss-btn-link");
                    }
                    else
                    {
                        InnerElement.classList.remove("tss-btn-link");
                    }
                }
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

        public bool CanWrap
        {
            get
            {
                return _textSpan.classList.contains("tss-text-ellipsis");
            }
            set
            {
                if (value)
                {
                    _textSpan.classList.add("tss-text-ellipsis");
                }
                else
                {
                    _textSpan.classList.remove("tss-text-ellipsis");
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

        public TextAlign TextAlign
        {
            get
            {
                var curFontSize = InnerElement.classList.FirstOrDefault(t => t.StartsWith("tss-textalign-"));
                if (curFontSize is object && Enum.TryParse<TextAlign>(curFontSize.Substring("tss-textalign-".Length), true, out var result))
                {
                    return result;
                }
                else
                {
                    return TextAlign.Center; //Button default is center
                }
            }
            set
            {
                var curFontSize = InnerElement.classList.FirstOrDefault(t => t.StartsWith("tss-textalign-"));
                if (curFontSize is object)
                {
                    InnerElement.classList.remove(curFontSize);
                }
                InnerElement.classList.add($"tss-textalign-{value.ToString().ToLower()}");
            }
        }

        public override HTMLElement Render()
        {
            return InnerElement;
        }


        public Button Link()
        {
            IsLink = true;
            return this;
        }

        public Button Primary()
        {
            IsPrimary = true;
            return this;
        }

        public Button Success()
        {
            IsSuccess = true;
            return this;
        }

        public Button Danger()
        {
            IsDanger = true;
            return this;
        }

        public Button Disabled()
        {
            IsEnabled = false;
            return this;
        }

        public Button NoBorder()
        {
            InnerElement.classList.add("tss-btn-noborder");
            return this;
        }

        public Button NoBackground()
        {
            InnerElement.classList.add("tss-btn-nobg");
            return this;
        }

        public Button Color(string background, string textColor = "white", string borderColor = "white")
        {
            InnerElement.classList.add("tss-btn-nobg");
            InnerElement.style.background = background;
            InnerElement.style.color = textColor;
            InnerElement.style.borderColor = borderColor;
            return this;
        }

        public Button SetText(string text)
        {
            Text = text;
            return this;
        }

        public Button SetTitle(string title)
        {
            Title = title;
            return this;
        }

        public Button SetIcon(string icon)
        {
            Icon = icon;
            return this;
        }

        public Button ReplaceContent(IComponent content)
        {
            ClearChildren(InnerElement);
            InnerElement.appendChild(content.Render());
            return this;
        }

        public Button Wrap()
        {
            CanWrap = true;
            return this;
        }

        public Button NoWrap()
        {
            CanWrap = false;
            return this;
        }
    }
}
