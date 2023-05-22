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
            grid.Gap(6.px());
            var btn = Button().SetText("this").WS().Primary();
            btn.GridColumnStretch();
            btn.GridRow(1, 2);
            grid.Add(btn);
            grid.Add(Button().SetText("other"));
            grid.Add(Button().SetText("other"));
            grid.Add(Button().SetText("other"));
            grid.Add(Button().SetText("other"));
            grid.Add(Button().SetText("other"));
            grid.Add(Button().SetText("other"));
            grid.Add(Button().SetText("other"));
            grid.Add(Button().SetText("other"));
            grid.Add(Button().SetText("other"));
            grid.Add(Button().SetText("other"));
            grid.Add(Button().SetText("other"));
            grid.Add(Button().SetText("other"));
            grid.Add(Button().SetText("other"));


            _content = SectionStack()
               .Title(SampleHeader(nameof(GridSample)))
               .Section(Stack().Children(
                    SampleTitle("Overview"),
                    TextBlock("This component let you add items to a grid")))
               .Section(Stack().Children(
                    SampleTitle("Usage"),
                    HorizontalSeparator("Daytime Example").Left(),
                    grid));
        }

        public HTMLElement Render() => _content.Render();
    }
}