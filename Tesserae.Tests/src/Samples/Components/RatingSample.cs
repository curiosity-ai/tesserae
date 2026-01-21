using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Components", Order = 16, Icon = UIcons.Star)]
    public class RatingSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public RatingSample()
        {
            _content = SectionStack()
               .Title(SampleHeader(nameof(RatingSample)))
               .Section(Stack().Children(
                    SampleTitle("Overview"),
                    TextBlock("Ratings allow users to provide feedback on a scale.")))
               .Section(Stack().Children(
                    SampleTitle("Usage"),
                    TextBlock("Editable").Medium(),
                    Rating(3).Do(r => r.OnChange((s, e) => Toast().Information($"Rating changed to {s.Value}"))),
                    TextBlock("Read-only").Medium(),
                    Rating(4).Do(r => r.IsReadOnly = true)
                ));
        }

        public H5.Core.dom.HTMLElement Render()
        {
            return _content.Render();
        }
    }
}
