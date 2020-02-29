using System;
using System.Linq;
using System.Threading.Tasks;
using static Retyped.dom;
using static Tesserae.UI;

namespace Tesserae.Components
{
    public class ItemsList: IComponent, ISpecialCaseStyling
    {
        private readonly Grid _grid;
        private readonly Defer _defered;
        private Func<IComponent> _emptyListMessageGenerator;

        public ObservableList<IComponent> Items { get; }

        public HTMLElement StylingContainer    => _grid.StylingContainer;

        public bool PropagateToStackItemParent => false;

        public ItemsList(IComponent[] items, params UnitSize[] columns) : this(new ObservableList<IComponent>(items ?? new IComponent[0]), columns)
        {
        }

        public ItemsList(ObservableList<IComponent> items, params UnitSize[] columns)
        {
            Items                      = items ?? throw new ArgumentNullException(nameof(items));
            _grid                      = Grid(columns);
            _emptyListMessageGenerator = null;

            _defered = Defer.Observe(Items, observedItems =>
            {
                if (!observedItems.Any())
                {
                    if (_emptyListMessageGenerator is object)
                    {
                        return Task.FromResult<IComponent>(_grid.Children(_emptyListMessageGenerator()));
                    }
                    else
                    {
                        _grid.Clear();
                        return Task.FromResult<IComponent>(_grid);
                    }
                }

                return Task.FromResult<IComponent>(_grid.Children(Items.ToArray()));
            });
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
