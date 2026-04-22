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
                    SampleTitle("Overview"),
                    TextBlock("Skeleton loaders are used to provide a placeholder for content that is still loading. They help reduce the perceived load time and prevent layout shifts by reserving the space that the final content will occupy."),
                    TextBlock("They come in various shapes like circles for avatars and rectangles for text or images.")))
               .Section(Stack().Children(
                    SampleTitle("Best Practices"),
                    TextBlock("Use skeleton loaders when content takes more than a second to load. Match the shape and size of the skeleton as closely as possible to the actual content it replaces. Avoid using skeletons for very fast-loading content as it can cause flickering. Ensure the skeleton's color and animation are subtle and fit with the overall theme.")))
               .Section(Stack().Children(
                    SampleTitle("Usage"),
                    SampleSubTitle("Avatar and Text Placeholder"),
                    HStack().Children(
                        Skeleton(SkeletonType.Circle).W(48).H(48),
                        VStack().Children(
                            Skeleton().W(200).H(16).ML(16).MB(8),
                            Skeleton().W(140).H(12).ML(16))),
                    SampleSubTitle("Article/Image Placeholder"),
                    VStack().Children(
                        Skeleton(SkeletonType.Rect).WS().H(200),
                        Skeleton().WS().H(16).MT(16),
                        Skeleton().W(80.percent()).H(16).MT(8),
                        Skeleton().W(60.percent()).H(16).MT(8)
                    )
                ));
        }

        public HTMLElement Render() => _content.Render();
    }
}
