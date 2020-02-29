﻿using Retyped;
using System;
using System.Linq;
using System.Threading.Tasks;
using static Retyped.dom;
using static Tesserae.UI;

namespace Tesserae.Components
{
    public class SearchableList<T> : IComponent, ISpecialCaseStyling where T : ISearchableItem
    {
        private readonly Defer _defered;
        private readonly Stack _stack;
        private readonly SearchBox _searchBox;
        private readonly Grid _grid;
        public HTMLElement StylingContainer => _grid.StylingContainer;
        public bool PropagateToStackItemParent => false;
        public ObservableList<T> Items { get; }
        private Func<IComponent> _emptyListMessageGenerator;

        public SearchableList(T[] items, UnitSize[] columns) : this(new ObservableList<T>(items ?? new T[0]), columns)
        {
        }

        public SearchableList(ObservableList<T> items, UnitSize[] columns)
        {
            Items = items;
            _searchBox = new SearchBox().Underlined().SetPlaceholder("Type to search").SearchAsYouType();
            _grid = Grid(columns);
            _defered = Defer.Observe(Items, item =>
            {
                var searchTerm = _searchBox.Text;
                var filteredItems = Items.Where(i => string.IsNullOrWhiteSpace(searchTerm) || i.IsMatch(searchTerm)).Select(i => (IComponent)i).ToArray();

                if (filteredItems.Any())
                {
                    return _grid.Children(filteredItems).AsTask();
                }
                else
                {
                    if (_emptyListMessageGenerator is object)
                    {
                        return _grid.Children(_emptyListMessageGenerator().GridColumnStrech()).AsTask();
                    }
                    else
                    {
                        _grid.Clear();
                        return _grid.AsTask();
                    }
                }
            });

            _searchBox.OnSearch((_, __) => _defered.Refresh());

            _stack = Stack().Vertical().Children(_searchBox, _defered.Grow(1)).HeightStretch().WidthStretch();
        }

        public SearchableList<T> WithNoResultsMessage(Func<IComponent> emptyListMessageGenerator)
        {
            _emptyListMessageGenerator = emptyListMessageGenerator ?? throw new ArgumentNullException(nameof(emptyListMessageGenerator));
            _defered.Refresh();
            return this;
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
