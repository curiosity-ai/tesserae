using System;
using static Transpose.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// A bordered, shadowed surface used to group related content into a self-contained block.
    /// </summary>
    [Transpose.Name("tss.Card")]
    public sealed class Card : ComponentBase<Card, HTMLElement>, IHasBackgroundColor, IRoundedStyle
    {
        private readonly HTMLElement _cardContainer;
        private HTMLElement _headerContainer;
        private HTMLElement _titleContainer;
        private HTMLElement _contentContainer;
        private HTMLElement _footerContainer;
        private bool _noPadding = false;

        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        public Card(IComponent content, bool noAnimation = false)
        {
            InnerElement   = Div(_("tss-card"),           content.Render());
            _cardContainer = Div(_("tss-card-container"), InnerElement);

            if (noAnimation)
            {
                InnerElement.classList.add("tss-noanimation", "tss-ismounted");
            }
            else
            {
                DomObserver.WhenMounted(InnerElement, () => InnerElement.classList.add("tss-ismounted"));
            }

            AttachClick();
            AttachContextMenu();
        }

        /// <summary>
        /// Gets or set whenever the card is rendered in a compact form
        /// </summary>
        private void EnsureLayout()
        {
            if (_contentContainer == null)
            {
                InnerElement.classList.add("tss-has-layout");
                _contentContainer = Div(_("tss-card-content"));

                if (_noPadding)
                {
                    _contentContainer.style.padding = "0px";
                }
                else
                {
                    _contentContainer.style.padding = InnerElement.style.padding;
                }

                InnerElement.style.padding = "0px"; // Reset inline padding on main container

                // Move existing styles
                _contentContainer.style.background = InnerElement.style.background;
                InnerElement.style.background = "transparent";

                _contentContainer.style.borderColor = InnerElement.style.borderColor;
                _contentContainer.style.borderWidth = InnerElement.style.borderWidth;
                _contentContainer.style.borderStyle = InnerElement.style.borderStyle;
                InnerElement.style.borderColor = "";
                InnerElement.style.borderWidth = "";
                InnerElement.style.borderStyle = "";

                while (InnerElement.firstChild != null)
                {
                    _contentContainer.appendChild(InnerElement.firstChild);
                }

                InnerElement.appendChild(_contentContainer);
            }
        }

        private void EnsureHeader()
        {
            EnsureLayout();
            if (_headerContainer == null)
            {
                _titleContainer = Div(_("tss-card-title"));
                _headerContainer = Div(_("tss-card-header"), _titleContainer);
                InnerElement.insertBefore(_headerContainer, _contentContainer);
            }
        }

        private void EnsureFooter()
        {
            EnsureLayout();
            if (_footerContainer == null)
            {
                _footerContainer = Div(_("tss-card-footer"));
                InnerElement.appendChild(_footerContainer);
            }
        }

        /// <summary>
        /// Gets or set whenever the card is rendered in a compact form
        /// </summary>
        public bool IsCompact
        {
            get => _cardContainer.classList.contains("tss-small");
            set => _cardContainer.UpdateClassIf(value, "tss-small");
        }

        /// <summary>
        /// Registers a callback invoked when the click event fires.
        /// </summary>
        public override Card OnClick(ComponentEventHandler<Card, MouseEvent> onClick, bool clearPrevious = true)
        {
            InnerElement.style.cursor = "pointer";
            return base.OnClick(onClick, clearPrevious);
        }

        /// <summary>
        /// Registers a callback invoked when the click event fires.
        /// </summary>
        public Card OnClick(Action action) => OnClick((_, __) => action.Invoke());

        /// <summary>
        /// Sets the title of the component.
        /// </summary>
        public Card SetTitle(string title)
        {
            EnsureHeader();
            SetTitle(TextBlock(title).SemiBold());
            return this;
        }

        /// <summary>
        /// Sets the title of the component.
        /// </summary>
        public Card SetTitle(IComponent title)
        {
            EnsureHeader();
            ClearChildren(_titleContainer);
            if (title != null) _titleContainer.appendChild(title.Render());
            return this;
        }

        /// <summary>
        /// Sets the content of the component.
        /// </summary>
        public Card SetContent(IComponent content)
        {
            if (_contentContainer != null)
            {
                ClearChildren(_contentContainer);
                _contentContainer.appendChild(content.Render());
            }
            else
            {
                ClearChildren(InnerElement);
                InnerElement.appendChild(content.Render());
            }
            return this;
        }

        /// <summary>
        /// Sets the footer of the component.
        /// </summary>
        public Card SetFooter(IComponent footer)
        {
            EnsureFooter();
            ClearChildren(_footerContainer);
            if (footer != null) _footerContainer.appendChild(footer.Render());
            return this;
        }

        /// <summary>
        /// Renders the component in a compact form.
        /// </summary>
        public Card Compact()
        {
            IsCompact = true;
            return this;
        }

        /// <summary>
        /// Gets or sets the CSS background of the component.
        /// </summary>
        public string Background
        {
            get => _contentContainer != null ? _contentContainer.style.background : InnerElement.style.background;
            set
            {
                if (_contentContainer != null) _contentContainer.style.background = value;
                else InnerElement.style.background = value;
                InnerElement.UpdateClassIf(!string.IsNullOrWhiteSpace(value), "tss-filter-effects");
            }
        }

        /// <summary>
        /// Sets the background colour of the card.
        /// </summary>
        public Card BackgroundColor(string color)
        {
            Background = color;
            return this;
        }

        /// <summary>
        /// Configures the component to border.
        /// </summary>
        public Card Border(string color, UnitSize size = null)
        {
            size                           = size ?? 1.px();
            if (_contentContainer != null)
            {
                _contentContainer.style.borderColor = color;
                _contentContainer.style.borderWidth = size.ToString();
                _contentContainer.style.borderStyle = "solid";
            }
            else
            {
                InnerElement.style.borderColor = color;
                InnerElement.style.borderWidth = size.ToString();
                InnerElement.style.borderStyle = "solid";
            }
            return this;
        }

        /// <summary>
        /// Removes / disables the padding on the component.
        /// </summary>
        public Card NoPadding()
        {
            _noPadding = true;
            if (_contentContainer != null)
            {
                _contentContainer.style.padding = "0px";
            }
            else
            {
                InnerElement.style.padding = "0px";
            }
            return this;
        }

        /// <summary>
        /// Enables or disables the hover colour overlay on the card.
        /// </summary>
        public Card HoverColor(bool enabled = true)
        {
            if (enabled)
            {
                InnerElement.classList.add("tss-card-hover");
            }
            else
            {
                InnerElement.classList.remove("tss-card-hover");
            }
            return this;
        }

        /// <summary>
        /// Renders the component's root HTML element.
        /// </summary>
        public override HTMLElement Render()
        {
            return _cardContainer;
        }
    }
}