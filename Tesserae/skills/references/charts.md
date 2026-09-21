---
name: charts
description: Five dependency-free responsive SVG charts — LineChart, BarChart, AreaChart, PieChart, HeatMap — with a shared fluent series/palette API, tooltips, legend, stacking, a continuous/time X axis, zoom and pan, spikelines, PNG export and observable-driven updates. Use to plot trends, comparisons, part-to-whole data, a value matrix, or live time series in a Tesserae (C#/Transpose) app.
---

# Charts

Five SVG chart types share a fluent API. Cartesian charts (`LineChart`, `BarChart`,
`AreaChart`) plot against either X-axis categories or a continuous X scale; `PieChart`
renders part-to-whole and can be a donut; `HeatMap` colours a matrix of values. Each
fills its container via a `ResizeObserver` — give it a height (e.g. `.H(200.px())`).

## Create

`UI.LineChart()`, `UI.BarChart()`, `UI.AreaChart()`, `UI.PieChart()`, `UI.HeatMap()` —
empty charts. Each also has a `(double[] data)` overload that sets a single unnamed
series (`HeatMap` takes `(double[][] data)`, one array per row). Bring factories into
scope with `using static Tesserae.UI;`.

## Key configuration

Data (all types):

- `.Data(double[])` — one unnamed series.
- `.Series(string name, double[] values, string color = null)` — append a named series.
- `.Series(IObservable<double[]> values, ...)` — bind to an observable; re-renders on change.
- `.Series(params ChartSeries[])` / `.Series(IObservable<ChartSeries[]>)` — full control.
- `double.NaN` marks a missing sample. `.ConnectGaps(false)` breaks the line at a gap
  instead of drawing straight across it.

`ChartSeries` carries `Name`, `Values`, `Color`, plus `XValues` (continuous X positions),
`LineWidth` (default 2) and `FillOpacity` (default 0.45, area charts):

```csharp
new ChartSeries("CPU %", times, values) { LineWidth = 1, FillOpacity = 0.2 }
```

Appearance (all types):

- `.Colors(params string[])`, `.Tooltips(bool = true)`, `.Title(string)` (aria summary),
  `.FormatValues(Func<double,string>)`.
- The value axis snaps outward onto a 1/2/5 x 10^n step, so gridlines land on round
  numbers (`0 / 200 / 400`, never `168.4 / 336.8`), and a series of whole numbers is
  never labelled in fractions. By default large values are abbreviated with an SI
  prefix (`1.4G`, `250k`) — pass `.FormatValues(...)` to take that over.
- `.Legend(bool = true)` and `.Legend(ChartLegendPosition)` — `Top` (default), `Bottom`,
  `Left`, `Right`. `PieChart` defaults to `Right`.
- `.ExportButton()` — a hover-revealed button that saves the chart as a PNG.
  `.ExportPng(fileName)` does the same from code (CSS-variable colors are flattened
  first, so the image matches the current theme).

Cartesian (`Line`/`Bar`/`Area`):

- `.XAxis(params string[])` — evenly spaced categories.
- `.XAxisTitle(string)`, `.YAxisTitle(string)`, `.Grid(bool)`, `.Axes(bool)`.
- `.MaxXTicks(int)` — cap the tick labels; 0 (default) derives the cap from the width.
- `.ZeroBaseline(bool)` — override whether the value axis includes zero. Bar and area
  default to including it, line to fitting the data; a metric that hovers far from zero
  reads better fitted (`.ZeroBaseline(false)`) with the fill still running to the bottom.
- `LineChart`/`AreaChart`: `.Points(bool)`. `BarChart`: `.Rounded(double radius = 2)`,
  `.Stacked()`.

The value axis sizes its own margin to the widest tick label, so a formatter that
produces long strings (byte counts, currency) is not clipped.

Markers (and their tooltips) are suppressed above 300 points in a series — use
`.Spikelines()` for dense data instead.

PieChart: `.Labels(params string[])`, `.Donut(double holeRatio = 0.6)`.

HeatMap: see its own section below.

## HeatMap

A matrix of cells coloured by value: one row per series (the series name is the row
label), one column per value. `.Data(double[][] rows)` sets the whole matrix at once —
`rows[y][x]` — and the observable `Series` overloads drive it like any other chart, so a
live matrix re-renders on change. `double.NaN` leaves a cell empty (drawn as a dashed
hole, not as the palest colour).

Labels — plain text, or components:

- `.XAxis(params string[])` / `.YAxis(params string[])` — text labels. They are drawn as
  SVG, so they are part of `.ExportPng()`, and are ellipsized to the space they have.
- `.XAxis(IComponent[] labels, params string[] names)` /
  `.YAxis(IComponent[] labels, params string[] names)` — a component per label (a link
  button, an icon plus a caption, a swatch). `names` is optional text for the tooltips,
  the click handlers and the accessibility summary, which a component cannot supply.
  Component labels are HTML positioned over the chart, so they do **not** appear in a
  PNG export.
- **The vertical (row) labels are rotated 90°, reading bottom-to-top** — including a
  component label, which is laid out normally and then turned as a whole. That is what
  makes a long row label cost the chart one line of text instead of its full length; the
  label's length is then bounded by the row's height, and is truncated past it, so give a
  heat map with long row labels the height for them.

Clicks:

- `.OnCellClick(Action<HeatMapCell>)` — `HeatMapCell` carries `Column`, `Row`, `Value`,
  `ColumnLabel` and `RowLabel`. Setting a handler also gives the cells a pointer cursor.
- `.OnXLabelClick(Action<int,string>)` / `.OnYLabelClick(Action<int,string>)` — index plus
  label text. A component label keeps its own handlers, so a label that is a `Button`
  can answer on its own instead and the chart needs neither of these.

Appearance:

- `.ScaleColor(string color, double minIntensity = 0.06)` — the default scale is one
  colour faded towards the page background as the value falls, which is what keeps the
  low end readable in both themes.
- `.ColorScale(params string[] stops)` — an explicit ramp instead; a cell takes the
  nearest stop.
- `.ValueRange(min, max)` / `.AutoValueRange()` — pin the range the colours are scaled
  against so two heat maps can be read against each other.
- `.CellGap(double)`, `.Rounded(double radius = 2)`,
  `.ShowValues(bool show = true, string color = null)` — printed in the cells with the
  room for them; the colour defaults to the theme foreground on pale cells and white on
  saturated ones.
- `.Legend()` draws the colour scale as a gradient bar with its end values
  (`ChartLegendPosition.Right` by default).

```csharp
var heat = HeatMap(matrix)                       // double[rows][columns]
    .XAxis("Jan", "Feb", "Mar", "Apr")
    .YAxis("Enterprise", "Mid-market", "Startups")
    .ShowValues()
    .Legend()
    .OnCellClick(c => Show($"{c.RowLabel} / {c.ColumnLabel}: {c.Value}"))
    .OnYLabelClick((i, label) => OpenSegment(label))
    .WS().H(320.px());

var labelled = HeatMap(matrix)
    .XAxis(months.Select(m => Button().SetText(m).Link().Compact().OnClick(() => Pick(m)) as IComponent).ToArray(), months)
    .YAxis(segments.Select(s => HStack().Children(Icon(UIcons.Building), TextBlock(s).Small()) as IComponent).ToArray(), segments)
    .ScaleColor(Theme.Colors.Teal600)
    .WS().H(440.px());
```

## Continuous / time X axis

Give the points real X positions and the chart switches from evenly spaced categories to
a continuous scale, so each series can have its own X values, its own point count and
irregular spacing:

- `.XValues(double[])` — shared X positions for every series.
- `ChartSeries.XValues` — per-series positions (these win over the shared array).
- `.FormatXAxis(Func<double,string>)` — tick label formatter.
- `.XAxisTime(string format = null)` — treats X as Unix seconds and formats as local time.
  Ticks land on whole seconds/minutes/hours/days, and with no explicit format the labels
  follow the tick step (`HH:mm:ss` zoomed into a minute, `HH:mm`, `MM-dd`, `yyyy-MM`), so
  neighbouring labels never read the same.

## Zoom, pan and spikelines

- `.Zoomable(bool enable = true, bool wheelNeedsCtrl = true)` — wheel zooms the X axis,
  drag pans it, double-click resets. The value axis rescales to the visible window.
  The wheel zooms only while **Ctrl (Cmd on a Mac)** is held — a plain wheel scrolls the
  page as usual, and the chart says "Hold Ctrl and scroll to zoom" for a moment so it does
  not read as broken. A trackpad pinch arrives as a Ctrl wheel, so it zooms too. Pass
  `wheelNeedsCtrl: false` for a chart that owns its surface and has nothing scrolling
  behind it.
- `.Spikelines()` — a vertical line follows the cursor with a readout of the X position
  and each series' nearest value.
- `.XRange(min, max)` / `.AutoRangeX()` / `.TryGetXRange(out min, out max)` / `.IsXRangePinned`.
- `.OnRangeChanged(Action<ChartRange>)` — raised on user zoom/pan/reset only. `.XRange()`
  never re-raises it, so pushing a range onto sibling charts cannot loop. A drag that
  moved nothing does not raise it either, so a plain click never reads as a pan.
- `.ZoomLimits(minSpan, maxSpan)` — the smallest and largest visible X span the wheel may
  reach, in X units. Pass 0 for either to keep the default, which is 1/1000 and 100× the
  data's own X extent. Set an explicit maximum on a chart that fetches its data to match
  the visible range: the widest span the user can reach decides how much has to be loaded.

Keeping two charts on one timeline:

```csharp
a.OnRangeChanged(r => { if (r.IsAutoRange) b.AutoRangeX(); else b.XRange(r.Min, r.Max); });
b.OnRangeChanged(r => { if (r.IsAutoRange) a.AutoRangeX(); else a.XRange(r.Min, r.Max); });
```

Loading data to match the window — `OnRangeChanged` is the trigger, debounced so a wheel
gesture fetches once rather than once a notch:

```csharp
chart.ZoomLimits(minSpan: 10, maxSpan: 3600)          // 10s to 1h of samples
     .OnRangeChanged(r => ScheduleLoad(r.Min, r.Max)); // then chart.XRange(from, to) once loaded
```

A chart that follows the clock wants a zoom to move only its left edge, so the right edge stays
at "now" and the timeline does not slide out from under the cursor while it is live. The chart
reports the range, not the gesture — but a drag reports the span it started with and a wheel notch
reports a new one, which is what tells the two apart:

```csharp
chart.OnRangeChanged(r =>
{
    var isZoom = Math.Abs((r.Max - r.Min) - currentSpanSeconds) >= 1;

    if (isZoom && isLive) SetWindow(now - (long)(r.Max - r.Min), now); // grows into the past only
    else                  SetWindow((long)r.Min, (long)r.Max);
});
```

A pinned range is itself a continuous X scale, so a range the data does not cover still draws
its axis and still answers the wheel and the drag — the period the user has to navigate out of
is exactly the one with nothing in it.

## Example

```csharp
using static Tesserae.UI;

var chart = LineChart()
    .Series("Revenue", new double[] { 12, 18, 15, 22, 30 })
    .Series("Target",  new double[] { 15, 15, 20, 20, 25 })
    .XAxis("Mon", "Tue", "Wed", "Thu", "Fri")
    .Legend()
    .WS().H(200.px());

var live = AreaChart()
    .Series(new ChartSeries("CPU %", unixSeconds, values) { LineWidth = 1, FillOpacity = 0.2 })
    .XAxisTime()
    .Zoomable()
    .Spikelines()
    .ExportButton()
    .WS().H(200.px());
```

## Related

- Sparkline — `/tesserae/components/sparkline`
- Diagram — relationships rather than numbers — `diagram.md`
- Full docs & API: `/tesserae/components/charts`
