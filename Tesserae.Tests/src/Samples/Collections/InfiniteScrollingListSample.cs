using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tesserae;
using static Tesserae.Tests.Samples.SamplesHelper;
using static Tesserae.UI;
using static H5.Core.dom;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Collections", Order = 20, Icon = UIcons.Infinity)]
    public class InfiniteScrollingListSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public InfiniteScrollingListSample()
        {
            var page     = 1;
            var pageGrid = 1;

            _content = SectionStack().WidthStretch()
               .Title(SampleHeader(nameof(InfiniteScrollingListSample)))
               .Section(Stack().Children(
                    SampleTitle("Overview"),
                    TextBlock("InfiniteScrollingList provides a way to render large sets of items by loading them on demand as the user scrolls. It uses a visibility sensor to detect when the end of the list is reached."),
                    TextBlock("This approach is great for social feeds, search results, or any collection where you want to avoid explicit pagination buttons.")))
               .Section(Stack().Children(
                    SampleTitle("Best Practices"),
                    TextBlock("Use infinite scrolling for content that is explored discovery-style rather than searched for specifically. Ensure that the loading state is clearly indicated to the user. Consider the performance impact of adding many DOM elements; for extremely large lists, VirtualizedList may be more appropriate. Provide a way for users to reach the footer of the page if necessary, perhaps by offering a 'Load More' button instead of fully automatic scrolling if the footer contains important links.")))
               .Section(Stack().Children(
                    SampleTitle("Usage"),
                    SampleSubTitle("Vertical Infinite List"),
                    TextBlock("Items are loaded 20 at a time with a small delay to simulate network latency."),
                    InfiniteScrollingList(GetSomeItems(20, 0, " (Initial Set)"), async () => await GetSomeItemsAsync(20, page++)).Height(400.px()).MB(32),
                    SampleSubTitle("Grid-based Infinite List"),
                    TextBlock("Displaying items in a 3-column grid that expands as you scroll."),
                    InfiniteScrollingList(GetSomeItems(20, 0, " (Initial Set)"), async () => await GetSomeItemsAsync(20, pageGrid++), 33.percent(), 33.percent(), 34.percent()).Height(400.px())
                ));
        }

        public HTMLElement Render() => _content.Render();

        private IComponent[] GetSomeItems(int count, int page = -1, string txt = "")
        {
            var pageString = page > 0 ? $"Page {page}" : "";
            return Enumerable.Range(1, count).Select(n => Card(TextBlock($"{pageString} - Item {n}{txt}").NonSelectable()).MinWidth(200.px())).ToArray();
        }

        private async Task<IComponent[]> GetSomeItemsAsync(int count, int page = -1, string txt = "")
        {
            await Task.Delay(500);
            return GetSomeItems(count, page, txt);
        }
    }
}
