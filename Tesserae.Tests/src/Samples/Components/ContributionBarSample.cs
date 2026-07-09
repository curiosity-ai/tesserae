using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;
using static H5.Core.dom;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Components", Order = 31, Icon = UIcons.ChartSimpleHorizontal)]
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
                    Card(VStack().WS().Children(
                        SampleSubTitle("Build directly from a list"),
                        TextBlock(".AddRange(items, item => label, item => value) builds every segment from a collection in a single call — no manual .Add loop. This is how a similarity / ranking result's per-candidate contributions bind straight to the bar."),
                        ContributionBar()
                           .Max(0.94)
                           .AddRange(SampleContributions, c => c.Name, c => c.Score))).SetTitle("AddRange")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                        SampleSubTitle("Ordering and partial fill"),
                        TextBlock(".SortByValue() orders the segments largest-first no matter the order they were added — here the parts are added smallest-first but render largest-first."),
                        ContributionBar()
                           .SortByValue()
                           .Add("Location", 0.07, Theme.Colors.Orange500)
                           .Add("Type", 0.15, Theme.Colors.Teal500)
                           .Add("ATA chapter", 0.17, Theme.Colors.Purple500)
                           .Add("Description", 0.36, Theme.Colors.Blue500),
                        TextBlock(".FillTo(fraction) fills only part of the track while keeping the segments' relative sizes; the rest stays empty. Two equal parts with .FillTo(0.5) each take a quarter of the bar.").PT(12),
                        ContributionBar()
                           .FillTo(0.5)
                           .Add("Signal X", 0.5, Theme.Colors.Blue500)
                           .Add("Signal Y", 0.5, Theme.Colors.Orange500))).SetTitle("Ordering & FillTo")))
               .FlatSection(Stack().Children(
                    Card(BuildSimilarityCard()).SetTitle("Example: similarity result card")));
        }

        private sealed class Contribution
        {
            public string Name  { get; set; }
            public double Score { get; set; }
        }

        // Mirrors the per-candidate score decomposition a similarity engine returns (a name + its score share).
        private static readonly Contribution[] SampleContributions =
        {
            new Contribution { Name = "Description", Score = 0.36 },
            new Contribution { Name = "ATA chapter", Score = 0.17 },
            new Contribution { Name = "Type",        Score = 0.15 },
            new Contribution { Name = "Damage",      Score = 0.13 },
            new Contribution { Name = "Location",    Score = 0.07 },
            new Contribution { Name = "Program",     Score = 0.06 },
        };

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
