using System;
using Tesserae.Components;
using static Retyped.dom;
using static Tesserae.UI;

namespace Tesserae.Tests.Samples
{
    public class ProgressIndicatorSample : IComponent
    {
        private IComponent _content;

        public ProgressIndicatorSample()
        {
            _content = SectionStack()
                .Title(TextBlock("Progress Indicator").XLarge().Bold())
                .Section(Stack().Children(
                    TextBlock("Overview").MediumPlus(),
                    TextBlock(
                        "ProgressIndicators are used to show the completion status of an operation lasting more than 2 seconds. If the state of progress cannot be determined, use a Spinner instead. ProgressIndicators can appear in a new panel, a flyout, under the UI initiating the operation, or even replacing the initiating UI, as long as the UI can return if the operation is canceled or is stopped."))
                )
                .Section(Stack().Children(
                    TextBlock("Best Practices").MediumPlus(),
                    Stack().Horizontal().Children(
                        Stack().Children(
                            TextBlock("Do").Medium(),
                            TextBlock("Use a ProgressIndicator when the total units to completion is known"),
                            TextBlock("Display operation description"),
                            TextBlock("Show text above and/or below the bar"),
                            TextBlock("Combine steps of a single operation into one bar")
                        ),
                        Stack().Children(
                            TextBlock("Don't").Medium(),
                            TextBlock("Use a ProgressIndicator when the total units to completion is indeterminate."),
                            TextBlock("Show text to the right or left of the bar"),
                            TextBlock("Cause progress to “rewind” to show new steps")
                        ))
                ))
                .Section(
                    Stack().Width(400, Unit.Pixels).Children(
                        TextBlock("Usage").MediumPlus(),
                        TextBlock("States").Medium(),
                        Label("Empty").SetContent(ProgressIndicator().Progress(0)).AlignCenter(),
                        Label("30%").SetContent(ProgressIndicator().Progress(30)).AlignCenter(),
                        Label("60%").SetContent(ProgressIndicator().Progress(60)).AlignCenter(),
                        Label("Full").SetContent(ProgressIndicator().Progress(100)).AlignCenter(),
                        Label("Indeterminate").SetContent(ProgressIndicator().Indeterminated()).AlignCenter()
                    ));
        }

        public HTMLElement Render()
        {
            return _content.Render();
        }
    }
}
