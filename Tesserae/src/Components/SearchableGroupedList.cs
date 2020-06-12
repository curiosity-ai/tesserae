using System;
using System.Collections.Generic;
using System.Linq;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae.Components
{
    public class SearchableGroupedList<TSearchableGroupedItem> : IComponent, ISpecialCaseStyling where TSearchableGroupedItem : ISearchableGroupedItem
    {
        private readonly Func<string, IComponent> _groupedItemHeaderGenerator;
        private readonly IDefer _defered;
        private readonly Stack _searchBoxContainer;
        private readonly List<IComponent> _searchBoxContainerComponents;
        private readonly Stack _stack;
        private readonly SearchBox _searchBox;
        private readonly ItemsList _list;
        public HTMLElement StylingContainer => _stack.InnerElement;
        public bool PropagateToStackItemParent => true;
        public ObservableList<IComponent> Items { get; }

        public SearchableGroupedList(TSearchableGroupedItem[] items, Func<string, IComponent> groupedItemHeaderGenerator, params UnitSize[] columns)
            : this(new ObservableList<TSearchableGroupedItem>(items ?? new TSearchableGroupedItem[0]), groupedItemHeaderGenerator, columns)
        {
        }

        public SearchableGroupedList(ObservableList<TSearchableGroupedItem> originalItems, Func<string, IComponent> groupedItemHeaderGenerator, params UnitSize[] columns)
        {
            _groupedItemHeaderGenerator = groupedItemHeaderGenerator;
            _searchBox                  = new SearchBox().Underlined().SetPlaceholder("Type to search").SearchAsYouType().Width(100.px()).Grow();
            _list                       = ItemsList(new IComponent[0], columns);

            Items = new ObservableList<IComponent>();

            _defered = Defer(Items, item =>
            {
                var searchTerm = _searchBox.Text;

                var filteredItems = originalItems.OfType<TSearchableGroupedItem>().Where(i => string.IsNullOrWhiteSpace(searchTerm) || i.IsMatch(searchTerm)).ToArray();

                AddGroupedItems(filteredItems, _list.Items, isGrid: (columns is object && columns.Length > 1));

                return _list.Stretch().AsTask();
            }).WidthStretch().Height(100.px()).Grow();

            originalItems.StartObserving(_ => _defered.Refresh());

            _searchBox.OnSearch((_, __) => _defered.Refresh());

            _searchBoxContainer           = Stack().Horizontal().WidthStretch().Children(_searchBox).AlignItems(ItemAlign.Center);
            _searchBoxContainerComponents = new List<IComponent> { _searchBox };
            _stack                        = Stack().Children(_searchBoxContainer, _defered.Scroll()).WidthStretch().MaxHeight(100.percent());
        }

        public SearchableGroupedList<TSearchableGroupedItem> WithNoResultsMessage(Func<IComponent> emptyListMessageGenerator)
        {
            _list.WithEmptyMessage(emptyListMessageGenerator ?? throw new ArgumentNullException(nameof(emptyListMessageGenerator)));
            _defered.Refresh();
            return this;
        }

        public SearchableGroupedList<TSearchableGroupedItem> SearchBox(Action<SearchBox> sb)
        {
            sb(_searchBox);
            return this;
        }

        public SearchableGroupedList<TSearchableGroupedItem> BeforeSearchBox(params IComponent[] beforeComponents)
        {
            foreach(var component in beforeComponents.Reverse<IComponent>())
            {
                _searchBoxContainerComponents.Insert(0, component);
            }
            _searchBoxContainer.Children(_searchBoxContainerComponents);
            return this;
        }

        public SearchableGroupedList<TSearchableGroupedItem> AfterSearchBox(params IComponent[] afterComponents)
        {
            _searchBoxContainerComponents.AddRange(afterComponents);
            _searchBoxContainer.Children(_searchBoxContainerComponents);
            return this;
        }

        public HTMLElement Render() => _stack.Render();


        private void AddGroupedItems(IEnumerable<TSearchableGroupedItem> items, ObservableList<IComponent> observableList, bool isGrid)
        {
            observableList.Clear();

            if (items is object)
            {
                items = items.ToList();

                if (items.Any())
                {
                    foreach (var groupedItems in items.GroupBy(item => item.Group))
                    {
                        var header = new GroupedItemsHeader(groupedItems.Key, _groupedItemHeaderGenerator);
                        if (isGrid) 
                        {
                            header.GridColumn(1, -1);
                        }

                        observableList.Add(header);
                        observableList.AddRange(groupedItems.Select(t => t.Render()));
                    }
                }
            }
        }

        private class GroupedItemsHeader : IComponent
        {
            private readonly IComponent _component;

            public GroupedItemsHeader(string group, Func<string, IComponent> groupedItemHeaderGenerator)
            {
                _component = groupedItemHeaderGenerator(group);
            }

            public HTMLElement Render() => _component.Render();
        }
    }

    public interface ISearchableGroupedItem : ISearchableItem
    {
        string Group { get; }
    }
}
