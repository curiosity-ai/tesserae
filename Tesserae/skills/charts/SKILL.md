---
name: charts
description: Four dependency-free responsive SVG charts — LineChart, BarChart, AreaChart, PieChart — with a shared fluent series/palette API, tooltips, legend, and observable-driven updates. Use to plot trends, comparisons, or part-to-whole data in a Tesserae (C#/h5) app.
---

# Charts

Four SVG chart types share a fluent API. Cartesian charts (`LineChart`, `BarChart`,
`AreaChart`) take X-axis categories; `PieChart` renders part-to-whole and can be a donut.
Each fills its container via a `ResizeObserver` — give it a height (e.g. `.H(200.px())`).

## Create

`UI.LineChart()`, `UI.BarChart()`, `UI.AreaChart()`, `UI.PieChart()` — empty charts.
Each also has a `(double[] data)` overload that sets a single unnamed series.
Bring factories into scope with `using static Tesserae.UI;`.

## Key configuration

Data (all types):

- `.Data(double[])` — one unnamed series.
- `.Series(string name, double[] values, string color = null)` — append a named series.
- `.Series(IObservable<double[]> values, ...)` — bind to an observable; re-renders on change.

Appearance (all types):

- `.Colors(params string[])`, `.Legend(bool = true)`, `.Tooltips(bool = true)`,
  `.Title(string)` (aria summary), `.FormatValues(Func<double,string>)`.

Cartesian (`Line`/`Bar`/`Area`):

- `.XAxis(params string[])`, `.XAxisTitle(string)`, `.YAxisTitle(string)`,
  `.Grid(bool)`, `.Axes(bool)`.
- `LineChart`/`AreaChart`: `.Points(bool)`. `BarChart`: `.Rounded(double radius = 2)`.

PieChart: `.Labels(params string[])`, `.Donut(double holeRatio = 0.6)`.

## Example

```csharp
using static Tesserae.UI;

var chart = LineChart()
    .Series("Revenue", new double[] { 12, 18, 15, 22, 30 })
    .Series("Target",  new double[] { 15, 15, 20, 20, 25 })
    .XAxis("Mon", "Tue", "Wed", "Thu", "Fri")
    .Legend()
    .WS().H(200.px());
```

## Related

- Sparkline — `/tesserae/components/sparkline`
- Full docs & API: `/tesserae/components/charts`
