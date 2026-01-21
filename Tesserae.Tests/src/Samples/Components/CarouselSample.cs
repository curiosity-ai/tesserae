using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Components", Order = 19, Icon = UIcons.Picture)]
    public class CarouselSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public CarouselSample()
        {
            _content = SectionStack()
               .Title(SampleHeader(nameof(CarouselSample)))
               .Section(Stack().Children(
                    SampleTitle("Overview"),
                    TextBlock("Carousels allow cycling through a set of items.")))
               .Section(Stack().Children(
                    SampleTitle("Usage"),
                    Carousel().Do(c => {
                        c.Add(Card(TextBlock("Slide 1").TextCenter()).Height(200.px()).WidthStretch());
                        c.Add(Card(TextBlock("Slide 2").TextCenter()).Height(200.px()).WidthStretch());
                        c.Add(Card(TextBlock("Slide 3").TextCenter()).Height(200.px()).WidthStretch());
                    })
                ));
        }

        public H5.Core.dom.HTMLElement Render()
        {
            return _content.Render();
        }
    }
}
