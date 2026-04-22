using System.Collections.Generic;
using System.Linq;
using Tesserae;
using static Tesserae.Tests.Samples.SamplesHelper;
using static Tesserae.UI;
using static H5.Core.dom;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Collections", Order = 20, Icon = UIcons.List)]
    public class VirtualizedListSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public VirtualizedListSample()
        {
            _content = SectionStack()
                   .Title(SampleHeader(nameof(VirtualizedListSample)))
                   .Section(Stack().Children(
                        SampleTitle("Overview"),
                        TextBlock("VirtualizedList is a high-performance component designed for rendering massive datasets—thousands or even tens of thousands of items—without sacrificing UI responsiveness."),
                        TextBlock("It achieves this by only rendering the items that are currently visible within the viewport (plus a small buffer), significantly reducing the number of DOM elements the browser needs to manage.")))
                   .Section(Stack().Children(
                        SampleTitle("Best Practices"),
                        TextBlock("Use VirtualizedList for any list that could potentially contain more than a few hundred items. Ensure that each item has a consistent height for accurate scroll position calculation. Virtualization is most effective when item components are relatively complex or resource-intensive to render. Always provide a clear 'Empty Message' if the dataset is expected to be empty.")))
                   .Section(Stack().Children(
                        SampleTitle("Usage"),
                        SampleSubTitle("Virtualized List with 5,000 Items"),
                        TextBlock("Scroll rapidly to see how the list handles a large number of items."),
                        VirtualizedList().WithListItems(GetALotOfItems()).Height(400.px()).MB(32),
                        SampleSubTitle("Empty State"),
                        VirtualizedList()
                           .WithEmptyMessage(() => BackgroundArea(Card(TextBlock("No items available"))).WS().HS().MinHeight(100.px()))
                           .WithListItems(Enumerable.Empty<IComponent>())
                           .Height(150.px())
                    ));
        }

        public HTMLElement Render() => _content.Render();

        private IEnumerable<SampleVirtualizedItem> GetALotOfItems()
        {
            return Enumerable.Range(1, 5000).Select(n => new SampleVirtualizedItem($"Virtualized Item {n}"));
        }

        public sealed class SampleVirtualizedItem : IComponent
        {
            private readonly HTMLElement _innerElement;
            public SampleVirtualizedItem(string text) { _innerElement = Div(_(text: text, styles: s => { s.display = "flex"; s.alignItems = "center"; s.padding = "0 16px"; s.height = "40px"; s.borderBottom = "1px solid #eee"; })); }
            public HTMLElement Render() => _innerElement;
        }
    }
}
