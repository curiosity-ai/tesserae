using System;
using System.Collections.Generic;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;
using static Transpose.Core.dom;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = SampleGroup.Charts, Order = 15, Icon = UIcons.ChartScatterBubble, Description = "Microsoft Flint charts from a spec, with clicks")]
    public class FlintChartSample : IComponent, ISample
    {
        private static readonly string[] Months  = { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
        private static readonly string[] Regions = { "North", "South", "West" };
        private static readonly string[] Origins = { "Europe", "Japan", "USA" };

        private readonly IComponent _content;

        public FlintChartSample()
        {
            var random = new SampleRandom(15_015);
            var cars   = Cars(random);
            var year   = SampleDate.Today.Year;

            _content = SectionStack().Secondary()
               .SampleTitle(typeof(FlintChartSample), UIcons.ChartScatterBubble, "Microsoft Flint charts from a spec, with clicks")
               .Section(Stack().Children(
                    Card(VStack().WS().Children(
                        TextBlock("FlintChart draws a chart with Microsoft's Flint (Vega-Lite underneath) from the same spec a ```flint fence in MarkdownBlock holds: the data, what its fields mean (semantic types), and which field goes on which channel of which chart type. Flint picks the scales, axes, formats and layout; the theme picks the look, and the default theme reads the page's Tesserae tokens, so every chart here follows light/dark mode and the gallery's theme. The spec can be given as JSON or built with the fluent methods, and OnElementClick reports a click on a mark, a series, a key entry or an axis label with the rows it stands for."))).SetTitle("Overview")))
               .Section(Stack().Children(Card(ClickDemo()).SetTitle("Clicks")))
               .Section(Stack().Children(Card(SpecDemo()).SetTitle("From a Flint spec (JSON)")))
               .Section(Stack().Children(Card(InteractionsDemo()).SetTitle("Interactions")))
               .Section(Stack().Children(Card(ErrorDemo()).SetTitle("A spec that cannot be drawn")))
               .Section(Stack().Children(Card(Gallery(
                    Cell(FlintChartType.ScatterPlot, "Weight against fuel economy", Rows(FlintChart(FlintChartType.ScatterPlot), CarColumns, cars)
                       .SemanticType("Weight", FlintSemanticType.Quantity).SemanticType("MPG", FlintSemanticType.Quantity).SemanticType("Horsepower", FlintSemanticType.Quantity)
                       .X("Weight").Y("MPG").ColorBy("Origin").Encode(FlintChannel.Size, "Horsepower")),
                    Cell(FlintChartType.MarginalPlot, "With each axis' distribution", Rows(FlintChart(FlintChartType.MarginalPlot), CarColumns, cars)
                       .X("Weight").Y("MPG").ColorBy("Origin")),
                    Cell(FlintChartType.Regression, "With a fitted trend", Rows(FlintChart(FlintChartType.Regression), CarColumns, cars)
                       .X("Weight").Y("MPG")),
                    Cell(FlintChartType.ConnectedScatterPlot, "Unemployment against inflation, by year", ConnectedScatter()),
                    Cell(FlintChartType.RangedDotPlot, "Scores before and after", RangedDots()),
                    Cell(FlintChartType.StripPlot, "Every car's economy, by origin", Rows(FlintChart(FlintChartType.StripPlot), CarColumns, cars)
                       .X("MPG").Y("Origin").ColorBy("Origin"))
                )).SetTitle("Points")))
               .Section(Stack().Children(Card(Gallery(
                    Cell(FlintChartType.BarChart, "Sales per product", ProductSales(FlintChart(FlintChartType.BarChart)).X("Product").Y("Sales")),
                    Cell(FlintChartType.GroupedBarChart, "Sales per quarter, year beside year", QuarterSales(FlintChartType.GroupedBarChart).Encode(FlintChannel.Group, "Year")),
                    Cell(FlintChartType.StackedBarChart, "Requests per month, by channel", ChannelRequests(FlintChartType.StackedBarChart).X("Month").Y("Requests").ColorBy("Channel")),
                    Cell(FlintChartType.LollipopChart, "Satisfaction per country", Lollipop()),
                    Cell(FlintChartType.WaterfallChart, "From opening to closing balance", Waterfall()),
                    Cell(FlintChartType.GanttChart, "Release plan", Gantt(year)),
                    Cell(FlintChartType.BulletChart, "Sales against quota", Bullet())
                )).SetTitle("Bars")))
               .Section(Stack().Children(Card(Gallery(
                    Cell(FlintChartType.Histogram, "Fuel economy", Rows(FlintChart(FlintChartType.Histogram), CarColumns, cars).X("MPG")),
                    Cell(FlintChartType.DensityPlot, "Fuel economy, by origin", Rows(FlintChart(FlintChartType.DensityPlot), CarColumns, cars).X("MPG").ColorBy("Origin")),
                    Cell(FlintChartType.RidgelinePlot, "Daily temperatures, by month", Ridgeline(random)),
                    Cell(FlintChartType.EcdfPlot, "Share of cars at or below an economy", Rows(FlintChart(FlintChartType.EcdfPlot), CarColumns, cars).X("MPG").ColorBy("Origin")),
                    Cell(FlintChartType.ViolinPlot, "Fuel economy, by origin", Rows(FlintChart(FlintChartType.ViolinPlot), CarColumns, cars).X("Origin").Y("MPG")),
                    Cell(FlintChartType.Boxplot, "Fuel economy, by origin", Rows(FlintChart(FlintChartType.Boxplot), CarColumns, cars).X("Origin").Y("MPG")),
                    Cell(FlintChartType.PyramidChart, "Population by age and sex", Pyramid()),
                    Cell(FlintChartType.CandlestickChart, "A month of a share price", Candlestick(random, year))
                )).SetTitle("Distributions")))
               .Section(Stack().Children(Card(Gallery(
                    Cell(FlintChartType.LineChart, "Revenue per month, by region", RegionRevenue(FlintChartType.LineChart).X("Month").Y("Revenue").ColorBy("Region")),
                    Cell(FlintChartType.Sparkline, "Visitors over a month", Sparkline(random)),
                    Cell(FlintChartType.BumpChart, "League position per week", Bump()),
                    Cell(FlintChartType.SlopeChart, "Market share, then and now", Slope()),
                    Cell(FlintChartType.AreaChart, "Revenue per month, by region", RegionRevenue(FlintChartType.AreaChart).X("Month").Y("Revenue").ColorBy("Region")),
                    Cell(FlintChartType.Streamgraph, "Requests per month, by channel", ChannelRequests(FlintChartType.Streamgraph).X("Month").Y("Requests").ColorBy("Channel")),
                    Cell(FlintChartType.RangeAreaChart, "Monthly low and high temperature", RangeArea())
                )).SetTitle("Lines & areas")))
               .Section(Stack().Children(Card(Gallery(
                    Cell(FlintChartType.PieChart, "Where visitors come from", TrafficShare(FlintChartType.PieChart)),
                    Cell(FlintChartType.DonutChart, "Where visitors come from", TrafficShare(FlintChartType.DonutChart)),
                    Cell(FlintChartType.RoseChart, "Rainfall per month", Rose()),
                    Cell(FlintChartType.RadarChart, "Two players' skills", Radar())
                )).SetTitle("Circular")))
               .Section(Stack().Children(Card(Gallery(
                    Cell(FlintChartType.Heatmap, "Commits per weekday and hour", Heatmap(random)),
                    Cell(FlintChartType.CalendarHeatmap, "Commits per day", Calendar(random, year)).GridColumnStretch(),
                    Cell(FlintChartType.BarTable, "Contribution per category", BarTable()),
                    Cell(FlintChartType.KpiCard, "This quarter against its goals", Kpis()),
                    Cell(FlintChartType.Table, "The rows themselves", ProductSales(FlintChart(FlintChartType.Table))),
                    Cell(FlintChartType.Map, "Largest US cities (base map from vega.github.io)", Cities()),
                    Cell(FlintChartType.Choropleth, "A measure per state (base map from vega.github.io)", States())
                )).SetTitle("Tables & maps")))
               .SeeAlso(typeof(MarkdownBlockSample), typeof(ChartsSample));
        }

        public HTMLElement Render() => _content.Render();

        // ---------------------------------------------------------------- demos

        private static IComponent ClickDemo()
        {
            var output = TextBlock("Click a bar, an axis label, a point, a line or a key entry.").Small().Id("flint-click-report");

            Action<FlintChartClick> report = click =>
            {
                var what = click.Target + (click.Field != null ? $" ({click.Field} = {click.Value})" : "");
                var rows = click.Rows.Count == 1 ? click.Row.ToJson() : $"{click.Rows.Count} rows, the first {click.Row?.ToJson()}";
                output.Text = $"{what}: {rows}";
            };

            var bars = ProductSales(FlintChart(FlintChartType.BarChart))
               .X("Product").Y("Sales")
               .Title("Sales per product")
               .OnElementClick(report);

            var lines = RegionRevenue(FlintChartType.LineChart)
               .X("Month").Y("Revenue").ColorBy("Region")
               .Title("Revenue per month")
               .Property("showPoints", true)
               .Interactions(FlintInteraction.ClickHighlight)
               .OnElementClick(report);

            return VStack().WS().Children(
                TextBlock("OnElementClick reports what was clicked (a mark, a series, a key entry, an axis label) and the rows of the data it stands for, as they were given. It changes nothing on the chart; the line chart adds the ClickHighlight interaction to also emphasise what was clicked. With showPoints on, a point is a mark of its own row and the line between is the whole series."),
                Grid(new UnitSize("repeat(auto-fill, minmax(340px, 1fr))")).WS().Gap(16.px()).PT(16).Children(bars, lines),
                Card(output).PT(8));
        }

        private const string SpecJson = @"{
  ""data"": { ""values"": [
    { ""quarter"": ""Q1"", ""plan"": 120, ""actual"": 108 },
    { ""quarter"": ""Q2"", ""plan"": 140, ""actual"": 151 },
    { ""quarter"": ""Q3"", ""plan"": 160, ""actual"": 149 },
    { ""quarter"": ""Q4"", ""plan"": 180, ""actual"": 197 }
  ] },
  ""semantic_types"": { ""quarter"": ""Quarter"", ""plan"": ""Amount"", ""actual"": ""Amount"" },
  ""chart_spec"": {
    ""chartType"": ""Line Chart"",
    ""title"": ""Bookings against plan"",
    ""subtitle"": ""Thousands of euros per quarter"",
    ""encodings"": { ""x"": ""quarter"", ""y"": [""plan"", ""actual""] },
    ""chartProperties"": { ""showPoints"": true }
  },
  ""options"": { ""addTooltips"": true }
}";

        private static IComponent SpecDemo()
        {
            return VStack().WS().Children(
                TextBlock("UI.FlintChart(json) takes the spec a ```flint fence holds. A list of fields on y draws one series each. The fluent methods can still adjust it afterwards: this one is switched to an area chart with .ChartType(...) when the button is pressed. It is also given a height, and FillHeight() lays it out for that height rather than choosing its own."),
                Grid(new UnitSize("repeat(auto-fill, minmax(340px, 1fr))")).WS().Gap(16.px()).PT(16).Children(
                    TextBlock(SpecJson).Style(s => { s.whiteSpace = "pre"; s.fontFamily = "var(--tss-monospace-font-family, monospace)"; s.fontSize = "12px"; s.overflow = "auto"; }),
                    SpecChart()));
        }

        private static IComponent SpecChart()
        {
            var chart  = FlintChart(SpecJson).H(340).FillHeight();
            var isArea = false;

            return VStack().WS().Children(
                chart,
                Button("Switch between line and area").OnClick(() =>
                {
                    isArea = !isArea;
                    chart.ChartType(isArea ? FlintChartType.AreaChart : FlintChartType.LineChart);
                }));
        }

        private static IComponent InteractionsDemo()
        {
            var brush = RegionRevenue(FlintChartType.LineChart)
               .X("Month").Y("Revenue").ColorBy("Region")
               .Title("Inspect and toggle")
               .Interactions(FlintInteraction.InspectIndex, FlintInteraction.LegendToggle);

            var zoom = ScatterOf(new SampleRandom(15_016))
               .Title("Drag to zoom")
               .Interactions(FlintInteraction.BrushZoom);

            return VStack().WS().Children(
                TextBlock("Interactions(...) mounts Flint's interaction presets: reading values along the x axis and hiding a series from the key on the left, zooming to a dragged rectangle on the right (double-click to reset)."),
                Grid(new UnitSize("repeat(auto-fill, minmax(340px, 1fr))")).WS().Gap(16.px()).PT(16).Children(brush, zoom));
        }

        private static IComponent ErrorDemo()
        {
            var reason = TextBlock("").Small().Secondary().Id("flint-error-report");

            var chart = ProductSales(FlintChart(FlintChartType.BarChart))
               .X("Product").Y("Revenue")
               .OnError(error => reason.Text = "OnError: " + error);

            return VStack().WS().Children(
                TextBlock("A spec Flint cannot draw (here, y names a field the data does not have) is shown as its reason in the chart's place, and reported through Error and OnError. A redraw that fails leaves the chart that was there."),
                chart.PT(16),
                reason.PT(8));
        }

        private static IComponent Gallery(params IComponent[] cells)
            => Grid(new UnitSize("repeat(auto-fill, minmax(340px, 1fr))")).WS().RowGap(24.px()).ColumnGap(16.px()).Children(cells);

        private static IComponent Cell(FlintChartType type, string caption, FlintChart chart)
        {
            return VStack().WS().Children(
                TextBlock(FlintChart.ChartTypeName(type)).SemiBold(),
                TextBlock(caption).Small().Secondary().PB(8),
                chart);
        }

        // ---------------------------------------------------------------- data

        private static readonly string[] CarColumns = { "Name", "Origin", "Weight", "MPG", "Horsepower" };

        private static List<object[]> Cars(SampleRandom random)
        {
            var cars = new List<object[]>();

            for (int i = 0; i < 60; i++)
            {
                var origin     = Origins[i % 3];
                var weight     = Math.Round(origin == "USA" ? random.NextDouble(2600, 4800) : random.NextDouble(1800, 3200));
                var mpg        = Math.Round(52 - weight / 110 + random.NextDouble(-3, 3) + (origin == "Japan" ? 3 : 0), 1);
                var horsepower = Math.Round(weight / 28 + random.NextDouble(-15, 15));
                cars.Add(new object[] { $"{origin} {i + 1}", origin, weight, mpg, horsepower });
            }

            return cars;
        }

        private static FlintChart Rows(FlintChart chart, string[] columns, List<object[]> rows) => chart.Columns(columns).Data(rows, r => r);

        private static FlintChart ScatterOf(SampleRandom random)
            => Rows(FlintChart(FlintChartType.ScatterPlot), CarColumns, Cars(random)).X("Weight").Y("MPG").ColorBy("Origin");

        private static FlintChart ProductSales(FlintChart chart)
        {
            return chart.Columns("Product", "Sales")
               .AddRow("Notebooks", 1840)
               .AddRow("Pens", 2650)
               .AddRow("Paper", 1210)
               .AddRow("Ink", 730)
               .AddRow("Folders", 980)
               .AddRow("Staplers", 410)
               .SemanticType("Product", FlintSemanticType.Category)
               .SemanticType("Sales", FlintSemanticType.Amount);
        }

        private static FlintChart QuarterSales(FlintChartType type)
        {
            var chart = FlintChart(type).Columns("Quarter", "Year", "Sales");
            var sales = new[] { new[] { 410, 460, 520, 610 }, new[] { 450, 500, 590, 680 } };

            for (int y = 0; y < 2; y++)
            {
                for (int q = 0; q < 4; q++) chart.AddRow($"Q{q + 1}", (2025 + y).ToString(), sales[y][q]);
            }

            return chart.SemanticType("Quarter", FlintSemanticType.Quarter).SemanticType("Year", FlintSemanticType.Category).SemanticType("Sales", FlintSemanticType.Amount)
               .X("Quarter").Y("Sales");
        }

        private static FlintChart ChannelRequests(FlintChartType type)
        {
            var chart    = FlintChart(type).Columns("Month", "Channel", "Requests");
            var channels = new[] { "Chat", "Search", "Indexing", "Export" };
            var base_    = new[] { 120, 80, 40, 20 };

            for (int m = 0; m < 12; m++)
            {
                for (int c = 0; c < channels.Length; c++) chart.AddRow(Months[m], channels[c], base_[c] + (int)(base_[c] * 0.6 * Math.Sin((m + c * 2) / 2.0)) + m * (4 - c) * 6);
            }

            return chart.SemanticType("Month", FlintSemanticType.Month).SemanticType("Channel", FlintSemanticType.Category).SemanticType("Requests", FlintSemanticType.Count);
        }

        private static FlintChart RegionRevenue(FlintChartType type)
        {
            var chart  = FlintChart(type).Columns("Month", "Region", "Revenue");
            var starts = new[] { 42, 30, 24 };

            for (int r = 0; r < Regions.Length; r++)
            {
                for (int m = 0; m < 12; m++) chart.AddRow(Months[m], Regions[r], starts[r] + m * (3 - r) + (int)(6 * Math.Sin(m / 1.7 + r)));
            }

            return chart.SemanticType("Month", FlintSemanticType.Month).SemanticType("Region", FlintSemanticType.Region).SemanticType("Revenue", FlintSemanticType.Amount);
        }

        private static FlintChart ConnectedScatter()
        {
            var chart = FlintChart(FlintChartType.ConnectedScatterPlot).Columns("Year", "Unemployment", "Inflation");
            var unemployment = new[] { 9.6, 8.9, 8.1, 7.4, 6.2, 5.3, 4.9, 4.4, 3.9, 3.7, 8.1, 5.4, 3.6, 3.6 };
            var inflation    = new[] { 1.6, 3.2, 2.1, 1.5, 1.6, 0.1, 1.3, 2.1, 2.4, 1.8, 1.2, 4.7, 8.0, 4.1 };

            for (int i = 0; i < unemployment.Length; i++) chart.AddRow((2010 + i).ToString(), unemployment[i], inflation[i]);

            return chart.SemanticType("Year", FlintSemanticType.Year).SemanticType("Unemployment", FlintSemanticType.Percentage).SemanticType("Inflation", FlintSemanticType.Percentage)
               .X("Unemployment").Y("Inflation").Encode(FlintChannel.Order, "Year");
        }

        private static FlintChart RangedDots()
        {
            var chart   = FlintChart(FlintChartType.RangedDotPlot).Columns("Metric", "Phase", "Score");
            var metrics = new[] { "Speed", "Accuracy", "Recall", "Coverage", "Latency" };
            var before  = new[] { 42, 61, 55, 38, 70 };
            var after   = new[] { 68, 74, 63, 66, 81 };

            for (int i = 0; i < metrics.Length; i++)
            {
                chart.AddRow(metrics[i], "Before", before[i]);
                chart.AddRow(metrics[i], "After", after[i]);
            }

            return chart.SemanticType("Score", FlintSemanticType.Score).Y("Metric").X("Score").ColorBy("Phase");
        }

        private static FlintChart Lollipop()
        {
            return FlintChart(FlintChartType.LollipopChart).Columns("Country", "Satisfaction")
               .AddRow("Denmark", 82).AddRow("Japan", 71).AddRow("Brazil", 64).AddRow("Canada", 77).AddRow("Kenya", 58).AddRow("Spain", 69)
               .SemanticType("Country", FlintSemanticType.Country).SemanticType("Satisfaction", FlintSemanticType.Score)
               .X("Country").Y("Satisfaction");
        }

        private static FlintChart Waterfall()
        {
            return FlintChart(FlintChartType.WaterfallChart).Columns("Step", "Amount", "Type")
               .AddRow("Opening", 820, "start")
               .AddRow("Sales", 340, "delta")
               .AddRow("Services", 120, "delta")
               .AddRow("Salaries", -410, "delta")
               .AddRow("Rent", -95, "delta")
               .AddRow("Tax", -130, "delta")
               .AddRow("Closing", 645, "end")
               .SemanticType("Amount", FlintSemanticType.Amount)
               .X("Step").Y("Amount").ColorBy("Type");
        }

        private static FlintChart Gantt(int year)
        {
            var start = new DateTime(year, 3, 2);

            return FlintChart(FlintChartType.GanttChart).Columns("Task", "Start", "End", "Phase")
               .AddRow("Research", start, start.AddDays(14), "Discovery")
               .AddRow("Design", start.AddDays(10), start.AddDays(30), "Discovery")
               .AddRow("Build API", start.AddDays(28), start.AddDays(56), "Delivery")
               .AddRow("Build UI", start.AddDays(35), start.AddDays(63), "Delivery")
               .AddRow("Beta", start.AddDays(63), start.AddDays(77), "Launch")
               .AddRow("Release", start.AddDays(77), start.AddDays(80), "Launch")
               .SemanticType("Task", FlintSemanticType.Category).SemanticType("Start", FlintSemanticType.Date).SemanticType("End", FlintSemanticType.Date).SemanticType("Phase", FlintSemanticType.Category)
               .Y("Task").X("Start").Encode(FlintChannel.X2, "End").ColorBy("Phase");
        }

        private static FlintChart Bullet()
        {
            return FlintChart(FlintChartType.BulletChart).Columns("Rep", "Sales", "Quota")
               .AddRow("Avery", 182, 160).AddRow("Blake", 131, 150).AddRow("Casey", 96, 140).AddRow("Devon", 158, 155).AddRow("Emery", 120, 120)
               .SemanticType("Sales", FlintSemanticType.Amount).SemanticType("Quota", FlintSemanticType.Amount)
               .Y("Rep").X("Sales").Encode(FlintChannel.Goal, "Quota");
        }

        private static FlintChart Ridgeline(SampleRandom random)
        {
            var chart = FlintChart(FlintChartType.RidgelinePlot).Columns("Month", "Temperature");

            for (int m = 0; m < 12; m++)
            {
                var mean = 5 + 15 * (1 - Math.Abs(m - 6.5) / 6.5);
                for (int i = 0; i < 40; i++) chart.AddRow(Months[m], Math.Round(mean + (random.NextDouble() + random.NextDouble() + random.NextDouble() - 1.5) * 5, 1));
            }

            return chart.SemanticType("Month", FlintSemanticType.Month).SemanticType("Temperature", FlintSemanticType.Temperature).X("Temperature").Y("Month");
        }

        private static FlintChart Pyramid()
        {
            var chart = FlintChart(FlintChartType.PyramidChart).Columns("Age", "Sex", "Population");
            var bands = new[] { "0-9", "10-19", "20-29", "30-39", "40-49", "50-59", "60-69", "70+" };
            var men   = new[] { 4100, 4300, 5200, 5600, 5100, 4800, 3900, 2600 };
            var women = new[] { 3900, 4100, 5000, 5500, 5200, 5000, 4300, 3600 };

            for (int i = 0; i < bands.Length; i++)
            {
                chart.AddRow(bands[i], "Men", men[i]);
                chart.AddRow(bands[i], "Women", women[i]);
            }

            return chart.SemanticType("Population", FlintSemanticType.Count).Y("Age").X("Population").ColorBy("Sex");
        }

        private static FlintChart Candlestick(SampleRandom random, int year)
        {
            var chart = FlintChart(FlintChartType.CandlestickChart).Columns("Date", "Open", "High", "Low", "Close");
            var price = 150.0;
            var day   = new DateTime(year, 4, 1);

            for (int i = 0; i < 30; i++)
            {
                var open  = price;
                var close = Math.Round(open + random.NextDouble(-5, 5), 2);
                var high  = Math.Round(Math.Max(open, close) + random.NextDouble(0, 3), 2);
                var low   = Math.Round(Math.Min(open, close) - random.NextDouble(0, 3), 2);
                chart.AddRow(day.AddDays(i), open, high, low, close);
                price = close;
            }

            return chart.SemanticType("Date", FlintSemanticType.Date).SemanticType("Open", FlintSemanticType.Price).SemanticType("High", FlintSemanticType.Price)
               .SemanticType("Low", FlintSemanticType.Price).SemanticType("Close", FlintSemanticType.Price)
               .X("Date").Encode(FlintChannel.Open, "Open").Encode(FlintChannel.High, "High").Encode(FlintChannel.Low, "Low").Encode(FlintChannel.Close, "Close");
        }

        private static FlintChart Sparkline(SampleRandom random)
        {
            var chart    = FlintChart(FlintChartType.Sparkline).Columns("Day", "Visitors");
            var visitors = 1200.0;

            for (int i = 1; i <= 30; i++)
            {
                visitors = Math.Max(400, visitors + random.NextDouble(-150, 170));
                chart.AddRow(i, Math.Round(visitors));
            }

            return chart.SemanticType("Day", FlintSemanticType.Day).SemanticType("Visitors", FlintSemanticType.Count).X("Day").Y("Visitors");
        }

        private static FlintChart Bump()
        {
            var chart = FlintChart(FlintChartType.BumpChart).Columns("Week", "Team", "Rank");
            var ranks = new[]
            {
                new[] { 1, 2, 3, 4, 5 },
                new[] { 2, 1, 3, 5, 4 },
                new[] { 2, 3, 1, 5, 4 },
                new[] { 3, 2, 1, 4, 5 },
                new[] { 4, 2, 1, 3, 5 },
                new[] { 4, 1, 2, 3, 5 },
            };
            var teams = new[] { "Lions", "Hawks", "Wolves", "Bears", "Foxes" };

            for (int w = 0; w < ranks.Length; w++)
            {
                for (int t = 0; t < teams.Length; t++) chart.AddRow($"W{w + 1}", teams[t], ranks[w][t]);
            }

            return chart.SemanticType("Rank", FlintSemanticType.Rank).X("Week").Y("Rank").ColorBy("Team");
        }

        private static FlintChart Slope()
        {
            return FlintChart(FlintChartType.SlopeChart).Columns("Year", "Browser", "Share")
               .AddRow("2019", "Aurora", 34).AddRow("2024", "Aurora", 41)
               .AddRow("2019", "Comet", 28).AddRow("2024", "Comet", 19)
               .AddRow("2019", "Nimbus", 21).AddRow("2024", "Nimbus", 26)
               .AddRow("2019", "Quasar", 17).AddRow("2024", "Quasar", 14)
               .SemanticType("Year", FlintSemanticType.Year).SemanticType("Share", FlintSemanticType.Percentage)
               .X("Year").Y("Share").ColorBy("Browser");
        }

        private static FlintChart RangeArea()
        {
            var chart = FlintChart(FlintChartType.RangeAreaChart).Columns("Month", "Low", "High");
            var lows  = new[] { -2, -1, 2, 6, 10, 13, 15, 15, 11, 7, 3, 0 };
            var highs = new[] { 4, 6, 10, 15, 19, 23, 25, 25, 20, 14, 8, 5 };

            for (int m = 0; m < 12; m++) chart.AddRow(Months[m], lows[m], highs[m]);

            return chart.SemanticType("Month", FlintSemanticType.Month).SemanticType("Low", FlintSemanticType.Temperature).SemanticType("High", FlintSemanticType.Temperature)
               .X("Month").Y("Low").Encode(FlintChannel.Y2, "High");
        }

        private static FlintChart TrafficShare(FlintChartType type)
        {
            var chart = FlintChart(type).Columns("Channel", "Visits")
               .AddRow("Direct", 4200).AddRow("Search", 2700).AddRow("Social", 1800).AddRow("Referral", 1300).AddRow("Email", 600)
               .SemanticType("Channel", FlintSemanticType.Category).SemanticType("Visits", FlintSemanticType.Count)
               .Encode(FlintChannel.Size, "Visits").ColorBy("Channel");

            return type == FlintChartType.DonutChart ? chart.Property("innerRadius", 50) : chart;
        }

        private static FlintChart Rose()
        {
            var chart = FlintChart(FlintChartType.RoseChart).Columns("Month", "Rainfall");
            var rain  = new[] { 78, 61, 55, 49, 62, 71, 84, 90, 66, 73, 88, 95 };

            for (int m = 0; m < 12; m++) chart.AddRow(Months[m], rain[m]);

            return chart.SemanticType("Month", FlintSemanticType.Month).SemanticType("Rainfall", FlintSemanticType.Quantity).X("Month").Y("Rainfall");
        }

        private static FlintChart Radar()
        {
            var chart  = FlintChart(FlintChartType.RadarChart).Columns("Skill", "Player", "Score");
            var skills = new[] { "Speed", "Power", "Passing", "Defence", "Stamina", "Vision" };
            var a      = new[] { 82, 64, 77, 51, 88, 73 };
            var b      = new[] { 61, 85, 58, 79, 70, 66 };

            for (int i = 0; i < skills.Length; i++)
            {
                chart.AddRow(skills[i], "Ada", a[i]);
                chart.AddRow(skills[i], "Ben", b[i]);
            }

            return chart.SemanticType("Score", FlintSemanticType.Score).X("Skill").Y("Score").ColorBy("Player");
        }

        private static FlintChart Heatmap(SampleRandom random)
        {
            var chart = FlintChart(FlintChartType.Heatmap).Columns("Weekday", "Hour", "Commits");
            var days  = new[] { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun" };

            for (int d = 0; d < days.Length; d++)
            {
                for (int h = 8; h <= 20; h += 2)
                {
                    var busy = d < 5 && h >= 10 && h <= 16 ? 18 : 4;
                    chart.AddRow(days[d], $"{h:00}:00", busy + random.Next(0, 8));
                }
            }

            return chart.SemanticType("Weekday", FlintSemanticType.Category).SemanticType("Hour", FlintSemanticType.Category).SemanticType("Commits", FlintSemanticType.Count)
               .X("Hour").Y("Weekday").ColorBy("Commits");
        }

        private static FlintChart Calendar(SampleRandom random, int year)
        {
            var chart = FlintChart(FlintChartType.CalendarHeatmap).Columns("Date", "Commits");
            var day   = new DateTime(year, 1, 1);

            for (int i = 0; i < 365; i++)
            {
                var date    = day.AddDays(i);
                var weekend = date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday;
                chart.AddRow(date, weekend ? random.Next(0, 3) : random.Next(0, 12));
            }

            return chart.SemanticType("Date", FlintSemanticType.Date).SemanticType("Commits", FlintSemanticType.Count).X("Date").ColorBy("Commits");
        }

        private static FlintChart BarTable()
        {
            return FlintChart(FlintChartType.BarTable).Columns("Category", "Contribution")
               .AddRow("Electronics", 331207).AddRow("Bicycles", 89774).AddRow("Perfume", 57668).AddRow("Clothing", 48210).AddRow("Furniture", 32104).AddRow("Toys", 20157)
               .SemanticType("Category", FlintSemanticType.Category).SemanticType("Contribution", FlintSemanticType.Amount)
               .Y("Category").X("Contribution");
        }

        private static FlintChart Kpis()
        {
            return FlintChart(FlintChartType.KpiCard).Columns("Metric", "Value", "Goal")
               .AddRow("Revenue ($k)", 1284, 1200).AddRow("New customers", 372, 500).AddRow("Churn saves", 41, 120).AddRow("NPS", 54, 50)
               .SemanticType("Metric", FlintSemanticType.Category).SemanticType("Value", FlintSemanticType.Quantity).SemanticType("Goal", FlintSemanticType.Quantity)
               .Encode(FlintChannel.Metric, "Metric").Encode(FlintChannel.Value, "Value").Encode(FlintChannel.Goal, "Goal");
        }

        private static FlintChart Cities()
        {
            return FlintChart(FlintChartType.Map).Columns("City", "Latitude", "Longitude", "Population")
               .AddRow("New York", 40.71, -74.01, 8336817).AddRow("Los Angeles", 34.05, -118.24, 3979576).AddRow("Chicago", 41.88, -87.63, 2693976)
               .AddRow("Houston", 29.76, -95.37, 2320268).AddRow("Phoenix", 33.45, -112.07, 1680992).AddRow("Philadelphia", 39.95, -75.17, 1584064)
               .AddRow("San Antonio", 29.42, -98.49, 1547253).AddRow("San Diego", 32.72, -117.16, 1423851).AddRow("Dallas", 32.78, -96.80, 1343573)
               .AddRow("Seattle", 47.61, -122.33, 753675).AddRow("Denver", 39.74, -104.99, 727211).AddRow("Boston", 42.36, -71.06, 692600)
               .SemanticType("City", FlintSemanticType.City).SemanticType("Latitude", FlintSemanticType.Latitude).SemanticType("Longitude", FlintSemanticType.Longitude).SemanticType("Population", FlintSemanticType.Count)
               .Encode(FlintChannel.Longitude, "Longitude").Encode(FlintChannel.Latitude, "Latitude").Encode(FlintChannel.Size, "Population");
        }

        private static FlintChart States()
        {
            return FlintChart(FlintChartType.Choropleth).Columns("State", "Index")
               .AddRow("California", 82).AddRow("Texas", 64).AddRow("Florida", 58).AddRow("New York", 77).AddRow("Washington", 88).AddRow("Ohio", 49)
               .AddRow("Colorado", 81).AddRow("Georgia", 55).AddRow("Arizona", 61).AddRow("Michigan", 52).AddRow("Oregon", 79).AddRow("Nevada", 57)
               .AddRow("Illinois", 66).AddRow("Utah", 73).AddRow("Montana", 44).AddRow("Maine", 68)
               .SemanticType("State", FlintSemanticType.State).SemanticType("Index", FlintSemanticType.Score)
               .Encode(FlintChannel.Id, "State").ColorBy("Index");
        }
    }
}
