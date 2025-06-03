using System;
using System.Linq;
using static H5.Core.dom;
using static Tesserae.Tests.Samples.SamplesHelper;
using static Tesserae.UI;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Collections", Order = 10, Icon = UIcons.Grid)]
    public class GridSample : IComponent, ISample
    {
        private readonly IComponent _content;
        public GridSample()
        {
            var grid = Grid(columns: new[] { 1.fr(), 1.fr(), 200.px() });
            grid.Gap(6.px());
            grid.Add(Button().SetText("this").WS().Primary().GridColumnStretch().GridRow(1, 2));

            Enumerable.Range(1, 12).ForEach(v => grid.Add(Button().SetText($"Button {v}")));


            var gridAutoSize = Grid(new UnitSize("repeat(auto-fit, minmax(min(200px, 100%), 1fr))"));

            Enumerable.Range(1, 12).ForEach(v => gridAutoSize.Add(Button().WS().TextCenter().SetText($"Button {v}")));


            _content = SectionStack()
               .Title(SampleHeader(nameof(GridSample)))
               .Section(VStack().Children(
                    SampleTitle("Overview"),
                    TextBlock("This component let you add items to a grid")))
               .Section(VStack().Children(
                    SampleTitle("Usage"),
                    TextBlock("Simple Grid").Medium().PB(16), grid,
                    TextBlock("Auto Sizing Grid").Medium().PB(16).PT(32), gridAutoSize));
        }

        public HTMLElement Render() => _content.Render();
    }
}