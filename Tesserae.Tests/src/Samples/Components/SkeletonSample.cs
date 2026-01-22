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
                            Skeleton().W(200).PL(8),
                            Skeleton().W(140).PL(8))),
                    Skeleton(SkeletonType.Rect).WS().H(120).PT(8)));
        }

        public HTMLElement Render() => _content.Render();
    }
}
