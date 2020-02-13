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
                        TextBlock("Basic List")
                            .XLarge()
                            .Bold())
                    .Section(
                        Stack()
                            .Children(
                                SampleTitle("Overview"),
                                TextBlock("List provides a base component for rendering large sets of items. " +
                                          "It is agnostic of the tile component used, and selection " +
                                          "management. These concerns can be layered separately.")))
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
