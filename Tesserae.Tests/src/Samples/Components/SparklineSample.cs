using static H5.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Components", Order = 206, Icon = UIcons.ChartLineUp)]
    public class SparklineSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public SparklineSample()
        {
            _content = SectionStack().Secondary()
               .SampleTitle(typeof(SparklineSample), UIcons.ChartLineUp, "A compact inline trend chart")
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    TextBlock("Sparkline renders a compact SVG area chart designed to be embedded alongside metric values, table cells, or dashboard cards. It conveys the shape of a trend at a glance without axes, labels, or interactive controls."),
                    TextBlock("The chart accepts a data array of doubles and optional width, height, and color parameters, making it easy to match any layout or brand color."))).SetTitle("Overview")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    TextBlock("Pair a Sparkline with a headline metric value so users can interpret the trend in context. Keep the data array between 10 and 50 points — fewer points produce sharp angular charts, while more than 50 can add unnecessary rendering overhead in dense dashboards. Choose colors that contrast with the card background; use semantic colors (green for growth, red for decline) when the trend direction carries meaning."))).SetTitle("Best Practices")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    SampleSubTitle("Upward Trend"),
                    HStack().AlignItemsCenter().Children(
                        TextBlock("Revenue").W(120.px()),
                        Sparkline(new double[] { 10, 14, 18, 15, 22, 28, 25, 35, 40, 50 }, width: 150, height: 40, color: Theme.Colors.Green600)
                    ),
                    SampleSubTitle("Downward Trend"),
                    HStack().AlignItemsCenter().Children(
                        TextBlock("Error Rate").W(120.px()),
                        Sparkline(new double[] { 50, 45, 40, 48, 30, 28, 20, 15, 12, 10 }, width: 150, height: 40, color: "var(--tss-danger-background-color)")
                    ),
                    SampleSubTitle("Flat / Stable"),
                    HStack().AlignItemsCenter().Children(
                        TextBlock("Latency (ms)").W(120.px()),
                        Sparkline(new double[] { 30, 31, 29, 30, 32, 30, 31, 29, 30, 31 }, width: 150, height: 40, color: Theme.Colors.Neutral500)
                    ),
                    SampleSubTitle("Volatile / Random"),
                    HStack().AlignItemsCenter().Children(
                        TextBlock("Requests").W(120.px()),
                        Sparkline(new double[] { 10, 40, 15, 55, 20, 60, 5, 45, 30, 70, 25, 50 }, width: 150, height: 40, color: Theme.Colors.Blue600)
                    ),
                    SampleSubTitle("Primary Color (default)"),
                    HStack().AlignItemsCenter().Children(
                        TextBlock("Conversions").W(120.px()),
                        Sparkline(new double[] { 10, 20, 15, 30, 25, 40, 35, 50 }, width: 150, height: 40)
                    ),
                    SampleSubTitle("Larger Chart"),
                    Sparkline(new double[] { 5, 10, 8, 20, 18, 35, 30, 45, 42, 60, 55, 70 }, width: 300, height: 80, color: Theme.Colors.Green600).WS()
                )).SetTitle("Usage")));
        }

        public HTMLElement Render() => _content.Render();
    }
}
