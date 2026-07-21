using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Transpose.Core;
using static Transpose.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// A searchable, scrollable list whose items are filtered live as the user types into a built-in search box.
    /// </summary>
    [Transpose.Name("tss.SearchableList")]
    public class SearchableList<T> : IComponent, ISpecialCaseStyling where T : ISearchableItem
    {
        private readonly IDefer           _defered;
        private readonly Stack            _searchBoxContainer;
        private readonly List<IComponent> _searchBoxContainerComponents;
        private readonly Stack            _stack;
        private readonly SearchBox        _searchBox;
        private readonly ItemsList        _list;

        private int               _minimumItemsToShowBox = 0;
        private Func<string, Task<T[]>> _backgroundSearcher;
        private Pagination        _pagination;
        private string            _lastSearchTerm = "";

        private UnitSize          _virtualizedItemHeight;
        private double            _virtualizedTimeout = 0;
        private double            _virtualizedViewportMinTop = 0;
        private double            _virtualizedViewportMaxTop = 0;
        private readonly List<LazyVirtualItem> _virtualItems = new List<LazyVirtualItem>();

        /// <summary>
        /// Gets or sets the styling container.
        /// </summary>
        public  HTMLElement       StylingContainer           => _stack.InnerElement;
        /// <summary>
        /// Gets or sets the propagate to stack item parent.
        /// </summary>
        public  bool              PropagateToStackItemParent => true;
        /// <summary>
        /// Adds the given items to the component.
        /// </summary>
        public  ObservableList<T> Items                      { get; }

        /// <summary>
        /// Shows the not matching items.
        /// </summary>
        public bool ShowNotMatchingItems { get; set; }
        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        public SearchableList(T[] items, params UnitSize[] columns) : this(new ObservableList<T>(initialValues: items ?? new T[0]), columns)
        {

        }

        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        public SearchableList(ObservableList<T> items, params UnitSize[] columns)
        {
            Items      = items ?? new ObservableList<T>();
            _searchBox = new SearchBox().Underlined().SetPlaceholder("Type to search").SearchAsYouType().Width(100.px()).Grow();
            _list      = ItemsList(new IComponent[0], columns);
            object marker;
            _defered =
                DeferSync(
                        Items,
                        item =>
                        {
                            var searchTerms = (_searchBox.Text ?? "").Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                            var includeItems = new List<IComponent>();
                            var includeItemsBackground = new List<IComponent>();
                            var excludeItems = new List<IComponent>();

                            _virtualItems.Clear();

                            foreach (var i in Items)
                            {
                                if (searchTerms.Length == 0 || searchTerms.All(st => i.IsMatch(st)))
                                {
                                    if (_virtualizedItemHeight is object)
                                    {
                                        var lazy = new LazyVirtualItem(i.Render().RemoveClass("tss-searchable-list-no-match"), _virtualizedItemHeight);
                                        _virtualItems.Add(lazy);
                                        includeItems.Add(lazy);
                                    }
                                    else
                                    {
                                        includeItems.Add(i.Render().RemoveClass("tss-searchable-list-no-match"));
                                    }
                                }
                                else if (ShowNotMatchingItems)
                                {
                                    if (_virtualizedItemHeight is object)
                                    {
                                        var lazy = new LazyVirtualItem(i.Render().Class("tss-searchable-list-no-match"), _virtualizedItemHeight);
                                        _virtualItems.Add(lazy);
                                        excludeItems.Add(lazy);
                                    }
                                    else
                                    {
                                        excludeItems.Add(i.Render().Class("tss-searchable-list-no-match"));
                                    }
                                }
                            }

                            if (_backgroundSearcher is object && !string.IsNullOrEmpty(_searchBox.Text))
                            {
                                var markerLocal = new object();
                                marker = markerLocal;
                                _backgroundSearcher(_searchBox.Text).ContinueWith(t =>
                                {
                                    if (markerLocal != marker) return;
                                    if (t.IsCompleted)
                                    {
                                        foreach(var bi in t.Result)
                                        {
                                            includeItemsBackground.Add(bi.Render().RemoveClass("tss-searchable-list-no-match"));
                                        }

                                        var filteredItemsWithBackground = includeItems.Concat(includeItemsBackground)
                                                                                      .Concat(excludeItems).ToArray();

                                        if (_pagination is object)
                                        {
                                            _pagination.SetTotalItems(filteredItemsWithBackground.Length);
                                            filteredItemsWithBackground = filteredItemsWithBackground.Skip((_pagination.CurrentPage - 1) * _pagination.PageSize).Take(_pagination.PageSize).ToArray();
                                        }

                                        _list.Items.Clear();

                                        if (filteredItemsWithBackground.Any())
                                        {
                                            _list.Items.AddRange(filteredItemsWithBackground);
                                        }

                                        _searchBox.Show();
                                        RecomputeVisibleVirtualItems();
                                    }
                                }).FireAndForget();
                            }

                            var filteredItems = includeItems.Concat(excludeItems).ToArray();

                            if (_searchBox.Text != _lastSearchTerm)
                            {
                                _lastSearchTerm = _searchBox.Text ?? "";
                                if (_pagination is object)
                                {
                                    _pagination.SetPage(1, false);
                                }
                            }

                            if (_pagination is object)
                            {
                                _pagination.SetTotalItems(filteredItems.Length);
                                filteredItems = filteredItems.Skip((_pagination.CurrentPage - 1) * _pagination.PageSize).Take(_pagination.PageSize).ToArray();
                            }

                            _list.Items.Clear();

                            if (filteredItems.Any())
                            {
                                _list.Items.AddRange(filteredItems);
                            }

                            // Base the show/hide decision on the total number of items in the list, not on how many
                            // survive the current query - otherwise typing a query that narrows the results below the
                            // threshold would collapse the search box out from under the user. Keep it shown while a
                            // query is active (even if the list shrinks below the threshold via an ObservableList
                            // update) so the query isn't stranded, and always show it when a background searcher is set.
                            if (Items.Count >= _minimumItemsToShowBox || _backgroundSearcher is object || !string.IsNullOrEmpty(_searchBox.Text))
                            {
                                _searchBox.Show();
                            }
                            else
                            {
                                _searchBox.Collapse();
                            }

                            window.setTimeout((_) => RecomputeVisibleVirtualItems(), 1);

                            return _list.S();
                        }
                    )
                   .WS()
                   .Grow(1);

            _searchBox.OnSearch((_, __) => _defered.Refresh());
            _searchBoxContainer           = Stack().Horizontal().WS().Children(_searchBox).AlignItems(ItemAlign.Center);
            _searchBoxContainerComponents = new List<IComponent>() { _searchBox };
            _stack                        = Stack().Children(_searchBoxContainer, _defered.Scroll()).WS().MaxHeight(100.percent());
        }

        /// <summary>
        /// Returns the component configured with the given no results message.
        /// </summary>
        public SearchableList<T> WithNoResultsMessage(Func<IComponent> emptyListMessageGenerator)
        {
            _list.WithEmptyMessage(emptyListMessageGenerator ?? throw new ArgumentNullException(nameof(emptyListMessageGenerator)));
            _defered.Refresh();
            return this;
        }

        /// <summary>
        /// Configures the inline search box using the supplied callback.
        /// </summary>
        public SearchableList<T> SearchBox(Action<SearchBox> sb)
        {
            sb(_searchBox);
            return this;
        }

        /// <summary>
        /// Captures the inline search box into the supplied <c>out</c> variable for later reference.
        /// </summary>
        public SearchableList<T> CaptureSearchBox(out SearchBox sb)
        {
            sb = _searchBox;
            return this;
        }

        /// <summary>
        /// Sets the keyboard shortcut of the component.
        /// </summary>
        public SearchableList<T> SetKeyboardShortcut(params string[] keys)
        {
            _searchBox.SetKeyboardShortcut(keys);
            return this;
        }

        /// <summary>
        /// Returns the component configured with the given background search.
        /// </summary>
        public SearchableList<T> WithBackgroundSearch(Func<string, Task<T[]>> searcher)
        {
            _backgroundSearcher = searcher;
            _minimumItemsToShowBox = 0;
            return this;
        }

        /// <summary>
        /// Hides the search box if less than.
        /// </summary>
        public SearchableList<T> HideSearchBoxIfLessThan(int items)
        {
            _minimumItemsToShowBox = items;
            return this;
        }

        /// <summary>
        /// Shows the not matching.
        /// </summary>
        public SearchableList<T> ShowNotMatching()
        {
            ShowNotMatchingItems = true;
            return this;
        }

        /// <summary>
        /// Adds the given components before the inline search box.
        /// </summary>
        public SearchableList<T> BeforeSearchBox(params IComponent[] beforeComponents)
        {
            foreach (var component in beforeComponents.Reverse<IComponent>())
            {
                _searchBoxContainerComponents.Insert(0, component);
            }
            _searchBoxContainer.Children(_searchBoxContainerComponents);
            return this;
        }

        /// <summary>
        /// Adds the given components after the inline search box.
        /// </summary>
        public SearchableList<T> AfterSearchBox(params IComponent[] afterComponents)
        {
            _searchBoxContainerComponents.AddRange(afterComponents);
            _searchBoxContainer.Children(_searchBoxContainerComponents);
            return this;
        }

        /// <summary>
        /// Configures the component to virtualize.
        /// </summary>
        public SearchableList<T> Virtualize(UnitSize itemHeight)
        {
            _virtualizedItemHeight = itemHeight;
            var container = _defered.Render();
            container.parentElement.addEventListener("scroll", (e) => RecomputeVisibleVirtualItems());
            container.parentElement.addEventListener("resize", (e) => RecomputeVisibleVirtualItems());
            _defered.Refresh();
            RecomputeVisibleVirtualItems();
            return this;
        }

        private void RecomputeVisibleVirtualItems()
        {
            window.clearTimeout(_virtualizedTimeout);
            var container = _defered.Render();
            double scrollTop = container.parentElement.scrollTop;
            if (scrollTop < _virtualizedViewportMinTop || scrollTop > _virtualizedViewportMaxTop)
            {
                RecomputeVisibleVirtualItemsInner();
            }
            else
            {
                _virtualizedTimeout = window.setTimeout((_) => RecomputeVisibleVirtualItemsInner(), 50);
            }
        }

        private void RecomputeVisibleVirtualItemsInner() 
        { 
            if (_virtualizedItemHeight is null || _virtualItems.Count == 0) return;
            var container = _defered.Render();
            double scrollTop = container.parentElement.scrollTop;
            double containerHeight = container.parentElement.clientHeight;
            if (containerHeight == 0) return;

            // We use the fixed height to calculate visible indices
            double itemH = _virtualizedItemHeight.Size;
            if (itemH <= 0) itemH = 1; // Prevent division by zero

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

            _virtualizedViewportMinTop = startIndex * itemH;
            _virtualizedViewportMaxTop = endIndex   * itemH;
        }

        /// <summary>
        /// Renders the component's root HTML element.
        /// </summary>
        public dom.HTMLElement Render() => _stack.Render();

        /// <summary>
        /// Returns the component configured with the given pagination.
        /// </summary>
        public SearchableList<T> WithPagination(int pageSize)
        {
            _pagination = new Pagination(0, pageSize, 1).WS();
            _pagination.OnPageChange(p => _defered.Refresh());
            _stack.Children(_searchBoxContainer, _defered.Scroll(), _pagination);
            return this;
        }
    }

    [Transpose.Name("tss.ISearchableItem")]
    public interface ISearchableItem
    {
        bool       IsMatch(string searchTerm);
        IComponent Render();
    }
}