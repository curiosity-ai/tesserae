using System.Collections.Generic;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    [H5.Name("tss.Accordion")]
    public sealed class Accordion : ComponentBase<Accordion, HTMLElement>
    {
        private readonly List<Expander> _items;
        private bool _allowMultiple;

        public Accordion(params Expander[] items)
        {
            InnerElement   = Div(_("tss-accordion"));
            _items         = new List<Expander>();
            _allowMultiple = true;

            AddItems(items);
        }

        /// <summary>
        /// Gets or sets whether multiple sections can be expanded at once.
        /// </summary>
        public bool AllowMultiple
        {
            get => _allowMultiple;
            set
            {
                _allowMultiple = value;
                if (!_allowMultiple)
                {
                    CollapseToSingle();
                }
            }
        }

        public Accordion AddItem(Expander item)
        {
            if (item == null)
            {
                return this;
            }

            _items.Add(item);
            InnerElement.appendChild(item.Render());

            item.OnToggle(expander =>
            {
                if (!_allowMultiple && expander.IsExpanded)
                {
                    CollapseOthers(expander);
                }
            });

            return this;
        }

        public Accordion AddItems(params Expander[] items)
        {
            if (items == null)
            {
                return this;
            }

            foreach (var item in items)
            {
                AddItem(item);
            }

            return this;
        }

        public Accordion Items(params Expander[] items) => AddItems(items);

        public Accordion AllowMultipleOpen(bool value = true)
        {
            AllowMultiple = value;
            return this;
        }

        private void CollapseToSingle()
        {
            var firstExpanded = _items.Find(item => item.IsExpanded);

            foreach (var item in _items)
            {
                if (item != firstExpanded)
                {
                    item.Collapse();
                }
            }
        }

        private void CollapseOthers(Expander keepOpen)
        {
            foreach (var item in _items)
            {
                if (item != keepOpen)
                {
                    item.Collapse();
                }
            }
        }

        public override HTMLElement Render()
        {
            return InnerElement;
        }
    }
}
