namespace Tesserae
{
    /// <summary>
    /// The chart types a <see cref="FlintChart"/> draws: every Vega-Lite template of
    /// <see href="https://github.com/microsoft/flint-chart">Flint</see>, grouped the way Flint groups them. Which
    /// <see cref="FlintChannel"/>s each one reads is listed on it.
    /// </summary>
    public enum FlintChartType
    {
        // Points

        /// <summary>Points at x and y. Channels: x, y, color, size, shape, detail, opacity, column, row.</summary>
        ScatterPlot,
        /// <summary>A scatter plot with the distribution of each axis on its margin. Channels: x, y, color, size, shape, opacity.</summary>
        MarginalPlot,
        /// <summary>A scatter plot with its fitted trend line (<c>regressionMethod</c>: linear, log, exp, pow, quad, poly). Channels: x, y, size, color, column, row.</summary>
        Regression,
        /// <summary>Points joined in the order of a field. Channels: x, y, order, color, detail, column, row.</summary>
        ConnectedScatterPlot,
        /// <summary>The span between two values of each category, drawn as two dots and a rule. Channels: x, y, color.</summary>
        RangedDotPlot,
        /// <summary>Every value of a measure as a tick or dot along one axis per category. Channels: x, y, color, size, column, row.</summary>
        StripPlot,

        // Bars

        /// <summary>One bar per category. Channels: x, y, color, opacity, column, row.</summary>
        BarChart,
        /// <summary>Bars side by side within each category, one per <c>group</c>. Channels: x, y, group, color, column, row.</summary>
        GroupedBarChart,
        /// <summary>Bars stacked within each category, by <c>color</c> (<c>stackMode</c>: stacked, normalize, center). Channels: x, y, color, column, row.</summary>
        StackedBarChart,
        /// <summary>A stem and a dot per category. Channels: x, y, color, column, row.</summary>
        LollipopChart,
        /// <summary>Running totals as floating bars, rises and falls in their own colours. Channels: x, y, color, column, row.</summary>
        WaterfallChart,
        /// <summary>Tasks as bars from <c>x</c> to <c>x2</c> along a timeline. Channels: y, x, x2, color, detail, column, row.</summary>
        GanttChart,
        /// <summary>A measure against its <c>goal</c>, as a bar on qualitative bands. Channels: y, x, goal, color, column, row.</summary>
        BulletChart,

        // Distributions

        /// <summary>A measure binned into counts (<c>binCount</c>). Channels: x, color, column, row.</summary>
        Histogram,
        /// <summary>A smoothed distribution of a measure. Channels: x, color, column, row.</summary>
        DensityPlot,
        /// <summary>One density per category, overlapping like ridges. Channels: x, y, color.</summary>
        RidgelinePlot,
        /// <summary>The cumulative share of rows at or below each value. Channels: x, color, detail, column, row.</summary>
        EcdfPlot,
        /// <summary>The distribution of a measure per category, mirrored. Channels: x, y, color, row.</summary>
        ViolinPlot,
        /// <summary>Quartiles, whiskers and outliers of a measure per category. Channels: x, y, color, opacity, column, row.</summary>
        Boxplot,
        /// <summary>Two distributions back to back, such as a population by age and sex. Channels: x, y, color.</summary>
        PyramidChart,
        /// <summary>Open, high, low and close per period. Channels: x, open, high, low, close, column, row.</summary>
        CandlestickChart,

        // Lines & areas

        /// <summary>A measure over a continuous axis, one line per <c>color</c>. Channels: x, y, color, strokeDash, detail, opacity, column, row.</summary>
        LineChart,
        /// <summary>A small line without axes, for a trend at a glance. Channels: x, y, color, detail, row, column.</summary>
        Sparkline,
        /// <summary>The rank of each series over time. Channels: x, y, color, detail, column, row.</summary>
        BumpChart,
        /// <summary>A change between two points in time, one line per series. Channels: x, y, color, detail, column, row.</summary>
        SlopeChart,
        /// <summary>A line with the area under it filled, stacked by <c>color</c>. Channels: x, y, color, opacity, column, row.</summary>
        AreaChart,
        /// <summary>Stacked areas around a moving centre line. Channels: x, y, color, column, row.</summary>
        Streamgraph,
        /// <summary>The band between <c>y</c> and <c>y2</c> over x. Channels: x, y, y2, color, column, row.</summary>
        RangeAreaChart,

        // Circular

        /// <summary>Shares of a whole as slices; the measure is <c>size</c>. Channels: size, color, column, row.</summary>
        PieChart,
        /// <summary>A pie with a hole (<c>innerRadius</c>). Channels: size, color, column, row.</summary>
        DonutChart,
        /// <summary>Equal-angle wedges whose radius is the measure. Channels: x, y, color, column, row.</summary>
        RoseChart,
        /// <summary>Measures on spokes around a centre, one polygon per series. Channels: x, y, color, column, row.</summary>
        RadarChart,

        // Tables & maps

        /// <summary>A grid of x by y cells coloured by a measure. Channels: x, y, color, column, row.</summary>
        Heatmap,
        /// <summary>A measure per day, laid out as weeks of a year. Channels: x, color.</summary>
        CalendarHeatmap,
        /// <summary>A table with an inline bar per row. Channels: y, x, color, column, row.</summary>
        BarTable,
        /// <summary>One headline number per <c>metric</c>, with its <c>goal</c>. Channels: metric, value, goal.</summary>
        KpiCard,
        /// <summary>The rows themselves, as a table. No channels.</summary>
        Table,
        /// <summary>Points at longitude and latitude on a map (<c>region</c>). Channels: longitude, latitude, color, size, opacity.</summary>
        Map,
        /// <summary>Regions of a map coloured by a measure (<c>region</c>, <c>level</c>). Channels: id, color, detail.</summary>
        Choropleth,
    }

    /// <summary>
    /// The encoding channels of a <see cref="FlintChart"/>: which field drives which visual property. Which ones a chart
    /// reads depends on its <see cref="FlintChartType"/>, and a channel it does not have fails the chart with Flint's reason.
    /// </summary>
    public enum FlintChannel
    {
        X,
        Y,
        X2,
        Y2,
        Color,
        Size,
        Shape,
        Opacity,
        Detail,
        StrokeDash,
        Order,
        Group,
        /// <summary>Small multiples: one panel per value, side by side.</summary>
        Column,
        /// <summary>Small multiples: one panel per value, one under another.</summary>
        Row,
        Goal,
        Open,
        High,
        Low,
        Close,
        Metric,
        Value,
        Longitude,
        Latitude,
        Id,
    }

    /// <summary>
    /// What a field of a <see cref="FlintChart"/>'s data means. Flint picks the encoding type, the zero baseline, the
    /// colour scheme and the number format from it; a field with none is inferred from its values.
    /// </summary>
    public enum FlintSemanticType
    {
        // Time
        DateTime, Date, Time, Timestamp, Year, Quarter, Month, Week, Day, Hour, YearMonth, YearQuarter, YearWeek, Decade, Duration,

        // Measures
        Amount, Price, Quantity, Temperature, Percentage, Profit, PercentageChange, Sentiment, Correlation, Count, Number,

        // Ordered
        Rank, Score,

        // Identifiers and places
        ID, Latitude, Longitude, Country, State, City, Region, Address, ZipCode,

        // Categories
        Category, Name, Status, Boolean, Direction, Range, Unknown,
    }

    /// <summary>How a field is encoded, when the one Flint infers from its <see cref="FlintSemanticType"/> is not the one meant.</summary>
    public enum FlintEncodingType
    {
        Quantitative,
        Nominal,
        Ordinal,
        Temporal,
    }

    /// <summary>How the rows behind one mark are summed into its value.</summary>
    public enum FlintAggregate
    {
        Count,
        Sum,
        Average,
    }

    /// <summary>
    /// The interactions Flint ships as presets, for <see cref="FlintChart.Interactions"/>. One the chart type cannot
    /// honour is dropped with a warning rather than failing the chart.
    /// </summary>
    public enum FlintInteraction
    {
        /// <summary>Move the pointer to read values with guides.</summary>
        Inspect,
        /// <summary>Move along one axis to read every series at that position.</summary>
        InspectIndex,
        /// <summary>Click a mark, key entry or axis label to emphasise it and mute the rest.</summary>
        ClickHighlight,
        /// <summary>Hover or click a category's axis label to emphasise it.</summary>
        AxisHighlight,
        /// <summary>Click a mark to emphasise every mark of its group.</summary>
        ClickGroupFocus,
        /// <summary>Hover a mark to preview its group.</summary>
        HoverGroupFocus,
        /// <summary>Click a mark to pin an annotation on it.</summary>
        ClickAnnotate,
        /// <summary>Drag a rectangle to emphasise the marks inside it.</summary>
        Select,
        /// <summary>Draw a freehand region to emphasise the marks inside it.</summary>
        LassoSelect,
        /// <summary>Drag an interval along x.</summary>
        BrushX,
        /// <summary>Drag an interval along y.</summary>
        BrushY,
        /// <summary>Drag an angular sector on a polar chart.</summary>
        BrushAngle,
        /// <summary>Drag a rectangle to zoom to it.</summary>
        BrushZoom,
        /// <summary>Brush marks and emphasise every mark of their group.</summary>
        LinkedBrush,
        /// <summary>Click a key entry to hide or restore its series.</summary>
        LegendToggle,
        /// <summary>Hold on a mark to emphasise it.</summary>
        LongPress,
        /// <summary>Double-click a mark to emphasise it.</summary>
        DoubleActivate,
        /// <summary>Drag to pan, wheel or pinch to zoom.</summary>
        Navigate,
        /// <summary>Drag a mark or an axis label to reorder the categories.</summary>
        DragReorder,
        /// <summary>Tab into the chart and walk it with the keyboard.</summary>
        AccessibleNavigation,
    }

    /// <summary>The names Flint gives the values of the enums above, which are not C# identifiers.</summary>
    internal static class FlintNames
    {
        private static readonly string[] ChartTypes =
        {
            "Scatter Plot", "Marginal Plot", "Regression", "Connected Scatter Plot", "Ranged Dot Plot", "Strip Plot",
            "Bar Chart", "Grouped Bar Chart", "Stacked Bar Chart", "Lollipop Chart", "Waterfall Chart", "Gantt Chart", "Bullet Chart",
            "Histogram", "Density Plot", "Ridgeline Plot", "ECDF Plot", "Violin Plot", "Boxplot", "Pyramid Chart", "Candlestick Chart",
            "Line Chart", "Sparkline", "Bump Chart", "Slope Chart", "Area Chart", "Streamgraph", "Range Area Chart",
            "Pie Chart", "Donut Chart", "Rose Chart", "Radar Chart",
            "Heatmap", "Calendar Heatmap", "Bar Table", "KPI Card", "Table", "Map", "Choropleth",
        };

        private static readonly string[] Channels =
        {
            "x", "y", "x2", "y2", "color", "size", "shape", "opacity", "detail", "strokeDash", "order", "group",
            "column", "row", "goal", "open", "high", "low", "close", "metric", "value", "longitude", "latitude", "id",
        };

        private static readonly string[] SemanticTypes =
        {
            "DateTime", "Date", "Time", "Timestamp", "Year", "Quarter", "Month", "Week", "Day", "Hour", "YearMonth", "YearQuarter", "YearWeek", "Decade", "Duration",
            "Amount", "Price", "Quantity", "Temperature", "Percentage", "Profit", "PercentageChange", "Sentiment", "Correlation", "Count", "Number",
            "Rank", "Score",
            "ID", "Latitude", "Longitude", "Country", "State", "City", "Region", "Address", "ZipCode",
            "Category", "Name", "Status", "Boolean", "Direction", "Range", "Unknown",
        };

        private static readonly string[] EncodingTypes = { "quantitative", "nominal", "ordinal", "temporal" };

        private static readonly string[] Aggregates = { "count", "sum", "average" };

        private static readonly string[] Interactions =
        {
            "inspect", "inspect-index", "click-highlight", "axis-highlight", "click-group-focus", "hover-group-focus", "click-annotate",
            "select", "lasso-select", "brush-x", "brush-y", "brush-angle", "brush-zoom", "linked-brush", "legend-toggle",
            "long-press", "double-activate", "navigate", "drag-reorder", "accessible-navigation",
        };

        internal static string Of(FlintChartType type)         => ChartTypes[(int)type];
        internal static string Of(FlintChannel channel)        => Channels[(int)channel];
        internal static string Of(FlintSemanticType type)      => SemanticTypes[(int)type];
        internal static string Of(FlintEncodingType type)      => EncodingTypes[(int)type];
        internal static string Of(FlintAggregate aggregate)    => Aggregates[(int)aggregate];
        internal static string Of(FlintInteraction interaction) => Interactions[(int)interaction];

        /// <summary>The chart type Flint calls <paramref name="name"/>, if it is one of them.</summary>
        internal static bool TryChartType(string name, out FlintChartType type)
        {
            var index = System.Array.IndexOf(ChartTypes, name);
            type = (FlintChartType)System.Math.Max(0, index);
            return index >= 0;
        }
    }
}
