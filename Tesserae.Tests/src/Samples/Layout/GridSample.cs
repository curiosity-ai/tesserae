using System;
using System.Linq;
using static Transpose.Core.dom;
using static Tesserae.Tests.Samples.SamplesHelper;
using static Tesserae.UI;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = SampleGroup.Layout, Order = 20, Icon = UIcons.Grid, Description = "CSS Grid with explicit rows and columns")]
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

            Grid section(int group)
            {
                var s = Grid(new UnitSize("repeat(auto-fit, minmax(min(160px, 100%), 1fr))")).Gap(8.px()).WS();
                Enumerable.Range(1, 12).ForEach(v => s.Add(Button().SetText($"Filter {group}.{v}").WS()));
                return s;
            }

            var scrollingSections = VStack().WS().H(220).ScrollY().Children(
                TextBlock("Group A").SemiBold().PB(8), section(1),
                TextBlock("Group B").SemiBold().PT(16).PB(8), section(2),
                TextBlock("Group C").SemiBold().PT(16).PB(8), section(3));

            _content = SectionStack().Secondary()
               .SampleTitle(typeof(GridSample), UIcons.Table, "A component to display a grid")
               .FlatSection(VStack().Children(
                    Card(VStack().WS().Children(
                    TextBlock("The Grid component provides a powerful layout system based on CSS Grid. It allows you to define columns, rows, and gaps between items."),
                    TextBlock("Items within a Grid can be explicitly positioned or stretched across multiple tracks, offering full control over complex 2D layouts."))).SetTitle("Overview")))
               .FlatSection(VStack().Children(
                    Card(VStack().WS().Children(
                    TextBlock("Use Grid for page-level layouts or complex component structures where both rows and columns need coordination. For simple one-dimensional layouts (horizontal or vertical), consider using HStack or VStack instead. Leverage 'fr' units for flexible columns that fill available space proportionally. Use 'auto-fit' or 'auto-fill' with 'minmax' to create responsive grids that adapt to different screen sizes without media queries."))).SetTitle("Best Practices")))
               .FlatSection(VStack().Children(
                    Card(VStack().WS().Children(
                    SampleSubTitle("Fixed and Flexible Columns"),
                    TextBlock("This grid uses two flexible columns (1fr) and one fixed column (200px). The first item is stretched across all columns."),
                    grid,
                    SampleSubTitle("Responsive Auto-fit Grid"),
                    TextBlock("This grid automatically adjusts the number of columns based on the available width (min 200px per item)."),
                    gridAutoSize,
                    SampleSubTitle("Sections in a Scrolling Stack"),
                    TextBlock("Each grid keeps its whole content height inside a stack that is too short for all of them: the stack scrolls, the sections don't shrink."),
                    scrollingSections
                )).SetTitle("Usage")))
               .SeeAlso(typeof(StackSample), typeof(SplitViewSample), typeof(MasonrySample), typeof(FloatSample), typeof(SectionStackSample));
        }

        public HTMLElement Render() => _content.Render();
    }
}
