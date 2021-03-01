using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    public sealed class InfiniteScrollingList : IComponent, ISpecialCaseStyling
    {
        private readonly Grid                _grid;
        private readonly Stack               _stack;
        private readonly UnitSize            _maxStackItemSize;
        private readonly HTMLDivElement      _container;
        private          Func<IComponent>    _emptyListMessageGenerator;
        private readonly HashSet<IComponent> _currentItems = new HashSet<IComponent>();

        public InfiniteScrollingList(Func<IComponent[]>       getNextItemPage, params UnitSize[]  columns) : this(new IComponent[0], () => Task.FromResult<IComponent[]>(getNextItemPage()), columns) { }
        public InfiniteScrollingList(IComponent[]             items,           Func<IComponent[]> getNextItemPage, params UnitSize[] columns) : this(items, () => Task.FromResult<IComponent[]>(getNextItemPage()), columns) { }
        public InfiniteScrollingList(Func<Task<IComponent[]>> getNextItemPage, params UnitSize[]  columns) : this(new IComponent[0], getNextItemPage, columns) { }

        public HTMLElement StylingContainer => _container;

        public bool PropagateToStackItemParent => true;

        private void AddItems(IComponent[] items)
        {
            if (items is object && items.Any() && _currentItems is object && _currentItems.Any())
            {
                var toAdd = items.Where(i => !_currentItems.Contains(i)).ToArray();

                if (_grid is object)
                {
                    foreach (var item in toAdd)
                    {
                        _currentItems.Add(item);
                        _grid.Add(item);
                    }
                }
                else
                {
                    foreach (var item in toAdd)
                    {
                        _currentItems.Add(item);
                        _stack.Add(item.W(_maxStackItemSize));
                    }
                }
            }
            if ((!(_currentItems is object) || _currentItems.Any()) && _emptyListMessageGenerator is object)
            {
                if (_grid is object)
                {
                    _grid.Children(_emptyListMessageGenerator().GridColumnStretch()).AsTask();
                }
                else
                {
                    _stack.Children(_emptyListMessageGenerator().WidthStretch().HeightStretch()).AsTask();
                }
            }
        }

        public InfiniteScrollingList(IComponent[] items, Func<Task<IComponent[]>> getNextItemPage, params UnitSize[] columns)
        {
            _container = Div(_("tss-basiclist"));

            if (columns.Length < 2)
            {
                _stack = Stack().Horizontal().Wrap().WidthStretch().MaxHeight(100.percent()).Scroll();
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
                                foreach (var item in nextPageItems)
                                {
                                    _grid.Add(item.W(_maxStackItemSize));
                                }
                                v.Reset();
                                _grid.Add(v);
                            }
                            else
                            {
                                var nextPageItems = await getNextItemPage();
                                _stack.Remove(v);
                                foreach (var item in nextPageItems)
                                {
                                    _stack.Add(item.W(_maxStackItemSize));
                                }
                                v.Reset();
                                _stack.Add(v);
                            }
                        }).FireAndForget();

                    },message: TextBlock("Loading..."));

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

        public InfiniteScrollingList WithEmptyMessage(Func<IComponent> emptyListMessageGenerator)
        {
            _emptyListMessageGenerator = emptyListMessageGenerator ?? throw new ArgumentNullException(nameof(emptyListMessageGenerator));
            return this;
        }

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