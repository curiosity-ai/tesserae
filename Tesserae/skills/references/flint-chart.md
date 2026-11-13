---
name: flint-chart
description: The FlintChart component draws a chart with Microsoft's Flint from a Flint chart spec (data, semantic types, chart type and encodings), given as JSON or built fluently, covering 39 Vega-Lite chart types, with click events on marks, series, key entries and axis labels, and Vega-Lite legend and scale binding (highlight a series from the key, pan and zoom). Use when a Tesserae (C#/Transpose) app needs a declarative, themed chart beyond the toolkit's own SVG charts, or needs to react to a click on a chart element.
---

# FlintChart

Draws a chart with [Flint](https://github.com/microsoft/flint-chart) (Vega-Lite
underneath) from a **Flint chart spec**: the data, what each field means (a semantic
type), and which field goes on which channel of which chart type. Flint works out the
scales, axes, formats and layout; the theme decides the look. The spec is the same JSON
a `flint` fence in Markdown holds (`flint-charts.md`), so a chart a model wrote for chat
can be drawn as a component unchanged.

For a light, dependency-free chart use the toolkit's own `charts.md` components instead:
Flint is a 1.8 MB library (fetched the first time a chart is drawn).

## Create

```csharp
using static Tesserae.UI;

// Fluent: a chart type, then data, meanings and encodings.
FlintChart(FlintChartType.BarChart)
   .Columns("Product", "Sales")
   .AddRow("Notebooks", 1840)
   .AddRow("Pens", 2650)
   .SemanticType("Sales", FlintSemanticType.Amount)
   .X("Product").Y("Sales")
   .Title("Sales per product");

// From a spec, as JSON (a ChartAssemblyInput).
FlintChart(@"{
  ""data"": { ""values"": [ { ""q"": ""Q1"", ""plan"": 120, ""actual"": 108 } ] },
  ""semantic_types"": { ""q"": ""Quarter"", ""plan"": ""Amount"", ""actual"": ""Amount"" },
  ""chart_spec"": { ""chartType"": ""Line Chart"", ""encodings"": { ""x"": ""q"", ""y"": [""plan"", ""actual""] } }
}");
```

- `UI.FlintChart(FlintChartType type)` — a chart of that type with no data yet.
- `UI.FlintChart(string specJson)` — a chart from a spec. Invalid JSON is reported like
  any other spec that cannot be drawn (below), not thrown.

The fluent methods write into the spec, so they also adjust a chart made from JSON.

## Data

- `.Columns(params string[] names)` — the fields, in order; drops existing rows.
- `.AddRow(params object[] values)` — one value per column: `string`, number, `bool`,
  `DateTime` (written as an ISO date, or date and time), or `null`.
- `.Data<T>(IEnumerable<T> items, Func<T, object[]> values)` — a row per item.
- `.DataJson(string rowsJson)` — rows as a JSON array of objects.
- `.DataUrl(string url)` — a JSON/CSV file, fetched by the browser.
- `.ClearData()`.

## What fields mean, and where they go

- `.SemanticType(string field, FlintSemanticType type)` — `Date`, `Month`, `Year`,
  `Quarter`, `Amount`, `Price`, `Percentage`, `Count`, `Quantity`, `Score`, `Rank`,
  `Category`, `Country`, `State`, `City`, `Latitude`, `Longitude`, … (44 in all). It
  decides the encoding type, zero baseline, colour scheme and number format; an
  unannotated field is inferred from its values.
- `.Encode(FlintChannel channel, string field)` — `X`, `Y`, `X2`, `Y2`, `Color`, `Size`,
  `Shape`, `Opacity`, `Detail`, `StrokeDash`, `Order`, `Group`, `Column`, `Row` (small
  multiples), `Goal`, `Open`/`High`/`Low`/`Close`, `Metric`, `Value`, `Longitude`,
  `Latitude`, `Id`.
- `.Encode(FlintChannel channel, params string[] fields)` — several measures on one
  measure channel, a series each (`.Encode(FlintChannel.Y, "plan", "actual")`).
- `.Encode(FlintChannel channel, FlintEncoding encoding)` — with options:
  `new FlintEncoding("field") { Type, Aggregate, SortAscending, SortBy, Scheme,
  SchemeColors, Reverse, Title }`.
- `.X(field)`, `.Y(field)`, `.ColorBy(field)` — shortcuts. `.RemoveEncoding(channel)`.
- `.ChartType(FlintChartType)` — switch type, keeping data and encodings.
- `.Property(string name, object value)` — the chart type's own `chartProperties`
  (`innerRadius`, `showPoints`, `stackMode`, `binCount`, `cornerRadius`, `region`, …).
- `.Title`, `.Subtitle`, `.Label` (a kicker above the title), `.Source(text, href = null)`.
- `.BaseSize(width, height)` — the size Flint aims for (default 400×320). Only the height is used: the width is always the chart's own box.
- `.Tooltips(bool = true)` — hover readouts, on by default.
- `.Interactions(params FlintInteraction[])` — Flint's presets: `Inspect`,
  `InspectIndex`, `ClickHighlight`, `LegendToggle`, `BrushX`, `BrushZoom`, `Navigate`,
  `Select`, `LassoSelect`, `DragReorder`, `AccessibleNavigation`, … One a chart type
  cannot honour is dropped with a console warning.
- `.Spec` — get or set the whole spec as JSON.
- `FlintChart.ChartTypeName(FlintChartType)` — Flint's name for it (`"Scatter Plot"`).

Which channels each chart type reads is in the doc comment of its `FlintChartType`
member. A channel the type does not have, or a field that is not in the data, fails the
chart with Flint's reason.

### The 39 chart types

| Group | Types |
|---|---|
| Points | ScatterPlot, MarginalPlot, Regression, ConnectedScatterPlot, RangedDotPlot, StripPlot |
| Bars | BarChart, GroupedBarChart (`Group` channel), StackedBarChart (`Color`), LollipopChart, WaterfallChart, GanttChart (`X`/`X2`), BulletChart (`Goal`) |
| Distributions | Histogram, DensityPlot, RidgelinePlot, EcdfPlot, ViolinPlot, Boxplot, PyramidChart, CandlestickChart (`Open`/`High`/`Low`/`Close`) |
| Lines & areas | LineChart, Sparkline, BumpChart, SlopeChart, AreaChart, Streamgraph, RangeAreaChart (`Y`/`Y2`) |
| Circular | PieChart, DonutChart (measure on `Size`), RoseChart, RadarChart |
| Tables & maps | Heatmap, CalendarHeatmap, BarTable, KpiCard (`Metric`/`Value`/`Goal`), Table, Map (`Longitude`/`Latitude`), Choropleth (`Id` = region name or code) |

`Map` and `Choropleth` fetch their base map from `vega.github.io` (US states, world
countries); a page that cannot reach it shows no map.

## Legend and scale binding

Vega-Lite's [selection bindings](https://vega.github.io/vega-lite/docs/bind.html),
written into the spec's `bind` (the same key a `flint` fence takes, `flint-charts.md`):

```csharp
var cars = FlintChart(FlintChartType.ScatterPlot)
   .DataJson(carsJson)                       // rows with Name, Origin, Weight, MPG
   .X("Weight").Y("MPG").ColorBy("Origin");

// Click a key entry to choose that series and fade the rest; shift-click adds one, double-click clears.
cars.LegendBinding();

// Drag to pan, wheel to zoom, double-click to reset.
cars.ScaleBinding();

// Small multiples share their scales, so the panels move together.
cars.Encode(FlintChannel.Column, "Origin")
    .ScaleBinding(new FlintScaleBinding { Y = false, ZoomEvents = "wheel![event.shiftKey]" })
    .LegendBinding(new FlintLegendBinding { ClickMarks = true, MutedOpacity = 0.15 });
```

- `.LegendBinding(bool bind = true)` / `.LegendBinding(FlintLegendBinding)` — `null` or
  `false` removes it. `FlintLegendBinding { Channel, Events, ClickMarks, MutedOpacity }`:
  `Channel` picks the key (`Color`, `Shape`, `StrokeDash`, `Size`, `Opacity`; default the
  first key drawn with symbols), `Events` the Vega event stream on an entry (default
  `"click"`), `ClickMarks` lets a mark click choose its series, `MutedOpacity` (0.2) is the
  opacity of the rest. Only a key drawn with symbols can be bound: a gradient key for a
  measure, or a chart that labels its series directly, draws the chart without it.
- `.ScaleBinding(bool bind = true)` / `.ScaleBinding(FlintScaleBinding)` —
  `FlintScaleBinding { X, Y, Zoom, Pan, ZoomEvents, PanEvents }`. Only continuous axes
  (numbers, dates) move: a bar chart's values pan and zoom, its categories stay. A bound
  axis gets ticks for the range in view, in Flint's format. By default the wheel zooms
  whenever the pointer is over the chart (the page does not scroll there);
  `ZoomEvents = "wheel![event.shiftKey]"` leaves the wheel to the page unless Shift is held.

Don't combine them with the Flint interactions that drive the same gestures their own
way: `LegendToggle` / `ClickHighlight` with a legend binding, `Navigate` / `BrushZoom` with
a scale binding. `OnElementClick` still reports key-entry clicks.

## Clicks

```csharp
FlintChart(FlintChartType.BarChart)
   .Columns("Product", "Sales").AddRow("Pens", 2650).AddRow("Ink", 730)
   .X("Product").Y("Sales")
   .OnElementClick(click =>
   {
       // click.Target: Mark, Series, LegendEntry or AxisLabel
       // click.Rows: the data rows it stands for, as given; click.Row the first
       // click.Field / click.Value: the field and value a key entry or axis label names
       Toast().Information(click.Target.ToString(), click.Row?.GetString("Product"));
   });
```

- `Mark` — a bar, point, slice, cell: its row (or the rows it sums).
- `Series` — a line or an area: every row of the series. Set `Property("showPoints", true)`
  on a line chart to make each point a `Mark` of its own row.
- `LegendEntry` / `AxisLabel` — every row of that series or category, plus `Field` and `Value`.
- `FlintChartRow` reads a row: `GetString(field)`, `GetNumber(field)` (`double?`),
  `this[field]`, `Has(field)`, `Fields`, `ToJson()`.

A reported click changes nothing on the chart. Add `FlintInteraction.ClickHighlight`
to also emphasise what was clicked: the click is still reported. With
`LegendToggle` or `ClickHighlight` mounted, key-entry clicks belong to them (Flint
gives each gesture one owner) and are reported from their events.

## Layout and lifecycle

- Width: the chart is drawn at exactly the width of its box and again when that changes
  (wider is a new mount, narrower is laid out in place): `.WS()`, a fixed `.W(...)`, a
  percentage, a grid cell, or what is left in a row with `.Grow()`. It never keeps a width
  of its own (Flint's 400px) or stops short of the box. A content-sized parent leaves it at
  its minimum.
- Height: the chart chooses its own height for its width. With `.FillHeight()` it is laid
  out for the height it is given (`.H(...)`, `.HS()`, `.Grow()`) instead, titles and key
  included. Without `FillHeight()`, an explicit `.H(...)` only clips or pads it.
- Changes coalesce: a fluent chain draws once. A redraw happens out of sight and is
  swapped in when drawn, so it never flashes.
- A page theme change (light/dark, custom theme) redraws the chart only when its theme
  reads differently.
- Taken off the page and put back, the chart keeps its drawing.

## Errors

A spec that cannot be drawn shows its reason in the chart's place
(`.tss-flintchart-message`) and calls `.OnError(Action<string>)`; `Error` holds the last
reason, `IsDrawn` whether a chart is on screen. A redraw that fails leaves the chart
that was there (and still reports).

## Themes

`.ChartTheme(IFlintTheme theme, bool overrideSpecTheme = false)` — default
`FlintThemes.Tesserae`, which reads the page's tokens around the chart and follows
every custom theme. See `flint-charts.md` for `FlintThemes` (presets such as
`Economist`, `Nyt`, `PowerBI`; `TesseraeOver(id)`; `FromJson`). A spec's own
`theme_spec` wins unless `overrideSpecTheme` is set.

The component draws no frame of its own (`.tss-flintchart` > `.tss-flint-frame`): put it
in a `Card` for a plate.

## Related

- Flint charts in Markdown — `flint-charts.md`
- Charts — the toolkit's own SVG charts — `charts.md`
- Mermaid — diagrams — `mermaid.md`
