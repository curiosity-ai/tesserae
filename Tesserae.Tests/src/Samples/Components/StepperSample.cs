using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;
using static H5.Core.dom;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Components", Order = 30, Icon = UIcons.ListCheck)]
    public class StepperSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public StepperSample()
        {
            _content = SectionStack()
               .Title(SampleHeader(nameof(StepperSample)))
               .Section(Stack().Children(
                    SampleTitle("Overview"),
                    TextBlock("Steppers (also known as Wizards) guide users through a multi-step process by breaking it down into smaller, logical chunks."),
                    TextBlock("They manage the visibility of content for each step and provide built-in navigation controls (Previous/Next) while tracking the current progress.")))
               .Section(Stack().Children(
                    SampleTitle("Best Practices"),
                    TextBlock("Use Steppers for complex tasks that have a clear sequential order. Keep each step focused on a single topic to avoid overwhelming the user. Provide clear labels for each step so the user knows what to expect. Use the 'Review' step to allow users to verify their input before the final submission. Ensure that the 'Previous' action allows users to return and modify their entries without losing data.")))
               .Section(Stack().Children(
                    SampleTitle("Usage"),
                    SampleSubTitle("Registration Wizard"),
                    Stepper(
                        Step("Personal Info", Stack().Children(
                            TextBlock("Tell us about yourself:").MB(16),
                            Label("Full Name").SetContent(TextBox().SetPlaceholder("John Doe")),
                            Label("Email Address").SetContent(TextBox().SetPlaceholder("john@example.com"))
                        )),
                        Step("Preferences", Stack().Children(
                            TextBlock("Customize your experience:").MB(16),
                            Toggle(onText: TextBlock("Yes"), offText: TextBlock("No")).Checked(),
                            Toggle(onText: TextBlock("Dark"), offText: TextBlock("Light")),
                            Label("Favorite Color").SetContent(ColorPicker())
                        )),
                        Step("Terms & Review", Stack().Children(
                            TextBlock("Please review and accept our terms:").MB(16),
                            Card(TextBlock("Detailed terms and conditions text goes here...").Small()),
                            Label("Acceptance").Required().SetContent(CheckBox("I agree to the terms of service")),
                            Button("Complete Registration").Primary().MT(16)
                        ))
                    ).OnStepChange(s => Toast().Information($"Step {s.CurrentStepIndex + 1}: {s.CurrentStep.Title}"))
                ));
        }

        public HTMLElement Render() => _content.Render();
    }
}
