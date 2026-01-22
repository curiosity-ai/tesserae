using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;
using static H5.Core.dom;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Components", Order = 30, Icon = UIcons.Pictures)]
    public class CarouselSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public CarouselSample()
        {
            var slide1 = Card(Stack().Children(TextBlock("Slide One").Large().Bold(), TextBlock("Introduce the gallery with a summary."))).W(100.pct());
            var slide2 = Card(Stack().Children(TextBlock("Slide Two").Large().Bold(), TextBlock("Highlight a new feature or update."))).W(100.pct());
            var slide3 = Card(Stack().Children(TextBlock("Slide Three").Large().Bold(), TextBlock("Showcase results or next steps."))).W(100.pct());

            var carousel = Carousel(slide1, slide2, slide3);

            _content = SectionStack()
               .Title(SampleHeader(nameof(CarouselSample)))
               .Section(Stack().Children(
                    SampleTitle("Carousel / Gallery"),
                    carousel));
        }

        public HTMLElement Render() => _content.Render();
    }
}
