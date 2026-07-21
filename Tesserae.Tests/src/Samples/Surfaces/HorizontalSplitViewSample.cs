using System;
using static Transpose.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Surfaces", Order = 207, Icon = UIcons.SplitScreen)]
    public class HorizontalSplitViewSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public HorizontalSplitViewSample()
        {
            var basicSplit = HorizontalSplitView()
               .Top(Stack().S().Background(Theme.Colors.Neutral100).Children(TextBlock("Top Pane").Bold().AlignCenter()))
               .Bottom(Stack().S().Background(Theme.Colors.Neutral200).Children(TextBlock("Bottom Pane").Bold().AlignCenter()))
               .WS().H(300);

            var resizableSplit = HorizontalSplitView()
               .Top(Stack().S().Background(Theme.Colors.Neutral100).Children(TextBlock("Editor").Bold().AlignCenter()))
               .Bottom(Stack().S().Background(Theme.Colors.Neutral200).Children(TextBlock("Preview").Bold().AlignCenter()))
               .Resizable(height => Toast().Information($"Top pane resized to {height}px"))
               .WS().H(300);

            var topSmaller = HorizontalSplitView()
               .Top(Stack().S().Background(Theme.Colors.Blue100).Children(TextBlock("Fixed Header (80px)").Bold().AlignCenter()))
               .Bottom(Stack().S().Background(Theme.Colors.Neutral200).Children(TextBlock("Grows to fill").Bold().AlignCenter()))
               .TopIsSmaller(80.px())
               .WS().H(200);

            var bottomSmaller = HorizontalSplitView()
               .Top(Stack().S().Background(Theme.Colors.Neutral200).Children(TextBlock("Grows to fill").Bold().AlignCenter()))
               .Bottom(Stack().S().Background(Theme.Colors.Blue100).Children(TextBlock("Fixed Footer (80px)").Bold().AlignCenter()))
               .BottomIsSmaller(80.px())
               .WS().H(200);

            _content = SectionStack().Secondary()
               .SampleTitle(typeof(HorizontalSplitViewSample), UIcons.SplitScreen, "Divides space into top and bottom panes with an optional resizable divider")
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    TextBlock("HorizontalSplitView divides a container into a top pane and a bottom pane separated by a thin splitter bar. It is well-suited for editor/preview layouts, terminal-style interfaces, or any surface where two stacked areas need to coexist."),
                    TextBlock("The divider can be made interactive so users can drag it to redistribute space between the two panes at runtime."))).SetTitle("Overview")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    TextBlock("Set an explicit height on the split container to prevent it from collapsing. Use TopIsSmaller or BottomIsSmaller when one pane should be compact and the other should grow to fill available space. Enable Resizable only when user preference for the split ratio is genuinely useful — unnecessary resize handles add visual noise. Provide meaningful minimum sizes via CSS or layout constraints so neither pane can be dragged to zero."))).SetTitle("Best Practices")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    SampleSubTitle("Basic Non-Resizable Split"),
                    basicSplit.MB(16),
                    SampleSubTitle("Resizable Split (drag the divider)"),
                    resizableSplit.MB(16),
                    SampleSubTitle("Top Pane Fixed, Bottom Grows"),
                    topSmaller.MB(16),
                    SampleSubTitle("Bottom Pane Fixed, Top Grows"),
                    bottomSmaller
                )).SetTitle("Usage")));
        }

        public HTMLElement Render() => _content.Render();
    }
}
