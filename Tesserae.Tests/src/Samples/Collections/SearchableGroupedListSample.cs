using System;
using System.Linq;
using Tesserae;
using static Tesserae.Tests.Samples.SamplesHelper;
using static Tesserae.UI;
using static H5.Core.dom;
using Tesserae.Tests;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Collections", Order = 20, Icon = UIcons.Search)]
    public class SearchableGroupedListSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public SearchableGroupedListSample()
        {
            _content =
                SectionStack()
                   .WidthStretch()
                   .Title(SampleHeader(nameof(SearchableGroupedListSample)))
                   .Section(
                        Stack()
                           .Children(
                                SampleTitle("Overview"),
                                TextBlock("This list provides a base component for implementing search over a known number of items." +
                                        "It is agnostic of the tile component used, and selection "                                   +
                                        "management. These concerns can be layered separately.")
                                   .PB(16),
                                TextBlock("You need to implement ISearchableGroupedItem interface on the items, and specially the IsMatch method to enable searching on them")))
                   .Section(
                        Stack()
                           .Children(
                                SampleTitle("Usage"),
                                TextBlock("Searchable Grouped List with No Results Message").Medium().PB(16).PaddingTop(16.px()),
                                SearchableGroupedList(GetItems(10), GroupedItemHeaderGenerator).PaddingBottom(32.px()).Height(500.px())
                                   .WithNoResultsMessage(() => BackgroundArea(Card(TextBlock("No Results").Padding(16.px()))).WidthStretch().HeightStretch().MinHeight(100.px())),
                                TextBlock("Searchable Grouped List with extra commands").Medium().PB(16).PaddingTop(16.px()),
                                SearchableGroupedList(GetItems(10), GroupedItemHeaderGenerator).PaddingBottom(32.px()).Height(500.px()).AfterSearchBox(Button("Sample Button After").Primary()).BeforeSearchBox(Button("Sample Button Before").Link())
                                   .WithNoResultsMessage(() => BackgroundArea(Card(TextBlock("No Results").Padding(16.px()))).WidthStretch().HeightStretch().MinHeight(100.px())),
                                TextBlock("Searchable Grouped List with Columns").Medium().PB(16).PaddingTop(16.px()),
                                SearchableGroupedList(GetItems(40), GroupedItemHeaderGenerator, 25.percent(), 25.percent(), 25.percent(), 25.percent()).Height(500.px())
                            )).PaddingBottom(32.px());

            IComponent GroupedItemHeaderGenerator(string s) => HorizontalSeparator(TextBlock(s).Primary().SemiBold()).Left();
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
            private readonly string     _value;
            private readonly IComponent _component;

            public SearchableGroupedListItem(string value, string group)
            {
                _value     = value;
                _component = Card(TextBlock(value).NonSelectable());

                Group = group;
            }

            public bool IsMatch(string searchTerm) => _value.ToLower().Contains(searchTerm.ToLower()) || Group.ToLower().Contains(searchTerm.ToLower());

            public string Group { get; }

            public IComponent Render() => _component;
        }
    }
}