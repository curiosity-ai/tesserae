---
name: gantt-chart
description: A timeline of labelled rows, each holding bars on a shared horizontal axis of named columns or a numeric range. Use when showing a schedule, the steps of a process or any set of spans over time in a Tesserae (C#/Transpose) app.
---

# GanttChart

Rows in a label column on the left; each row's bars drawn on a horizontal axis
shared by every row, so when things happen and what overlaps what can be read at
a glance. The label column stays put while the timeline scrolls sideways once it
is narrower than its minimum width.

## Create

`UI.GanttChart()` — empty chart; add rows with `.Add(GanttRow(...))`.
`UI.GanttRow(string label, params GanttBar[] bars)` / `UI.GanttRow(IComponent label, params GanttBar[] bars)` — one row.
`UI.GanttBar(double start, double end)` — one bar, in the axis's unit.
`UI.GanttBand(double start, double end, string label)` — one span of the band row, in the axis's unit.
Bring factories into scope with `using static Tesserae.UI;`.

## The axis

- `.Columns(params string[] names)` — named, equal-width columns (days, sprints, phases). Column `i` spans `i` to `i + 1`, so `GanttBar(0, 2)` covers the first two.
- Otherwise numeric: spans the bars, or `.Range(double start, double end)`.
  - `.Ticks(int approximateCount)` — roughly how many ticks (step rounded to 1/2/5 × 10ⁿ; default 6).
  - `.TickStep(double step)` — a tick at every multiple of `step`, for steps that are not 1/2/5 × 10ⁿ (3, 6 or 12 hours, 7 days). Overrides `.Ticks(...)`; `0` goes back to it.
  - `.TickFormat(Func<double, string>)` — how a tick is written (e.g. `s => s.ToString("0.0") + " s"`).
  - `.Bands(params GanttBand[] bands)` — a second header row above the ticks for the coarser unit they fall in (the day of hour ticks, the month of day ticks), with a solid line through the chart at each band boundary. Bands are clipped to the axis; none removes the row. Use it instead of writing two units into one tick ("19 Nov 06:00"): each row then holds one unit.

## Chart configuration

- `.Title(string)` — heading of the label column (default `"Task"`).
- `.LabelWidth(UnitSize)` (default `240px`), `.RowHeight(UnitSize)` (default `44px`), `.TrackMinWidth(UnitSize)` (default `480px`, narrower scrolls).
- `.Rows(params GanttRow[])` replaces every row; `.Clear()` empties; `.EmptyMessage(string)` is shown when there are none.
- `.OnBarClick(Action<GanttBar>)` — the clicked bar; use `bar.Tag` to find your own object again.

## Rows and bars

- `GanttRow.Indent(int level = 1)` — nest a row under the one above (16px per level).
- `GanttRow.Add(GanttBar)` — a row can hold any number of bars.
- `GanttBar.Color(GanttBarColor)` — `Blue` (default), `Green`, `Purple`, `Orange`, `Red`, `Teal`, `Magenta`, `Yellow`, `Neutral`; a pastel fill under an outline of the same hue.
- `GanttBar.Color(string css)` — any CSS color; the fill and outline are derived from it.
- `GanttBar.Text(string)` — a label beside the bar (after it; before it near the right edge; inside it when the bar spans the axis).
- `GanttBar.Tooltip(string)` / `.Tooltip(IComponent)` — shown on hover.
- `GanttBar.Striped()` — hatched, for a span that was waiting or is still running.
- `GanttBar.SetTag(object)` — handed back through `OnBarClick`.

## Example

```csharp
using static Tesserae.UI;

var week = GanttChart()
    .Columns("Monday", "Tuesday", "Wednesday", "Thursday", "Friday")
    .Add(GanttRow("Wireframing", GanttBar(0, 2).Color(GanttBarColor.Blue)))
    .Add(GanttRow("Development", GanttBar(1, 3).Color(GanttBarColor.Green)))
    .Add(GanttRow("Testing",     GanttBar(3, 5).Color(GanttBarColor.Orange).Tooltip("Thursday to Friday")));

var trace = GanttChart()
    .Title("Step")
    .TickFormat(s => s.ToString("0.0") + " s")
    .Add(GanttRow("Request",  GanttBar(0,   9.4).Color(GanttBarColor.Neutral).Text("9.4 s")))
    .Add(GanttRow("Thinking", GanttBar(0.8, 2.1).Color(GanttBarColor.Purple).Text("1.3 s")).Indent())
    .Add(GanttRow("Search",   GanttBar(2.1, 4.6).Color(GanttBarColor.Green).Text("2.5 s")).Indent());

// Hours since midnight of the first day; the days go in the band row, the hours in the ticks.
var shifts = GanttChart()
    .Range(18, 42)
    .TickStep(3)
    .TickFormat(h => (((int)h) % 24).ToString("00") + ":00")
    .Bands(GanttBand(0, 24, "Monday 25 May"), GanttBand(24, 48, "Tuesday 26 May"))
    .Add(GanttRow("Night shift", GanttBar(22, 30).Color(GanttBarColor.Purple)));
```

## Related

- Timeline — events as a vertical list rather than spans on an axis — `timeline.md`
- Charts — line, bar and area charts over a numeric axis — `charts.md`
- ContributionBar — one stacked bar of how parts add up — `contribution-bar.md`
- Full docs & API: `/tesserae/charts/gantt-chart`
