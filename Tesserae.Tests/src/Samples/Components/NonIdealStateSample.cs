using Tesserae;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;
using static H5.Core.dom;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Components", Order = 100, Icon = UIcons.Folder)]
    public class NonIdealStateSample : IComponent, ISample
    {
        private IComponent _content;

        public NonIdealStateSample()
        {
            _content = SectionStack()
                .Title(SampleHeader(nameof(NonIdealStateSample)))
                .Section(
                    Stack()
                        .Children(
                            TextBlock("NonIdealState").MediumPlus().SemiBold(),
                            NonIdealState(
                                icon: Icon(UIcons.Search, size: TextSize.Large, color: Theme.Secondary.Foreground),
                                title: "No search results",
                                description: "Your search didn't match any files.",
                                action: Button("Clear search").Primary()
                            )
                        )
                );
        }

        public HTMLElement Render()
        {
            return _content.Render();
        }
    }
}