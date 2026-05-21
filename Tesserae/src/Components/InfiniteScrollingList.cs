using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// A list that lazily loads additional items as the user scrolls toward the bottom, suitable for very large or
    /// unbounded result sets.
    /// </summary>
    [H5.Name("tss.InfiniteScrollingList")]
    public sealed class InfiniteScrollingList : IComponent, ISpecialCaseStyling
    {
        private readonly Grid             _grid;
        private readonly Stack            _stack;
        private readonly UnitSize         _maxStackItemSize;
        private readonly HTMLDivElement   _container;
        private          Func<IComponent> _emptyListMessageGenerator;

        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        public InfiniteScrollingList(Func<IComponent[]>       getNextItemPage, params UnitSize[]  columns) : this(new IComponent[0], () => Task.FromResult<IComponent[]>(getNextItemPage()), columns) { }
        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        public InfiniteScrollingList(IComponent[]             items,           Func<IComponent[]> getNextItemPage, params UnitSize[] columns) : this(items, () => Task.FromResult<IComponent[]>(getNextItemPage()), columns) { }
        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        public InfiniteScrollingList(Func<Task<IComponent[]>> getNextItemPage, params UnitSize[]  columns) : this(new IComponent[0], getNextItemPage, columns) { }

        /// <summary>
        /// Gets or sets the styling container.
        /// </summary>
        public HTMLElement StylingContainer => _container;

        /// <summary>
        /// Gets or sets the propagate to stack item parent.
        /// </summary>
        public bool PropagateToStackItemParent => true;


        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        public InfiniteScrollingList(IComponent[] items, Func<Task<IComponent[]>> getNextItemPage, params UnitSize[] columns)
        {
            _container = Div(_("tss-basiclist"));

            if (columns.Length < 2)
            {
                _stack            = HStack().Wrap().WS().MaxHeight(100.percent()).Scroll();
                _maxStackItemSize = columns.FirstOrDefault() ?? 100.percent();
            }
            else
            {
                _grid = Grid(columns).WS().MaxHeight(100.percent()).GridColumnStretch().Scroll();
            }
            _emptyListMessageGenerator = null;
            AddItems(items);

            if (getNextItemPage is object)
            {
                var vs = VisibilitySensor(v =>
                {
                    Task.Run<Task>(async () =>
                    {
                        if (_grid is object)
                        {
                            var nextPageItems = await getNextItemPage();
                            _grid.Remove(v);

                            if (nextPageItems is object && nextPageItems.Any())
                            {
                                foreach (var item in nextPageItems)
                                {
                                    _grid.Add(item);
                                }
                                v.Reset();
                                _grid.Add(v);
                            }
                        }
                        else
                        {
                            var nextPageItems = await getNextItemPage();
                            _stack.Remove(v);

                            if (nextPageItems is object && nextPageItems.Any())
                            {
                                foreach (var item in nextPageItems)
                                {
                                    _stack.Add(item.W(_maxStackItemSize));
                                }
                                v.Reset();
                                _stack.Add(v);
                            }
                        }
                    }).FireAndForget();

                }, message: TextBlock("Loading..."));

                if (_grid is object)
                {
                    _grid.Add(vs);
                }
                else
                {
                    _stack.Add(vs);
                }
            }
        }


        private void AddItems(IComponent[] items)
        {
            if (items is object && items.Any())
            {
                if (_grid is object)
                {
                    foreach (var item in items)
                    {
                        _grid.Add(item);
                    }
                }
                else
                {
                    foreach (var item in items)
                    {
                        _stack.Add(item.W(_maxStackItemSize));
                    }
                }
            }

            if (_emptyListMessageGenerator is object)
            {
                if (_grid is object)
                {
                    _grid.Children(_emptyListMessageGenerator().GridColumnStretch());
                }
                else
                {
                    _stack.Children(_emptyListMessageGenerator().WS().HeightStretch());
                }
            }
        }

        /// <summary>
        /// Returns the component configured with the given empty message.
        /// </summary>
        public InfiniteScrollingList WithEmptyMessage(Func<IComponent> emptyListMessageGenerator)
        {
            _emptyListMessageGenerator = emptyListMessageGenerator ?? throw new ArgumentNullException(nameof(emptyListMessageGenerator));
            return this;
        }

        /// <summary>
        /// Renders the component's root HTML element.
        /// </summary>
        public HTMLElement Render()
        {
            if (_grid is object)
            {
                _container.appendChild(_grid.Render());
            }
            else
            {
                _container.appendChild(_stack.Render());
            }
            return _container;
        }
    }
}