using System.Collections.Generic;
using System.Linq;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    [H5.Name("tss.IconToggle")]
    public class IconToggle : IComponent
    {
        private Stack _stack;
        private Dictionary<Item, Button> _items;
        private SettableObservable<Item> _itemsObservable;  
        public IconToggle(Item[] items)
        {
            _stack = HStack().NoDefaultMargin().Class("tss-icon-toggle").NoWrap();
            _items = new Dictionary<Item, Button>();
            _itemsObservable = new SettableObservable<Item>(items.First());
            foreach(var item in items)
            {
                var b = Button().Class("tss-icon-toggle-item").SetIcon(item.Icon).Tooltip(item.Tooltip).OnClick(() => Select(item));
                _items[item] = b;
                _stack.Add(b);
            }
        }

        public HTMLElement Render() => _stack.Render();

        public void Select(Item item)
        {
            foreach(var kv in _items)
            {
                if (kv.Key == item)
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

        public IObservable<Item> AsObservable() => _itemsObservable;

        public class Item
        {
            public Item(UIcons icon, string tooltip, object data = null)
            {
                Icon = icon;
                Tooltip = tooltip;
                Data = data;
            }

            public UIcons Icon { get; }
            public string Tooltip { get; }
            public object Data { get; }
        }
    }
}
