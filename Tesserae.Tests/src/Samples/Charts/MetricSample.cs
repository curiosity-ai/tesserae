using static Transpose.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = SampleGroup.Charts, Order = 30, Icon = UIcons.ChartSimple)]
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
               )).SetTitle("Usage")))
               .FlatSection(Stack().Children(Icons()))
               .FlatSection(Stack().Children(WithContributionBar()))
               .SeeAlso(typeof(DeltaComponentSample), typeof(SparklineSample), typeof(ChartsSample), typeof(ContributionBarSample));
        }

        // ---------- Icon tiles ----------

        private static IComponent Icons()
        {
            return Card(VStack().WS().Children(
                TextBlock("SetIcon puts an IconTile in front of the title and the value — the same rounded, tinted square an OmniResult row leads with. Pass a UIcons glyph, a few letters, or a component of your own, plus the full-strength color the glyph should be: the wash behind it is computed from that one color, light under a light theme and deep under a dark one."),
                TextBlock("ValueFirst() flips the order so the number reads first and the words under it only say what was counted — the shape a counter tile takes.").MT(8),
                SampleSubTitle("Counters"),
                HStack().WS().Wrap().Gap(16.px()).Children(
                    Card(Metric("In my scope", "5").SetIcon(UIcons.Inbox, Theme.Colors.Purple600).ValueFirst()).W(240.px()),
                    Card(Metric("Awaiting review", "12").SetIcon(UIcons.ClipboardList, Theme.Colors.Blue600).ValueFirst()).W(240.px()),
                    Card(Metric("Rejected", "3").SetIcon(UIcons.CircleXmark, Theme.Colors.Red600).ValueFirst()).W(240.px())),
                SampleSubTitle("Letters instead of a glyph, and a bigger tile"),
                HStack().WS().Wrap().Gap(16.px()).Children(
                    Card(Metric("Monthly spend", "$4,182").SetIcon("USD", Theme.Colors.Green600)).W(240.px()),
                    Card(Metric("Documents", "2,410").SetIcon("PDF", Theme.Colors.Red600)).W(240.px()),
                    Card(Metric("Team", "18").SetIcon(UIcons.Users, Theme.Colors.Teal600).IconSize(56.px()).ValueFirst()).W(240.px()))))
               .SetTitle("Icons and text", UIcons.Inbox, Theme.Colors.Purple600);
        }

        // ---------- ContributionBar inside a metric ----------

        private static IComponent WithContributionBar()
        {
            return Card(VStack().WS().Children(
                TextBlock("Chart() takes any component, so a ContributionBar can sit under the value and break the number down: what the percentage is made of, and how each part contributes. ChangeInHeader() pulls the trend up level with the title, which leaves the value, the bar and its legend reading straight down the card."),
                SampleSubTitle("Rejection rate, broken down"),
                HStack().WS().Wrap().Gap(16.px()).Children(
                    Card(RejectionMetric()).W(360.px()),
                    Card(CoverageMetric()).W(360.px()))))
               .SetTitle("With a ContributionBar", UIcons.ChartSimpleHorizontal, Theme.Colors.Blue600);
        }

        private static Metric RejectionMetric()
        {
            // One bar carries both jobs: the green run is the share that was validated, and the legend
            // under it counts every outcome — so the number, the bar and the counts are one component.
            var bar = ContributionBar()
               .Max(47)
               .Thickness(8.px())
               .ShowValues(false)
               .Add("38 validated", 38, Theme.Colors.Green600)
               .Add("5 rej.",        5, Theme.Colors.Red500)
               .Add("4 deleg.",      4, Theme.Colors.Blue500);

            return Metric(TextBlock("Rejection by DOV").Tiny().SemiBold().Foreground(Theme.Secondary.Foreground), TextBlock("11%").XXLarge().Bold())
               .ChangeInHeader()
               .Change(HStack().AlignItemsCenter().Gap(4.px()).Children(
                    Icon(UIcons.ArrowTrendDown, size: TextSize.Small).Foreground(Theme.Colors.Green600),
                    TextBlock("-3pt").Small().SemiBold().Foreground(Theme.Colors.Green600)))
               .Chart(bar);
        }

        private static Metric CoverageMetric()
        {
            var bar = ContributionBar()
               .Max(100)
               .Thickness(8.px())
               .ShowValues(false)
               .Add("Reviewed",   62, Theme.Colors.Blue600)
               .Add("In review",  21, Theme.Colors.Blue400)
               .Add("Not started", 17, Theme.Colors.Neutral400);

            return Metric(TextBlock("Coverage").Tiny().SemiBold().Foreground(Theme.Secondary.Foreground), TextBlock("83%").XXLarge().Bold())
               .SetIcon(UIcons.ClipboardListCheck, Theme.Colors.Blue600)
               .ChangeInHeader()
               .Change(HStack().AlignItemsCenter().Gap(4.px()).Children(
                    Icon(UIcons.ArrowTrendUp, size: TextSize.Small).Foreground(Theme.Colors.Green600),
                    TextBlock("+6pt").Small().SemiBold().Foreground(Theme.Colors.Green600)))
               .Chart(bar);
        }

        public HTMLElement Render()
        {
            return content.Render();
        }
    }
}
