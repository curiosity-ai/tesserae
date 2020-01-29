using System;
using Tesserae.Components;
using static Retyped.dom;
using static Tesserae.UI;

namespace Tesserae.Tests.Samples
{
    public class SpinnerSample : IComponent
    {
        private IComponent _content;

        public SpinnerSample()
        {
            _content = SectionStack()
                .Title(TextBlock("Spinner").XLarge().Bold())
                .Section(Stack().Children(
                    TextBlock("Overview").MediumPlus(),
                    TextBlock(
                        "A Spinner is an outline of a circle which animates around itself indicating to the user that things are processing. A Spinner is shown when it's unsure how long a task will take making it the indeterminate version of a ProgressIndicator. They can be various sizes, located inline with content or centered. They generally appear after an action is being processed or committed. They are subtle and generally do not take up much space, but are transitions from the completed task."))
                )
                .Section(Stack().Children(
                    TextBlock("Best Practices").MediumPlus(),
                    Stack().Horizontal().Children(
                        Stack().Children(
                            TextBlock("Do").Medium(),
                            TextBlock("Use a Spinner when a task is not immediate."),
                            TextBlock("Use one Spinner at a time."),
                            TextBlock(
                                "Descriptive verbs are appropriate under a Spinner to help the user understand what's happening. Ie: Saving, processing, updating."),
                            TextBlock(
                                "Use a Spinner when confirming a change has been made or a task is being processed.")
                        ),
                        Stack().Children(
                            TextBlock("Don't").Medium(),
                            TextBlock("Don’t use a Spinner when performing immediate tasks."),
                            TextBlock("Don't show multiple Spinners at the same time."),
                            TextBlock("Don't include more than a few words when paired with a Spinner.")
                        ))
                ))
                .Section(
                    Stack().WidthPixels(400).Children(
                        TextBlock("Usage").MediumPlus(),
                        TextBlock("Spinner sizes").Medium(),
                        Label("Extra small spinner").Content(Spinner().XSmall()),
                        Label("Small spinner").Content(Spinner().Small()),
                        Label("Medium spinner").Content(Spinner().Medium()),
                        Label("Large spinner").Content(Spinner().Large())
                    ))
                .Section(
                    Stack().WidthPixels(400).Children(
                        TextBlock("Spinner label positioning").Medium(),
                        Label("Spinner with label positioned below").Content(Spinner("I am definitely loading...").Below()),
                        Label("Spinner with label positioned above").Content(Spinner("Seriously, still loading...").Above()),
                        Label("Spinner with label positioned to right").Content(Spinner("Wait, wait...").Right()),
                        Label("Spinner with label positioned to left").Content(Spinner("Nope, still loading...").Left())
                    ));
        }

        public HTMLElement Render()
        {
            return _content.Render();
        }
    }
}
