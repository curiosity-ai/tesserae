---
name: details-grid
description: A table of label/value rows - the Owner / Size / Modified block of metadata a preview shows - either side by side or stacked, with the label as a small header over its value, in one or more columns. Use when listing a handful of named fields about one thing in a Tesserae (C#/Transpose) app.
---

# DetailsGrid

A bordered table of label/value rows. The labels read in the secondary color and
share one column so the values line up; values are plain text or any component.
This is the metadata block a preview (e.g. an `OmniResult` modal) shows about the
thing it is previewing — not a data grid: for many rows of many columns, use
`DetailsList`.

## Create

`UI.DetailsGrid(int columns = 1)` — an empty grid, laid out in that many label/value columns; add rows fluently.
Bring factories into scope with `using static Tesserae.UI;`.

## Key configuration

- `.Row(string label, string value, bool skipIfEmpty = false)` — a row with a plain-text value. A null or empty value still gets its row, drawn as an em dash, unless `skipIfEmpty` is true. Pass `(string)null` explicitly when the value is a null literal, or the `IComponent` overload is picked instead.
- `.Row(string label, IComponent value)` — a row whose value is a component (a `Link`, a `Badge`, an `Avatar`, a row of them). A null component leaves the row out.
- `.Clear()` — remove every row.
- `.LabelWidth(UnitSize)` — how wide the label column is (default `120px`).
- `.Columns(int)` — lay the rows out n-up instead of one under the other (same as the factory's parameter). Past one column the rules between rows are dropped — the last row of the grid is several children, not the last child — and the rows are told apart by the space around them.
- `.Stacked(bool = true)` / `.Mode(DetailsGridMode)` — put each label *above* its value as a small, semibold, uppercase header in the secondary colour, and drop the frame and the rules. The mode for a sheet of dates and references whose labels ("FIRST MSN AFFECTED") are longer than the values under them; pair it with `Columns(n)` to read two or three across.
- `.Compact(bool = true)` — tighter rows.
- `.NoBorder(bool = true)` — drop the frame and the rules between rows.
- `.Count` — how many rows the grid has.

## Example

```csharp
using static Tesserae.UI;

var details = DetailsGrid()
    .Row("Location", "sample-files / analysis")
    .Row("Size",     "480 KB")
    .Row("Owner",    HStack().AlignItemsCenter().Children(Avatar(initials: "ML").Size(AvatarSize.Small).MR(8), TextBlock("Marie Lang")))
    .Row("Status",   Badge("Approved").Pill().Success())
    .Row("Retention", (string)null)   // shown as an em dash
    .MaxWidth(480.px());
```

## Stacked

```csharp
using static Tesserae.UI;

var header = DetailsGrid(2).Stacked()
    .Row("ATA · Sub-ATA",      "73 · 73-21")
    .Row("First MSN affected", "MSN 0142")
    .Row("Issue number",       "Iss. 3")
    .Row("Impacted docs",      "12")
    .Row("Mod available",      "14 Oct 2025")
    .Row("PAWO end date",      "23 Oct 2025");
```

## Related

- OmniResult — the modal a result opens into is where this usually sits — `omni-result.md`
- DetailsList — many rows, many columns, sorting and grouping — `details-list.md`
- Metric — one number with a label, for a dashboard tile — `metric.md`
- Full docs & API: `/tesserae/components/details-grid`
