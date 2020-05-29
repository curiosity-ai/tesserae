using System;
using System.Linq;
using Tesserae.Components;
using static Tesserae.Tests.Samples.SamplesHelper;
using static Tesserae.UI;
using static H5.Core.dom;

namespace Tesserae.Tests.Samples
{
    public class SearchableGroupedListSample : IComponent
    {
        private readonly IComponent _content;

        public SearchableGroupedListSample()
        {
            Func<string, IComponent> groupedItemHeaderGenerator = HorizontalSeparator;

            _content =
                SectionStack()
                    .WidthStretch()
                    .Title(SampleHeader(nameof(SearchableGroupedListSample)))
                    .Section(
                        Stack()
                            .Children(
                                SampleTitle("Overview"),
                                TextBlock("This list provides a base component for implementing search over a known number of items." +
                                          "It is agnostic of the tile component used, and selection " +
                                          "management. These concerns can be layered separately.")
                                    .PaddingBottom(16.px()),
                                TextBlock("You need to implement ISearchableGroupedItem interface on the items, and specially the IsMatch method to enable searching on them")))
                    .Section(
                        Stack()
                            .Children(
                                SampleTitle("Usage"),
                                TextBlock("Searchable Grouped List with No Results Message").Medium().PaddingBottom(16.px()).PaddingTop(16.px()),
                                SearchableGroupedList(GetItems(10), groupedItemHeaderGenerator).PaddingBottom(32.px()).Height(500.px())
                                    .WithNoResultsMessage(() => BackgroundArea(Card(TextBlock("No Results").Padding(16.px()))).WidthStretch().HeightStretch().MinHeight(100.px())),
                                TextBlock("Searchable Grouped List with extra commands").Medium().PaddingBottom(16.px()).PaddingTop(16.px()),
                                SearchableGroupedList(GetItems(10), groupedItemHeaderGenerator).PaddingBottom(32.px()).Height(500.px()).AfterSearchBox(Button("Sample Button After").Primary()).BeforeSearchBox(Button("Sample Button Before").Link())
                                    .WithNoResultsMessage(() => BackgroundArea(Card(TextBlock("No Results").Padding(16.px()))).WidthStretch().HeightStretch().MinHeight(100.px())),
                                TextBlock("Searchable Grouped List with Columns").Medium().PaddingBottom(16.px()).PaddingTop(16.px()),
                                SearchableGroupedList(GetItems(40), groupedItemHeaderGenerator, 25.percent(), 25.percent(), 25.percent(), 25.percent()).Height(500.px())
                                )).PaddingBottom(32.px());
        }

        public HTMLElement Render() => _content.Render();

        private SearchableGroupedListItem[] GetItems(int count)
        {
            return Enumerable
                .Range(1, count)
                .Select((number, index) =>
                {
                    var group = string.Empty;

                    if (index % 2 == 0)
                    {
                        group = "Group A";
                    }
                    else if (index % 3 == 0)
                    {
                        group = "Group B";
                    }
                    else
                    {
                        group = "Group C";
                    }

                    return new SearchableGroupedListItem($"Lorem Ipsum {number}", group);
                })
                .ToArray();
        }

        private class SearchableGroupedListItem : ISearchableGroupedItem
        {
            private string     _value;
            private IComponent _component;

            public SearchableGroupedListItem(string value, string group)
            {
                _value     = value;
                _component = Card(TextBlock(value).NonSelectable());

                Group = group;
            }

            public bool IsMatch(string searchTerm) => _value.Contains(searchTerm);

            public string Group { get; }

            public HTMLElement Render() => _component.Render();
        }
    }
}
