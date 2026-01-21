using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Components", Order = 12, Icon = UIcons.ArrowProgress)]
    public class ShimmerSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public ShimmerSample()
        {
            _content = SectionStack()
               .Title(SampleHeader(nameof(ShimmerSample)))
               .Section(Stack().Children(
                    SampleTitle("Overview"),
                    TextBlock("Shimmers are loading indicators that mimic the layout of content.")))
               .Section(Stack().Children(
                    SampleTitle("Usage"),
                    TextBlock("Shapes").Medium(),
                    VStack().Children(
                        Shimmer().Width(100.percent()).Height(16.px()),
                        Shimmer().Width(50.percent()).Height(16.px()),
                        Shimmer().Circle().Width(50.px()).Height(50.px()))
                ));
        }

        public H5.Core.dom.HTMLElement Render()
        {
            return _content.Render();
        }
    }
}
