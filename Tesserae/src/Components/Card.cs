using System;
using System.Collections.Generic;
using Retyped;
using static Tesserae.UI;
using static Retyped.dom;
using Tesserae.HTML;

namespace Tesserae.Components
{
    public class Card : IComponent
    {
        private HTMLElement InnerElement;

        public Card(IComponent content)
        {
            InnerElement = Div(_("tss-card"), content.Render());
            DomMountedObserver.NotifyWhenMounted(InnerElement, () => InnerElement.classList.add("ismounted"));
        }

        public Card SetContent(IComponent content)
        {
            ClearChildren(InnerElement);
            InnerElement.appendChild(content.Render());
            return this;
        }

        public HTMLElement Render()
        {
            return InnerElement;
        }
    }
}
