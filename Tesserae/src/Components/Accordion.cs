using System;
using System.Collections.Generic;
using System.Linq;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    [H5.Name("tss.Accordion")]
    public class Accordion : ComponentBase<Accordion, HTMLDivElement>, IContainer<Accordion, Accordion.Item>
    {
        private readonly List<Item> _items = new List<Item>();

        public Accordion()
        {
            InnerElement = Div(_("tss-accordion"));
        }

        public void Add(Item item)
        {
            _items.Add(item);
            InnerElement.appendChild(item.Render());
        }

        public void Clear()
        {
            _items.Clear();
            ClearChildren(InnerElement);
        }

        public void Replace(Item newComponent, Item oldComponent)
        {
            var index = _items.IndexOf(oldComponent);
            if (index >= 0)
            {
                _items[index] = newComponent;
                InnerElement.replaceChild(newComponent.Render(), oldComponent.Render());
            }
        }

        public override HTMLElement Render()
        {
            return InnerElement;
        }

        [H5.Name("tss.AccordionItem")]
        public class Item : IComponent
        {
            private readonly HTMLElement _container;
            private readonly HTMLElement _header;
            private readonly HTMLElement _content;
            private readonly HTMLElement _icon;

            public Item(IComponent title, IComponent content, bool isOpen = false)
            {
                _icon = I(_("tss-accordion-header-icon " + UIcons.AngleDown.ToString()));
                _header = Div(_("tss-accordion-header"), Div(_("tss-accordion-header-title"), title.Render()), _icon);
                _content = Div(_("tss-accordion-content"), content.Render());
                _container = Div(_("tss-accordion-item"), _header, _content);

                if (isOpen)
                {
                    _container.classList.add("tss-open");
                }

                _header.onclick = (e) =>
                {
                    StopEvent(e);
                    Toggle();
                };
            }

            public void Toggle()
            {
                if (_container.classList.contains("tss-open"))
                {
                    _container.classList.remove("tss-open");
                }
                else
                {
                    _container.classList.add("tss-open");
                }
            }

            public void Open()
            {
                _container.classList.add("tss-open");
            }

            public void Close()
            {
                _container.classList.remove("tss-open");
            }

            public HTMLElement Render()
            {
                return _container;
            }
        }
    }
}
