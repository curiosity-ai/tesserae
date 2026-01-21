using System;
using System.Collections.Generic;
using System.Linq;
using H5.Core;
using static H5.Core.dom;

namespace Tesserae
{
    /// <summary>
    /// A Stack component that allows its children to be reordered via drag and drop.
    /// </summary>
    public class SortableStack : IComponent, IHasBackgroundColor, IHasMarginPadding, ISpecialCaseStyling, ICanWrap
    {
        private Stack                   _container;
        private List<SortableStackItem> _items     = new List<SortableStackItem>();
        private List<string>            _itemOrder = new List<string>();
        private Action<string[]>        _onSortingChanged;
        /// <summary>
        /// Initializes a new instance of the SortableStack class.
        /// </summary>
        /// <param name="orientation">The orientation of the stack (vertical or horizontal).</param>
        public SortableStack(Stack.Orientation orientation = Stack.Orientation.Vertical)
        {
            _container = new Stack(orientation);

            var sortable = new Sortable(_container.Render(), new SortableOptions()
            {
                animation  = 150,
                invertSwap = true,
                ghostClass = "tss-sortable-ghost",
                filter = ".tss-sortable-disable",
                swapThreshold = 0.65,
                onEnd = e =>
                {
                    if (e.oldIndex != e.newIndex)
                    {
                        var old = _itemOrder[e.oldIndex];
                        _itemOrder.RemoveAt(e.oldIndex);
                        _itemOrder.Insert(e.newIndex, old);
                        _onSortingChanged?.Invoke(GetCurrentSorting());
                    }
                }
            });
        }
        /// <summary>
        /// Adds a component to the sortable stack.
        /// </summary>
        /// <param name="identifier">A unique identifier for the component.</param>
        /// <param name="component">The component to add.</param>
        public void Add(string identifier, IComponent component)
        {
            _container.Add((IComponent)component);
            _items.Add(new SortableStackItem { Component = component, Identifier = identifier });
            _itemOrder.Add(identifier);
        }

        /// <summary>
        /// Adds a SortableStackItem to the stack.
        /// </summary>
        /// <param name="item">The item to add.</param>
        public void Add(SortableStackItem item)
        {
            _container.Add(item.Component);
            _items.Add(item);
            _itemOrder.Add(item.Identifier);
        }

        /// <summary>
        /// Sets the children of the sortable stack.
        /// </summary>
        /// <param name="children">The children items.</param>
        /// <returns>The current instance of the type.</returns>
        public SortableStack Children(params SortableStackItem[] children)
        {
            _container.Clear();
            _items.Clear();

            foreach (var item in children)
            {
                Add(item);
            }
            return this;
        }
        /// <summary>
        /// Renders the sortable stack.
        /// </summary>
        /// <returns>The rendered HTMLElement.</returns>
        public dom.HTMLElement Render()
        {
            return _container.Render();
        }

        /// <summary>
        /// Aligns items in the center.
        /// </summary>
        /// <returns>The current instance of the type.</returns>
        public SortableStack AlignItemsCenter()
        {
            _container.AlignItemsCenter();
            return this;
        }

        /// <summary>
        /// Loads the sorting order of the items.
        /// Should be called after all items have been added.
        /// </summary>
        /// <param name="itemOrder">An array of identifiers in the desired order.</param>
        public void LoadSorting(string[] itemOrder)
        {
            _itemOrder = itemOrder.ToList();

            Refresh();
        }
        private void Refresh()
        {
            _container.Clear();

            foreach (var item in _items.OrderBy(i => _itemOrder.IndexOf(i.Identifier)))
            {
                _container.Add(item.Component);
            }
        }
        private string[] GetCurrentSorting()
        {
            return _itemOrder.ToArray();
        }
        /// <summary>
        /// Adds a sorting change event handler.
        /// </summary>
        /// <param name="onSortingChanged">The event handler action.</param>
        public void OnSortingChanged(Action<string[]> onSortingChanged)
        {
            _onSortingChanged = onSortingChanged;
        }

        /// <summary>Gets or sets the background color.</summary>
        public string Background
        {
            get => _container.Background;
            set => _container.Background = value;
        }
        /// <summary>Gets or sets the margin.</summary>
        public string Margin
        {
            get => _container.Margin;
            set => _container.Margin = value;
        }

        /// <summary>Gets or sets the padding.</summary>
        public string Padding
        {
            get => _container.Padding;
            set => _container.Padding = value;
        }

        /// <summary>Gets the styling container.</summary>
        public dom.HTMLElement StylingContainer           => _container.StylingContainer;

        /// <summary>Gets whether styling should propagate to the stack item parent.</summary>
        public bool            PropagateToStackItemParent => _container.PropagateToStackItemParent;

        /// <summary>Gets or sets whether the stack can wrap its items.</summary>
        public bool CanWrap
        {
            get => _container.CanWrap;
            set => _container.CanWrap = value;
        }

        /// <summary>
        /// Disables wrapping of items.
        /// </summary>
        /// <returns>The current instance of the type.</returns>
        public SortableStack NoWrap()
        {
            CanWrap = false;
            return this;
        }

        /// <summary>
        /// Enables wrapping of items.
        /// </summary>
        /// <returns>The current instance of the type.</returns>
        public SortableStack Wrap()
        {
            CanWrap = true;
            return this;
        }

        /// <summary>
        /// Sets the alignment of items.
        /// </summary>
        /// <param name="align">The alignment.</param>
        /// <returns>The current instance of the type.</returns>
        public SortableStack AlignItems(ItemAlign align)
        {
            _container.AlignItems(align);
            return this;
        }
        /// <summary>
        /// Sets the stack to use relative positioning.
        /// </summary>
        /// <returns>The current instance of the type.</returns>
        public SortableStack Relative()
        {
            _container.Relative();
            return this;
        }

        /// <summary>
        /// Sets the content alignment.
        /// </summary>
        /// <param name="align">The alignment.</param>
        /// <returns>The current instance of the type.</returns>
        public SortableStack AlignContent(ItemAlign align)
        {
            _container.AlignContent(align);
            return this;
        }
        /// <summary>
        /// Sets the content justification.
        /// </summary>
        /// <param name="justify">The justification.</param>
        /// <returns>The current instance of the type.</returns>
        public SortableStack JustifyContent(ItemJustify justify)
        {
            _container.JustifyContent(justify);
            return this;
        }

        /// <summary>
        /// Sets the item justification.
        /// </summary>
        /// <param name="justify">The justification.</param>
        /// <returns>The current instance of the type.</returns>
        public SortableStack JustifyItems(ItemJustify justify)
        {
            _container.JustifyItems(justify);
            return this;
        }
        /// <summary>
        /// Removes event propagation.
        /// </summary>
        /// <returns>The current instance of the type.</returns>
        public SortableStack RemovePropagation()
        {
            _container.RemovePropagation();
            return this;
        }

        /// <summary>
        /// Adds a mouse over event handler.
        /// </summary>
        public SortableStack OnMouseOver(ComponentEventHandler<Stack, Event> onMouseOver)
        {
            _container.OnMouseOver(onMouseOver);
            return this;
        }
        /// <summary>
        /// Adds a mouse out event handler.
        /// </summary>
        public SortableStack OnMouseOut(ComponentEventHandler<Stack, Event> onMouseOut)
        {
            _container.OnMouseOut(onMouseOut);
            return this;
        }

        /// <summary>
        /// Clears all items from the sortable stack.
        /// </summary>
        public void Clear()
        {
            _items.Clear();
            _itemOrder.Clear();
            _container.Clear();
        }

        /// <summary>
        /// Removes the default margin from the stack.
        /// </summary>
        /// <returns>The current instance of the type.</returns>
        public SortableStack NoDefaultMargin()
        {
            _container.NoDefaultMargin();
            return this;
        }

        /// <summary>
        /// Removes an item from the sortable stack.
        /// </summary>
        /// <param name="identifer">The identifier of the item to remove.</param>
        public void Remove(string identifer)
        {
            _itemOrder.Remove(identifer);

            foreach (var i in _items)
            {
                if (i.Identifier == identifer)
                {
                    _container.Remove(i.Component);
                }
            }

            _items.RemoveAll(i => i.Identifier == identifer);
        }
    }

    /// <summary>
    /// Represents an item within a SortableStack.
    /// </summary>
    public class SortableStackItem
    {
        /// <summary>Gets or sets the component.</summary>
        public IComponent Component  { get; set; }
        /// <summary>Gets or sets the unique identifier.</summary>
        public string     Identifier { get; set; }
    }
}