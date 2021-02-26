using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tesserae;
using static Tesserae.Tests.Samples.SamplesHelper;
using static Tesserae.UI;
using static H5.Core.dom;

namespace Tesserae.Tests.Samples
{
    public class InfiniteScrollingListSample : IComponent
    {
        private readonly IComponent _content;

        public InfiniteScrollingListSample()
        {

            var page = 0;
            _content = SectionStack().WidthStretch()
               .Title(SampleHeader(nameof(InfiniteScrollingListSample)))
               .Section(
                    Stack()
                       .Children(
                            SampleTitle("Overview"),
                            TextBlock("List provides a base component for rendering paginates sets of items. " +
                                      "It is agnostic of the tile component used, and selection " +
                                      "management. These concerns can be layered separately.")
                               .PaddingBottom(16.px()),
                            TextBlock("Performance is adequate for smaller lists, for large number of items use VirtualizedList.")
                               .PaddingBottom(16.px())))
               .Section(
                    Stack()
                       .Children(
                            SampleTitle("Usage"),
                            TextBlock("Basic List with VisibilitySensor")
                               .Medium()
                               .PaddingBottom(16.px()),
                            InfiniteScrollingList(() => GetSomeItemsAsync(20, page++)).Height(500.px()).PaddingBottom(32.px())
                        ));
        }

        public HTMLElement Render()
        {
            return _content.Render();
        }

        private async Task<IComponent[]> GetSomeItemsAsync(int count, int page = -1)
        {
            await Task.Delay(200);
            var pageString = page > 0 ? page.ToString() : "";
            return Enumerable
               .Range(1, count)
               .Select(number =>
                {
                    var card = Card(TextBlock($"Lorem Ipsum {pageString}{number}").NonSelectable()).MinWidth(200.px());
                    return card;
                })
               .ToArray();
        }
    }
}