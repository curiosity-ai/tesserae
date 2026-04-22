using System;
using Tesserae;
using static H5.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;
using Tesserae.Tests;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Progress", Order = 10, Icon = UIcons.Barcode)]
    public class ProgressIndicatorSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public ProgressIndicatorSample()
        {
            _content = SectionStack()
               .Title(SampleHeader(nameof(ProgressIndicatorSample)))
               .Section(Stack().Children(
                    SampleTitle("Overview"),
                    TextBlock("ProgressIndicators provide visual feedback for operations that take more than a few seconds. They show the current completion status and help set expectations for how much work remains. If the total amount of work is unknown, use the indeterminate state or a Spinner instead.")))
               .Section(Stack().WidthStretch().Children(
                    SampleTitle("Best Practices"),
                    TextBlock("Use a ProgressIndicator when the total units to completion can be quantified. Provide a clear label describing the operation in progress. Use the indeterminate state only when the duration is unknown. Combine multiple related steps into a single progress bar for a smoother experience. Avoid letting progress appear to move backwards unless a step failed and is being retried.")))
               .Section(
                    Stack().Children(
                        SampleTitle("Usage"),
                        TextBlock("States").Medium(),
                        Label("Empty").SetContent(ProgressIndicator().Progress(0).Width(400.px())).AlignCenter(),
                        Label("30%").SetContent(ProgressIndicator().Progress(30).Width(400.px())).AlignCenter(),
                        Label("60%").SetContent(ProgressIndicator().Progress(60).Width(400.px())).AlignCenter(),
                        Label("Full").SetContent(ProgressIndicator().Progress(100).Width(400.px())).AlignCenter(),
                        Label("Indeterminate").SetContent(ProgressIndicator().Indeterminated().Width(400.px())).AlignCenter()
                    ));
        }

        public HTMLElement Render()
        {
            return _content.Render();
        }
    }
}