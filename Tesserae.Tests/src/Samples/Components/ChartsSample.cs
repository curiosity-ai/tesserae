using System;
using static H5.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Components", Order = 208, Icon = UIcons.ChartHistogram)]
    public class ChartsSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public ChartsSample()
        {
            var months = new[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun" };

            var revenue = new double[] { 12, 19, 15, 27, 24, 33 };
            var costs   = new double[] { 8, 11, 13, 14, 18, 20 };

            // An observable series so the chart re-renders when the data changes.
            var liveData = new SettableObservable<double[]>(new double[] { 5, 8, 6, 12, 9, 15 });

            var lineChart = LineChart()
               .Series(new ChartSeries("Revenue", revenue), new ChartSeries("Costs", costs))
               .XAxis(months)
               .Legend()
               .Title("Monthly revenue versus costs");

            var barChart = BarChart()
               .Series(new ChartSeries("Revenue", revenue), new ChartSeries("Costs", costs))
               .XAxis(months)
               .Legend()
               .Rounded(3);

            var areaChart = AreaChart()
               .Series(liveData, "Sessions")
               .XAxis(months);

            var pieChart = PieChart()
               .Data(new double[] { 42, 27, 18, 13 })
               .Labels("Direct", "Search", "Social", "Referral")
               .Donut()
               .Legend();

            _content = SectionStack().Secondary()
               .SampleTitle(typeof(ChartsSample), UIcons.ChartHistogram, "Lightweight, dependency-free SVG charts")
               .Section(Stack().Children(
                    Card(VStack().WS().Children(
                        TextBlock("LineChart, BarChart, AreaChart and PieChart render as responsive, dependency-free SVG that scales to its container, adapts to the light/dark theme, and exposes hover tooltips plus a role=\"img\" accessibility summary. Data can be supplied as plain values or as an observable that re-renders the chart on change."))).SetTitle("Overview")))
               .Section(Stack().Children(
                    Card(VStack().WS().Children(
                        SampleSubTitle("Line chart"),
                        lineChart.H(280).WS())).SetTitle("LineChart")))
               .Section(Stack().Children(
                    Card(VStack().WS().Children(
                        SampleSubTitle("Bar chart (grouped series)"),
                        barChart.H(280).WS())).SetTitle("BarChart")))
               .Section(Stack().Children(
                    Card(VStack().WS().Children(
                        SampleSubTitle("Area chart bound to an observable"),
                        areaChart.H(280).WS(),
                        HStack().WS().Children(
                            Button("Randomize data").SetIcon(UIcons.Dice).OnClick(() =>
                            {
                                var rnd = new Random();
                                var next = new double[6];
                                for (var i = 0; i < next.Length; i++) next[i] = rnd.Next(2, 30);
                                liveData.Value = next;
                            })))).SetTitle("AreaChart + observable")))
               .Section(Stack().Children(
                    Card(VStack().WS().Children(
                        SampleSubTitle("Donut chart"),
                        pieChart.H(280).WS())).SetTitle("PieChart")));
        }

        public HTMLElement Render() => _content.Render();
    }
}
