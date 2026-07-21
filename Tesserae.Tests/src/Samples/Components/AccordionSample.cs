using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;
using static Transpose.Core.dom;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Components", Order = 30, Icon = UIcons.Accordion)]
    public class AccordionSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public AccordionSample()
        {
            _content = SectionStack().Secondary()
               .SampleTitle(typeof(AccordionSample), UIcons.Apps, "A collapsible content component")
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    TextBlock("An accordion contains a list of expanders that can be toggled to reveal more information. They are useful for organizing content into manageable chunks and reducing vertical space usage when not all information needs to be visible at once."),
                    TextBlock("Tesserae's Accordion component manages multiple Expanders, allowing you to control whether one or multiple sections can be open at the same time."))).SetTitle("Overview")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    TextBlock("Use accordions to organize related content that might be too long to display all at once. Ensure the header of each expander clearly describes the content within. Avoid nesting accordions within accordions as it can lead to confusion. Consider using a single Expander if you only have one block of optional content."))).SetTitle("Best Practices")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
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
                    Expander("What is Tesserae?", TextBlock("Tesserae provides a fluent API for building UI components.")).Expanded(),
                    SampleSubTitle("Expander with OptionIcon and ChevronRight"),
                    Accordion(
                        Expander("What does indexing do?", TextBlock("Indexing reads each document, extracts structured text, and stores it in a vector + keyword index so queries match in milliseconds.\n\nSource files stay where they are — only metadata leaves your machine.")).OptionIcon(UIcons.Info, Theme.Colors.Blue600, Theme.Colors.Blue100).ChevronRight().Expanded(),
                        Expander("Where are my files stored?", TextBlock("")).OptionIcon(UIcons.Check, Theme.Colors.Green600, Theme.Colors.Green100).ChevronRight(),
                        Expander("Why did my last build fail?", TextBlock("")).OptionIcon(UIcons.Exclamation, Theme.Colors.Orange600, Theme.Colors.Orange100).ChevronRight(),
                        Expander("How do I rotate the IMAP token?", TextBlock("")).OptionIcon(UIcons.TriangleWarning, Theme.Colors.Red600, Theme.Colors.Red100).ChevronRight(),
                        Expander("Can I use a custom embedding model?", TextBlock("")).OptionIcon(UIcons.Settings, Theme.Colors.Blue600, Theme.Colors.Blue100).ChevronRight()
                    ).AllowMultipleOpen(false))).SetTitle("Usage")));
        }

        public HTMLElement Render() => _content.Render();
    }
}
