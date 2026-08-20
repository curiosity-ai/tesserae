---
name: grid
description: A CSS-Grid container with explicit column/row tracks, gaps, alignment, and per-item placement. Use for two-dimensional layouts in a Tesserae (C#/Transpose) app.
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

- `.Add(IComponent)` — add a child (its element becomes the grid item, tagged `tss-stack-item`).
- `.Columns(params UnitSize[])` / `.Rows(UnitSize[])` — (re)define tracks.
- `.Gap(UnitSize)` / `.RowGap(...)` / `.ColumnGap(...)` — spacing.
- `.AutoRows(UnitSize)` / `.AutoColumn(UnitSize)` / `.FlowColumn()` — implicit tracks and flow.
- `.AlignItems(ItemAlign)` / `.AlignItemsCenter()` / `.AlignContent(...)` / `.JustifyItems(ItemJustify)` / `.JustifyContent(...)`.
- `.Relative()`, `.OverflowHidden()`, `.NoDefaultMargin()`, `.Clear()`, `.Remove(c)`, `.Replace(new, old)`.

Place children (call **before** `.Add`, via `IComponent` extensions):
`.GridColumn(start, end)` / `.GridColumnStretch()` / `.GridRow(start, end)` / `.GridRowStretch()`.

## Height inside a Stack

A grid keeps its content height when it is a child of a vertical `Stack`, even when the
stack's children together overflow it — the stack scrolls (or clips), the sections inside
it do not shrink. That is what `min-height: min-content` on `.tss-grid` guarantees, and it
is what a page of grouped sections needs.

A grid that *is* a scroll viewport wants the opposite: give it a definite size with
`.Height(...)` (or `.MinHeight(0.px())` alongside `.MaxHeight(...)`) plus `.Scroll()` /
`.ScrollY()`, and it will shrink to the space it is given and scroll its own content.

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
