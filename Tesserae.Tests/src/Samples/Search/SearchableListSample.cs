using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tesserae;
using static Tesserae.Tests.Samples.SamplesHelper;
using static Tesserae.UI;
using static Transpose.Core.dom;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = SampleGroup.Search, Order = 20, Icon = UIcons.SearchAlt, Description = "A list filtered as you type")]
    public class SearchableListSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public SearchableListSample()
        {
            //Items that arrive in batches: the list renders each batch as it lands and stays searchable
            //throughout, while the bar along the bottom of its search box says how much is still to come -
            //determinate when the total is known up front, a sweep while the task runs when it is not.
            var determinateItems   = new ObservableList<SearchableListItem>();
            var determinateList    = new SearchableList<SearchableListItem>(determinateItems);
            var indeterminateItems = new ObservableList<SearchableListItem>();
            var indeterminateList  = new SearchableList<SearchableListItem>(indeterminateItems);

            _content = SectionStack().Secondary().WidthStretch()
                   .SampleTitle(typeof(SearchableListSample), UIcons.Search, "A list that can be searched")
                   .FlatSection(Stack().Children(
                        Card(VStack().WS().Children(
                        TextBlock("SearchableList combines a search box with a list of items, providing instant filtering as the user types."),
                        TextBlock("Items must implement the 'ISearchableItem' interface, which defines the matching logic and how each item is rendered."))).SetTitle("Overview")))
                   .FlatSection(Stack().Children(
                        Card(VStack().WS().Children(
                        TextBlock("Use SearchableList when you have a moderately sized collection that users need to filter quickly. Ensure the 'IsMatch' implementation is performant and covers all relevant fields. Provide a clear 'No Results' message to help users understand when their search doesn't match anything. Use the 'BeforeSearchBox' and 'AfterSearchBox' slots to add relevant actions like 'Add New' or 'Filter' buttons. For very large datasets, consider server-side filtering or a VirtualizedList."))).SetTitle("Best Practices")))
                   .FlatSection(Stack().Children(
                        Card(VStack().WS().Children(
                        SampleSubTitle("Basic Searchable List"),
                        SearchableList(GetItems(10))
                           .WithNoResultsMessage(() => BackgroundArea(Card(TextBlock("No matching items found").Padding(16.px()))).WS().HS().MinHeight(100.px()))
                           .Height(400.px()).MB(32),
                        SampleSubTitle("Searchable Grid with Commands"),
                        SearchableList(GetItems(24), 25.percent(), 25.percent(), 25.percent(), 25.percent())
                           .BeforeSearchBox(Button("Filter").SetIcon(UIcons.Filter))
                           .AfterSearchBox(Button("Add Item").Primary().SetIcon(UIcons.Plus))
                           .Height(400.px()),
                        SampleSubTitle("Virtualized Searchable List (10000 items)"),
                        SearchableList(GetItems(10000))
                           .Virtualize(64.px())
                           .WithNoResultsMessage(() => BackgroundArea(Card(TextBlock("No matching items found").Padding(16.px()))).WS().HS().MinHeight(100.px()))
                           .Height(400.px()).MB(32),
                        SampleSubTitle("Loading Items While Searching (determinate)"),
                        TextBlock("Items arrive in batches; search while they load. The bar on the search box shows how many have arrived."),
                        determinateList
                           .AfterSearchBox(Button("Reload").Id("searchable-list-reload-determinate").SetIcon(UIcons.ArrowsRepeat).OnClick(() => LoadDeterminateAsync(determinateItems, determinateList).FireAndForget()))
                           .WithNoResultsMessage(() => BackgroundArea(Card(TextBlock("No matching items found").Padding(16.px()))).WS().HS().MinHeight(100.px()))
                           .Height(400.px()).MB(32),
                        SampleSubTitle("Loading Items While Searching (indeterminate)"),
                        TextBlock("The same load without a known total: the bar sweeps for as long as the loading task runs (ShowProgressWhile)."),
                        indeterminateList
                           .AfterSearchBox(Button("Reload").Id("searchable-list-reload-indeterminate").SetIcon(UIcons.ArrowsRepeat).OnClick(() => LoadIndeterminate(indeterminateItems, indeterminateList)))
                           .WithNoResultsMessage(() => BackgroundArea(Card(TextBlock("No matching items found").Padding(16.px()))).WS().HS().MinHeight(100.px()))
                           .Height(400.px()).MB(32),
                        SampleSubTitle("Paginated Searchable List"),
                        SearchableList(GetItems(50))
                           .WithPagination(5)
                           .WithNoResultsMessage(() => BackgroundArea(Card(TextBlock("No matching items found").Padding(16.px()))).WS().HS().MinHeight(100.px()))
                           .Height(400.px()).MB(32)
                    )).SetTitle("Usage")))
                   .SeeAlso(typeof(SearchableGroupedListSample), typeof(ItemsListSample), typeof(SearchBoxSample), typeof(PickerSample), typeof(DetailsListSample));

            LoadDeterminateAsync(determinateItems, determinateList).FireAndForget();
            LoadIndeterminate(indeterminateItems, indeterminateList);
        }

        public HTMLElement Render() => _content.Render();

        private const int LOAD_TOTAL      = 60;
        private const int LOAD_BATCH_SIZE = 6;

        private bool _loadingDeterminate;
        private bool _loadingIndeterminate;

        private async Task LoadDeterminateAsync(ObservableList<SearchableListItem> items, SearchableList<SearchableListItem> list)
        {
            if (_loadingDeterminate) return;
            _loadingDeterminate = true;

            items.Clear();
            list.Progress(0, LOAD_TOTAL);

            for (var loaded = 0; loaded < LOAD_TOTAL; loaded += LOAD_BATCH_SIZE)
            {
                await Task.Delay(400);
                items.AddRange(Enumerable.Range(loaded + 1, LOAD_BATCH_SIZE).Select(n => new SearchableListItem($"Item {n}")));
                list.Progress(loaded + LOAD_BATCH_SIZE, LOAD_TOTAL);
            }

            list.HideProgress();
            _loadingDeterminate = false;
        }

        private void LoadIndeterminate(ObservableList<SearchableListItem> items, SearchableList<SearchableListItem> list)
        {
            if (_loadingIndeterminate) return;

            list.ShowProgressWhile(LoadIndeterminateAsync(items));
        }

        private async Task LoadIndeterminateAsync(ObservableList<SearchableListItem> items)
        {
            _loadingIndeterminate = true;

            items.Clear();

            for (var loaded = 0; loaded < LOAD_TOTAL; loaded += LOAD_BATCH_SIZE)
            {
                await Task.Delay(400);
                items.AddRange(Enumerable.Range(loaded + 1, LOAD_BATCH_SIZE).Select(n => new SearchableListItem($"Item {n}")));
            }

            _loadingIndeterminate = false;
        }

        private SearchableListItem[] GetItems(int count)
        {
            return Enumerable.Range(1, count).Select(n => new SearchableListItem($"Item {n}")).ToArray();
        }

        private class SearchableListItem : ISearchableItem
        {
            private readonly string _value;
            private readonly IComponent _component;
            public SearchableListItem(string value) { _value = value; _component = Card(TextBlock(value), noAnimation: true).Height(64.px()); }
            public bool IsMatch(string searchTerm) => _value.ToLower().Contains(searchTerm.ToLower());
            public HTMLElement Render() => _component.Render();
            IComponent ISearchableItem.Render() => _component;
        }
    }
}
