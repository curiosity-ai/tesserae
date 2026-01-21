using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Components", Order = 15, Icon = UIcons.ListCheck)]
    public class StepperSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public StepperSample()
        {
            _content = SectionStack()
               .Title(SampleHeader(nameof(StepperSample)))
               .Section(Stack().Children(
                    SampleTitle("Overview"),
                    TextBlock("Steppers guide users through a multi-step process.")))
               .Section(Stack().Children(
                    SampleTitle("Usage"),
                    Stepper().SetSteps("Personal Info", "Shipping", "Payment", "Review")
                        .Do(s => s.CurrentStep = 1)
                ));
        }

        public H5.Core.dom.HTMLElement Render()
        {
            return _content.Render();
        }
    }
}
