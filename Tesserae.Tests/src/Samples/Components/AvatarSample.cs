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
            _content = SectionStack()
               .Title(SampleHeader(nameof(AvatarSample)))
               .Section(Stack().Children(
                    SampleTitle("Overview"),
                    TextBlock("Avatars are used to represent users, teams, or entities in the system. They can display images, initials, and presence indicators."),
                    TextBlock("The Persona component builds upon Avatar by adding textual information like name, role, and status, making it ideal for profile cards or contact lists.")))
               .Section(Stack().Children(
                    SampleTitle("Best Practices"),
                    TextBlock("Use avatars to provide visual recognition for users. Always provide initials as a fallback for when images fail to load or aren't available. Use the appropriate size for the context—smaller for lists or chat, larger for profiles. Presence indicators should be used when real-time availability information is relevant to the user's task.")))
               .Section(Stack().Children(
                    SampleTitle("Usage"),
                    SampleSubTitle("Avatar Sizes and Presence"),
                    TextBlock("Avatars support various sizes from XSmall to XLarge and optional presence states."),
                    HStack().Children(
                        Avatar(initials: "JD", image: "https://cataas.com/cat").Size(AvatarSize.XSmall),
                        Avatar(initials: "JD", image: "https://cataas.com/cat").Size(AvatarSize.Small).Presence(AvatarPresence.Online),
                        Avatar(initials: "JD", image: "https://cataas.com/cat").Size(AvatarSize.Medium).Presence(AvatarPresence.Away),
                        Avatar(initials: "JD", image: "https://cataas.com/cat").Size(AvatarSize.Large).Presence(AvatarPresence.Busy),
                        Avatar(initials: "JD", image: "https://cataas.com/cat").Size(AvatarSize.XLarge).Presence(AvatarPresence.Offline)),
                    SampleSubTitle("Initials Fallback"),
                    TextBlock("When no image is provided, initials are displayed with a generated background color."),
                    HStack().Children(
                        Avatar(initials: "JD").Size(AvatarSize.Small).Presence(AvatarPresence.Online),
                        Avatar(initials: "AS").Size(AvatarSize.Medium).Presence(AvatarPresence.Away),
                        Avatar(initials: "KL").Size(AvatarSize.Large).Presence(AvatarPresence.Busy),
                        Avatar(initials: "MW").Size(AvatarSize.XLarge).Presence(AvatarPresence.Offline)),
                    SampleSubTitle("Persona Component"),
                    TextBlock("Personas combine an avatar with descriptive text."),
                    VStack().Children(
                        Persona("Jordan Diaz", "Product Designer", "Available for collaboration", Avatar(initials: "JD").Presence(AvatarPresence.Online)),
                        Persona("Alex Smith", "Software Engineer", "Focusing...", Avatar(initials: "AS").Presence(AvatarPresence.Busy)),
                        Persona("Kelly Lee", "Project Manager", "Away", Avatar(initials: "KL").Presence(AvatarPresence.Away))
                    )
                ));
        }

        public HTMLElement Render() => _content.Render();
    }
}
