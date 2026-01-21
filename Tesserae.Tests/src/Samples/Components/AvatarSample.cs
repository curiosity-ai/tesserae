using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Components", Order = 11, Icon = UIcons.User)]
    public class AvatarSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public AvatarSample()
        {
            _content = SectionStack()
               .Title(SampleHeader(nameof(AvatarSample)))
               .Section(Stack().Children(
                    SampleTitle("Overview"),
                    TextBlock("Avatars represent users or entities using images or initials.")))
               .Section(Stack().Children(
                    SampleTitle("Usage"),
                    TextBlock("Sizes").Medium(),
                    HStack().Children(
                        Avatar("SM").Small(),
                        Avatar("MD").Medium(),
                        Avatar("LG").Large(),
                        Avatar("HG").Huge()),
                    TextBlock("With images").Medium(),
                    HStack().Children(
                        Avatar("JS", "https://via.placeholder.com/150").Medium(),
                        Avatar("AB", "https://via.placeholder.com/150").Large()),
                    TextBlock("Square shape").Medium(),
                    HStack().Children(
                        Avatar("SQ").Square().Medium(),
                        Avatar("SQ").Square().Large())
                ));
        }

        public H5.Core.dom.HTMLElement Render()
        {
            return _content.Render();
        }
    }
}
