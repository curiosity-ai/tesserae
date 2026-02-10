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
                    SampleTitle("Overview"),
                    TextBlock("Badges, Tags, and Chips are small visual elements used to categorize content, highlight status, or display metadata."),
                    TextBlock("They come in various styles: Badges are typically static indicators, Tags are for categorization, and Chips often include interactive elements like a removal button.")))
               .Section(Stack().Children(
                    SampleTitle("Best Practices"),
                    TextBlock("Use badges to call attention to small pieces of information like counts or status. Use tags for categorization where multiple labels might apply. Use chips for entities that can be removed or interacted with individually. Ensure colors are used consistently to convey meaning (e.g., red for danger/errors, green for success).")))
               .Section(Stack().Children(
                    SampleTitle("Usage"),
                    SampleSubTitle("Standard Badges"),
                    HStack().Children(
                        Badge("Default"),
                        Badge("Primary").Primary(),
                        Badge("Success").Success(),
                        Badge("Warning").Warning(),
                        Badge("Danger").Danger(),
                        Badge("Info").Info().Outline()),
                    SampleSubTitle("Tags and Chips"),
                    TextBlock("Tags and chips support icons, pill shapes, and interactive removal."),
                    HStack().Children(
                        Tag("Categorization").Outline().Pill(),
                        Tag("Metadata").SetIcon(Icon.Transform(UIcons.Tags, UIconsWeight.Regular)).Outline(),
                        Chip("Interactive Chip").Filled().OnRemove(c => Toast().Success("Removed chip")),
                        Chip("Status Chip").Success().Pill())
                ));
        }

        public HTMLElement Render() => _content.Render();
    }
}
