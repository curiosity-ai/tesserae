using System;
using Tesserae;
using static H5.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;
using Tesserae.Tests;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Progress", Order = 10, Icon = UIcons.Spinner)]
    public class SpinnerSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public SpinnerSample()
        {
            _content = SectionStack()
               .Title(SampleHeader(nameof(SpinnerSample)))
               .Section(Stack().Children(
                    SampleTitle("Overview"),
                    TextBlock("Spinners are animated circular indicators used to show that a task is in progress when the exact duration is unknown. They are subtle, lightweight, and can be easily placed inline with content or centered within a container to provide feedback without disrupting the layout.")))
               .Section(Stack().WidthStretch().Children(
                    SampleTitle("Best Practices"),
                    TextBlock("Use a Spinner for tasks that take more than a second but have an indeterminate end time. Include a brief, descriptive label (e.g., 'Loading...', 'Processing...') to give users context. Choose a size that is appropriate for the surrounding content—smaller for inline elements and larger for full-page loading states. Avoid showing multiple spinners simultaneously if possible.")))
               .Section(
                    Stack().Width(400.px()).Children(
                        SampleTitle("Usage"),
                        TextBlock("Spinner sizes").Medium(),
                        Label("Extra small spinner").SetContent(Spinner().XSmall()).AlignCenter(),
                        Label("Small spinner").SetContent(Spinner().Small()).AlignCenter(),
                        Label("Medium spinner").SetContent(Spinner().Medium()).AlignCenter(),
                        Label("Large spinner").SetContent(Spinner().Large()).AlignCenter()
                    ))
               .Section(
                    Stack().Width(400.px()).Children(
                        TextBlock("Spinner label positioning").Medium(),
                        Label("Spinner with label positioned below").SetContent(Spinner("I am definitely loading...").Below()),
                        Label("Spinner with label positioned above").SetContent(Spinner("Seriously, still loading...").Above()),
                        Label("Spinner with label positioned to right").SetContent(Spinner("Wait, wait...").Right()),
                        Label("Spinner with label positioned to left").SetContent(Spinner("Nope, still loading...").Left())
                    ));
        }

        public HTMLElement Render()
        {
            return _content.Render();
        }
    }
}