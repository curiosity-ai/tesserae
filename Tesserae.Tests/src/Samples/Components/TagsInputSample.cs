using static H5.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Components", Order = 10, Icon = UIcons.Tags)]
    public class TagsInputSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public TagsInputSample()
        {
            _content = SectionStack().Secondary()
               .SampleTitle(typeof(TagsInputSample), UIcons.Tags, "A multi-value chip / tag input")
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    TextBlock("TagsInput lets users assemble a list of short string values by typing them in and confirming each with Enter (or a configurable delimiter such as comma)."),
                    TextBlock("Tags are removed with the chip's '×' affordance, or by pressing Backspace when the inline entry field is empty. Use it for keywords, recipients, labels and other free-form multi-value fields."))).SetTitle("Overview")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    SampleSubTitle("Empty"),
                    Label("Tags").SetContent(TagsInput().SetPlaceholder("Type and press Enter")),
                    SampleSubTitle("Pre-populated, comma-friendly"),
                    Label("Categories").SetContent(
                        TagsInput("design", "engineering", "research")
                            .SetPlaceholder("Add a category")
                            .WithDelimiters(',', ';')
                            .OnChange(() => { /* respond to changes */ })),
                    SampleSubTitle("Capped at 5, duplicates allowed, normalize to lower-case"),
                    Label("Keywords").SetContent(
                        TagsInput()
                            .WithMaxTags(5)
                            .AllowingDuplicates()
                            .WithNormalizer(s => s?.Trim().ToLower())
                            .SetPlaceholder("Up to 5 keywords"))
                )).SetTitle("Usage")));
        }

        public HTMLElement Render() => _content.Render();
    }
}
