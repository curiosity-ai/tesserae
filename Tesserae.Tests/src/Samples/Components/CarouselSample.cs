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
            var textSlide1 = VStack().P(32).Children(TextBlock("Discover Tesserae").Large().Bold(), TextBlock("Build stunning user interfaces in C# that compile to high-performance JavaScript.")).WS();
            var textSlide2 = VStack().P(32).Children(TextBlock("Fluent API").Large().Bold(), TextBlock("Leverage a powerful, typed, and fluent API to define your layout and logic efficiently.")).WS();
            var textSlide3 = VStack().P(32).Children(TextBlock("Native Performance").Large().Bold(), TextBlock("Zero-overhead abstractions mean your application runs fast on any modern browser.")).WS();

            var imgSlide1 = Image("https://cataas.com/cat").WS().H(300).Contain();
            var imgSlide2 = Image("https://cataas.com/cat").WS().H(300).Contain();
            var imgSlide3 = Image("https://cataas.com/cat").WS().H(300).Contain();

            _content = SectionStack()
               .Title(SampleHeader(nameof(CarouselSample)))
               .Section(Stack().Children(
                    SampleTitle("Overview"),
                    TextBlock("Carousels allow users to cycle through a set of related content, such as images, features, or messages. They are effective for showcasing highlights in a limited space."),
                    TextBlock("The component supports any Tesserae component as a slide and provides automatic or manual navigation.")))
               .Section(Stack().Children(
                    SampleTitle("Best Practices"),
                    TextBlock("Use carousels for high-impact visual content. Keep the number of slides low (typically 3-5) to ensure users can reasonably see all content. Ensure that each slide has a clear and unique message. Provide navigation controls (arrows/dots) and ensure they are accessible. For slides with text content, ensure sufficient contrast and use .PadSlides() to prevent overlapping with controls.")))
               .Section(Stack().Children(
                    SampleTitle("Usage"),
                    SampleSubTitle("Text Carousel"),
                    Carousel(textSlide1, textSlide2, textSlide3).PadSlides().H(150),
                    SampleSubTitle("Image Gallery Carousel"),
                    Carousel(imgSlide1, imgSlide2, imgSlide3).H(300),
                    SampleSubTitle("Interactive Carousel"),
                    Carousel(
                        VStack().Children(TextBlock("Interactive Slide").Medium(), Button("Click me").OnClick(() => Toast().Success("Clicked!"))).P(32),
                        VStack().Children(TextBlock("Configuration Slide").Medium(), CheckBox("Enable feature")).P(32)
                    ).PadSlides().H(150)
                ));
        }

        public HTMLElement Render() => _content.Render();
    }
}
