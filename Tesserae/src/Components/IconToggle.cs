using System.Collections.Generic;
using System.Linq;
using static Transpose.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// A group of icon buttons of which exactly one is selected at a time, like a segmented control.
    /// </summary>
    [Transpose.Name("tss.IconToggle")]
    public class IconToggle<T> : IComponent, IBindableComponent<T>
    {
        private Stack                    _stack;
        private Dictionary<Item, Button> _items;
        private SettableObservable<T>    _itemsObservable;
        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        public IconToggle(Item[] items)
        {
            _stack           = HStack().NoDefaultMargin().Class("tss-icon-toggle").NoWrap();
            _items           = new Dictionary<Item, Button>();
            _itemsObservable = new SettableObservable<T>(items.First().Data);

            foreach (var item in items)
            {
                var b = Button().Class("tss-icon-toggle-item").SetIcon(item.Icon).Tooltip(item.Tooltip).OnClick(() => Select(item.Data));
                _items[item] = b;
                _stack.Add(b);
            }
        }

        /// <summary>
        /// Renders the component's root HTML element.
        /// </summary>
        public HTMLElement Render() => _stack.Render();

        /// <summary>
        /// Configures the component to select.
        /// </summary>
        public void Select(T item)
        {
            foreach (var kv in _items)
            {
                if (EqualityComparer<T>.Default.Equals(kv.Key.Data, item))
                {
                    kv.Value.Class("tss-icon-togle-item-selected");
                    _itemsObservable.Value = item;
                }
                else
                {
                    kv.Value.RemoveClass("tss-icon-togle-item-selected");
                }
            }
        }

        /// <summary>
        /// Returns the component's state as a(n) observable.
        /// </summary>
        public IObservable<T> AsObservable() => _itemsObservable;

        /// <summary>
        /// Programmatically selects an item as part of a two-way binding.
        /// Values that don't match any item are ignored.
        /// </summary>
        public void SetBoundValue(T value) => Select(value);

        public class Item
        {
            /// <summary>
            /// Initializes a new instance of this class.
            /// </summary>
            public Item(UIcons icon, string tooltip, T data)
            {
                Icon    = icon;
                Tooltip = tooltip;
                Data    = data;
            }

            /// <summary>
            /// Gets or sets the icon shown by the component.
            /// </summary>
            public UIcons Icon { get; }
            /// <summary>
            /// Sets the tooltip shown when the user hovers over the component.
            /// </summary>
            public string Tooltip { get; }
            /// <summary>
            /// Gets or sets the data.
            /// </summary>
            public T Data { get; }
        }
    }
}