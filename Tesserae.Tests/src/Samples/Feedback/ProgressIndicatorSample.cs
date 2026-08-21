using System;
using Tesserae;
using static Transpose.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;
using Tesserae.Tests;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = SampleGroup.Feedback, Order = 70, Icon = UIcons.BarsProgress)]
    public class ProgressIndicatorSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public ProgressIndicatorSample()
        {
            _content = SectionStack().Secondary()
               .SampleTitle(typeof(ProgressIndicatorSample), UIcons.Spinner, "A component to indicate progress")
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    TextBlock("ProgressIndicators provide visual feedback for operations that take more than a few seconds. They show the current completion status and help set expectations for how much work remains. If the total amount of work is unknown, use the indeterminate state or a Spinner instead."))).SetTitle("Overview")))
               .FlatSection(Stack().WidthStretch().Children(
                    Card(VStack().WS().Children(
                    TextBlock("Use a ProgressIndicator when the total units to completion can be quantified. Provide a clear label describing the operation in progress. Use the indeterminate state only when the duration is unknown. Combine multiple related steps into a single progress bar for a smoother experience. Avoid letting progress appear to move backwards unless a step failed and is being retried."))).SetTitle("Best Practices")))
               .FlatSection(
                    Stack().Children(
                        Card(VStack().WS().Children(
                        TextBlock("States").Medium(),
                        Label("Empty").SetContent(ProgressIndicator().Progress(0).Width(400.px())).AlignCenter(),
                        Label("30%").SetContent(ProgressIndicator().Progress(30).Width(400.px())).AlignCenter(),
                        Label("60%").SetContent(ProgressIndicator().Progress(60).Width(400.px())).AlignCenter(),
                        Label("Full").SetContent(ProgressIndicator().Progress(100).Width(400.px())).AlignCenter(),
                        Label("Indeterminate").SetContent(ProgressIndicator().Indeterminated().Width(400.px())).AlignCenter()
                    )).SetTitle("Usage")))
               .FlatSection(
                    Stack().Children(
                        Card(VStack().WS().Children(
                        TextBlock("AI() paints the bar with the purple-to-blue gradient - a model working through something, determinate or not. The indeterminate sweep keeps its speed; only the colour of the band changes."),
                        Label("35%").SetContent(ProgressIndicator().Progress(35).AI().Width(400.px())).AlignCenter(),
                        Label("3 of 4 checks").SetContent(ProgressIndicator().Progress(3, 4).AI().Width(400.px())).AlignCenter(),
                        Label("Indeterminate").SetContent(ProgressIndicator().Indeterminated().AI().Width(400.px())).AlignCenter()
                    )).SetTitle("A model working", UIcons.Sparkles, Theme.Colors.Purple600)))
               .SeeAlso(typeof(ProgressRingSample), typeof(SpinnerSample), typeof(SkeletonSample), typeof(ProgressModalSample), typeof(DeferWithProgressSample), typeof(AIVariantsSample));
        }

        public HTMLElement Render()
        {
            return _content.Render();
        }
    }
}