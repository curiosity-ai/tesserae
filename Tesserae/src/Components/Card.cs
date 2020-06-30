using Tesserae.HTML;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae.Components
{
    public sealed class Card : ComponentBase<Card, HTMLElement>
    {
        private readonly HTMLElement _cardContainer;
        public Card(IComponent content)
        {
            InnerElement = Div(_("tss-card"), content.Render());
            _cardContainer = Div(_("tss-card-container"), InnerElement);
            DomObserver.WhenMounted(InnerElement, () => InnerElement.classList.add("tss-ismounted"));
            AttachClick();
        }

        /// <summary>
        /// Gets or set whenever the card is rendered in a compact form
        /// </summary>
        public bool IsCompact
        {
            get => _cardContainer.classList.contains("tss-small");
            set => _cardContainer.UpdateClassIf(value, "tss-small");
        }

        public override Card OnClick(ComponentEventHandler<Card, MouseEvent> onClick)
        {
            InnerElement.style.cursor = "pointer";
            return base.OnClick(onClick);
        }

        public Card SetContent(IComponent content)
        {
            ClearChildren(InnerElement);
            InnerElement.appendChild(content.Render());
            return this;
        }

        public Card Compact()
        {
            IsCompact = true;
            return this;
        }

        public Card NoPadding()
        {
            InnerElement.style.padding = "0px";
            return this;
        }

        public override HTMLElement Render()
        {
            return _cardContainer;
        }
    }
}
