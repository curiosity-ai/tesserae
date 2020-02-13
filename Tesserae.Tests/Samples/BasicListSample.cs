using System.Collections.Generic;
using System.Linq;
using Tesserae.Components;
using static Retyped.dom;
using static Tesserae.Tests.Samples.SamplesHelper;
using static Tesserae.UI;

namespace Tesserae.Tests.Samples
{
    public class BasicListSample : IComponent
    {
        private readonly IComponent _content;

        public BasicListSample()
        {
            _content =
                SectionStack()
                    .Title(
                        TextBlock("BasicList")
                            .XLarge()
                            .Bold())
                    .Section(
                        Stack()
                            .Children(
                                SampleTitle("Overview"),
                                TextBlock("DetailsList is a derivative of the List component. It is a robust way to " +
                                          "display an information rich collection of items. It can support powerful " +
                                          "ways to aid a user in finding content with sorting, grouping and " +
                                          "filtering.  Lists are a great way to handle large amounts of content, " +
                                          "but poorly designed Lists can be difficult to parse. Use a DetailsList " +
                                          "when density of information is critical. Lists can support single and " +
                                          "multiple selection, as well as drag and drop and marquee selection. " +
                                          "They are composed of a column header, which contains the metadata " +
                                          "fields which are attached to the list items, and provide the ability " +
                                          "to sort, filter and even group the list. List items are composed of " +
                                          "selection, icon, and name columns at minimum. One can also include " +
                                          "other columns such as Date Modified, or any other metadata field " +
                                          "associated with the collection. Place the most important columns from " +
                                          "left to right for ease of recall and comparison."),
                                TextBlock("DetailsList is classically used to display files, but is also used to " +
                                          "render custom lists that can be purely metadata. Avoid using file type " +
                                          "icon overlays to denote status of a file as it can make the entire icon " +
                                          "unclear. Be sure to leave ample width for each column’s data. " +
                                          "If there are multiple lines of text in a column, " +
                                          "consider the variable row height variant.")))
                    .Section(
                        Stack()
                            .Children(
                                SampleTitle("Usage"),
                                TextBlock("Basic List with Virtualization")
                                    .Medium()
                                    .PaddingBottom(Unit.Pixels, 16),
                                BasicList(
                                    GetBasicListItems())));
        }

        public HTMLElement Render()
        {
            return _content.Render();
        }

        private IEnumerable<BasicListItem> GetBasicListItems()
        {
            return Enumerable
                .Range(1, 5000)
                .Select(number => new BasicListItem($"Lorem Ipsum {number}"));
        }
    }
}
