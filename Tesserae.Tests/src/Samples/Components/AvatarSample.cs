using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;
using static H5.Core.dom;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Components", Order = 30, Icon = UIcons.User)]
    public class AvatarSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public AvatarSample()
        {
            var avatars = HStack().Children(
                Avatar(initials: "JD").Size(AvatarSize.XSmall),
                Avatar(initials: "JD").Size(AvatarSize.Small).Presence(AvatarPresence.Online),
                Avatar(initials: "JD").Size(AvatarSize.Medium).Presence(AvatarPresence.Away),
                Avatar(initials: "JD").Size(AvatarSize.Large).Presence(AvatarPresence.Busy),
                Avatar(initials: "JD").Size(AvatarSize.XLarge).Presence(AvatarPresence.Offline));

            var persona = Persona("Jordan Diaz", "Product Designer", "Available for collaboration", Avatar(initials: "JD").Presence(AvatarPresence.Online));

            _content = SectionStack()
               .Title(SampleHeader(nameof(AvatarSample)))
               .Section(Stack().Children(
                    SampleTitle("Avatars"),
                    avatars))
               .Section(Stack().Children(
                    SampleTitle("Persona"),
                    persona));
        }

        public HTMLElement Render() => _content.Render();
    }
}
