using System;
using System.Collections.Generic;
using System.Linq;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    [H5.Name("tss.SearchableGroupedList")]
    public class SearchableGroupedList<T> : IComponent, ISpecialCaseStyling where T : ISearchableGroupedItem
    {
        private readonly Func<string, IComponent> _groupedItemHeaderGenerator;
        private readonly IDefer                   _defered;
        private readonly Stack                    _searchBoxContainer;
        private readonly List<IComponent>         _searchBoxContainerComponents;
        private readonly Stack                    _stack;
        private readonly SearchBox                _searchBox;
        private readonly ItemsList                _list;
        private          IComparer<string>        _groupComparer;

        private UnitSize          _virtualizedItemHeight;
        private readonly List<LazyVirtualItem> _virtualItems = new List<LazyVirtualItem>();

        public HTMLElement                StylingContainer           => _stack.InnerElement;
        public bool                       PropagateToStackItemParent => true;
        public ObservableList<IComponent> Items                      { get; }

        public SearchableGroupedList(T[] items, Func<string, IComponent> groupedItemHeaderGenerator, params UnitSize[] columns)
            : this(new ObservableList<T>(initialValues: items ?? new T[0]), groupedItemHeaderGenerator, columns)
        {
        }

        public SearchableGroupedList(ObservableList<T> originalItems, Func<string, IComponent> groupedItemHeaderGenerator, params UnitSize[] columns)
        {
            _groupedItemHeaderGenerator = groupedItemHeaderGenerator;
            _searchBox                  = new SearchBox().Underlined().SetPlaceholder("Type to search").SearchAsYouType().Width(100.px()).Grow();
            _list                       = ItemsList(new IComponent[0], columns);

            _groupComparer = StringComparer.OrdinalIgnoreCase;

            Items = new ObservableList<IComponent>();

            _defered =
                DeferSync(
                        Items,
                        item =>
                        {
                            var searchTerms   = (_searchBox.Text ?? "").Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            var filteredItems = originalItems.OfType<T>().Where(i => searchTerms.Length == 0 || searchTerms.All(st => i.IsMatch(st))).ToArray();
                            AddGroupedItems(filteredItems, _list.Items, isGrid: (columns is object && columns.Length > 1));
                            window.setTimeout((_) => RecomputeVisibleVirtualItems(), 1);
                            return _list.S();
                        }
                    )
                   .WS()
                   .Grow();

            originalItems.Observe(_ => _defered.Refresh());

            _searchBox.OnSearch((_, __) => _defered.Refresh());

            _searchBoxContainer           = Stack().Horizontal().WS().Children(_searchBox).AlignItems(ItemAlign.Center);
            _searchBoxContainerComponents = new List<IComponent> { _searchBox };
            _stack                        = Stack().Children(_searchBoxContainer, _defered.Scroll()).WS().MaxHeight(100.percent());
        }

        public SearchableGroupedList<T> WithNoResultsMessage(Func<IComponent> emptyListMessageGenerator)
        {
            _list.WithEmptyMessage(emptyListMessageGenerator ?? throw new ArgumentNullException(nameof(emptyListMessageGenerator)));
            _defered.Refresh();
            return this;
        }

        public SearchableGroupedList<T> WithGroupOrdering(IComparer<string> groupComparer)
        {
            _groupComparer = groupComparer;
            _defered.Refresh();
            return this;
        }

        public SearchableGroupedList<T> SearchBox(Action<SearchBox> sb)
        {
            sb(_searchBox);
            return this;
        }

        public SearchableGroupedList<T> CaptureSearchBox(out SearchBox sb)
        {
            sb = _searchBox;
            return this;
        }

        public SearchableGroupedList<T> BeforeSearchBox(params IComponent[] beforeComponents)
        {
            foreach (var component in beforeComponents.Reverse<IComponent>())
            {
                _searchBoxContainerComponents.Insert(0, component);
            }
            _searchBoxContainer.Children(_searchBoxContainerComponents);
            return this;
        }

        public SearchableGroupedList<T> AfterSearchBox(params IComponent[] afterComponents)
        {
            _searchBoxContainerComponents.AddRange(afterComponents);
            _searchBoxContainer.Children(_searchBoxContainerComponents);
            return this;
        }

        public SearchableGroupedList<T> Virtualize(UnitSize itemHeight)
        {
            _virtualizedItemHeight = itemHeight;
            _defered.Render().addEventListener("scroll", (e) => RecomputeVisibleVirtualItems());
            _defered.Render().addEventListener("resize", (e) => RecomputeVisibleVirtualItems());
            return this;
        }

        private void RecomputeVisibleVirtualItems()
        {
            if (_virtualizedItemHeight is null || _virtualItems.Count == 0) return;
            var container = _defered.Render();
            double scrollTop = container.scrollTop;
            double containerHeight = container.clientHeight;
            if (containerHeight == 0) return;

            // We use the fixed height to calculate visible indices
            double itemH = _virtualizedItemHeight.Size;
            if (itemH <= 0) itemH = 1;

            int firstVisibleIndex = (int)(scrollTop / itemH);
            int visibleCount = (int)(containerHeight / itemH) + 1;

            // Add overscan (e.g., 1x container height)
            int overscan = visibleCount;
            int startIndex = Math.Max(0, firstVisibleIndex - overscan);
            int endIndex = Math.Min(_virtualItems.Count - 1, firstVisibleIndex + visibleCount + overscan);

            for (int i = 0; i < _virtualItems.Count; i++)
            {
                bool isVisible = (i >= startIndex && i <= endIndex);
                _virtualItems[i].UpdateVisibility(isVisible);
            }
        }

        public HTMLElement Render() => _stack.Render();


        private void AddGroupedItems(IEnumerable<T> items, ObservableList<IComponent> observableList, bool isGrid)
        {
            _virtualItems.Clear();
            observableList.Clear();

            if (items is object)
            {
                items = items.ToList();

                if (items.Any())
                {
                    foreach (var groupedItems in items.GroupBy(item => item.Group).OrderBy(g => g.Key, _groupComparer))
                    {
                        var header = new GroupedItemsHeader(groupedItems.Key, _groupedItemHeaderGenerator);

                        if (isGrid)
                        {
                            header.GridColumn(1, -1);
                        }

                        observableList.Add(header);

                        if (_virtualizedItemHeight is object)
                        {
                            foreach (var groupedItem in groupedItems)
                            {
                                var lazy = new LazyVirtualItem(() => groupedItem.Render(), _virtualizedItemHeight);
                                _virtualItems.Add(lazy);
                                observableList.Add(lazy);
                            }
                        }
                        else
                        {
                            observableList.AddRange(groupedItems.Select(t => t.Render()));
                        }
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

    [H5.Name("tss.ISearchableGroupedItem")]
    public interface ISearchableGroupedItem : ISearchableItem
    {
        string Group { get; }
    }
}