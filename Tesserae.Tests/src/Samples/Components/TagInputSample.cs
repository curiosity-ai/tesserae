using Tesserae;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;
using static H5.Core.dom;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Components", Order = 103, Icon = UIcons.Tags)]
    public class TagInputSample : IComponent, ISample
    {
        private IComponent _content;

        public TagInputSample()
        {
            var tagInput = TagInput()
                .Placeholder("Add a tag...")
                .AddTag("React")
                .AddTag("TypeScript")
                .AddTag("C#");

            var result = TextBlock("Tags: React, TypeScript, C#").MarginTop(16.px());

            tagInput.OnChange((s, tags) =>
            {
                result.Text = $"Tags: {string.Join(", ", tags)}";
            });

            _content = SectionStack()
                .Title(SampleHeader(nameof(TagInputSample)))
                .Section(
                    Stack()
                        .Children(
                            TextBlock("TagInput").MediumPlus().SemiBold(),
                            tagInput,
                            result
                        )
                );
        }

        public HTMLElement Render()
        {
            return _content.Render();
        }
    }
}