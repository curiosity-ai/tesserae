using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;
using static H5.Core.dom;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Components", Order = 30, Icon = UIcons.Accordion)]
    public class AccordionSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public AccordionSample()
        {
            var accordion = Accordion(
                Expander("Getting started", TextBlock("Use expanders to reveal details in place without navigating away."))
                    .Expanded(),
                Expander("Configuration", Stack().Children(
                    TextBlock("You can nest any component inside an expander."),
                    Button("Primary action").Primary())),
                Expander("Advanced", TextBlock("Combine with SectionStack or Card for complex layouts.")))
               .AllowMultipleOpen(false);

            _content = SectionStack()
               .Title(SampleHeader(nameof(AccordionSample)))
               .Section(Stack().Children(
                    SampleTitle("Accordion"),
                    accordion))
               .Section(Stack().Children(
                    SampleTitle("Standalone Expander"),
                    Expander("What is Tesserae?", TextBlock("Tesserae provides a fluent API for building UI components."))
                       .Expanded()));
        }

        public HTMLElement Render() => _content.Render();
    }
}
