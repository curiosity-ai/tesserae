using System;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    [H5.Name("tss.Expander")]
    public sealed class Expander : ComponentBase<Expander, HTMLElement>
    {
        private readonly HTMLElement       _header;
        private readonly HTMLElement       _headerContent;
        private readonly HTMLSpanElement   _title;
        private readonly HTMLElement       _chevron;
        private readonly HTMLElement       _content;
        private          bool              _isExpanded;
        private          bool              _customHeader;
        private          Action<Expander>  _onToggle;
        private          Action<Expander>  _onExpand;
        private          Action<Expander>  _onCollapse;

        public Expander(string title = null, IComponent content = null)
        {
            _title         = Span(_("tss-expander-title", text: title ?? string.Empty));
            _headerContent = Div(_("tss-expander-header-content"), _title);
            _chevron       = I(UIcons.AngleDown, cssClass: "tss-expander-chevron");
            _header        = Div(_("tss-expander-header", role: "button", ariaLabel: "Toggle section"), _chevron, _headerContent);
            _content       = Div(_("tss-expander-content"));

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

        public Expander SetTitle(string title)
        {
            _title.innerText = title ?? string.Empty;

            if (!_customHeader && _headerContent.firstChild != _title)
            {
                ClearChildren(_headerContent);
                _headerContent.appendChild(_title);
            }

            return this;
        }

        public Expander SetHeader(IComponent header)
        {
            ClearChildren(_headerContent);

            if (header == null)
            {
                _customHeader = false;
                _headerContent.appendChild(_title);
            }
            else
            {
                _customHeader = true;
                _headerContent.appendChild(header.Render());
            }

            return this;
        }

        public Expander SetContent(IComponent content)
        {
            ClearChildren(_content);

            if (content is object)
            {
                _content.appendChild(content.Render());
            }

            return this;
        }

        public Expander Expanded(bool value = true)
        {
            IsExpanded = value;
            return this;
        }

        public Expander Collapse()
        {
            IsExpanded = false;
            return this;
        }

        public Expander Expand()
        {
            IsExpanded = true;
            return this;
        }

        public Expander Toggle()
        {
            IsExpanded = !IsExpanded;
            return this;
        }

        public Expander OnToggle(Action<Expander> onToggle)
        {
            _onToggle += onToggle;
            return this;
        }

        public Expander OnExpand(Action<Expander> onExpand)
        {
            _onExpand += onExpand;
            return this;
        }

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

        public override HTMLElement Render()
        {
            return InnerElement;
        }
    }
}
