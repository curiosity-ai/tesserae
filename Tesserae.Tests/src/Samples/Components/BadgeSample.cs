using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Components", Order = 10, Icon = UIcons.Info)]
    public class BadgeSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public BadgeSample()
        {
            _content = SectionStack()
               .Title(SampleHeader(nameof(BadgeSample)))
               .Section(Stack().Children(
                    SampleTitle("Overview"),
                    TextBlock("Badges are used to display small bits of information, such as status, counts, or categories.")))
               .Section(Stack().Children(
                    SampleTitle("Usage"),
                    TextBlock("Different variants").Medium(),
                    HStack().Children(
                        Badge("Primary").Primary(),
                        Badge("Success").Success(),
                        Badge("Danger").Danger(),
                        Badge("Warning").Warning(),
                        Badge("Info").Info()),
                    TextBlock("Small size").Medium(),
                    HStack().Children(
                        Badge("New").Small(),
                        Badge("10").Small().Success())
                ));
        }

        public H5.Core.dom.HTMLElement Render()
        {
            return _content.Render();
        }
    }
}
