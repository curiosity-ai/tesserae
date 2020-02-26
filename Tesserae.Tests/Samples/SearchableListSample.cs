using System.Collections.Generic;
using System.Linq;
using Tesserae.Components;
using static Tesserae.Tests.Samples.SamplesHelper;
using static Tesserae.UI;
using static Retyped.dom;

namespace Tesserae.Tests.Samples
{
    public class SearchableListSample : IComponent
    {
        private readonly IComponent _content;

        public SearchableListSample()
        {
            _content =
                SectionStack()
                    .Title(
                        TextBlock("SearchableList")
                            .XLarge()
                            .Bold())
                    .Section(
                        Stack()
                            .Children(
                                SampleTitle("Overview"),
                                TextBlock("This list provides a base component for implementing search over a known number of items." +
                                          "It is agnostic of the tile component used, and selection " +
                                          "management. These concerns can be layered separately.")
                                    .PaddingBottom(16.px()),
                                TextBlock("You need to implement ISearchableItem interface on the items, and specially the IsMatch method to enable searching on them")))
                    .Section(
                        Stack()
                            .Children(
                                SampleTitle("Usage"),
                                TextBlock("Searchable List")
                                    .Medium()
                                    .PaddingBottom(16.px()),
                                SearchableList(GetItems(10)).PaddingBottom(32.px()).MaxHeight(300.px()),
                                TextBlock("Searchable List with Columns")
                                    .Medium()
                                    .PaddingBottom(16.px()),
                                SearchableList(GetItems(40), 25.percent(), 25.percent(), 25.percent(), 25.percent()))).PaddingBottom(32.px()).MaxHeight(300.px());
        }

        public HTMLElement Render()
        {
            return _content.Render();
        }

        private IEnumerable<SearchableListItem> GetItems(int count)
        {
            return Enumerable
                .Range(1, count)
                .Select(number => new SearchableListItem($"Lorem Ipsum {number}"));
        }

        private class SearchableListItem : ISearchableItem
        {
            private string Value;
            private IComponent component;
            public SearchableListItem(string value)
            {
                Value = value;
                component = Card(TextBlock(value).NonSelectable());
            }

            public bool IsMatch(string searchTerm) => Value.Contains(searchTerm);

            public HTMLElement Render() => component.Render();
        }
    }
}
