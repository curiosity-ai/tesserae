using static Transpose.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Components", Order = 25, Icon = UIcons.ChartHistogram)]
    public class MetricSample : IComponent, ISample
    {
        private readonly IComponent content;

        public MetricSample()
        {
            content = SectionStack().Secondary()
               .SampleTitle(typeof(MetricSample), UIcons.ChartHistogram, "A component to display a metric")
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    TextBlock("A Metric component displays a key value alongside a title and an optional indicator of change."))).SetTitle("Overview")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    TextBlock("Use Metric to display important data points, such as requests, tokens, costs or errors. Keep titles short and clear. Combine with charts or grids to provide more context."))).SetTitle("Best Practices")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    SampleSubTitle("Basic Metrics"),
                    HStack().Children(
                        Card(Metric("Requests", "1.1k").Change(TextBlock("").SmallPlus().Foreground(Theme.Colors.Neutral600))).W(200.px()),
                        Card(Metric("Tokens", "196.97k")).W(200.px()),
                        Card(Metric("Cost", "$0.09")).W(200.px())
                    ),
                    SampleSubTitle("Metrics with Change Indicator"),
                    HStack().Children(
                        Card(Metric("Requests", "688.46k").Change(HStack().AlignItemsCenter().PT(16).Children(Icon(UIcons.ArrowDown).Foreground(Theme.Colors.Red600).S(), TextBlock("-0.4%").Foreground(Theme.Colors.Red600)))).W(250.px()),
                        Card(Metric("Tokens", "10.57B").Change(HStack().AlignItemsCenter().PT(16).Children(Icon(UIcons.ArrowDown).Foreground(Theme.Colors.Red600).S(), TextBlock("-0.32%").Foreground(Theme.Colors.Red600)))).W(250.px())
                    ),
                    SampleSubTitle("Card With Header & Tags"),
                    HStack().Children(
                        Card(HStack().Children(Metric("Requests", "688.46k").Change(HStack().AlignItemsCenter().PT(16).Children(Icon(UIcons.ArrowDown).Foreground(Theme.Colors.Red600).S(), TextBlock("-0.4%").Foreground(Theme.Colors.Red600))).W(250.px()), Metric("Tokens", "10.57B").Change(HStack().AlignItemsCenter().PT(16).Children(Icon(UIcons.ArrowDown).Foreground(Theme.Colors.Red600).S(), TextBlock("-0.32%").Foreground(Theme.Colors.Red600))).W(250.px()))).SetTitle("Metrics")
                    ),
                    SampleSubTitle("With Tooltips inside title"),
                    HStack().Children(
                        Card(Metric(HStack().AlignItemsCenter().PT(16).Children(TextBlock("Requests").SmallPlus().SemiBold().Foreground(Theme.Secondary.Foreground), Icon(UIcons.Info).S().PL(4).Tooltip("Total number of requests")), TextBlock("1.1k").XLarge().SemiBold())).W(200.px()),
                        Card(Metric(HStack().AlignItemsCenter().PT(16).Children(TextBlock("Cost").SmallPlus().SemiBold().Foreground(Theme.Secondary.Foreground), Icon(UIcons.Info).S().PL(4).Tooltip("Total estimated cost")), TextBlock("$0.09").XLarge().SemiBold())).W(200.px())
                    ),
                    SampleSubTitle("Metrics with Sparkline Charts"),
                    HStack().Children(
                        Card(Metric("Web traffic", "1,234,567").Chart(Sparkline(new double[] { 10, 20, 15, 30, 25, 40, 35, 50 })).Change(HStack().AlignItemsCenter().PT(16).Children(Icon(UIcons.ArrowUp).Foreground(Theme.Colors.Green600).S(), TextBlock("+12.3%").Foreground(Theme.Colors.Green600)))).W(250.px()),
                        Card(Metric("Worker invocations", "14,352").Chart(Sparkline(new double[] { 50, 45, 40, 48, 30, 20, 15, 10 }, color: "var(--tss-danger-background-color)")).Change(HStack().AlignItemsCenter().PT(16).Children(Icon(UIcons.ArrowDown).Foreground(Theme.Colors.Red600).S(), TextBlock("-5.1%").Foreground(Theme.Colors.Red600)))).W(250.px())
                    )
               )).SetTitle("Usage")));
        }

        public HTMLElement Render()
        {
            return content.Render();
        }
    }
}
