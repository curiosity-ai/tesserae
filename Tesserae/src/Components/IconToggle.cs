using System.Collections.Generic;
using System.Linq;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// A group of icon buttons of which exactly one is selected at a time, like a segmented control.
    /// </summary>
    [H5.Name("tss.IconToggle")]
    public class IconToggle<T> : IComponent
    {
        private Stack _stack;
        private Dictionary<Item, Button> _items;
        private SettableObservable<T> _itemsObservable;  
        public IconToggle(Item[] items)
        {
            _stack = HStack().NoDefaultMargin().Class("tss-icon-toggle").NoWrap();
            _items = new Dictionary<Item, Button>();
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

        public void Select(T item)
        {
            foreach(var kv in _items)
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

        public class Item
        {
            public Item(UIcons icon, string tooltip, T data)
            {
                Icon = icon;
                Tooltip = tooltip;
                Data = data;
            }

            public UIcons Icon { get; }
            public string Tooltip { get; }
            public T Data { get; }
        }
    }
}
