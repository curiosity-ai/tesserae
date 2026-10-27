using Tesserae.Themes.Curiosity;
using static Tesserae.UI;
using static Transpose.Core.dom;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = SampleGroup.Curiosity, Order = 50, Icon = UIcons.Picture, Description = "The pixel placeholder for an image that is being made")]
    public class GeneratingImageSample : IComponent, ISample
    {
        private readonly IComponent _content;
        private double              _timer;

        public GeneratingImageSample()
        {
            IComponent Cell(string title, string note, GeneratingImage image) =>
                VStack().WS().Gap(8.px()).Children(
                    image.WidthFraction(1),
                    TextBlock(title).SemiBold(),
                    TextBlock(note).Small().Secondary());

            // A chat that is making a picture: the message is allowed the whole chat area, and the placeholder takes half of it.
            var progressing = new GeneratingImage(3, 2).Progress(89);
            var chat = ChatArea().H(440);
            chat.Add(ChatMessage(TextBlock("I want you to create a high resolution version of the portraits of the original game, keeping the art direction and style"), Avatar(null, "U")).RightAligned().MaxWidth());
            chat.Add(ChatMessage(VStack().WS().Gap(10.px()).Children(
                TextBlock("Setting the scene"),
                progressing), Avatar(null, "AI")).FullWidth());

            var run = Button("Run").Primary().SetIcon(UIcons.Play);
            run.OnClick(() =>
            {
                window.clearInterval(_timer);
                var percent = 0;
                progressing.Progress(percent);
                _timer = window.setInterval(_ =>
                {
                    percent += 1;
                    if (percent > 100)
                    {
                        window.clearInterval(_timer);
                        return;
                    }
                    progressing.Progress(percent);
                }, 80);
            });
            var reset = Button("Back to 89%").OnClick(() =>
            {
                window.clearInterval(_timer);
                progressing.Progress(89);
            });

            _content = SectionStack().Secondary()
               .SampleTitle(typeof(GeneratingImageSample), UIcons.Picture, "A frame for the image that is not there yet")
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                        TextBlock("GeneratingImage stands in for a picture a model is still making. It is a field of square pixels in the theme's primary colour that swell and fade in rolling waves from the middle of the frame, small and quiet toward the edges, so the frame looks busy and a little different every moment without ever showing a false picture."),
                        TextBlock("It takes the shape of the finished image from an aspect ratio and, by default, is half as wide as the element it sits in: put it in a chat message that has the full width of the chat area and it takes half of that, whatever the width of the window. The height follows, so nothing moves when the image arrives. An optional badge in the bottom right corner reports progress, and the pixels swell toward the full size as it nears 100."))).SetTitle("Overview")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Gap(12.px()).Children(
                        chat,
                        HStack().Gap(8.px()).Children(run, reset),
                        TextBlock("new GeneratingImage(3, 2).Progress(89) in a FullWidth() chat message. Run counts up from 0; the field fills as it goes.").Small().Secondary()
                    )).SetTitle("In a chat")))
               .FlatSection(Stack().Children(
                    Card(Grid(new UnitSize("repeat(auto-fill, minmax(220px, 1fr))")).WS().Gap(16.px()).Children(
                        Cell("Square", "new GeneratingImage() - a ratio of 1.", new GeneratingImage()),
                        Cell("Landscape 3:2", "new GeneratingImage(3, 2) - a photograph.", new GeneratingImage(3, 2)),
                        Cell("Widescreen 16:9", "new GeneratingImage(16.0 / 9) - a single ratio.", new GeneratingImage(16.0 / 9)),
                        Cell("Portrait 3:4", "new GeneratingImage(0.75) - a tall frame.", new GeneratingImage(0.75)),
                        Cell("Banner 3:1", "new GeneratingImage(3, 1) - a wide strip.", new GeneratingImage(3, 1)),
                        Cell("Story 9:16", "new GeneratingImage(9, 16) - a phone screen.", new GeneratingImage(9, 16))
                    )).SetTitle("Aspect ratios")))
               .FlatSection(Stack().Children(
                    Card(Grid(new UnitSize("repeat(auto-fill, minmax(260px, 1fr))")).WS().Gap(16.px()).Children(
                        Cell("No progress", "No badge, the field at its resting fill. This is how it starts.", new GeneratingImage(4, 3)),
                        Cell("Progress", ".Progress(42) - a badge, and a field about half full.", new GeneratingImage(4, 3).Progress(42)),
                        Cell("Nearly there", ".Progress(96)", new GeneratingImage(4, 3).Progress(96)),
                        Cell("A label instead", ".Progress(0).Label(\"Queued\") - a state with no number.", new GeneratingImage(4, 3).Progress(0).Label("Queued"))
                    )).SetTitle("Progress badge")))
               .FlatSection(Stack().Children(
                    Card(Grid(new UnitSize("repeat(auto-fill, minmax(260px, 1fr))")).WS().Gap(16.px()).Children(
                        Cell("Coarser", ".Pitch(28) - a larger grid.", new GeneratingImage(4, 3).Pitch(28).Progress(64)),
                        Cell("Finer", ".Pitch(10) - a denser grid.", new GeneratingImage(4, 3).Pitch(10).Progress(64)),
                        Cell("Still", ".NoAnimation() - drawn once; reduced motion does the same.", new GeneratingImage(4, 3).NoAnimation().Progress(64))
                    )).SetTitle("Options")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Gap(12.px()).Children(
                        new GeneratingImage(16.0 / 9).WidthFraction(0.75).Progress(30),
                        TextBlock(".WidthFraction(0.75) - three quarters of the parent. The default is 0.5, and an explicit .W(...) replaces it.").Small().Secondary()
                    )).SetTitle("Width")))
               .SeeAlso(typeof(DashMosaicSample), typeof(FlowFieldSample), typeof(PixelGlyphSample));
        }

        public HTMLElement Render() => _content.Render();
    }
}
