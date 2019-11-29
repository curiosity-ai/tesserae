using System;
using static Tesserae.HTML.HtmlUtil;
using static Tesserae.HTML.HtmlAttributes;
using static Retyped.dom;

namespace Tesserae.Components
{
    public enum TextSize
    {
        Tiny,
        XSmall,
        Small,
        SmallPlus,
        Medium,
        MediumPlus,
        Large,
        XLarge,
        XXLarge,
        Mega
    }

    public enum TextWeight
    {
        Regular,
        SemiBold,
        Bold
    }

    public class TextBlock : ComponentBase<TextBlock, HTMLElement>
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
            get { return (TextSize)Enum.Parse(typeof(TextSize), InnerElement.classList.item(1).Substring(InnerElement.classList[1].LastIndexOf('-') + 1), true); }
            set
            {
                InnerElement.classList.replace(InnerElement.classList.item(1), $"mss-fontSize-{value.ToString().ToLower()}");
            }
        }

        public TextWeight Weight
        {
            get { return (TextWeight)Enum.Parse(typeof(TextWeight), InnerElement.classList.item(2).Substring(InnerElement.classList[1].LastIndexOf('-') + 1), true); }
            set
            {
                InnerElement.classList.replace(InnerElement.classList.item(2), $"mss-fontWeight-{value.ToString().ToLower()}");
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


        public static T Tiny<T>(this T textBlock) where T : TextBlock
        {
            textBlock.Size = TextSize.Tiny;
            return textBlock;
        }

        public static T XSmall<T>(this T textBlock) where T : TextBlock
        {
            textBlock.Size = TextSize.XSmall;
            return textBlock;
        }
        public static T Small<T>(this T textBlock) where T : TextBlock
        {
            textBlock.Size = TextSize.Small;
            return textBlock;
        }

        public static T SmallPlus<T>(this T textBlock) where T : TextBlock
        {
            textBlock.Size = TextSize.SmallPlus;
            return textBlock;
        }

        public static T Medium<T>(this T textBlock) where T : TextBlock
        {
            textBlock.Size = TextSize.Medium;
            return textBlock;
        }
        public static T MediumPlus<T>(this T textBlock) where T : TextBlock
        {
            textBlock.Size = TextSize.MediumPlus;
            return textBlock;
        }

        public static T Large<T>(this T textBlock) where T : TextBlock
        {
            textBlock.Size = TextSize.Large;
            return textBlock;
        }
        public static T XLarge<T>(this T textBlock) where T : TextBlock
        {
            textBlock.Size = TextSize.XLarge;
            return textBlock;
        }

        public static T XXLarge<T>(this T textBlock) where T : TextBlock
        {
            textBlock.Size = TextSize.XXLarge;
            return textBlock;
        }

        public static T Mega<T>(this T textBlock) where T : TextBlock
        {
            textBlock.Size = TextSize.Mega;
            return textBlock;
        }

        public static T Regular<T>(this T textBlock) where T : TextBlock
        {
            textBlock.Weight = TextWeight.Regular;
            return textBlock;
        }

        public static T SemiBold<T>(this T textBlock) where T : TextBlock
        {
            textBlock.Weight = TextWeight.SemiBold;
            return textBlock;
        }

        public static T Bold<T>(this T textBlock) where T : TextBlock
        {
            textBlock.Weight = TextWeight.Bold;
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
