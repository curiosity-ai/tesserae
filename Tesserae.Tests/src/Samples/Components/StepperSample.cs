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
            var stepper = Stepper(
                Step("Profile", Stack().Children(TextBlock("Collect basic profile information."), TextBox().SetPlaceholder("Full name"))),
                Step("Preferences", Stack().Children(TextBlock("Select preferences."), Toggle("Enable notifications").Checked())),
                Step("Review", Stack().Children(TextBlock("Review and submit your data."), Button("Submit").Primary())))
               .OnStepChange(s => Toast().Success($"Moved to step {s.CurrentStepIndex + 1}"));

            _content = SectionStack()
               .Title(SampleHeader(nameof(StepperSample)))
               .Section(Stack().Children(
                    SampleTitle("Stepper / Wizard"),
                    stepper));
        }

        public HTMLElement Render() => _content.Render();
    }
}
