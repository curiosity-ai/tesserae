using System;
using System.Linq;
using Tesserae.Themes.Curiosity;
using static Tesserae.UI;
using static Transpose.Core.dom;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = SampleGroup.Curiosity, Order = 40, Icon = UIcons.Barcode, Description = "The website's animated dash tiles, in four schemes")]
    public class DashMosaicSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public DashMosaicSample()
        {
            IComponent Tile(DashMosaic mosaic, string caption, int height = 180) =>
                VStack().WS().Gap(8.px()).Children(mosaic.H(height), TextBlock(caption).XSmall().Secondary());

            var patternGrid = Grid(new UnitSize("repeat(auto-fill, minmax(220px, 1fr))")).WS().Gap(12.px());
            foreach (var pattern in Enum.GetValues(typeof(DashMosaicPattern)).Cast<DashMosaicPattern>())
            {
                patternGrid.Add(Tile(new DashMosaic(DashMosaicScheme.Dark, pattern), pattern.ToString(), 140));
            }

            _content = SectionStack().Secondary()
               .SampleTitle(typeof(DashMosaicSample), UIcons.Barcode, "The Curiosity website's animated dash tiles")
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                        TextBlock("DashMosaic is a grid of short vertical bars in which a shape is made by which bars are lit: a scanning row, a wave, a line chart, a skyline, a slanted band, a drifting patch. The website draws its industry tiles and its Studio and Developers panels with it."),
                        TextBlock("It fills its own box with the website's spacing (bars 3 by 9px, 11px across and 17px down, smaller under 260px wide, at least 6px clear of every edge) and redraws when the box changes size. It animates at 20 frames a second only while on screen; NoAnimation() and reduced motion draw it once."))).SetTitle("Overview")))
               .FlatSection(Stack().Children(
                    Card(Grid(new UnitSize("repeat(auto-fill, minmax(220px, 1fr))")).WS().Gap(12.px()).Children(
                        Tile(new DashMosaic(DashMosaicScheme.LightBlue), "LightBlue - Wave"),
                        Tile(new DashMosaic(DashMosaicScheme.Dark, DashMosaicPattern.Mountain), "Dark - Mountain"),
                        Tile(new DashMosaic(DashMosaicScheme.Light), "Light - Chart"),
                        Tile(new DashMosaic(DashMosaicScheme.DarkBlue, DashMosaicPattern.Rows), "DarkBlue - Rows"),
                        Tile(new DashMosaic(DashMosaicScheme.Light, DashMosaicPattern.Band), "Light - Band")
                    )).SetTitle("The industry tiles")))
               .FlatSection(Stack().Children(
                    Card(Grid(new UnitSize("repeat(auto-fill, minmax(320px, 1fr))")).WS().Gap(12.px()).Children(
                        Tile(new DashMosaic(DashMosaicScheme.Dark), "new DashMosaic(DashMosaicScheme.Dark) - the Studio panel", 200),
                        Tile(new DashMosaic(DashMosaicScheme.DarkBlue), "new DashMosaic(DashMosaicScheme.DarkBlue) - the Developers panel", 200)
                    )).SetTitle("The panels")))
               .FlatSection(Stack().Children(Card(patternGrid).SetTitle("Every pattern")))
               .FlatSection(Stack().Children(
                    Card(Grid(new UnitSize("repeat(auto-fill, minmax(220px, 1fr))")).WS().Gap(12.px()).Children(
                        Tile(new DashMosaic(DashMosaicScheme.Dark, DashMosaicPattern.Chart).NoAnimation(), ".NoAnimation()", 140),
                        Tile(new DashMosaic(DashMosaicScheme.Light, DashMosaicPattern.ChartGrid).Seed(42), ".Seed(42) - another line", 140),
                        Tile(new DashMosaic(DashMosaicScheme.LightBlue, DashMosaicPattern.Dither).EdgePadding(24), ".EdgePadding(24)", 140),
                        Tile(new DashMosaic(DashMosaicScheme.DarkBlue, DashMosaicPattern.Wave).FramesPerSecond(60), ".FramesPerSecond(60)", 140)
                    )).SetTitle("Options")))
               .SeeAlso(typeof(FlowFieldSample), typeof(PixelGlyphSample), typeof(PixelIntroSample));
        }

        public HTMLElement Render() => _content.Render();
    }
}
