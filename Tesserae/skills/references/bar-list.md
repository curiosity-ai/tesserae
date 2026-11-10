---
name: bar-list
description: A ranked list of horizontal bars - label on the left, a thin bar on a grey track, the formatted value on the right - for "top paths", "top countries" or "TLS versions" cards. Use in a Tesserae (C#/Transpose) app when a few labelled values are compared by size and the labels are too long for the axis of a BarChart.
---

# BarList

One row per item: the label, a thin rounded bar drawn against a grey track, and the
value. Bars are scaled to the largest value (or to `Max`), a label that does not fit
is cut with an ellipsis and shows its full text in a tooltip, and the value column
lines up across rows. It is plain HTML (not SVG) and fills the width it is given.

## Create

`UI.BarList()` — an empty list. Bring the factory into scope with `using static Tesserae.UI;`.

## Key configuration

- `.Add(string label, double value, string color = null)` — append a row; a null color uses the list's.
- `.Items(string[] labels, double[] values)` — replace all rows.
- `.Clear()` — remove every row.
- `.FormatValues(Func<double, string>)` — how a value is written (default: `69.28k`, `1.2M`, ...); pass your own for bytes, currency, durations.
- `.Max(double)` — the value a full track stands for (default: the largest value in the list).
- `.Color(string)` — color of bars with none of their own (default: the primary color).
- `.Thickness(UnitSize)` — bar height (default 6px).
- `.LabelWidth(UnitSize)` — fix the label column so stacked lists line up (default: flexible).

## Example

```csharp
using static Tesserae.UI;

Card(BarList()
    .Add("/downloads/installer.zip", 104.08e9)
    .Add("/pricing",           1.62e9)
    .Add("/contact", 173.38e6)
    .FormatValues(b => b >= 1e9 ? (b / 1e9).ToString("0.00") + " GB" : (b / 1e6).ToString("0.00") + " MB"))
  .SetTitle("Top pages by data transfer");
```

## Related

- [charts](charts.md) — the SVG chart types; use `BarChart` when the categories are short and an axis is wanted.
- [contribution-bar](contribution-bar.md) — one stacked bar showing how parts add up to a total.
- [metric](metric.md) — headline numbers to sit above a list like this.
