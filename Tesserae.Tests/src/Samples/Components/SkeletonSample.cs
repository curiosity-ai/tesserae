using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;
using static H5.Core.dom;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Components", Order = 30, Icon = UIcons.Spinner)]
    public class SkeletonSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public SkeletonSample()
        {
            _content = SectionStack()
               .Title(SampleHeader(nameof(SkeletonSample)))
               .Section(Stack().Children(
                    SampleTitle("Skeleton Loader"),
                    HStack().Children(
                        Skeleton(SkeletonType.Circle),
                        VStack().Children(
                            Skeleton().W(200.px()),
                            Skeleton().W(140.px()))),
                    Skeleton(SkeletonType.Rect).W(100.pct()).H(120.px())));
        }

        public HTMLElement Render() => _content.Render();
    }
}
