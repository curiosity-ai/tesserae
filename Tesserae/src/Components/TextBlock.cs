using System;
using Retyped;
using static Tesserae.HTML.HtmlUtil;
using static Tesserae.HTML.HtmlAttributes;
using static Retyped.dom;
using System.Linq;

namespace Tesserae.Components
{
    public class TextBlock : ComponentBase<TextBlock, HTMLElement>, IHasTextSize
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

        public virtual string Text
        {
            get { return InnerElement.innerText; }
            set { InnerElement.innerText = value; }
        }

        public TextSize Size
        {
            get
            {
                var curFontSize = InnerElement.classList.FirstOrDefault(t => t.StartsWith("mss-fontSize-"));
                if (curFontSize is object && Enum.TryParse<TextSize>(curFontSize.Substring("mss-fontSize-".Length), true, out var result))
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
                var curFontSize = InnerElement.classList.FirstOrDefault(t => t.StartsWith("mss-fontSize-"));
                if(curFontSize is object)
                {
                    InnerElement.classList.remove(curFontSize);
                }
                InnerElement.classList.add($"mss-fontSize-{value.ToString().ToLower()}");
            }
        }

        public TextWeight Weight
        {
            get
            {
                var curFontSize = InnerElement.classList.FirstOrDefault(t => t.StartsWith("mss-fontWeight-"));
                if (curFontSize is object && Enum.TryParse<TextWeight>(curFontSize.Substring("mss-fontWeight-".Length), true, out var result))
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
                var curFontSize = InnerElement.classList.FirstOrDefault(t => t.StartsWith("mss-fontWeight-"));
                if (curFontSize is object)
                {
                    InnerElement.classList.remove(curFontSize);
                }
                InnerElement.classList.add($"mss-fontWeight-{value.ToString().ToLower()}");
            }
        }

        public bool IsInvalid
        {
            get { return InnerElement.classList.contains("mss-fontColor-invalid"); }
            set
            {
                if (value != IsInvalid)
                {
                    if (value)
                    {
                        InnerElement.classList.add("mss-fontColor-invalid");
                    }
                    else
                    {
                        InnerElement.classList.remove("mss-fontColor-invalid");
                    }
                }
            }
        }

        public virtual bool IsRequired
        {
            get { return InnerElement.classList.contains("mss-required"); }
            set
            {
                if (value != IsInvalid)
                {
                    if (value)
                    {
                        InnerElement.classList.add("mss-required");
                    }
                    else
                    {
                        InnerElement.classList.remove("mss-required");
                    }
                }
            }
        }

        public bool CanWrap
        {
            get
            {
                return InnerElement.style.whiteSpace != "nowrap";
            }
            set
            {
                if (value != CanWrap)
                {
                    if (value)
                    {
                        InnerElement.style.whiteSpace = "unset";
                        InnerElement.style.overflow = "unset";
                        InnerElement.style.textOverflow = "unset";
                    }
                    else
                    {
                        InnerElement.style.whiteSpace = "nowrap";
                        InnerElement.style.overflow = "hidden";
                        InnerElement.style.textOverflow = "ellipsis";
                    }
                }
            }
        }

        #endregion

        public TextBlock(string text = string.Empty)
        {
            InnerElement = Div(_("mss-textBlock mss-fontSize-small mss-fontWeight-regular", text: text));
        }

        public override HTMLElement Render()
        {
            return InnerElement;
        }
    }

    public static class TextBlockExtensions
    {
        public static T Text<T>(this T textBlock, string text) where T : TextBlock
        {
            textBlock.Text = text;
            return textBlock;
        }

        public static T Required<T>(this T textBlock) where T : TextBlock
        {
            textBlock.IsRequired = true;
            return textBlock;
        }

        public static T Wrap<T>(this T textBlock) where T : TextBlock
        {
            textBlock.CanWrap = true;
            return textBlock;
        }

        public static T NoWrap<T>(this T textBlock) where T : TextBlock
        {
            textBlock.CanWrap = false;
            return textBlock;
        }

        public static T Invalid<T>(this T textBlock) where T : TextBlock
        {
            textBlock.IsInvalid = true;
            return textBlock;
        }

        public static T Disabled<T>(this T button) where T : TextBlock
        {
            button.IsEnabled = false;
            return button;
        }
    }
}
