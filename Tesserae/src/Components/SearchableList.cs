using Retyped;
using System;
using System.Linq;
using System.Threading.Tasks;
using static Tesserae.UI;

namespace Tesserae.Components
{
    public class SearchableList<T> : IComponent where T : ISearchableItem
    {
        private readonly Defer _defered;
        private readonly Stack _stack;
        private readonly SearchBox _searchBox;

        public ObservableList<T> Items { get; }

        public SearchableList(T[] items, UnitSize[] columns) : this(new ObservableList<T>(items ?? new T[0]), columns)
        {
        }

        public SearchableList(ObservableList<T> items, UnitSize[] columns)
        {
            Items = items;
            _searchBox = new SearchBox().Underlined().SetPlaceholder("Type to search").SearchAsYouType();
            _defered = Defer.Observe(Items, item =>
            {
                var searchTerm = _searchBox.Text;
                return Task.FromResult<IComponent>(Grid(columns).Children(Items.Where(i => string.IsNullOrWhiteSpace(searchTerm) || i.IsMatch(searchTerm)).Select(i => (IComponent)i).ToArray()));
            });

            _searchBox.OnSearch((_, __) => _defered.Refresh());

            _stack = Stack().Vertical().Children(_searchBox, _defered.Grow(1)).HeightStretch().WidthStretch();
        }

        public SearchableList<T> SearchBox(Action<SearchBox> sb)
        {
            sb(_searchBox);
            return this;
        }

        public dom.HTMLElement Render() => _stack.Render();
    }

    public interface ISearchableItem : IComponent
    {
        bool IsMatch(string searchTerm);
    }
}
