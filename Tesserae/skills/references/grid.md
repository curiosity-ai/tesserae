---
name: grid
description: A CSS-Grid container with explicit column/row tracks, gaps, alignment, and per-item placement. Use for two-dimensional layouts in a Tesserae (C#/h5) app.
---

# Grid

A CSS Grid layout container. Define column (and optionally row) tracks with
`UnitSize[]`, set gaps and alignment, and add children with `.Add(...)`. Items
can be placed/stretched across tracks. Implements `ISpecialCaseStyling`, so
sizing helpers write directly onto the grid element (no extra wrapper). Same
component is documented under both `/tesserae/components/grid` and
`/tesserae/collections/grid`.

## Create

`UI.Grid(params UnitSize[] columns)` or `UI.Grid(UnitSize[] columns, UnitSize[] rows)` —
or `new Grid(...)`. Track sizes use unit helpers like `1.fr()`, `200.px()`, or a raw
`new UnitSize("repeat(auto-fit, minmax(min(200px, 100%), 1fr))")`.
Bring factories into scope with `using static Tesserae.UI;`.

## Key configuration

- `.Add(IComponent)` — add a child (wrapped in a `tss-stack-item`).
- `.Columns(params UnitSize[])` / `.Rows(UnitSize[])` — (re)define tracks.
- `.Gap(UnitSize)` / `.RowGap(...)` / `.ColumnGap(...)` — spacing (a `4px` gap is applied by default).
- `.NoGap()` — removes the default gap between items.
- `.AutoRows(UnitSize)` / `.AutoColumn(UnitSize)` / `.FlowColumn()` — implicit tracks and flow.
- `.AlignItems(ItemAlign)` / `.AlignItemsCenter()` / `.AlignContent(...)` / `.JustifyItems(ItemJustify)` / `.JustifyContent(...)`.
- `.Relative()`, `.OverflowHidden()`, `.Clear()`, `.Remove(c)`, `.Replace(new, old)`.

Place children (call **before** `.Add`, via `IComponent` extensions):
`.GridColumn(start, end)` / `.GridColumnStretch()` / `.GridRow(start, end)` / `.GridRowStretch()`.

## Example

```csharp
using static Tesserae.UI;

var grid = Grid(columns: new[] { 1.fr(), 1.fr(), 200.px() }).Gap(8.px());

grid.Add(Button().SetText("Header").WS().Primary().GridColumnStretch().GridRow(1, 2));
Enumerable.Range(1, 10).ForEach(v => grid.Add(Button().SetText($"Item {v}")));

// responsive auto-fit:
var responsive = Grid(new UnitSize("repeat(auto-fit, minmax(min(200px, 100%), 1fr))")).Gap(8.px());
```

## Related

- Stack — `stack.md` (one-axis flow)
- Masonry — variable-height columns
- Full docs & API: `/tesserae/collections/grid` and `/tesserae/components/grid`
