---
name: activity-calendar
description: ActivityCalendar — a calendar heat map of daily activity (one square per day, a month up to a year), with month/weekday labels, a Less–More legend, an optional streak/average/total summary, built-in tooltips and a per-day click handler. Use when showing how much happened on each day (usage, spend, requests, commits) in a Tesserae (C#/Transpose) app.
---

# ActivityCalendar

One square per day, coloured by the day's value in a few levels. Two layouts: columns
of weeks with months along the top (the year view, the default) or rows of weeks with
weekdays along the top (a wall-calendar month). Designed for a month to a year; it
accepts up to ~10 years.

Every day has a tooltip, drawn by the calendar itself: one element shared by every
calendar on the page, moved by a single listener on the grid. Do **not** wrap days in
Tippy tooltips of your own. The legend's squares show the value range of each colour.

## Create

`UI.ActivityCalendar()` returns an `ActivityCalendar` (`using static Tesserae.UI;`).

## Data and range

- `.Data(IEnumerable<(DateTime date, double value)>)` / `.Data(DateTime[] dates, double[] values)`
  — replaces the data. Time of day is ignored; two entries on the same day are added.
- `.SetValue(DateTime, double)` — one day.
- Range (inclusive): `.LastDays(365)` (ending today), `.Year(2026)`, `.Month(2026, 5)`,
  `.Range(from, to)`. With none set, the range is the data's first to last day (or the
  last 365 days when there is no data).
- `.Today(DateTime)` — what counts as today (`LastDays` end; later days are drawn as
  "still to come", are not clickable and are left out of the summary).

Changing the data re-colours the existing squares in place; changing the range or
layout rebuilds them. Either way, calling the setter on the live component is the
update — no `Defer`/`DeferSync` needed.

## Colouring

- `.Scale(ActivityCalendarScale.Linear | Sqrt | Log2 | Log10 | Quantile)` — default `Linear`.
  `Log10` gives each level whole orders of magnitude (legend reads 1, 10, 100…), `Log2`
  whole doublings, both counted down from the largest value; `Quantile` puts the same
  number of active days in each level. Use a log scale when a few days dwarf the rest.
- `.Levels(int)` — active levels besides "no activity" (default 4, max 10).
- `.Thresholds(params double[] lowerBounds)` — explicit level bounds (overrides `Scale`).
- `.Max(double)` — pin the top of the scale so side-by-side calendars agree.
- `.Color(string)` — the colour faded across the levels (default theme primary).
- `.Palette(params string[])` — one explicit colour per level, lowest first.

## Layout and decoration

- `.Vertical()` / `.Horizontal()` / `.Orientation(...)`; `.FirstDayOfWeek(DayOfWeek.Monday)` (default Sunday).
- `.CellSize(px)` — the largest a square grows to (default 18); squares shrink to fit,
  down to a floor past which the calendar scrolls sideways. `.CellGap(px)` (default 3).
- `.ShowDayNumbers()` — print the day in each square (pair with `CellSize(36+)`).
- `.MonthLabels(bool)` (default: shown unless the range is one month), `.WeekdayLabels(bool)`.
- `.Title(string title, string info = null)` and `.Commands(params IComponent[])` — the
  header row: title, info icon, components on the right (e.g. a metric picker).
- `.Summary()` — longest streak, average per day and per week, total.
- `.Legend(false)` — hide the legend.
- `.Labels(new ActivityCalendarLabels { Less = …, More = …, MonthNames = …, WeekdayNames = … })` — translations.

## Tooltips, formatting and clicks

- `.FormatValue(Func<double, string>)` — used by tooltips, legend and summary.
- `.FormatDate(Func<DateTime, string>)` — the default tooltip's date.
- `.TooltipText(Func<ActivityCalendarDay, string>)` or `.Tooltip(Func<ActivityCalendarDay, IComponent>)`
  — replace the tooltip; return null for none.
- `.OnDayClick(Action<ActivityCalendarDay>)` — click, or Enter/Space when focused.
  `ActivityCalendarDay` has `Date`, `Value`, `HasValue`, `Level`.
- `.Selected(DateTime?)` — mark a day.
- Keyboard: the grid is one tab stop; arrow keys move between days, Home/End jump.

## Example

```csharp
using static Tesserae.UI;

var calendar = ActivityCalendar()
   .LastDays(365)
   .Data(days)                                 // IEnumerable<(DateTime, double)>
   .Scale(ActivityCalendarScale.Log10)
   .FormatValue(v => "$" + v.ToString("n4"))
   .Title("Activity", "Spend per day")
   .Commands(metricDropdown)
   .Summary()
   .OnDayClick(day => ShowDay(day.Date))
   .WS();

// Later: new data, same component.
calendar.Data(otherDays).FormatValue(v => v.ToString("n0"));

// A month as a wall calendar.
var month = ActivityCalendar().Month(2026, 5).Vertical().FirstDayOfWeek(DayOfWeek.Monday)
   .ShowDayNumbers().CellSize(40).Data(days);
```

## Related

- Uptime (status per day rather than an amount) — `uptime.md`
- Charts (HeatMap for an arbitrary matrix, BarChart for buckets) — `charts.md`
- Metric (headline numbers next to it) — `metric.md`
- Full docs & API: `/tesserae/components/activity-calendar`
