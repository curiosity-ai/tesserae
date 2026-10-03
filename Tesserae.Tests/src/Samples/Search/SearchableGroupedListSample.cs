using System;
using System.Linq;
using System.Threading.Tasks;
using Tesserae;
using static Tesserae.Tests.Samples.SamplesHelper;
using static Tesserae.UI;
using static Transpose.Core.dom;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = SampleGroup.Search, Order = 30, Icon = UIcons.Filter, Description = "A grouped list filtered as you type")]
    public class SearchableGroupedListSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public SearchableGroupedListSample()
        {
            //Records that arrive in batches, grouped as they land. Determinate when the total is known up front,
            //a sweep while the loading task runs when it is not.
            var determinateItems   = new ObservableList<SearchableGroupedListItem>();
            var determinateList    = new SearchableGroupedList<SearchableGroupedListItem>(determinateItems, s => HorizontalSeparator(TextBlock(s).Primary().SemiBold()).Left());
            var indeterminateItems = new ObservableList<SearchableGroupedListItem>();
            var indeterminateList  = new SearchableGroupedList<SearchableGroupedListItem>(indeterminateItems, s => HorizontalSeparator(TextBlock(s).Primary().SemiBold()).Left());

            _content = SectionStack().Secondary().WidthStretch()
                   .SampleTitle(typeof(SearchableGroupedListSample), UIcons.Search, "A grouped list that can be searched")
                   .FlatSection(Stack().Children(
                        Card(VStack().WS().Children(
                        TextBlock("SearchableGroupedList extends the functionality of SearchableList by adding automatic grouping of items based on a 'Group' property."),
                        TextBlock("It provides a structured way to display filtered results, categorized by logical groups like file types, departments, or priority levels."))).SetTitle("Overview")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    TextBlock("Use SearchableGroupedList when your dataset has a natural hierarchy or categorization that helps users find items faster. Provide a clear header for each group using the header generator. Ensure that the 'IsMatch' logic considers both the item content and the group name if appropriate. Like SearchableList, provide a meaningful 'No Results' message and use additional command slots for relevant actions."))).SetTitle("Best Practices")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    SampleSubTitle("Grouped Search with Custom Headers"),
                    SearchableGroupedList(GetItems(20), s => HorizontalSeparator(TextBlock(s).Primary().SemiBold()).Left())
                       .WithNoResultsMessage(() => BackgroundArea(Card(TextBlock("No matching records").Padding(16.px()))).WS().HS().MinHeight(100.px()))
                       .Height(400.px()).MB(32),
                    SampleSubTitle("Grouped Grid Layout"),
                    SearchableGroupedList(GetItems(40), s => Label(s).Primary().Bold(), 33.percent(), 33.percent(), 34.percent())
                       .Height(500.px()),
                    SampleSubTitle("Virtualized Grouped List (10000 items)"),
                    SearchableGroupedList(GetItems(10000), s => HorizontalSeparator(TextBlock(s).Primary().SemiBold()).Left())
                       .Virtualize(64.px())
                       .WithNoResultsMessage(() => BackgroundArea(Card(TextBlock("No matching records").Padding(16.px()))).WS().HS().MinHeight(100.px()))
                       .Height(400.px()).MB(32),
                    SampleSubTitle("Loading Records While Searching (determinate)"),
                    TextBlock("Records arrive in batches; search while they load. The bar on the search box shows how many have arrived."),
                    determinateList
                       .AfterSearchBox(Button("Reload").Id("grouped-list-reload-determinate").SetIcon(UIcons.ArrowsRepeat).OnClick(() => LoadDeterminateAsync(determinateItems, determinateList).FireAndForget()))
                       .WithNoResultsMessage(() => BackgroundArea(Card(TextBlock("No matching records").Padding(16.px()))).WS().HS().MinHeight(100.px()))
                       .Height(400.px()).MB(32),
                    SampleSubTitle("Loading Records While Searching (indeterminate)"),
                    TextBlock("The same load without a known total: the bar sweeps for as long as the loading task runs (ShowProgressWhile)."),
                    indeterminateList
                       .AfterSearchBox(Button("Reload").Id("grouped-list-reload-indeterminate").SetIcon(UIcons.ArrowsRepeat).OnClick(() => LoadIndeterminate(indeterminateItems, indeterminateList)))
                       .WithNoResultsMessage(() => BackgroundArea(Card(TextBlock("No matching records").Padding(16.px()))).WS().HS().MinHeight(100.px()))
                       .Height(400.px()).MB(32),
                    SampleSubTitle("Paginated Grouped List"),
                    SearchableGroupedList(GetItems(50), s => HorizontalSeparator(TextBlock(s).Primary().SemiBold()).Left())
                       .WithPagination(5)
                       .WithNoResultsMessage(() => BackgroundArea(Card(TextBlock("No matching records").Padding(16.px()))).WS().HS().MinHeight(100.px()))
                       .Height(400.px()).MB(32)
                )).SetTitle("Usage")))
                   .SeeAlso(typeof(SearchableListSample), typeof(ItemsListSample), typeof(SearchBoxSample), typeof(TreeSample));

            LoadDeterminateAsync(determinateItems, determinateList).FireAndForget();
            LoadIndeterminate(indeterminateItems, indeterminateList);
        }

        public HTMLElement Render() => _content.Render();

        private const int LOAD_TOTAL      = 60;
        private const int LOAD_BATCH_SIZE = 6;

        private bool _loadingDeterminate;
        private bool _loadingIndeterminate;

        private async Task LoadDeterminateAsync(ObservableList<SearchableGroupedListItem> items, SearchableGroupedList<SearchableGroupedListItem> list)
        {
            if (_loadingDeterminate) return;
            _loadingDeterminate = true;

            items.Clear();
            list.Progress(0, LOAD_TOTAL);

            for (var loaded = 0; loaded < LOAD_TOTAL; loaded += LOAD_BATCH_SIZE)
            {
                await Task.Delay(400);
                items.AddRange(GetItems(loaded, LOAD_BATCH_SIZE));
                list.Progress(loaded + LOAD_BATCH_SIZE, LOAD_TOTAL);
            }

            list.HideProgress();
            _loadingDeterminate = false;
        }

        private void LoadIndeterminate(ObservableList<SearchableGroupedListItem> items, SearchableGroupedList<SearchableGroupedListItem> list)
        {
            if (_loadingIndeterminate) return;

            list.ShowProgressWhile(LoadIndeterminateAsync(items));
        }

        private async Task LoadIndeterminateAsync(ObservableList<SearchableGroupedListItem> items)
        {
            _loadingIndeterminate = true;

            items.Clear();

            for (var loaded = 0; loaded < LOAD_TOTAL; loaded += LOAD_BATCH_SIZE)
            {
                await Task.Delay(400);
                items.AddRange(GetItems(loaded, LOAD_BATCH_SIZE));
            }

            _loadingIndeterminate = false;
        }

        private SearchableGroupedListItem[] GetItems(int count) => GetItems(0, count);

        private SearchableGroupedListItem[] GetItems(int start, int count)
        {
            return Enumerable.Range(start, count).Select(i =>
                new SearchableGroupedListItem($"Record {i + 1}", (i % 3 == 0) ? "Category A" : (i % 2 == 0) ? "Category B" : "Category C")
            ).ToArray();
        }

        private class SearchableGroupedListItem : ISearchableGroupedItem
        {
            private readonly string _value;
            private readonly IComponent _component;
            public SearchableGroupedListItem(string value, string group) { _value = value; Group = group; _component = Card(TextBlock(value), noAnimation: true).Height(64.px()); }
            public bool IsMatch(string searchTerm) => _value.ToLower().Contains(searchTerm.ToLower()) || Group.ToLower().Contains(searchTerm.ToLower());
            public string Group { get; }
            public IComponent Render() => _component;
        }
    }
}
