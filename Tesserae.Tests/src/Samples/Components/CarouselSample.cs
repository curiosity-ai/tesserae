using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;
using static H5.Core.dom;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Components", Order = 30, Icon = UIcons.Picture)]
    public class CarouselSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public CarouselSample()
        {
            var textSlide1 = VStack().P(16).Children(TextBlock("Slide One").Large().Bold(), TextBlock("Introduce the gallery with a summary.")).WS();
            var textSlide2 = VStack().P(16).Children(TextBlock("Slide Two").Large().Bold(), TextBlock("Highlight a new feature or update.")).WS();
            var textSlide3 = VStack().P(16).Children(TextBlock("Slide Three").Large().Bold(), TextBlock("Showcase results or next steps.")).WS();

            var imgSlide1 = Image("https://cataas.com/cat").WS().H(500).Contain();
            var imgSlide2 = Image("https://cataas.com/cat").WS().H(500).Contain();
            var imgSlide3 = Image("https://cataas.com/cat").WS().H(500).Contain();

            var carousel = Carousel(textSlide1, textSlide2, textSlide3).PadSlides(); //For text content, it's better to pad the slides so they don't overlap with the controls

            var imageCarousel = Carousel(imgSlide1, imgSlide2, imgSlide3);

            _content = SectionStack()
               .Title(SampleHeader(nameof(CarouselSample)))
               .Section(
                    VStack().Children(
                        SampleTitle("Carousel"),
                        carousel,
                        SampleTitle("Carousel Gallery"),
                        imageCarousel));
        }

        public HTMLElement Render() => _content.Render();
    }
}
