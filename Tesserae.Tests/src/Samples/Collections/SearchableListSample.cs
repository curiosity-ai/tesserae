using System.Collections.Generic;
using System.Linq;
using Tesserae;
using static Tesserae.Tests.Samples.SamplesHelper;
using static Tesserae.UI;
using static H5.Core.dom;
using Tesserae.Tests;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Collections", Order = 20, Icon = UIcons.Search)]
    public class SearchableListSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public SearchableListSample()
        {
            _content =
                SectionStack()
                   .WidthStretch()
                   .Title(SampleHeader(nameof(SearchableListSample)))
                   .Section(
                        Stack()
                           .Children(
                                SampleTitle("Overview"),
                                TextBlock("This list provides a base component for implementing search over a known number of items." +
                                        "It is agnostic of the tile component used, and selection "                                   +
                                        "management. These concerns can be layered separately.")
                                   .PB(16),
                                TextBlock("You need to implement ISearchableItem interface on the items, and specially the IsMatch method to enable searching on them")))
                   .Section(
                        Stack()
                           .Children(
                                SampleTitle("Usage"),
                                TextBlock("Searchable List with No Results Message").Medium().PB(16).PaddingTop(16.px()),
                                SearchableList(GetItems(10)).PaddingBottom(32.px()).Height(500.px())
                                   .WithNoResultsMessage(() => BackgroundArea(Card(TextBlock("No Results").Padding(16.px()))).WidthStretch().HeightStretch().MinHeight(100.px())),
                                TextBlock("Searchable List with extra commands").Medium().PB(16).PaddingTop(16.px()),
                                SearchableList(GetItems(10)).PaddingBottom(32.px()).Height(500.px()).AfterSearchBox(Button("Sample Button After").Primary()).BeforeSearchBox(Button("Sample Button Before").Link())
                                   .WithNoResultsMessage(() => BackgroundArea(Card(TextBlock("No Results").Padding(16.px()))).WidthStretch().HeightStretch().MinHeight(100.px())),
                                TextBlock("Searchable List with Columns").Medium().PB(16).PaddingTop(16.px()),
                                SearchableList(GetItems(40), 25.percent(), 25.percent(), 25.percent(), 25.percent()).Height(500.px())
                            )).PaddingBottom(32.px());
        }

        public HTMLElement Render()
        {
            return _content.Render();
        }

        private SearchableListItem[] GetItems(int count)
        {
            return Enumerable
               .Range(1, count)
               .Select(number => new SearchableListItem($"Lorem Ipsum {number}"))
               .ToArray();

        }

        private class SearchableListItem : ISearchableItem
        {
            private readonly string     _value;
            private readonly IComponent _component;
            public SearchableListItem(string value)
            {
                _value     = value;
                _component = Card(TextBlock(value).NonSelectable());
            }

            public bool IsMatch(string searchTerm) => _value.Contains(searchTerm);

            public HTMLElement Render() => _component.Render();

            IComponent ISearchableItem.Render() => _component;
        }
    }
}