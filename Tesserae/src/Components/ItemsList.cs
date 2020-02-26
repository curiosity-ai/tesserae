using Retyped;
using System.Linq;
using System.Threading.Tasks;
using static Retyped.dom;
using static Tesserae.UI;

namespace Tesserae.Components
{
    public class ItemsList: IComponent, ISpecialCaseStyling
    {
        public HTMLElement StylingContainer => _grid.StylingContainer;

        public bool PropagateToStackItemParent => false;

        internal readonly Defer _defered;

        public ObservableList<IComponent> Items { get; }

        private readonly Grid _grid;

        public ItemsList(IComponent[] items, UnitSize[] columns) : this(new ObservableList<IComponent>(items ?? new IComponent[0]), columns)
        {
        }

        public ItemsList(ObservableList<IComponent> items, UnitSize[] columns)
        {
            Items = items;
            _grid = Grid(columns);
            _defered = Defer.Observe(Items, item =>
            {
                return Task.FromResult<IComponent>(_grid.Children(Items.ToArray()));
            });
        }

        public dom.HTMLElement Render() => _defered.Render();
    }
}
