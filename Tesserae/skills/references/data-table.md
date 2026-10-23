---
name: data-table
description: A table whose cells are components, with a header band and an optional state-coloured edge on each row. Use for KPI scorecards and dashboard panels of a dozen rows in a Tesserae (C#/Transpose) app.
---

# DataTable

A header band over rows that hold whatever the columns call for: a label, a `Sparkline`, a `TrendBadge`,
a `TargetBar`. A row can carry a coloured accent on its leading edge. Rows are just the components you hand
it; there is no sorting, selection or paging (use `DetailsList` for those). Below its minimum width (640px,
`--tss-datatable-min-width`) the table scrolls sideways.

## Create

`UI.DataTable()`. Bring factories into scope with `using static Tesserae.UI;`.

## Key configuration

- `.Column(string title, UnitSize width = null, DataTableAlign align = Start)` — add every column first; no width shares the remainder (`3.fr()`, `120.px()`).
- `.Row(params IComponent[] cells)` / `.Row(string accent, params IComponent[] cells)` — one component per column; `accent` is any CSS colour.
- `.Clear()` — remove the rows, keep the columns.
- `.Compact()` — tighter padding.

## Example

```csharp
using static Tesserae.UI;

DataTable()
    .Column("Metric", 3.fr())
    .Column("Trend (52wk)", 2.fr(), DataTableAlign.Center)
    .Column("This week", 1.fr(), DataTableAlign.End)
    .Column("WoW", 1.fr(), DataTableAlign.Center)
    .Column("vs run rate", 2.fr())
    .Row(Theme.Colors.Orange600,
        TextBlock("Net revenue per customer").Small(),
        Sparkline(series, 200, 36, Theme.Colors.Orange600).EndMarker().ReferenceLine().WS(),
        TextBlock("\u00a32,918").SemiBold(),
        TrendBadge.For(-31, "\u00a331", higherIsBetter: false),
        TargetBar(84).Warning().Label("Watch").ValueText("84%"));
```

## Related

- Sparkline, TrendBadge, TargetBar — `sparkline.md`, `trend-badge.md`, `target-bar.md`
- DetailsList — `details-list.md`
- Badge/Chip (`.Dot(...)` for count pills) — `badge.md`
