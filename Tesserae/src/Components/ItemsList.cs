using System;
using System.Linq;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    public sealed class ItemsList : IComponent, ISpecialCaseStyling
    {
        private readonly Grid _grid;
        private readonly Stack _stack;
        private readonly UnitSize _maxStackItemSize;
        private readonly DeferedComponent _defered;
        private Func<IComponent> _emptyListMessageGenerator;
        public ItemsList(IComponent[] items, params UnitSize[] columns) : this(new ObservableList<IComponent>(initialValues: items ?? new IComponent[0]), columns) { }

        public ObservableList<IComponent> Items { get; }

        public HTMLElement StylingContainer => _defered.Container;

        public bool PropagateToStackItemParent => true;

        public ItemsList(ObservableList<IComponent> items, params UnitSize[] columns)
        {
            Items = items ?? new ObservableList<IComponent>();

            if (columns.Length < 2)
            {
                _stack = Stack().Horizontal().Wrap().WidthStretch().MaxHeight(100.percent()).Scroll();
                _maxStackItemSize = columns.FirstOrDefault();
            }
            else
            {
                _grid = Grid(columns).WidthStretch().MaxHeight(100.percent()).Scroll();
            }
            _emptyListMessageGenerator = null;

            _defered = DeferedComponent.Observe(
                Items,
                observedItems =>
                {
                    if (!observedItems.Any())
                    {
                        if (_emptyListMessageGenerator is object)
                        {
                            if(_grid is object)
                            {
                                return _grid.Children(_emptyListMessageGenerator().GridColumnStretch()).AsTask();
                            }
                            else
                            {
                                return _stack.Children(_emptyListMessageGenerator().WidthStretch().HeightStretch()).AsTask();
                            }
                        }
                        else
                        {
                            if(_grid is object)
                            {
                                _grid.Clear();
                                return _grid.AsTask();
                            }
                            else
                            {
                                _stack.Clear();
                                return _stack.AsTask();
                            }
                        }
                    }
                    else
                    {
                        if(_grid is object)
                        {
                            return _grid.Children(observedItems).AsTask();
                        }
                        else
                        {
                            if (_maxStackItemSize is object)
                            {
                                return _stack.Children(observedItems.Select(i => i.Width(_maxStackItemSize)).ToArray()).AsTask();
                            }
                            else
                            {
                                return _stack.Children(observedItems.ToArray()).AsTask();
                            }
                        }
                    }
                }
            );
        }

        public ItemsList WithEmptyMessage(Func<IComponent> emptyListMessageGenerator)
        {
            _emptyListMessageGenerator = emptyListMessageGenerator ?? throw new ArgumentNullException(nameof(emptyListMessageGenerator));
            _defered.Refresh();
            return this;
        }

        public HTMLElement Render() => _defered.Render();
    }
}