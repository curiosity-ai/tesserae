using System;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// A single expand / collapse section, with a clickable header that reveals its body content.
    /// </summary>
    [H5.Name("tss.Expander")]
    public sealed class Expander : ComponentBase<Expander, HTMLElement>
    {
        private readonly HTMLElement       _header;
        private readonly HTMLElement       _headerContent;
        private readonly HTMLSpanElement   _title;
        private readonly HTMLElement       _iconContainer;
        private readonly HTMLElement       _chevron;
        private readonly HTMLElement       _content;
        private          bool              _isExpanded;
        private          bool              _customHeader;
        private          Action<Expander>  _onToggle;
        private          Action<Expander>  _onExpand;
        private          Action<Expander>  _onCollapse;

        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        public Expander(string title = null, IComponent content = null)
        {
            var contentId = "tss-expander-content-" + Guid.NewGuid().ToString("N").Substring(0, 8);

            _title         = Span(_("tss-expander-title", text: title ?? string.Empty));
            _iconContainer = Div(_("tss-expander-icon"));
            _iconContainer.style.display = "none";
            _headerContent = Div(_("tss-expander-header-content"), _iconContainer, _title);
            _chevron       = I(UIcons.AngleDown, cssClass: "tss-expander-chevron");
            _header        = Div(_("tss-expander-header", role: "button", ariaLabel: "Toggle section"), _chevron, _headerContent);
            _header.setAttribute("aria-controls", contentId);
            _content       = Div(_("tss-expander-content"));
            _content.id    = contentId;
            _content.setAttribute("role", "region");

            InnerElement = Div(_("tss-expander"), _header, _content);

            SetContent(content);
            UpdateExpandedState();

            _header.addEventListener("click", _ => Toggle());
        }

        /// <summary>
        /// Gets or sets whether the expander is expanded.
        /// </summary>
        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                if (_isExpanded == value)
                {
                    return;
                }

                _isExpanded = value;
                UpdateExpandedState();

                _onToggle?.Invoke(this);

                if (_isExpanded)
                {
                    _onExpand?.Invoke(this);
                }
                else
                {
                    _onCollapse?.Invoke(this);
                }
            }
        }

        /// <summary>
        /// Gets or sets the title text when no custom header is provided.
        /// </summary>
        public string Title
        {
            get => _title.innerText;
            set => SetTitle(value);
        }

        /// <summary>
        /// Sets the title of the component.
        /// </summary>
        public Expander SetTitle(string title)
        {
            _title.innerText = title ?? string.Empty;

            if (!_customHeader && _headerContent.lastChild != _title)
            {
                ClearChildren(_headerContent);
                _headerContent.appendChild(_iconContainer);
                _headerContent.appendChild(_title);
            }

            return this;
        }

        /// <summary>
        /// Sets the header of the component.
        /// </summary>
        public Expander SetHeader(IComponent header)
        {
            ClearChildren(_headerContent);

            if (header == null)
            {
                _customHeader = false;
                _headerContent.appendChild(_iconContainer);
                _headerContent.appendChild(_title);
            }
            else
            {
                _customHeader = true;
                _headerContent.appendChild(header.Render());
            }

            return this;
        }

        /// <summary>
        /// Sets the content of the component.
        /// </summary>
        public Expander SetContent(IComponent content)
        {
            ClearChildren(_content);

            if (content is object)
            {
                _content.appendChild(content.Render());
            }

            return this;
        }

        /// <summary>
        /// Configures the option icon on the component.
        /// </summary>
        public Expander OptionIcon(UIcons icon, string color = "", string background = "")
        {
            ClearChildren(_iconContainer);
            _iconContainer.style.display = "flex";
            if (!string.IsNullOrEmpty(color))
            {
                _iconContainer.style.color = color;
            }
            if (!string.IsNullOrEmpty(background))
            {
                _iconContainer.style.background = background;
            }
            _iconContainer.appendChild(I(icon));
            return this;
        }

        /// <summary>
        /// Configures the chevron right on the component.
        /// </summary>
        public Expander ChevronRight()
        {
            _header.appendChild(_chevron);
            return this;
        }

        /// <summary>
        /// Expands the component.
        /// </summary>
        public Expander Expanded(bool value = true)
        {
            IsExpanded = value;
            return this;
        }

        /// <summary>
        /// Collapses the component.
        /// </summary>
        public Expander Collapse()
        {
            IsExpanded = false;
            return this;
        }

        /// <summary>
        /// Expands the component.
        /// </summary>
        public Expander Expand()
        {
            IsExpanded = true;
            return this;
        }

        /// <summary>
        /// Toggles the component's state.
        /// </summary>
        public Expander Toggle()
        {
            IsExpanded = !IsExpanded;
            return this;
        }

        /// <summary>
        /// Registers a callback invoked when the toggle event fires.
        /// </summary>
        public Expander OnToggle(Action<Expander> onToggle)
        {
            _onToggle += onToggle;
            return this;
        }

        /// <summary>
        /// Registers a callback invoked when the expand event fires.
        /// </summary>
        public Expander OnExpand(Action<Expander> onExpand)
        {
            _onExpand += onExpand;
            return this;
        }

        /// <summary>
        /// Registers a callback invoked when the collapse event fires.
        /// </summary>
        public Expander OnCollapse(Action<Expander> onCollapse)
        {
            _onCollapse += onCollapse;
            return this;
        }

        private void UpdateExpandedState()
        {
            InnerElement.UpdateClassIf(_isExpanded, "tss-expanded");
            _content.style.display = _isExpanded ? "block" : "none";
            _header.setAttribute("aria-expanded", _isExpanded ? "true" : "false");
        }

        /// <summary>
        /// Renders the component's root HTML element.
        /// </summary>
        public override HTMLElement Render()
        {
            return InnerElement;
        }
    }
}
