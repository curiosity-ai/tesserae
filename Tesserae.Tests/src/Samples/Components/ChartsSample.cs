using System;
using static Transpose.Core.dom;
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

            // Seeded, and outside the click handler: every click still shows different numbers, but
            // the Nth click of a given session always shows the same ones.
            var randomizer = new SampleRandom(3_101);

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

            var stackedChart = BarChart()
               .Series(new ChartSeries("Chat", new double[] { 120, 180, 150, 260, 240, 310 }),
                       new ChartSeries("Search", new double[] { 80, 95, 130, 140, 175, 205 }),
                       new ChartSeries("Indexing", new double[] { 40, 35, 60, 55, 70, 90 }))
               .XAxis(months)
               .Stacked()
               .Legend(ChartLegendPosition.Bottom)
               .Rounded(1);

            var areaChart = AreaChart()
               .Series(liveData, "Sessions")
               .XAxis(months);

            var pieChart = PieChart()
               .Data(new double[] { 42, 27, 18, 13 })
               .Labels("Direct", "Search", "Social", "Referral")
               .Donut()
               .Legend();

            var gapsChart = LineChart()
               .Series("With a dropout", new double[] { 14, 17, double.NaN, double.NaN, 21, 19, 24 })
               .XAxis("Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun")
               .ConnectGaps(false)
               .Legend();

            _content = SectionStack().Secondary()
               .SampleTitle(typeof(ChartsSample), UIcons.ChartHistogram, "Lightweight, dependency-free SVG charts")
               .Section(Stack().Children(
                    Card(VStack().WS().Children(
                        TextBlock("LineChart, BarChart, AreaChart and PieChart render as responsive, dependency-free SVG that scales to its container, adapts to the light/dark theme, and exposes hover tooltips plus a role=\"img\" accessibility summary. Data can be supplied as plain values, as an observable that re-renders the chart on change, or as (x, y) pairs on a continuous scale that supports zoom, pan and a spikeline readout."))).SetTitle("Overview")))
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
                        SampleSubTitle("Stacked bars with the legend below"),
                        stackedChart.H(300).WS())).SetTitle("BarChart (stacked)")))
               .Section(Stack().Children(
                    Card(VStack().WS().Children(
                        SampleSubTitle("Area chart bound to an observable"),
                        areaChart.H(280).WS(),
                        HStack().WS().Children(
                            Button("Randomize data").SetIcon(UIcons.Dice).OnClick(() =>
                            {
                                var next = new double[6];
                                for (var i = 0; i < next.Length; i++) next[i] = randomizer.Next(2, 30);
                                liveData.Value = next;
                            })))).SetTitle("AreaChart + observable")))
               .Section(Stack().Children(
                    Card(VStack().WS().Children(
                        SampleSubTitle("Donut chart"),
                        pieChart.H(280).WS())).SetTitle("PieChart")))
               .Section(Stack().Children(
                    Card(VStack().WS().Children(
                        SampleSubTitle("Missing samples: NaN breaks the line when gaps are not connected"),
                        gapsChart.H(260).WS())).SetTitle("Gaps")))
               .Section(Stack().Children(
                    Card(BuildTimeSeriesSection()).SetTitle("Time series: continuous X, zoom, spikelines")))
               .SeeAlso(typeof(SparklineSample), typeof(MetricSample), typeof(ContributionBarSample), typeof(DeltaComponentSample), typeof(TimeHistogramPickerSample));
        }

        // Two charts sharing one timeline: zooming or panning either one pushes its range onto the other,
        // which is what OnRangeChanged + XRange are for (XRange itself never re-raises the event).
        private static IComponent BuildTimeSeriesSection()
        {
            // Anchored to SampleDate rather than "two hours ago": the axis labels are rendered text,
            // so a clock-driven start makes this page differ from itself every minute.
            var start = SampleDate.UnixSeconds(SampleDate.Now);

            var rnd = new SampleRandom(7_720);

            var times = new double[120];
            var cpu   = new double[120];
            var ram   = new double[120];

            double cpuValue = 35;
            double ramValue = 1_400_000_000; // raw bytes, as a real metric reports them

            for (var i = 0; i < times.Length; i++)
            {
                times[i] = start + i * 60;

                cpuValue = Math.Max(2, Math.Min(98, cpuValue + rnd.Next(-6, 7)));
                ramValue = Math.Max(600_000_000, ramValue + rnd.Next(-40, 55) * 1_000_000);

                cpu[i] = cpuValue;
                ram[i] = ramValue;
            }

            var cpuChart = AreaChart()
               .Series(new ChartSeries("CPU %", times, cpu) { LineWidth = 1, FillOpacity = 0.2 })
               .XAxisTime()
               .FormatValues(v => v.ToString("0") + "%")
               .Zoomable()
               .Spikelines()
               .ExportButton(fileName: "cpu")
               .Legend();

            var ramChart = LineChart()
               .Series(new ChartSeries("Working set", times, ram) { LineWidth = 1 })
               .XAxisTime()
               .Points(false)
               .Zoomable()
               .Spikelines()
               .ExportButton(fileName: "working-set")
               .Legend();

            cpuChart.OnRangeChanged(range =>
            {
                if (range.IsAutoRange) ramChart.AutoRangeX();
                else ramChart.XRange(range.Min, range.Max);
            });

            ramChart.OnRangeChanged(range =>
            {
                if (range.IsAutoRange) cpuChart.AutoRangeX();
                else cpuChart.XRange(range.Min, range.Max);
            });

            return VStack().WS().Children(
                SampleSubTitle("Wheel to zoom, drag to pan, double-click to reset — both charts stay on the same timeline"),
                cpuChart.H(200).WS(),
                ramChart.H(200).WS().PT(8));
        }

        public HTMLElement Render() => _content.Render();
    }
}
