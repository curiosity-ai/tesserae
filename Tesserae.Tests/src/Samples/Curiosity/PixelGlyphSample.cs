using System;
using System.Collections.Generic;
using System.Linq;
using Tesserae.Themes.Curiosity;
using static Tesserae.UI;
using static Transpose.Core.dom;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = SampleGroup.Curiosity, Order = 30, Icon = UIcons.SquareDashed, Description = "The website's animated 4 by 4 pixel glyphs")]
    public class PixelGlyphSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public PixelGlyphSample()
        {
            var all = Enum.GetValues(typeof(PixelGlyphKind)).Cast<PixelGlyphKind>().ToArray();

            var plays = new List<PixelGlyph>();

            IComponent Family(string prefix, int size, bool withLabels = false)
            {
                var grid = Grid(new UnitSize($"repeat(auto-fill, minmax({Math.Max(96, size + 48)}px, 1fr))")).WS().Gap(16.px());
                foreach (var kind in all.Where(k => k.ToString().StartsWith(prefix)))
                {
                    var glyph = new PixelGlyph(kind, size);
                    if (withLabels) { glyph.ShowLabel(); plays.Add(glyph); }
                    grid.Add(VStack().AlignItems(ItemAlign.Start).Gap(8.px()).Children(
                        glyph,
                        TextBlock(Words(kind.ToString().Substring(prefix.Length))).XSmall().Secondary()));
                }
                return grid;
            }

            // A custom glyph: 16 palette keys in reading order.
            var escape = new[]
            {
                PixelGlyph.Ink, PixelGlyph.Ink, PixelGlyph.Empty, PixelGlyph.Signal,
                PixelGlyph.Ink, PixelGlyph.Ink, PixelGlyph.Ink,   PixelGlyph.Empty,
                PixelGlyph.Ink, PixelGlyph.Ink, PixelGlyph.Ink,   PixelGlyph.Empty,
                PixelGlyph.Empty, PixelGlyph.Empty, PixelGlyph.Empty, PixelGlyph.Empty,
            };

            var tones = new[] { 1, 2, 1, 3, 1, 1, 3, 2, 1, 2, 1, 1, 3, 2, 1, 4 };

            var warm = new Dictionary<int, string> { [PixelGlyph.Ink] = "#0029E7", [PixelGlyph.Slate] = "#2F6BFF", [PixelGlyph.Ash] = "#A9BEFF", [PixelGlyph.Signal] = "#111418" };

            _content = SectionStack().Secondary()
               .SampleTitle(typeof(PixelGlyphSample), UIcons.SquareDashed, "The Curiosity website's 4 by 4 pixel glyphs")
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                        TextBlock("PixelGlyph draws sixteen squares on the brand's 24 unit grid, each a palette tone, and plays a short animation as frames. Every glyph on the website is in PixelGlyphKind: the value glyphs, the use cases, the Studio capabilities, the integration categories, the landing page's three problem stories and the 54 loops on the developer pages."),
                        TextBlock("An animation is a string: frames separated by spaces, each 16 cells in reading order (optionally split into rows by /), then an optional :ms hold and :label caption. A cell is a palette key - a digit, or the website's letters: . empty, k ink, s slate, a ash, * Signal, o hollow, d deep blue, t stone. The last frame is the glyph at rest; with animation off or reduced motion that is what shows. Loops play only while on screen and hold their rest frame 1.7 seconds between runs; plays run once, when they come into view."))).SetTitle("Overview")))
               .FlatSection(Stack().Children(Card(Family("Value", 48)).SetTitle("Value glyphs")))
               .FlatSection(Stack().Children(Card(Family("UseCase", 48)).SetTitle("Use cases")))
               .FlatSection(Stack().Children(Card(Family("Capability", 48)).SetTitle("Studio capabilities")))
               .FlatSection(Stack().Children(Card(Family("Category", 40)).SetTitle("Integration categories")))
               .FlatSection(Stack().Children(Card(VStack().WS().Gap(12.px()).Children(
                    TextBlock("The landing page's problem stories play once as they come into view, captioned, with a hairline on every empty cell so the box stays visible. Replay restarts them.").Small(),
                    Family("Play", 96, withLabels: true),
                    Button("Replay").SetIcon(UIcons.Refresh).OnClick(() => plays.ForEach(g => g.Replay())))).SetTitle("Problem stories")))
               .FlatSection(Stack().Children(Card(Family("Loop", 40)).SetTitle("Developer loops")))
               .FlatSection(Stack().Children(Card(VStack().WS().Gap(16.px()).Children(
                    TextBlock("PixelGlyph.Custom(cells) takes 16 palette keys and builds itself in: a diagonal sweep lays each cell down behind an ash leading edge, and the Signal cells land last.").Small(),
                    HStack().Gap(32.px()).Wrap().Children(
                        VStack().Gap(8.px()).Children(PixelGlyph.Custom(escape, 72), TextBlock("The Escape mark, 4 by 4").XSmall().Secondary()),
                        VStack().Gap(8.px()).Children(PixelGlyph.Custom(tones, 72), TextBlock("Tones and the Signal").XSmall().Secondary()),
                        VStack().Gap(8.px()).Children(PixelGlyph.Custom(tones, 72).Palette(warm), TextBlock(".Palette(...) - blues, ink as the accent").XSmall().Secondary()),
                        VStack().Gap(8.px()).Children(PixelGlyph.Custom(tones, 72).NoAnimation(), TextBlock(".NoAnimation() - the rest frame").XSmall().Secondary()),
                        VStack().Gap(8.px()).Children(
                            PixelGlyph.FromAnimation("*.../..../..../....:90 .*../..../..../....:90 ..*./..../..../....:90 ...*/..../..../....:90 ..../...*/..../....:90 ..../..../...*/....:90 ..../..../..../...*:90 ..../..../..../..*.:90 ..../..../..../.*..:90 ..../..../..../*...:90 ..../..../*.../....:90 ..../*.../..../....:90", PixelGlyphPlayback.Loop, 72).RestHold(90).ShowEmptyCells(),
                            TextBlock("FromAnimation(...) - a pixel walking the edge").XSmall().Secondary()))
                    )).SetTitle("Custom glyphs")))
               .SeeAlso(typeof(DashMosaicSample), typeof(FlowFieldSample), typeof(PixelIntroSample));
        }

        // "TechnicalCustomerSupport" -> "Technical Customer Support", so a long name wraps inside its cell.
        private static string Words(string pascal)
        {
            var sb = new System.Text.StringBuilder();
            for (int i = 0; i < pascal.Length; i++)
            {
                var upper = char.IsUpper(pascal[i]);
                var startsWord = !char.IsUpper(pascal[i - (i > 0 ? 1 : 0)]) || (i + 1 < pascal.Length && char.IsLower(pascal[i + 1]));
                if (i > 0 && upper && startsWord) sb.Append(' ');
                sb.Append(pascal[i]);
            }
            return sb.ToString();
        }

        public HTMLElement Render() => _content.Render();
    }
}
