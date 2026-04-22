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
            _content = SectionStack()
               .Title(SampleHeader(nameof(AccordionSample)))
               .Section(Stack().Children(
                    SampleTitle("Overview"),
                    TextBlock("An accordion contains a list of expanders that can be toggled to reveal more information. They are useful for organizing content into manageable chunks and reducing vertical space usage when not all information needs to be visible at once."),
                    TextBlock("Tesserae's Accordion component manages multiple Expanders, allowing you to control whether one or multiple sections can be open at the same time.")))
               .Section(Stack().Children(
                    SampleTitle("Best Practices"),
                    TextBlock("Use accordions to organize related content that might be too long to display all at once. Ensure the header of each expander clearly describes the content within. Avoid nesting accordions within accordions as it can lead to confusion. Consider using a single Expander if you only have one block of optional content.")))
               .Section(Stack().Children(
                    SampleTitle("Usage"),
                    SampleSubTitle("Basic Accordion"),
                    Accordion(
                        Expander("Getting started", TextBlock("Use expanders to reveal details in place without navigating away.")).Expanded(),
                        Expander("Configuration", Stack().Children(
                            TextBlock("You can nest any component inside an expander."),
                            Button("Primary action").Primary())),
                        Expander("Advanced", TextBlock("Combine with SectionStack or Card for complex layouts.")))
                       .AllowMultipleOpen(false),
                    SampleSubTitle("Accordion with Multiple Open Allowed"),
                    Accordion(
                        Expander("Section 1", TextBlock("Multiple sections can be open simultaneously here.")),
                        Expander("Section 2", TextBlock("This is useful for comparing information between sections.")),
                        Expander("Section 3", TextBlock("Just set .AllowMultipleOpen(true) on the accordion.")))
                       .AllowMultipleOpen(true),
                    SampleSubTitle("Standalone Expander"),
                    Expander("What is Tesserae?", TextBlock("Tesserae provides a fluent API for building UI components."))
                       .Expanded()));
        }

        public HTMLElement Render() => _content.Render();
    }
}
