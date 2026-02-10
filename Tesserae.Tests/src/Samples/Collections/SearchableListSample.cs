using System.Collections.Generic;
using System.Linq;
using Tesserae;
using static Tesserae.Tests.Samples.SamplesHelper;
using static Tesserae.UI;
using static H5.Core.dom;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Collections", Order = 20, Icon = UIcons.Search)]
    public class SearchableListSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public SearchableListSample()
        {
            _content = SectionStack().WidthStretch()
                   .Title(SampleHeader(nameof(SearchableListSample)))
                   .Section(Stack().Children(
                        SampleTitle("Overview"),
                        TextBlock("SearchableList combines a search box with a list of items, providing instant filtering as the user types."),
                        TextBlock("Items must implement the 'ISearchableItem' interface, which defines the matching logic and how each item is rendered.")))
                   .Section(Stack().Children(
                        SampleTitle("Best Practices"),
                        TextBlock("Use SearchableList when you have a moderately sized collection that users need to filter quickly. Ensure the 'IsMatch' implementation is performant and covers all relevant fields. Provide a clear 'No Results' message to help users understand when their search doesn't match anything. Use the 'BeforeSearchBox' and 'AfterSearchBox' slots to add relevant actions like 'Add New' or 'Filter' buttons. For very large datasets, consider server-side filtering or a VirtualizedList.")))
                   .Section(Stack().Children(
                        SampleTitle("Usage"),
                        SampleSubTitle("Basic Searchable List"),
                        SearchableList(GetItems(10))
                           .WithNoResultsMessage(() => BackgroundArea(Card(TextBlock("No matching items found").Padding(16.px()))).WS().HS().MinHeight(100.px()))
                           .Height(400.px()).MB(32),
                        SampleSubTitle("Searchable Grid with Commands"),
                        SearchableList(GetItems(24), 25.percent(), 25.percent(), 25.percent(), 25.percent())
                           .BeforeSearchBox(Button("Filter").SetIcon(UIcons.Filter))
                           .AfterSearchBox(Button("Add Item").Primary().SetIcon(UIcons.Plus))
                           .Height(400.px())
                    ));
        }

        public HTMLElement Render() => _content.Render();

        private SearchableListItem[] GetItems(int count)
        {
            return Enumerable.Range(1, count).Select(n => new SearchableListItem($"Item {n}")).ToArray();
        }

        private class SearchableListItem : ISearchableItem
        {
            private readonly string _value;
            private readonly IComponent _component;
            public SearchableListItem(string value) { _value = value; _component = Card(TextBlock(value)); }
            public bool IsMatch(string searchTerm) => _value.ToLower().Contains(searchTerm.ToLower());
            public HTMLElement Render() => _component.Render();
            IComponent ISearchableItem.Render() => _component;
        }
    }
}
