using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tesserae;
using static Tesserae.Tests.Samples.SamplesHelper;
using static Tesserae.UI;
using static H5.Core.dom;
using Tesserae.Tests;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Collections", Order = 20, Icon = LineAwesome.Infinity)]
    public class InfiniteScrollingListSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public InfiniteScrollingListSample()
        {

            var page = 1;
            var pageGrid = 1;

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
                            InfiniteScrollingList(GetSomeItems(20, 0, " initial"), async () => await GetSomeItemsAsync(20, page++)).Height(500.px()).PaddingBottom(32.px()),
                            TextBlock("Basic Grid List with VisibilitySensor")
                               .Medium()
                               .PaddingBottom(16.px()),
                            InfiniteScrollingList(GetSomeItems(20, 0, " initial"), async () => await GetSomeItemsAsync(20, pageGrid++), 33.percent(), 33.percent(), 34.percent()).Height(500.px()).PaddingBottom(32.px())
                        ));
        }

        public HTMLElement Render()
        {
            return _content.Render();
        }

        private IComponent[] GetSomeItems(int count, int page = -1, string txt = "")
        {
            var pageString = page > 0 ? page.ToString() : "";

            return Enumerable
               .Range(1, count)
               .Select(number =>
                {
                    var card = Card(TextBlock($"Lorem Ipsum {pageString} / {number}" + txt).NonSelectable()).MinWidth(200.px());
                    return card;
                })
               .ToArray();
        }

        private async Task<IComponent[]> GetSomeItemsAsync(int count, int page = -1, string txt = "")
        {
            await Task.Delay(200);
            return GetSomeItems(count, page, txt);
        }
    }
}