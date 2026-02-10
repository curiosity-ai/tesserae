using System.Collections.Generic;
using System.Linq;
using Tesserae;
using static Tesserae.Tests.Samples.SamplesHelper;
using static Tesserae.UI;
using static H5.Core.dom;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Collections", Order = 20, Icon = UIcons.List)]
    public class ItemsListSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public ItemsListSample()
        {
            var obsList = new ObservableList<IComponent>();
            var vs = VisibilitySensor((v) =>
            {
                obsList.Remove(v);
                obsList.AddRange(GetSomeItems(10, " (Dynamic)"));
                v.Reset();
                obsList.Add(v);
            });
            obsList.AddRange(GetSomeItems(10, " (Initial)"));
            obsList.Add(vs);

            _content = SectionStack().WidthStretch()
               .Title(SampleHeader(nameof(ItemsListSample)))
               .Section(Stack().Children(
                    SampleTitle("Overview"),
                    TextBlock("ItemsList is a versatile component for displaying collections of items. It supports static lists, observable lists, and grid layouts."),
                    TextBlock("It is ideal for smaller to medium-sized collections. For very large datasets, consider using VirtualizedList for better performance.")))
               .Section(Stack().Children(
                    SampleTitle("Best Practices"),
                    TextBlock("Use ItemsList when you want full control over the rendering of each item. Leverage observable lists to automatically update the UI when the underlying data changes. Use columns to create grid layouts that adapt to the container width. Always provide a meaningful empty message if there are no items to display. If you expect a very high number of items, ensure you test the performance or switch to a virtualized approach.")))
               .Section(Stack().Children(
                    SampleTitle("Usage"),
                    SampleSubTitle("Simple List"),
                    ItemsList(GetSomeItems(5)).Height(250.px()).MB(32),
                    SampleSubTitle("Multi-column Grid"),
                    ItemsList(GetSomeItems(12), 33.percent(), 33.percent(), 34.percent()).Height(300.px()).MB(32),
                    SampleSubTitle("Dynamic Observable List"),
                    TextBlock("This list uses a VisibilitySensor to append more items as you scroll."),
                    ItemsList(obsList, 50.percent(), 50.percent()).Height(300.px()).MB(32),
                    SampleSubTitle("Empty State"),
                    ItemsList(new IComponent[0])
                       .WithEmptyMessage(() => BackgroundArea(Card(TextBlock("No items to show").Padding(16.px()))).WS().HS().MinHeight(100.px()))
                       .Height(150.px())
                ));
        }

        public HTMLElement Render() => _content.Render();

        private IComponent[] GetSomeItems(int count, string suffix = "")
        {
            return Enumerable.Range(1, count).Select(n => Card(TextBlock($"Item {n}{suffix}")).MinWidth(150.px())).ToArray();
        }
    }
}
