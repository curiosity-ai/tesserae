using Tesserae.Themes.Curiosity;
using static Tesserae.UI;
using static Transpose.Core.dom;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = SampleGroup.Curiosity, Order = 20, Icon = UIcons.Wind, Description = "The brand's dash field, its pixel following the pointer")]
    public class FlowFieldSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public FlowFieldSample()
        {
            IComponent Cell(string title, string note, FlowField field) =>
                VStack().WS().Gap(8.px()).Children(
                    field.H(240),
                    TextBlock(title).SemiBold(),
                    TextBlock(note).Small().Secondary());

            _content = SectionStack().Secondary()
               .SampleTitle(typeof(FlowFieldSample), UIcons.Wind, "The brand's field, live")
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                        TextBlock("FlowField is the brand's only texture at scale: a grid of short dashes whose angle bends around one point, the signal pixel. It is drawn on a canvas at the size it is shown and redrawn when that size changes."),
                        TextBlock("As in the website's hero, the pixel drifts slowly round its place and, while the pointer is over the field, eases toward it (the brand's lerp of 0.06 a frame), hiding the mouse cursor so the square is the only cursor there is. It goes home when the pointer leaves. The loop runs only while the pixel moves and the field is on screen; reduced motion gets the drawing with no drift and no follow."))).SetTitle("Overview")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                        new FlowField().H(360),
                        TextBlock("new FlowField() - on ink, drifting, following the pointer. Point at it.").Small().Secondary().PT(8)
                    )).SetTitle("The hero field")))
               .FlatSection(Stack().Children(
                    Card(Grid(new UnitSize("repeat(auto-fill, minmax(280px, 1fr))")).WS().Gap(16.px()).Children(
                        Cell("On paper, fading left", ".OnPaper().Fade(FlowFieldFade.Left) - the page bands, fading into copy beside them.",
                            new FlowField().OnPaper().Fade(FlowFieldFade.Left).DotAt(0.55, 0.45)),
                        Cell("No animation", ".NoAnimation() - drawn once; the pixel jumps to the pointer instead of easing.",
                            new FlowField().NoAnimation()),
                        Cell("No interaction", ".NoInteraction() - it drifts, but the pointer leaves it alone and keeps its cursor.",
                            new FlowField().NoInteraction()),
                        Cell("Still", ".NoAnimation().NoInteraction() - a static drawing, sized to its box.",
                            new FlowField().OnPaper().NoAnimation().NoInteraction()),
                        Cell("Pixel on hover", ".Mark(FlowFieldMark.Hover).Drift(false) - no pixel at rest; one in the ground's tone while pointed at.",
                            new FlowField().Mark(FlowFieldMark.Hover).Drift(false)),
                        Cell("Denser", ".Pitch(20).Fade(FlowFieldFade.Down) - a smaller cell pitch, fading toward the bottom.",
                            new FlowField().Pitch(20).Fade(FlowFieldFade.Down))
                    )).SetTitle("Options")))
               .SeeAlso(typeof(DashMosaicSample), typeof(PixelGlyphSample), typeof(PixelIntroSample));
        }

        public HTMLElement Render() => _content.Render();
    }
}
