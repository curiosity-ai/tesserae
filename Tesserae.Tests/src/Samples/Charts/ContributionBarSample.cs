using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;
using static Transpose.Core.dom;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = SampleGroup.Charts, Order = 40, Icon = UIcons.ChartSimpleHorizontal)]
    public class ContributionBarSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public ContributionBarSample()
        {
            _content = SectionStack().Secondary()
               .SampleTitle(typeof(ContributionBarSample), UIcons.ChartSimpleHorizontal, "A stacked bar showing how weighted parts add up to a total")
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                        TextBlock("A ContributionBar renders a single stacked bar where each segment is sized proportionally to its value, plus an optional legend listing each part with its value."),
                        TextBlock("Use it to make the composition of a score always visible at a glance — for example, how each signal contributes to a similarity score, how a budget splits across categories, or how a result's relevance breaks down."))).SetTitle("Overview")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                        SampleSubTitle("Basic usage"),
                        TextBlock("Segments are added with .Add(label, value) and get a color from the default palette. By default the bar fills entirely (the maximum equals the sum of the segments)."),
                        ContributionBar()
                           .Add("Description", 0.36)
                           .Add("ATA chapter", 0.17)
                           .Add("Type", 0.15)
                           .Add("Damage", 0.13)
                           .Add("Location", 0.07)
                           .Add("Program", 0.06))).SetTitle("Default palette")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                        SampleSubTitle("Pinned maximum with a remainder track"),
                        TextBlock("Call .Max(value) to pin the full-width value. When the segments add up to less than the maximum, the remaining space is shown as an empty track."),
                        ContributionBar()
                           .Max(1.0)
                           .Add("Description", 0.36, Theme.Colors.Blue600)
                           .Add("ATA chapter", 0.17, Theme.Colors.Blue400)
                           .Add("Type", 0.15, Theme.Colors.Teal500)
                           .Add("Damage", 0.13, Theme.Colors.Green500)
                           .Add("Location", 0.07, Theme.Colors.Orange500)
                           .Add("Program", 0.06, Theme.Colors.Neutral500))).SetTitle("Explicit colors")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                        SampleSubTitle("Collapsable bar"),
                        TextBlock("Call .Collapsable(color) to add a tiny toggle button next to the bar. When collapsed, every colored segment merges into a single bar painted with the given color (Theme.Primary.Background by default) and the legend is hidden. Click the AngleUp / AngleDown button to expand or collapse."),
                        ContributionBar()
                           .Max(1.0)
                           .Collapsable(Theme.Primary.Background)
                           .Add("Description", 0.36, Theme.Colors.Blue600)
                           .Add("ATA chapter", 0.17, Theme.Colors.Blue400)
                           .Add("Type", 0.15, Theme.Colors.Teal500)
                           .Add("Damage", 0.13, Theme.Colors.Green500)
                           .Add("Location", 0.07, Theme.Colors.Orange500)
                           .Add("Program", 0.06, Theme.Colors.Neutral500))).SetTitle("Collapsable")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                        SampleSubTitle("Tooltip reveal"),
                        TextBlock("Pass ContributionBarReveal.Tooltip to show an info icon instead of an expand/collapse chevron. The bar stays as a single color and hovering the icon reveals the multi-colored breakdown and legend in a popover."),
                        ContributionBar()
                           .Max(1.0)
                           .Collapsable(Theme.Primary.Background, reveal: ContributionBarReveal.Tooltip)
                           .Add("Description", 0.36, Theme.Colors.Blue600)
                           .Add("ATA chapter", 0.17, Theme.Colors.Blue400)
                           .Add("Type", 0.15, Theme.Colors.Teal500)
                           .Add("Damage", 0.13, Theme.Colors.Green500)
                           .Add("Location", 0.07, Theme.Colors.Orange500)
                           .Add("Program", 0.06, Theme.Colors.Neutral500))).SetTitle("Tooltip")))
               .FlatSection(Stack().Children(
                    Card(BuildSimilarityCard()).SetTitle("Example: similarity result card")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                        SampleSubTitle("A card built out of nothing but bars"),
                        TextBlock("Every proportion on this card is a ContributionBar. The headline split is one two-segment bar with its legend off and its own labels under the ends; each rejection reason is a one-segment bar pinned to Max(100), so the orange runs are all measured against the same width and can be read off against each other."),
                        BuildPeerFeedbackCard().MT(16).MaxWidth(560.px()))).SetTitle("Example: peer feedback card")))
               .SeeAlso(typeof(ChartsSample), typeof(SparklineSample), typeof(UptimeSample), typeof(DeltaComponentSample), typeof(MetricSample));
        }

        // ---------- Peer feedback card ----------

        private static IComponent BuildPeerFeedbackCard()
        {
            var split = ContributionBar()
               .Max(100)
               .Thickness(14.px())
               .HideLegend()
               .Add("Accepted", 71, Theme.Colors.Green600)
               .Add("Rejected", 29, Theme.Colors.Red500);

            var headline = HStack().WS().AlignItems(ItemAlign.Center).Gap(20.px()).PT(4).Children(
                VStack().NoShrink().AlignItems(ItemAlign.Center).Children(
                    TextBlock("71%").XXLarge().Bold().Foreground(Theme.Colors.Green600),
                    TextBlock("Accepted").Tiny().SemiBold().Foreground(Theme.Secondary.Foreground)),
                VStack().Grow().WS().Children(
                    split,
                    HStack().WS().PT(6).Children(
                        TextBlock("71% accepted").XSmall().Foreground(Theme.Secondary.Foreground).Grow(),
                        TextBlock("29% rejected").XSmall().Foreground(Theme.Secondary.Foreground))));

            var reasons = VStack().WS().PT(20).Children(
                TextBlock("Top rejection reasons").Tiny().SemiBold().Foreground(Theme.Secondary.Foreground).PB(10),
                ReasonRow("Scope too broad",  40),
                ReasonRow("Wrong criticality", 20),
                ReasonRow("Source mismatch",   16));

            return VStack().WS().Children(
                HStack().WS().AlignItems(ItemAlign.Center).Gap(8.px()).Children(
                    Icon(UIcons.Users, size: TextSize.Medium).Foreground(Theme.Colors.Purple600),
                    TextBlock("Peer feedback on similar suggestions").MediumPlus().SemiBold(),
                    Badge("PAH3.5.2").Pill()),
                TextBlock("Anonymized — 23 comparable past cases").Small().Foreground(Theme.Secondary.Foreground).PB(16),
                headline,
                reasons);
        }

        // One reason: its name, a bar measured against the same 100 every other reason is, and the number.
        private static IComponent ReasonRow(string label, double percent)
        {
            var bar = ContributionBar()
               .Max(100)
               .Thickness(8.px())
               .HideLegend()
               .Add(label, percent, Theme.Colors.Orange500);

            return HStack().WS().AlignItems(ItemAlign.Center).Gap(12.px()).PB(8).Children(
                TextBlock(label).Small().W(160.px()).NoShrink(),
                bar.Grow(),
                TextBlock($"{percent:0}%").Small().SemiBold().W(40.px()).NoShrink().TextRight());
        }

        private static IComponent BuildSimilarityCard()
        {
            var bar = ContributionBar()
               .Max(1.0)
               .Add("Description", 0.36, Theme.Colors.Blue600)
               .Add("ATA chapter", 0.17, Theme.Colors.Blue400)
               .Add("Type", 0.15, Theme.Colors.Teal500)
               .Add("Damage", 0.13, Theme.Colors.Green500)
               .Add("Location", 0.07, Theme.Colors.Orange500)
               .Add("Program", 0.06, Theme.Colors.Neutral500);

            var score = VStack().AlignItems(ItemAlign.Center).Children(
                TextBlock("0.94").XXLarge().Bold().Foreground(Theme.Colors.Green600),
                Badge("HIGH MATCH").Success().Pill());

            var header = VStack().WS().Children(
                TextBlock("NC-2023-04412 · 2023-06-18").Tiny().SemiBold().Foreground(Theme.Secondary.Foreground),
                TextBlock("Composite skin delamination on LH wing trailing-edge panel").MediumPlus().SemiBold(),
                TextBlock("CONTRIBUTION TO SIMILARITY · SUMS TO 0.94").Tiny().SemiBold().Foreground(Theme.Secondary.Foreground).PT(12).PB(4),
                bar,
                HStack().Wrap().PT(12).Children(
                    Badge("A350-900").Outline().Pill(),
                    Badge("ATA 57 · Wings").Outline().Pill(),
                    Badge("Delamination").Outline().Pill(),
                    Badge("LH wing trailing edge").Outline().Pill(),
                    Badge("Composite delamination").Outline().Pill()));

            return HStack().WS().Children(
                score.NoShrink().PR(16),
                header.Grow());
        }

        public HTMLElement Render() => _content.Render();
    }
}
