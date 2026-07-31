using System;
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
    public class IconToggle<T> : IComponent, IBindableComponent<T>, IRoundedStyle
    {
        private readonly Stack                    _stack;
        private readonly Dictionary<Item, Button> _items;
        private readonly SettableObservable<T>    _itemsObservable;

        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        public IconToggle(Item[] items)
        {
            if (items == null || items.Length == 0)
            {
                throw new ArgumentException("An IconToggle needs at least one item.", nameof(items));
            }

            _stack = HStack().NoDefaultMargin().Class("tss-icon-toggle").NoWrap();
            _items = new Dictionary<Item, Button>();

            // A disabled item can't be clicked, so it can't be the one the control starts on either.
            var initial = items.FirstOrDefault(i => !i.IsDisabled) ?? items[0];

            _itemsObservable = new SettableObservable<T>(initial.Data);

            foreach (var item in items)
            {
                var b = Button().Class("tss-icon-toggle-item").SetIcon(item.Icon).Tooltip(item.Tooltip).OnClick(() => Select(item.Data));

                if (!string.IsNullOrEmpty(item.Text))
                {
                    b.SetText(item.Text);
                }

                if (item.IsDisabled)
                {
                    b.Disabled();
                }

                _items[item] = b;
                _stack.Add(b);
            }

            // The initial value is already in the observable, so paint it too — otherwise the control
            // renders with no visible selection until something calls Select.
            MarkSelected(initial.Data);
        }

        /// <summary>
        /// Renders the component's root HTML element.
        /// </summary>
        public HTMLElement Render() => _stack.Render();

        /// <summary>
        /// Selects the item carrying the given data. Values that don't match any item are ignored.
        /// </summary>
        public IconToggle<T> Select(T item)
        {
            if (!_items.Keys.Any(k => EqualityComparer<T>.Default.Equals(k.Data, item)))
            {
                return this;
            }

            MarkSelected(item);
            _itemsObservable.Value = item;

            return this;
        }

        /// <summary>
        /// Gets the currently selected data.
        /// </summary>
        public T Selected => _itemsObservable.Value;

        /// <summary>
        /// Returns the component's state as a(n) observable.
        /// </summary>
        public IObservable<T> AsObservable() => _itemsObservable;

        /// <summary>
        /// Programmatically selects an item as part of a two-way binding.
        /// Values that don't match any item are ignored.
        /// </summary>
        public void SetBoundValue(T value) => Select(value);

        /// <summary>
        /// Calls the handler whenever the selection changes, but not for the initial selection.
        /// </summary>
        public IconToggle<T> OnChange(Action<IconToggle<T>, T> onChange)
        {
            _itemsObservable.ObserveFutureChanges(value => onChange(this, value));

            return this;
        }

        /// <summary>
        /// Renders a denser control, for toolbars and other tight spots.
        /// </summary>
        public IconToggle<T> Compact()
        {
            var element = _stack.Render();
            element.classList.remove("tss-icon-toggle-large");
            element.classList.add("tss-icon-toggle-compact");

            return this;
        }

        /// <summary>
        /// Renders a roomier control, for use as a primary, page-level switch.
        /// </summary>
        public IconToggle<T> Large()
        {
            var element = _stack.Render();
            element.classList.remove("tss-icon-toggle-compact");
            element.classList.add("tss-icon-toggle-large");

            return this;
        }

        /// <summary>
        /// Stacks the items top to bottom instead of left to right.
        /// </summary>
        public IconToggle<T> Vertical()
        {
            _stack.Vertical();
            _stack.Render().classList.add("tss-icon-toggle-vertical");

            return this;
        }

        /// <summary>
        /// Stacks the items left to right, the default.
        /// </summary>
        public IconToggle<T> Horizontal()
        {
            _stack.Horizontal();
            _stack.Render().classList.remove("tss-icon-toggle-vertical");

            return this;
        }

        /// <summary>
        /// Stretches the control to the width of its container, with every item taking an equal share.
        /// </summary>
        public IconToggle<T> FullWidth()
        {
            _stack.Render().classList.add("tss-icon-toggle-full-width");

            return this;
        }

        /// <summary>
        /// Configures whether the whole control is disabled.
        /// </summary>
        public IconToggle<T> Disabled(bool value = true)
        {
            foreach (var kv in _items)
            {
                // An item disabled on its own stays disabled when the control is re-enabled.
                kv.Value.Disabled(value || kv.Key.IsDisabled);
            }

            _stack.Render().UpdateClassIf(value, "tss-icon-toggle-disabled");

            return this;
        }

        private void MarkSelected(T item)
        {
            foreach (var kv in _items)
            {
                if (EqualityComparer<T>.Default.Equals(kv.Key.Data, item))
                {
                    kv.Value.Class("tss-icon-toggle-item-selected");
                }
                else
                {
                    kv.Value.RemoveClass("tss-icon-toggle-item-selected");
                }
            }
        }

        public class Item
        {
            /// <summary>
            /// Initializes a new instance of this class.
            /// </summary>
            public Item(UIcons icon, string tooltip, T data, string text = null)
            {
                Icon    = icon;
                Tooltip = tooltip;
                Data    = data;
                Text    = text;
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
            /// <summary>
            /// Gets the label shown next to the icon, if any.
            /// </summary>
            public string Text { get; private set; }
            /// <summary>
            /// Gets whether this item can be selected.
            /// </summary>
            public bool IsDisabled { get; private set; }

            /// <summary>
            /// Sets a label to show next to the icon.
            /// </summary>
            public Item SetText(string text)
            {
                Text = text;

                return this;
            }

            /// <summary>
            /// Configures whether this item is disabled.
            /// </summary>
            public Item Disabled(bool value = true)
            {
                IsDisabled = value;

                return this;
            }
        }
    }
}
