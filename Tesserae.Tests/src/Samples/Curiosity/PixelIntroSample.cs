using Tesserae.Themes.Curiosity;
using static Tesserae.UI;
using static Transpose.Core.dom;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = SampleGroup.Curiosity, Order = 10, Icon = UIcons.PlayAlt, Description = "The website's first-load animation, played once")]
    public class PixelIntroSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public PixelIntroSample()
        {
            _content = SectionStack().Secondary()
               .SampleTitle(typeof(PixelIntroSample), UIcons.PlayAlt, "The Curiosity website's first-load animation")
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                        TextBlock("PixelIntro draws the website's intro over the whole window and removes itself when it ends. It is decoration: it takes no pointer events, is hidden from assistive technology, and does not play at all when the reader asked for reduced motion."),
                        TextBlock("Call PixelIntro.PlayOnce() once as the app starts. Like the website it plays the first time in a tab, again on a reload, and not when the app navigates. PixelIntro.Play() plays it now, for a preview like the buttons below."))).SetTitle("Overview")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Gap(12.px()).Children(
                        TextBlock("Square field").SemiBold(),
                        TextBlock("The website's default (its Blend style): on ink, paper squares grow in on a diagonal sweep, turn toward the Signal the way the brand's field bends round it, the Signal square lands last, then everything shrinks away in the same order and the ink lifts off the page.").Small(),
                        Button("Play the square field").Primary().OnClick(() => PixelIntro.Play(PixelIntroStyle.SquareField).FireAndForget()),
                        TextBlock("Tiles").SemiBold().PT(8),
                        TextBlock("The brand style's intro: the window is laid with ink and slate tiles from the corner and uncovered in the same order, the escaped Signal square, top right, leaving last.").Small(),
                        Button("Play the tiles").OnClick(() => PixelIntro.Play(PixelIntroStyle.Tiles).FireAndForget()),
                        TextBlock("In an app").SemiBold().PT(8),
                        TextBlock("PixelIntro.PlayOnce();                     // the square field, once per tab\nPixelIntro.PlayOnce(PixelIntroStyle.Tiles);\nawait PixelIntro.Play();                   // now, whatever has played").Small()
                           .Style(s => { s.fontFamily = Theme.Fonts.Monospace; s.whiteSpace = "pre"; })
                    )).SetTitle("Usage")))
               .SeeAlso(typeof(FlowFieldSample), typeof(PixelGlyphSample), typeof(DashMosaicSample));
        }

        public HTMLElement Render() => _content.Render();
    }
}
