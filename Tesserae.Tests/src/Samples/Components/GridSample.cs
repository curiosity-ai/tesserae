using System;
using System.Linq;
using static H5.Core.dom;
using static Tesserae.Tests.Samples.SamplesHelper;
using static Tesserae.UI;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Components", Order = 10, Icon = UIcons.Grid)]
    public class GridSample : IComponent, ISample
    {
        private readonly IComponent _content;
        public GridSample()
        {
            var grid = Grid(columns: new[] { 1.fr(), 1.fr(), 200.px() });
            grid.Gap(8.px());
            grid.Add(Button().SetText("Stretched Item").WS().Primary().GridColumnStretch().GridRow(1, 2));
            Enumerable.Range(1, 10).ForEach(v => grid.Add(Button().SetText($"Item {v}")));

            var gridAutoSize = Grid(new UnitSize("repeat(auto-fit, minmax(min(200px, 100%), 1fr))"));
            gridAutoSize.Gap(8.px());
            Enumerable.Range(1, 10).ForEach(v => gridAutoSize.Add(Card(TextBlock($"Responsive Item {v}").TextCenter())));

            _content = SectionStack()
               .Title(SampleHeader(nameof(GridSample)))
               .Section(VStack().Children(
                    SampleTitle("Overview"),
                    TextBlock("The Grid component provides a powerful layout system based on CSS Grid. It allows you to define columns, rows, and gaps between items."),
                    TextBlock("Items within a Grid can be explicitly positioned or stretched across multiple tracks, offering full control over complex 2D layouts.")))
               .Section(VStack().Children(
                    SampleTitle("Best Practices"),
                    TextBlock("Use Grid for page-level layouts or complex component structures where both rows and columns need coordination. For simple one-dimensional layouts (horizontal or vertical), consider using HStack or VStack instead. Leverage 'fr' units for flexible columns that fill available space proportionally. Use 'auto-fit' or 'auto-fill' with 'minmax' to create responsive grids that adapt to different screen sizes without media queries.")))
               .Section(VStack().Children(
                    SampleTitle("Usage"),
                    SampleSubTitle("Fixed and Flexible Columns"),
                    TextBlock("This grid uses two flexible columns (1fr) and one fixed column (200px). The first item is stretched across all columns."),
                    grid,
                    SampleSubTitle("Responsive Auto-fit Grid"),
                    TextBlock("This grid automatically adjusts the number of columns based on the available width (min 200px per item)."),
                    gridAutoSize
                ));
        }

        public HTMLElement Render() => _content.Render();
    }
}
