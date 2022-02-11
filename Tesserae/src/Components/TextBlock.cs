using System;
using System.Linq;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    [H5.Name("tss.txt")]
    public class TextBlock : ComponentBase<TextBlock, HTMLElement>, ITextFormating, IHasBackgroundColor, IHasForegroundColor, ICanWrap
    {
        public TextBlock(string text = string.Empty, bool treatAsHTML = false, bool selectable = false, TextSize textSize = TextSize.Small, TextWeight textWeight = TextWeight.Regular)
        {
            text = text ?? string.Empty;
            
            InnerElement = Div(_("tss-textblock tss-fontcolor-default " + textSize.ToString() + " " + textWeight.ToString()));

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

        public string Foreground { get => GetTarget().style.color; set => GetTarget().style.color = value; }

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
            get => GetTarget().style.userSelect != "none";
            set => GetTarget().style.userSelect = value ? "" : "none";
        }

        public string Text
        {
            get => GetTarget().innerText;
            set => GetTarget().innerText = value;
        }
        
        public string HTML
        {
            get => GetTarget().innerHTML;
            set => GetTarget().innerHTML = value;
        }

        public string Title
        {
            get => GetTarget().title;
            set => GetTarget().title = value;
        }

        private HTMLElement GetTarget()
        {
            if (InnerElement.classList.contains("tss-label")) return InnerElement.firstElementChild.As<HTMLElement>();
            return InnerElement;
        }

        public TextSize Size
        {
            get => ITextFormatingExtensions.FromClassList(GetTarget(), TextSize.Small);
            set
            {
                var el = GetTarget();
                el.classList.remove(Size.ToString());
                el.classList.add(value.ToString());
            }
        }

        public TextWeight Weight
        {
            get => ITextFormatingExtensions.FromClassList(GetTarget(), TextWeight.Regular);
            set
            {
                var el = GetTarget();
                el.classList.remove(Weight.ToString());
                el.classList.add(value.ToString());
            }
        }

        public TextAlign TextAlign
        {
            get
            {
                return ITextFormatingExtensions.FromClassList(GetTarget(), TextAlign.Left);
            }
            set
            {
                var el = GetTarget();
                el.classList.remove(TextAlign.ToString());
                el.classList.add(value.ToString());
            }
        }

        /// <summary>
        /// Gets or set whenever text block color is primary
        /// </summary>
        public bool IsPrimary
        {
            get => GetTarget().classList.contains("tss-fontcolor-primary");
            set
            {
                var el = GetTarget();
                if (value)
                {
                    el.classList.add("tss-fontcolor-primary");
                    el.classList.remove("tss-fontcolor-invalid", "tss-fontcolor-success", "tss-fontcolor-secondary", "tss-fontcolor-danger", "tss-fontcolor-default");
                }
                else
                {
                    el.classList.add("tss-fontcolor-default");
                    el.classList.remove("tss-fontcolor-invalid", "tss-fontcolor-success", "tss-fontcolor-secondary", "tss-fontcolor-danger", "tss-fontcolor-primary");
                }
            }
        }

        /// <summary>
        /// Gets or set whenever text block color is primary
        /// </summary>
        public bool IsSecondary
        {
            get => GetTarget().classList.contains("tss-fontcolor-secondary");
            set
            {
                var el = GetTarget();
                if (value)
                {
                    el.classList.add("tss-fontcolor-secondary");
                    el.classList.remove("tss-fontcolor-invalid", "tss-fontcolor-success", "tss-fontcolor-primary", "tss-fontcolor-danger", "tss-fontcolor-default");
                }
                else
                {
                    el.classList.add("tss-fontcolor-default");
                    el.classList.remove("tss-fontcolor-invalid", "tss-fontcolor-success", "tss-fontcolor-secondary", "tss-fontcolor-danger", "tss-fontcolor-primary");
                }
            }
        }

        /// <summary>
        /// Gets or set whenever text block color is success
        /// </summary>
        public bool IsSuccess
        {
            get => GetTarget().classList.contains("tss-fontcolor-success");
            set
            {
                var el = GetTarget();
                if (value)
                {
                    el.classList.add("tss-fontcolor-success");
                    el.classList.remove("tss-fontcolor-invalid", "tss-fontcolor-secondary", "tss-fontcolor-primary", "tss-fontcolor-danger", "tss-fontcolor-default");
                }
                else
                {
                    el.classList.add("tss-fontcolor-default");
                    el.classList.remove("tss-fontcolor-invalid", "tss-fontcolor-success", "tss-fontcolor-secondary", "tss-fontcolor-danger", "tss-fontcolor-primary");
                }
            }
        }

        /// <summary>
        /// Gets or set whenever text block color is danger
        /// </summary>
        public bool IsDanger
        {
            get => GetTarget().classList.contains("tss-fontcolor-danger");
            set
            {
                var el = GetTarget();
                if (value)
                {
                    el.classList.add("tss-fontcolor-danger");
                    el.classList.remove("tss-fontcolor-invalid", "tss-fontcolor-secondary", "tss-fontcolor-primary", "tss-fontcolor-success", "tss-fontcolor-default");
                }
                else
                {
                    el.classList.add("tss-fontcolor-default");
                    el.classList.remove("tss-fontcolor-invalid", "tss-fontcolor-success", "tss-fontcolor-secondary", "tss-fontcolor-danger", "tss-fontcolor-primary");
                }
            }
        }

        /// <summary>
        /// Gets or set whenever text block color is invalid
        /// </summary>
        public bool IsInvalid
        {
            get => GetTarget().classList.contains("tss-fontcolor-invalid");
            set
            {
                var el = GetTarget();
                if (value)
                {
                    el.classList.add("tss-fontcolor-invalid");
                }
                else
                {
                    el.classList.remove("tss-fontcolor-invalid");
                }
            }
        }

        public virtual bool IsRequired
        {
            get => GetTarget().classList.contains("tss-required");
            set
            {
                var el = GetTarget();
                if (value)
                {
                    el.classList.add("tss-required");
                }
                else
                {
                    el.classList.remove("tss-required");
                }
            }
        }

        public bool CanWrap
        {
            get => !GetTarget().classList.contains("tss-text-nowrap");
            set => GetTarget().UpdateClassIfNot(value, "tss-text-nowrap");
        }

        public bool EnableEllipsis
        {
            get => !GetTarget().classList.contains("tss-text-ellipsis");
            set => GetTarget().UpdateClassIf(value, "tss-text-ellipsis");
        }

        public bool EnableBreakSpaces
        {
            get => !GetTarget().classList.contains("tss-text-breakspaces");
            set => GetTarget().UpdateClassIf(value, "tss-text-breakspaces");
        }

        public string Cursor
        {
            get => GetTarget().style.cursor;
            set => GetTarget().style.cursor = value;
        }

        public override HTMLElement Render()
        {
            return InnerElement;
        }
    }
}