using System;
using System.Collections.Generic;
using System.Linq;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    [H5.Name("tss.Carousel")]
    public class Carousel : ComponentBase<Carousel, HTMLDivElement>, IContainer<Carousel, IComponent>
    {
        private readonly HTMLElement _itemsContainer;
        private readonly List<IComponent> _items = new List<IComponent>();
        private int _currentIndex = 0;

        public Carousel()
        {
            _itemsContainer = Div(_("tss-carousel-items"));
            InnerElement = Div(_("tss-carousel"), _itemsContainer);

            var prevBtn = Button().SetIcon(UIcons.AngleLeft).Class("tss-carousel-btn tss-carousel-btn-prev").OnClick(() => Previous());
            var nextBtn = Button().SetIcon(UIcons.AngleRight).Class("tss-carousel-btn tss-carousel-btn-next").OnClick(() => Next());

            InnerElement.appendChild(prevBtn.Render());
            InnerElement.appendChild(nextBtn.Render());
        }

        public void Add(IComponent component)
        {
            _items.Add(component);
            var itemWrapper = Div(_("tss-carousel-item"), component.Render());
            _itemsContainer.appendChild(itemWrapper);
            UpdateLayout();
        }

        public void Clear()
        {
            _items.Clear();
            ClearChildren(_itemsContainer);
            _currentIndex = 0;
            UpdateLayout();
        }

        public void Replace(IComponent newComponent, IComponent oldComponent)
        {
            var index = _items.IndexOf(oldComponent);
            if (index >= 0)
            {
                _items[index] = newComponent;
                var wrapper = _itemsContainer.children[(uint)index];
                ClearChildren((HTMLElement)wrapper);
                ((HTMLElement)wrapper).appendChild(newComponent.Render());
            }
        }

        public void Next()
        {
            if (_items.Count == 0) return;
            _currentIndex = (_currentIndex + 1) % _items.Count;
            UpdateLayout();
        }

        public void Previous()
        {
            if (_items.Count == 0) return;
            _currentIndex = (_currentIndex - 1 + _items.Count) % _items.Count;
            UpdateLayout();
        }

        private void UpdateLayout()
        {
            _itemsContainer.style.transform = $"translateX(-{_currentIndex * 100}%)";
        }

        public override HTMLElement Render()
        {
            return InnerElement;
        }
    }
}
