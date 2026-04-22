using System;
using System.Threading.Tasks;
using static H5.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Components", Order = 100, Icon = UIcons.IdBadge)]
    public class ResourceCardSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public ResourceCardSample()
        {
            var searchList = new SearchableList<ModelItem>(GetModelItems(), 1.fr(), 1.fr(), 1.fr())
                .WithNoResultsMessage(() => TextBlock("No models found").MediumPlus());

            _content = SectionStack()
                .SampleTitle(nameof(ResourceCardSample), UIcons.IdCard, "A card to display a resource")
                .Section(Stack().Children(
                    SampleTitle("Overview"),
                    TextBlock("ResourceCards are used to display a summary of a resource like an AI model, document, or service. They provide optional sections for title, subtitle, tags, description, date, icon, and a footer for commands."),
                    TextBlock("The example below uses a SearchableList in grid mode to render a set of ResourceCards.")))
                .Section(Stack().Children(
                    SampleTitle("Usage"),
                    searchList
                ));
        }

        private ModelItem[] GetModelItems()
        {
            return new ModelItem[]
            {
                new ModelItem("seedream-4.0", "bytedance", "Text-to-Image", "Seedream 4.0 is ByteDance's image creation model combining text-to-image generation and advanced styling capabilities.", "Apr 8, 2026", UIcons.Picture),
                new ModelItem("nano-banana-2", "google", "Text-to-Image", "Nano Banana 2 is Google's latest image generation model, built on Gemini 3.1 Flash with enhanced efficiency.", "Apr 8, 2026", UIcons.Picture),
                new ModelItem("veo-3.1", "google", "Text-to-Video", "Veo 3.1 is Google's state-of-the-art video generation model with synchronized native audio generation.", "Apr 8, 2026", UIcons.VideoCameraAlt),
                new ModelItem("kimi-k2.5", "moonshotai", "Image-Text-to-Text", "Kimi K2.5 is a native multimodal language model from Moonshot AI that understands images and text prompts.", "Apr 8, 2026", UIcons.Text),
                new ModelItem("gpt-5.4", "openai", "Text Generation", "GPT-5.4 is OpenAI's most capable frontier model for coding, reasoning, and professional tasks.", "Apr 8, 2026", UIcons.Comment),
                new ModelItem("claude-opus-4.6", "anthropic", "Text Generation", "Claude Opus 4.6 is Anthropic's flagship language model built for complex, multi-step problem solving.", "Apr 8, 2026", UIcons.Comment)
            };
        }

        public HTMLElement Render()
        {
            return _content.Render();
        }

        private class ModelItem : ISearchableItem
        {
            public string Title { get; }
            public string Author { get; }
            public string Capability { get; }
            public string Description { get; }
            public string Date { get; }
            public UIcons Icon { get; }

            public ModelItem(string title, string author, string capability, string description, string date, UIcons icon)
            {
                Title = title;
                Author = author;
                Capability = capability;
                Description = description;
                Date = date;
                Icon = icon;
            }

            public bool IsMatch(string searchTerm)
            {
                if (string.IsNullOrWhiteSpace(searchTerm)) return true;
                searchTerm = searchTerm.ToLower();
                return Title.ToLower().Contains(searchTerm) ||
                       Author.ToLower().Contains(searchTerm) ||
                       Capability.ToLower().Contains(searchTerm) ||
                       Description.ToLower().Contains(searchTerm);
            }

            public IComponent Render()
            {
                return UI.ResourceCard()
                    .SetIcon(UI.Icon(Icon, size: TextSize.Large))
                    .SetTitle(Title)
                    .SetSubtitle(Author)
                    .SetTags(Badge(Capability))
                    .SetDescription(Description)
                    .SetDate(Date)
                    .SetFooter(Link("https://example.com/terms", "Terms"))
                    .SetFooterCommands(Button("Copy ID").SetIcon(UIcons.Copy).NoBorder().NoBackground());
            }
        }
    }
}
