using System;
using System.Linq;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    [H5.Name("tss.txt")]
    public class TextBlock : ComponentBase<TextBlock, HTMLElement>, ITextFormating, IHasBackgroundColor, IHasForegroundColor, ICanWrap
    {
        public TextBlock(string text = string.Empty, bool treatAsHTML = false, bool selectable = false)
        {
            text = text ?? string.Empty;
            InnerElement = Div(_("tss-textblock tss-fontsize-small tss-fontweight-regular"));

            if (treatAsHTML)
            {
                InnerElement.innerHTML = text;
            }
            else
            {
                InnerElement.textContent = text;
            }

            if (selectable)
            {
                InnerElement.classList.add("tss-textblock-selectable");
            }

            AttachClick();
            AttachContextMenu();
        }

        public string Background { get => InnerElement.style.background; set => InnerElement.style.background = value; }

        public string Foreground { get => InnerElement.style.color; set => InnerElement.style.color = value; }

        public bool IsEnabled
        {
            get => !InnerElement.classList.contains("tss-disabled");
            set
            {
                if (value)
                {
                    InnerElement.classList.remove("tss-disabled");
                }
                else
                {
                    InnerElement.classList.add("tss-disabled");
                }
            }
        }

        public bool IsSelectable
        {
            get => InnerElement.style.userSelect != "none";
            set => InnerElement.style.userSelect = value ? "" : "none";
        }

        public virtual string Text
        {
            get => InnerElement.innerText;
            set => InnerElement.innerText = value;
        }
        
        public string HTML
        {
            get => InnerElement.innerHTML;
            set => InnerElement.innerHTML = value;
        }

        public string Title
        {
            get => InnerElement.title;
            set => InnerElement.title = value;
        }

        public virtual TextSize Size
        {
            get => ITextFormatingExtensions.FromClassList(InnerElement, TextSize.Small);
            set
            {
                InnerElement.classList.remove(Size.ToString());
                InnerElement.classList.add(value.ToString());
            }
        }

        public virtual TextWeight Weight
        {
            get => ITextFormatingExtensions.FromClassList(InnerElement, TextWeight.Regular);
            set
            {
                InnerElement.classList.remove(Weight.ToString());
                InnerElement.classList.add(value.ToString());
            }
        }

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

        /// <summary>
        /// Gets or set whenever text block color is primary
        /// </summary>
        public bool IsPrimary
        {
            get => InnerElement.classList.contains("tss-fontcolor-primary");
            set
            {
                if (value)
                {
                    InnerElement.classList.add("tss-fontcolor-primary");
                    InnerElement.classList.remove("tss-fontcolor-invalid");
                    InnerElement.classList.remove("tss-fontcolor-success");
                    InnerElement.classList.remove("tss-fontcolor-secondary");
                    InnerElement.classList.remove("tss-fontcolor-danger");
                }
                else
                {
                    InnerElement.classList.remove("tss-fontcolor-invalid");
                    InnerElement.classList.remove("tss-fontcolor-success");
                    InnerElement.classList.remove("tss-fontcolor-danger");
                    InnerElement.classList.remove("tss-fontcolor-secondary");
                    InnerElement.classList.remove("tss-fontcolor-primary");
                }
            }
        }

        /// <summary>
        /// Gets or set whenever text block color is primary
        /// </summary>
        public bool IsSecondary
        {
            get => InnerElement.classList.contains("tss-fontcolor-secondary");
            set
            {
                if (value)
                {
                    InnerElement.classList.add("tss-fontcolor-secondary");
                    InnerElement.classList.remove("tss-fontcolor-primary");
                    InnerElement.classList.remove("tss-fontcolor-invalid");
                    InnerElement.classList.remove("tss-fontcolor-success");
                    InnerElement.classList.remove("tss-fontcolor-danger");
                }
                else
                {
                    InnerElement.classList.remove("tss-fontcolor-invalid");
                    InnerElement.classList.remove("tss-fontcolor-success");
                    InnerElement.classList.remove("tss-fontcolor-danger");
                    InnerElement.classList.remove("tss-fontcolor-secondary");
                    InnerElement.classList.remove("tss-fontcolor-primary");
                }
            }
        }

        /// <summary>
        /// Gets or set whenever text block color is success
        /// </summary>
        public bool IsSuccess
        {
            get => InnerElement.classList.contains("tss-fontcolor-success");
            set
            {
                if (value)
                {
                    InnerElement.classList.add("tss-fontcolor-success");
                    InnerElement.classList.remove("tss-fontcolor-invalid");
                    InnerElement.classList.remove("tss-fontcolor-primary");
                    InnerElement.classList.remove("tss-fontcolor-secondary");
                    InnerElement.classList.remove("tss-fontcolor-danger");
                }
                else
                {
                    InnerElement.classList.remove("tss-fontcolor-invalid");
                    InnerElement.classList.remove("tss-fontcolor-success");
                    InnerElement.classList.remove("tss-fontcolor-danger");
                    InnerElement.classList.remove("tss-fontcolor-secondary");
                    InnerElement.classList.remove("tss-fontcolor-primary");
                }
            }
        }

        /// <summary>
        /// Gets or set whenever text block color is danger
        /// </summary>
        public bool IsDanger
        {
            get => InnerElement.classList.contains("tss-fontcolor-danger");
            set
            {
                if (value)
                {
                    InnerElement.classList.add("tss-fontcolor-danger");
                    InnerElement.classList.remove("tss-fontcolor-invalid");
                    InnerElement.classList.remove("tss-fontcolor-primary");
                    InnerElement.classList.remove("tss-fontcolor-secondary");
                    InnerElement.classList.remove("tss-fontcolor-success");
                }
                else
                {
                    InnerElement.classList.remove("tss-fontcolor-invalid");
                    InnerElement.classList.remove("tss-fontcolor-success");
                    InnerElement.classList.remove("tss-fontcolor-danger");
                    InnerElement.classList.remove("tss-fontcolor-secondary");
                    InnerElement.classList.remove("tss-fontcolor-primary");
                }
            }
        }

        /// <summary>
        /// Gets or set whenever text block color is invalid
        /// </summary>
        public bool IsInvalid
        {
            get => InnerElement.classList.contains("tss-fontcolor-invalid");
            set
            {
                if (value)
                {
                    InnerElement.classList.add("tss-fontcolor-invalid");
                }
                else
                {
                    InnerElement.classList.remove("tss-fontcolor-invalid");
                }
            }
        }

        public virtual bool IsRequired
        {
            get => InnerElement.classList.contains("tss-required");
            set
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

        public bool CanWrap
        {
            get => !InnerElement.classList.contains("tss-text-nowrap");
            set => InnerElement.UpdateClassIfNot(value, "tss-text-nowrap");
        }

        public bool EnableEllipsis
        {
            get => !InnerElement.classList.contains("tss-text-ellipsis");
            set => InnerElement.UpdateClassIf(value, "tss-text-ellipsis");
        }

        public bool EnableBreakSpaces
        {
            get => !InnerElement.classList.contains("tss-text-breakspaces");
            set => InnerElement.UpdateClassIf(value, "tss-text-breakspaces");
        }

        public string Cursor
        {
            get => InnerElement.style.cursor;
            set => InnerElement.style.cursor = value;
        }

        public override HTMLElement Render()
        {
            return InnerElement;
        }
    }
}