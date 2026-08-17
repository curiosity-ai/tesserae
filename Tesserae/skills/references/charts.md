---
name: charts
description: Four dependency-free responsive SVG charts — LineChart, BarChart, AreaChart, PieChart — with a shared fluent series/palette API, tooltips, legend, stacking, a continuous/time X axis, zoom and pan, spikelines, PNG export and observable-driven updates. Use to plot trends, comparisons, part-to-whole data or live time series in a Tesserae (C#/Transpose) app.
---

# Charts

Four SVG chart types share a fluent API. Cartesian charts (`LineChart`, `BarChart`,
`AreaChart`) plot against either X-axis categories or a continuous X scale; `PieChart`
renders part-to-whole and can be a donut. Each fills its container via a
`ResizeObserver` — give it a height (e.g. `.H(200.px())`).

## Create

`UI.LineChart()`, `UI.BarChart()`, `UI.AreaChart()`, `UI.PieChart()` — empty charts.
Each also has a `(double[] data)` overload that sets a single unnamed series.
Bring factories into scope with `using static Tesserae.UI;`.

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

- `.Zoomable()` — wheel zooms the X axis, drag pans it, double-click resets. The value
  axis rescales to the visible window.
- `.Spikelines()` — a vertical line follows the cursor with a readout of the X position
  and each series' nearest value.
- `.XRange(min, max)` / `.AutoRangeX()` / `.TryGetXRange(out min, out max)` / `.IsXRangePinned`.
- `.OnRangeChanged(Action<ChartRange>)` — raised on user zoom/pan/reset only. `.XRange()`
  never re-raises it, so pushing a range onto sibling charts cannot loop.

Keeping two charts on one timeline:

```csharp
a.OnRangeChanged(r => { if (r.IsAutoRange) b.AutoRangeX(); else b.XRange(r.Min, r.Max); });
b.OnRangeChanged(r => { if (r.IsAutoRange) a.AutoRangeX(); else a.XRange(r.Min, r.Max); });
```

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
- Full docs & API: `/tesserae/components/charts`
