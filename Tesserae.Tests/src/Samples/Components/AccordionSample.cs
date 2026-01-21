using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Components", Order = 13, Icon = UIcons.MenuBurger)]
    public class AccordionSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public AccordionSample()
        {
            _content = SectionStack()
               .Title(SampleHeader(nameof(AccordionSample)))
               .Section(Stack().Children(
                    SampleTitle("Overview"),
                    TextBlock("Accordions allow users to expand and collapse sections of content.")))
               .Section(Stack().Children(
                    SampleTitle("Usage"),
                    Accordion().Do(a => {
                        a.Add(new Accordion.Item(TextBlock("Section 1"), TextBlock("This is the content of section 1. It is currently collapsed by default.")));
                        a.Add(new Accordion.Item(TextBlock("Section 2"), TextBlock("This is the content of section 2. It is expanded by default."), true));
                        a.Add(new Accordion.Item(TextBlock("Section 3"), TextBlock("This is the content of section 3.")));
                    })
                ));
        }

        public H5.Core.dom.HTMLElement Render()
        {
            return _content.Render();
        }
    }
}
