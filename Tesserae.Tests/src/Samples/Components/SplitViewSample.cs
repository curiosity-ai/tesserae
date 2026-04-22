using System;
using static H5.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Components", Order = 0, Icon = UIcons.TableColumns)]
    public class SplitViewSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public SplitViewSample()
        {
            var splitView     = SplitView().Left(Stack().S().Background(Theme.Colors.Neutral100).Children(TextBlock("Left Panel").Bold().AlignCenter())).Right(Stack().S().Background(Theme.Colors.Neutral200).Children(TextBlock("Right Panel").Bold().AlignCenter())).Resizable().WS().H(200);
            var horzSplitView = HorizontalSplitView().Top(Stack().S().Background(Theme.Colors.Neutral100).Children(TextBlock("Top Panel").Bold().AlignCenter())).Bottom(Stack().S().Background(Theme.Colors.Neutral200).Children(TextBlock("Bottom Panel").Bold().AlignCenter())).Resizable().WS().H(200);

            _content = SectionStack()
               .S()
               .Title(SampleHeader(nameof(SplitViewSample)))
               .Section(Stack().Children(
                    SampleTitle("Overview"),
                    TextBlock("SplitViews divide a surface into two areas, either horizontally or vertically. They are commonly used for master-detail layouts, navigation sidebars, or resizable workspace areas."),
                    TextBlock("Tesserae provides both 'SplitView' (vertical split) and 'HorizontalSplitView' with support for resizable handles and initial sizing.")))
               .Section(Stack().Children(
                    SampleTitle("Best Practices"),
                    TextBlock("Use SplitViews when users need to see two related sets of content at the same time. Enable resizability if the ideal balance between the two panels depends on the user's task or screen size. Set sensible minimum and maximum sizes for the panels to prevent them from disappearing or becoming too large. Use distinct background colors or borders to help users distinguish between the two areas.")))
               .Section(
                    VStack().S().Children(
                        SampleTitle("Usage"),
                        SampleSubTitle("Interaction Controls"),
                        HStack().WS().Children(
                            Button("Make Non-Resizable").OnClick(() => { splitView.NotResizable(); horzSplitView.NotResizable(); Toast().Information("Resizing disabled"); }),
                            Button("Make Resizable").Primary().OnClick(() => { splitView.Resizable(); horzSplitView.Resizable(); Toast().Information("Resizing enabled"); })
                        ).MB(16),
                        SampleSubTitle("Vertical SplitView"),
                        splitView.MB(16),
                        SampleSubTitle("Horizontal SplitView"),
                        horzSplitView
                    )
                  , grow: true);
        }

        public HTMLElement Render() => _content.Render();
    }
}
