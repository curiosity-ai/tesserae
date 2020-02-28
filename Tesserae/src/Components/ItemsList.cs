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

        public ItemsList(IComponent[] items, Func<IComponent> emptyListMessageGenerator = null, params UnitSize[] columns) : this(new ObservableList<IComponent>(items ?? new IComponent[0]), emptyListMessageGenerator, columns)
        {
        }

        public ItemsList(ObservableList<IComponent> items, Func<IComponent> emptyListMessageGenerator, params UnitSize[] columns)
        {
            Items                      = items ?? throw new ArgumentNullException(nameof(items));
            _grid                      = Grid(columns);
            _emptyListMessageGenerator = emptyListMessageGenerator;

            _defered = Defer.Observe(Items, item =>
            {
                if (!Items.Any() && _emptyListMessageGenerator != null)
                {
                     return Task.FromResult<IComponent>(_grid.Children(_emptyListMessageGenerator()));
                }

                return Task.FromResult<IComponent>(_grid.Children(Items.ToArray()));
            });
        }

        public ItemsList WithEmptyMessage(Func<IComponent> emptyListMessageGenerator)
        {
            _emptyListMessageGenerator = emptyListMessageGenerator ?? throw new ArgumentNullException(nameof(emptyListMessageGenerator));

            return this;
        }

        public HTMLElement Render() => _defered.Render();
    }
}
