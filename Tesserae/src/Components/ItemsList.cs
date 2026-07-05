using System;
using System.Linq;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// A simple, non-virtualised vertical list of arbitrary items, used when a <see cref="DetailsList{T}"/> would be
    /// overkill.
    /// </summary>
    [H5.Name("tss.ItemsList")]
    public sealed class ItemsList : IComponent, ISpecialCaseStyling
    {
        private readonly Grid             _grid;
        private readonly Stack            _stack;
        private readonly UnitSize         _maxStackItemSize;
        private readonly DeferedComponent _defered;
        private          Func<IComponent> _emptyListMessageGenerator;
        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        public ItemsList(IComponent[] items, params UnitSize[] columns) : this(new ObservableList<IComponent>(initialValues: items ?? new IComponent[0]), columns) { }

        /// <summary>
        /// Adds the given items to the component.
        /// </summary>
        public ObservableList<IComponent> Items { get; }

        /// <summary>
        /// Gets or sets the styling container.
        /// </summary>
        public HTMLElement StylingContainer => _defered.Container;

        /// <summary>
        /// Gets or sets the propagate to stack item parent.
        /// </summary>
        public bool PropagateToStackItemParent => true;

        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        public ItemsList(ObservableList<IComponent> items, params UnitSize[] columns)
        {
            Items = items ?? new ObservableList<IComponent>();

            if (columns.Length < 2)
            {
                _stack            = HStack().NoGap().Wrap().WS().MaxHeight(100.percent()).Scroll();
                _maxStackItemSize = columns.FirstOrDefault() ?? 100.percent();
            }
            else
            {
                _grid = Grid(columns).NoGap().WS().MaxHeight(100.percent()).Scroll();
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
                            if (_grid is object)
                            {
                                return _grid.Children(_emptyListMessageGenerator().GridColumnStretch()).AsTask();
                            }
                            else
                            {
                                return _stack.Children(_emptyListMessageGenerator().WS().HeightStretch()).AsTask();
                            }
                        }
                        else
                        {
                            if (_grid is object)
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
                        if (_grid is object)
                        {
                            return _grid.Children(observedItems).AsTask();
                        }
                        else
                        {
                            return _stack.Children(observedItems.Select(i => i.Width(_maxStackItemSize)).ToArray()).AsTask();
                        }
                    }
                }
            );
        }

        /// <summary>
        /// Returns the component configured with the given empty message.
        /// </summary>
        public ItemsList WithEmptyMessage(Func<IComponent> emptyListMessageGenerator)
        {
            _emptyListMessageGenerator = emptyListMessageGenerator ?? throw new ArgumentNullException(nameof(emptyListMessageGenerator));
            _defered.Refresh();
            return this;
        }

        /// <summary>
        /// Renders the component's root HTML element.
        /// </summary>
        public HTMLElement Render() => _defered.Render();
    }
}