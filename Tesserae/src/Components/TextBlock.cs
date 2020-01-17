using static Retyped.dom;
using static Tesserae.HTML.HtmlAttributes;
using static Tesserae.HTML.HtmlUtil;

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
                return TextSizeExtensions.FromClassList(InnerElement, TextSize.Small);
            }
            set
            {
                InnerElement.classList.remove(Size.ToClassName());
                InnerElement.classList.add(value.ToClassName());
            }
        }

        public TextWeight Weight
        {
            get
            {
                return TextSizeExtensions.FromClassList(InnerElement, TextWeight.Regular);
            }
            set
            {
                InnerElement.classList.remove(Weight.ToClassName());
                InnerElement.classList.add(value.ToClassName());
            }
        }

        public bool IsInvalid
        {
            get { return InnerElement.classList.contains("tss-fontColor-invalid"); }
            set
            {
                if (value != IsInvalid)
                {
                    if (value)
                    {
                        InnerElement.classList.add("tss-fontColor-invalid");
                    }
                    else
                    {
                        InnerElement.classList.remove("tss-fontColor-invalid");
                    }
                }
            }
        }

        public virtual bool IsRequired
        {
            get { return InnerElement.classList.contains("tss-required"); }
            set
            {
                if (value != IsInvalid)
                {
                    if (value)
                    {
                        InnerElement.classList.add("tss-required");
                    }
                    else
                    {
                        InnerElement.classList.remove("tss-required");
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
            InnerElement = Div(_("tss-textBlock tss-fontSize-small tss-fontWeight-regular", text: text));
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

        public static T Disabled<T>(this T button) where T : TextBlock
        {
            button.IsEnabled = false;
            return button;
        }
    }
}