using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;
using static H5.Core.dom;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Components", Order = 30, Icon = UIcons.Tags)]
    public class BadgeSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public BadgeSample()
        {
            _content = SectionStack()
               .Title(SampleHeader(nameof(BadgeSample)))
               .Section(Stack().Children(
                    SampleTitle("Badges"),
                    HStack().Children(
                        Badge("Default"),
                        Badge("Primary").Primary(),
                        Badge("Success").Success(),
                        Badge("Warning").Warning(),
                        Badge("Danger").Danger(),
                        Badge("Info").Info().Outline())))
               .Section(Stack().Children(
                    SampleTitle("Tags & Chips"),
                    HStack().Children(
                        Tag("Outlined").Outline().Pill(),
                        Tag("Metadata").SetIcon(Icon.Transform(UIcons.Tag, UIconsWeight.Regular)).Outline(),
                        Chip("Editable").Filled().OnRemove(c => Toast().Success("Removed chip")),
                        Chip("Status").Success().Pill()))));
        }

        public HTMLElement Render() => _content.Render();
    }
}
