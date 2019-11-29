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

    public class TextBlock : ComponentBase<TextBlock, HTMLDivElement>
    {
        #region Properties

        public string Text
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

        public bool IsRequired
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

        #endregion

        public TextBlock(string text = string.Empty)
        {
            InnerElement = Div(_("mss-textBlock mss-fontSize-small mss-fontWeight-regular", text: text, styles: s =>
                 {
                     s.position = "relative";
                 }));
        }

        public override HTMLElement Render()
        {
            return InnerElement;
        }
    }

    public static class TextBlockExtensions
    {
        public static TextBlock Text(this TextBlock textBlock, string text)
        {
            textBlock.Text = text;
            return textBlock;
        }

        public static TextBlock Tiny(this TextBlock textBlock)
        {
            textBlock.Size = TextSize.Tiny;
            return textBlock;
        }

        public static TextBlock XSmall(this TextBlock textBlock)
        {
            textBlock.Size = TextSize.XSmall;
            return textBlock;
        }
        public static TextBlock Small(this TextBlock textBlock)
        {
            textBlock.Size = TextSize.Small;
            return textBlock;
        }

        public static TextBlock SmallPlus(this TextBlock textBlock)
        {
            textBlock.Size = TextSize.SmallPlus;
            return textBlock;
        }

        public static TextBlock Medium(this TextBlock textBlock)
        {
            textBlock.Size = TextSize.Medium;
            return textBlock;
        }
        public static TextBlock MediumPlus(this TextBlock textBlock)
        {
            textBlock.Size = TextSize.MediumPlus;
            return textBlock;
        }

        public static TextBlock Large(this TextBlock textBlock)
        {
            textBlock.Size = TextSize.Large;
            return textBlock;
        }
        public static TextBlock XLarge(this TextBlock textBlock)
        {
            textBlock.Size = TextSize.XLarge;
            return textBlock;
        }

        public static TextBlock XXLarge(this TextBlock textBlock)
        {
            textBlock.Size = TextSize.XXLarge;
            return textBlock;
        }

        public static TextBlock Mega(this TextBlock textBlock)
        {
            textBlock.Size = TextSize.Mega;
            return textBlock;
        }

        public static TextBlock Regular(this TextBlock textBlock)
        {
            textBlock.Weight = TextWeight.Regular;
            return textBlock;
        }

        public static TextBlock SemiBold(this TextBlock textBlock)
        {
            textBlock.Weight = TextWeight.SemiBold;
            return textBlock;
        }

        public static TextBlock Bold(this TextBlock textBlock)
        {
            textBlock.Weight = TextWeight.Bold;
            return textBlock;
        }

        public static TextBlock Invalid(this TextBlock textBlock)
        {
            textBlock.IsInvalid = true;
            return textBlock;
        }
    }
}
