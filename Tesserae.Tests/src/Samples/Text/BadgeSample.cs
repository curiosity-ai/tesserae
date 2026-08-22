using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;
using static Transpose.Core.dom;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = SampleGroup.Text, Order = 50, Icon = UIcons.Sticker)]
    public class BadgeSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public BadgeSample()
        {
            _content = SectionStack().Secondary()
               .SampleTitle(typeof(BadgeSample), UIcons.Certificate, "A component to display a badge")
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    TextBlock("Badges, Tags, and Chips are small visual elements used to categorize content, highlight status, or display metadata."),
                    TextBlock("They come in various styles: Badges are typically static indicators, Tags are for categorization, and Chips often include interactive elements like a removal button."))).SetTitle("Overview")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    TextBlock("Use badges to call attention to small pieces of information like counts or status. Use tags for categorization where multiple labels might apply. Use chips for entities that can be removed or interacted with individually. Ensure colors are used consistently to convey meaning (e.g., red for danger/errors, green for success)."))).SetTitle("Best Practices")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
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
                        Chip("Status Chip").Success().Pill()),
                    SampleSubTitle("AI Tone"),
                    TextBlock("BadgeTone.AI - reached through AI() - marks a value, a row or a section as something a model produced. A badge is small enough that a tint would not read, so this tone fills with the purple-to-blue gradient; asking for Outline() gives you the tint and the accent instead. AIBadge() is the shorthand for the common one: a Sparkles-led gradient pill saying \"AI\".").MB(8),
                    HStack().Children(
                        AIBadge(),
                        Badge("Generated").AI(),
                        Tag("Suggested").AI().Outline().Pill(),
                        Chip("AI draft").AI().OnRemove(c => Toast().Success("Dismissed the suggestion")))
                )).SetTitle("Usage")))
               .SeeAlso(typeof(ButtonSample), typeof(MetricSample), typeof(DeltaComponentSample), typeof(ContextCardSample), typeof(AIVariantsSample));
        }

        public HTMLElement Render() => _content.Render();
    }
}
