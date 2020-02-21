using System.Collections.Generic;
using System.Linq;
using Tesserae.Components;
using static Tesserae.Tests.Samples.SamplesHelper;
using static Tesserae.UI;
using static Retyped.dom;

namespace Tesserae.Tests.Samples
{
    public class ItemsListSample : IComponent
    {
        private readonly IComponent _content;

        public ItemsListSample()
        {
            _content =
                SectionStack()
                    .Title(
                        TextBlock("ItemsList")
                            .XLarge()
                            .Bold())
                    .Section(
                        Stack()
                            .Children(
                                SampleTitle("Overview"),
                                TextBlock("List provides a base component for rendering small sets of items. " +
                                          "It is agnostic of the tile component used, and selection " +
                                          "management. These concerns can be layered separately.")
                                    .PaddingBottom(16.px()),
                                TextBlock("Performance is adequate for smaller lists, for large number of items use VirtualizedList.")
                                    .PaddingBottom(16.px())))
                    .Section(
                        Stack()
                            .Children(
                                SampleTitle("Usage"),
                                TextBlock("Basic List")
                                    .Medium()
                                    .PaddingBottom(16.px()),
                                ItemsList(GetSomeItems(10)).PaddingBottom(16.px()),
                                TextBlock("Basic List with columns")
                                    .Medium()
                                    .PaddingBottom(16.px()),
                                ItemsList(GetSomeItems(100), 25.percent(), 25.percent(), 25.percent(), 25.percent())));
        }

        public HTMLElement Render()
        {
            return _content.Render();
        }

        private IEnumerable<IComponent> GetSomeItems(int count)
        {
            return Enumerable
                .Range(1, count)
                .Select(number => Card(TextBlock($"Lorem Ipsum {number}").NonSelectable()).MinWidth(200.px()));
        }
    }
}
