using System;
using System.Linq;
using Tesserae;
using static Tesserae.Tests.Samples.SamplesHelper;
using static Tesserae.UI;
using static H5.Core.dom;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Collections", Order = 20, Icon = UIcons.Search)]
    public class SearchableGroupedListSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public SearchableGroupedListSample()
        {
            _content = SectionStack().WidthStretch()
                   .Title(SampleHeader(nameof(SearchableGroupedListSample)))
                   .Section(Stack().Children(
                        SampleTitle("Overview"),
                        TextBlock("SearchableGroupedList extends the functionality of SearchableList by adding automatic grouping of items based on a 'Group' property."),
                        TextBlock("It provides a structured way to display filtered results, categorized by logical groups like file types, departments, or priority levels.")))
               .Section(Stack().Children(
                    SampleTitle("Best Practices"),
                    TextBlock("Use SearchableGroupedList when your dataset has a natural hierarchy or categorization that helps users find items faster. Provide a clear header for each group using the header generator. Ensure that the 'IsMatch' logic considers both the item content and the group name if appropriate. Like SearchableList, provide a meaningful 'No Results' message and use additional command slots for relevant actions.")))
               .Section(Stack().Children(
                    SampleTitle("Usage"),
                    SampleSubTitle("Grouped Search with Custom Headers"),
                    SearchableGroupedList(GetItems(20), s => HorizontalSeparator(TextBlock(s).Primary().SemiBold()).Left())
                       .WithNoResultsMessage(() => BackgroundArea(Card(TextBlock("No matching records").Padding(16.px()))).WS().HS().MinHeight(100.px()))
                       .Height(400.px()).MB(32),
                    SampleSubTitle("Grouped Grid Layout"),
                    SearchableGroupedList(GetItems(40), s => Label(s).Primary().Bold(), 33.percent(), 33.percent(), 34.percent())
                       .Height(500.px())
                ));
        }

        public HTMLElement Render() => _content.Render();

        private SearchableGroupedListItem[] GetItems(int count)
        {
            return Enumerable.Range(1, count).Select((n, i) =>
                new SearchableGroupedListItem($"Record {n}", (i % 3 == 0) ? "Category A" : (i % 2 == 0) ? "Category B" : "Category C")
            ).ToArray();
        }

        private class SearchableGroupedListItem : ISearchableGroupedItem
        {
            private readonly string _value;
            private readonly IComponent _component;
            public SearchableGroupedListItem(string value, string group) { _value = value; Group = group; _component = Card(TextBlock(value)); }
            public bool IsMatch(string searchTerm) => _value.ToLower().Contains(searchTerm.ToLower()) || Group.ToLower().Contains(searchTerm.ToLower());
            public string Group { get; }
            public IComponent Render() => _component;
        }
    }
}
