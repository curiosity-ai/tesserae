using Tesserae;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;
using static H5.Core.dom;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Components", Order = 101, Icon = UIcons.IdBadge)]
    public class EntityTitleSample : IComponent, ISample
    {
        private IComponent _content;

        public EntityTitleSample()
        {
            _content = SectionStack()
                .Title(SampleHeader(nameof(EntityTitleSample)))
                .Section(
                    Stack()
                        .Children(
                            TextBlock("EntityTitle").MediumPlus().SemiBold(),
                            EntityTitle(
                                title: "John Doe",
                                subtitle: "Software Engineer",
                                icon: Avatar("https://picsum.photos/40/40")
                            ).Tags(Badge("Internal"), Badge("Admin").Primary())
                        )
                );
        }

        public HTMLElement Render()
        {
            return _content.Render();
        }
    }
}