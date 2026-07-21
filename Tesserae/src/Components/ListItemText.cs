using System;
using static Transpose.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// A simple two-line list item that shows a bold title with a lighter subtitle
    /// underneath, plus an optional leading icon rendered inside a rounded-square background.
    /// </summary>
    [Transpose.Name("tss.ListItemText")]
    public class ListItemText : ComponentBase<ListItemText, HTMLElement>
    {
        private readonly HTMLElement _title;
        private readonly HTMLElement _subtitle;
        private readonly HTMLElement _content;

        private HTMLElement _iconContainer;
        private Icon        _icon;

        /// <summary>
        /// Initializes a new instance of the <see cref="ListItemText"/> class.
        /// </summary>
        /// <param name="title">The (bold) title text.</param>
        /// <param name="subtitle">The (lighter) subtitle text. When null or empty the subtitle is hidden.</param>
        public ListItemText(string title, string subtitle = null)
        {
            _title             = Div(_("tss-listitemtext-title"));
            _title.textContent = title ?? string.Empty;

            _subtitle = Div(_("tss-listitemtext-subtitle"));

            if (string.IsNullOrEmpty(subtitle))
            {
                _subtitle.style.display = "none";
            }
            else
            {
                _subtitle.textContent = subtitle;
            }

            _content = Div(_("tss-listitemtext-content"), _title, _subtitle);

            InnerElement = Div(_("tss-listitemtext"), _content);

            AttachClick();
            AttachContextMenu();
        }

        /// <summary>Gets or sets the title text.</summary>
        public string Title
        {
            get => _title.textContent;
            set => _title.textContent = value ?? string.Empty;
        }

        /// <summary>Gets or sets the subtitle text. Setting it to null or empty hides the subtitle.</summary>
        public string Subtitle
        {
            get => _subtitle.textContent;
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    _subtitle.textContent   = string.Empty;
                    _subtitle.style.display = "none";
                }
                else
                {
                    _subtitle.textContent   = value;
                    _subtitle.style.display = "";
                }
            }
        }

        /// <summary>Sets the title text.</summary>
        public ListItemText SetTitle(string title)
        {
            Title = title;
            return this;
        }

        /// <summary>Sets the subtitle text.</summary>
        public ListItemText SetSubtitle(string subtitle)
        {
            Subtitle = subtitle;
            return this;
        }

        /// <summary>
        /// Shows a leading icon inside a rounded-square background. Calling this again updates the existing icon.
        /// </summary>
        public ListItemText SetIcon(UIcons icon, UIconsWeight weight = UIconsWeight.Regular, TextSize size = TextSize.Medium)
        {
            if (_icon is null)
            {
                _icon          = new Icon(icon, weight, size);
                _iconContainer = Div(_("tss-listitemtext-icon"), _icon.Render());
                InnerElement.insertBefore(_iconContainer, _content);
            }
            else
            {
                _icon.SetIcon(icon, weight, size);
            }

            return this;
        }

        /// <summary>Sets the color of the leading icon. Has no effect until an icon is set.</summary>
        public ListItemText IconForeground(string color)
        {
            if (_icon is object)
            {
                _icon.Foreground = color;
            }

            return this;
        }

        /// <summary>Sets the background color of the rounded square behind the leading icon. Has no effect until an icon is set.</summary>
        public ListItemText IconBackground(string color)
        {
            if (_iconContainer is object)
            {
                _iconContainer.style.background = color;
            }

            return this;
        }

        /// <summary>
        /// Renders the component's root HTML element.
        /// </summary>
        public override HTMLElement Render() => InnerElement;
    }
}
