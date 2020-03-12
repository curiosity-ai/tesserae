using System;
using System.Collections.Generic;
using Retyped;
using static Tesserae.UI;
using static Retyped.dom;
using Tesserae.HTML;

namespace Tesserae.Components
{
    public class Card : ComponentBase<Card, HTMLElement>
    {
        private HTMLElement _cardContainer;
        public Card(IComponent content)
        {
            InnerElement = Div(_("tss-card"), content.Render());
            _cardContainer = Div(_("tss-card-container"), InnerElement);
            DomObserver.WhenMounted(InnerElement, () => InnerElement.classList.add("ismounted"));
            AttachClick();
        }

        public override Card OnClick(ComponentEventHandler<MouseEvent> onClick)
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

        public override HTMLElement Render()
        {
            return _cardContainer;
        }
    }
}
