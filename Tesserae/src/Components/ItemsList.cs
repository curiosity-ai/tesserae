using Retyped;
using System.Linq;
using System.Threading.Tasks;
using static Tesserae.UI;

namespace Tesserae.Components
{
    public class ItemsList: IComponent
    {
        private readonly Defer _defered;

        public ObservableList<IComponent> Items { get; }

        public ItemsList(IComponent[] items, UnitSize[] columns) : this(new ObservableList<IComponent>(items ?? new IComponent[0]), columns)
        {
        }

        public ItemsList(ObservableList<IComponent> items, UnitSize[] columns)
        {
            Items = items;
            _defered = Defer.Observe(Items, item =>
            {
                return Task.FromResult<IComponent>(Grid(columns).Children(Items.ToArray()));
            });
        }

        public dom.HTMLElement Render() => _defered.Render();
    }
}
