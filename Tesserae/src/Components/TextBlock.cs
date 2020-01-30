using Retyped;
using static Retyped.dom;
using static Tesserae.UI;

namespace Tesserae.Components
{
    public class TextBlock : ComponentBase<TextBlock, HTMLElement>, IHasTextSize
    {
        public TextBlock(string text = string.Empty)
        {
            InnerElement = Div(_("tss-textBlock tss-fontsize-small tss-fontweight-regular", text: text));
            AttachClick();
        }

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

        /// <summary>
        /// Gets or set whenever text block color is primary 
        /// </summary>
        public bool IsPrimary
        {
            get { return InnerElement.classList.contains("tss-fontcolor-primary"); }
            set
            {
                if (value != IsPrimary)
                {
                    if (value)
                    {
                        InnerElement.classList.add("tss-fontcolor-primary");
                        InnerElement.classList.remove("tss-fontcolor-invalid");
                        InnerElement.classList.remove("tss-fontcolor-success");
                        InnerElement.classList.remove("tss-fontcolor-danger");
                    }
                    else
                    {
                        InnerElement.classList.remove("tss-fontcolor-invalid");
                        InnerElement.classList.remove("tss-fontcolor-success");
                        InnerElement.classList.remove("tss-fontcolor-danger");
                        InnerElement.classList.remove("tss-fontcolor-primary");
                    }
                }
            }
        }

        /// <summary>
        /// Gets or set whenever text block color is success 
        /// </summary>
        public bool IsSuccess
        {
            get { return InnerElement.classList.contains("tss-fontcolor-success"); }
            set
            {
                if (value != IsSuccess)
                {
                    if (value)
                    {
                        InnerElement.classList.add("tss-fontcolor-success");
                        InnerElement.classList.remove("tss-fontcolor-invalid");
                        InnerElement.classList.remove("tss-fontcolor-primary");
                        InnerElement.classList.remove("tss-fontcolor-danger");
                    }
                    else
                    {
                        InnerElement.classList.remove("tss-fontcolor-invalid");
                        InnerElement.classList.remove("tss-fontcolor-success");
                        InnerElement.classList.remove("tss-fontcolor-danger");
                        InnerElement.classList.remove("tss-fontcolor-primary");
                    }
                }
            }
        }

        /// <summary>
        /// Gets or set whenever text block color is danger
        /// </summary>
        public bool IsDanger
        {
            get { return InnerElement.classList.contains("tss-fontcolor-danger"); }
            set
            {
                if (value != IsDanger)
                {
                    if (value)
                    {
                        InnerElement.classList.add("tss-fontcolor-danger");
                        InnerElement.classList.remove("tss-fontcolor-invalid");
                        InnerElement.classList.remove("tss-fontcolor-primary");
                        InnerElement.classList.remove("tss-fontcolor-success");
                    }
                    else
                    {
                        InnerElement.classList.remove("tss-fontcolor-invalid");
                        InnerElement.classList.remove("tss-fontcolor-success");
                        InnerElement.classList.remove("tss-fontcolor-danger");
                        InnerElement.classList.remove("tss-fontcolor-primary");
                    }
                }
            }
        }

        /// <summary>
        /// Gets or set whenever text block color is invalid
        /// </summary>
        public bool IsInvalid
        {
            get { return InnerElement.classList.contains("tss-fontcolor-invalid"); }
            set
            {
                if (value != IsInvalid)
                {
                    if (value)
                    {
                        InnerElement.classList.add("tss-fontcolor-invalid");
                        InnerElement.classList.remove("tss-fontcolor-danger");
                        InnerElement.classList.remove("tss-fontcolor-primary");
                        InnerElement.classList.remove("tss-fontcolor-success");
                    }
                    else
                    {
                        InnerElement.classList.remove("tss-fontcolor-invalid");
                        InnerElement.classList.remove("tss-fontcolor-success");
                        InnerElement.classList.remove("tss-fontcolor-danger");
                        InnerElement.classList.remove("tss-fontcolor-primary");
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

        public string Cursor
        {
            get
            {
                return InnerElement.style.cursor;
            }
            set
            {
                InnerElement.style.cursor = value;
            }
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

        public static T Disabled<T>(this T textBlock) where T : TextBlock
        {
            textBlock.IsEnabled = false;
            return textBlock;
        }

        public static T Primary<T>(this T textBlock) where T : TextBlock
        {
            textBlock.IsPrimary = true;
            return textBlock;
        }

        public static T Success<T>(this T textBlock) where T : TextBlock
        {
            textBlock.IsSuccess = true;
            return textBlock;
        }

        public static T Danger<T>(this T textBlock) where T : TextBlock
        {
            textBlock.IsDanger = true;
            return textBlock;
        }

    }
}