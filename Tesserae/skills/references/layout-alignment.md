---
name: layout-alignment
description: Stack (flexbox) and Grid (CSS grid) layout primitives plus alignment, placement, and sizing extensions. Use when arranging, aligning, or placing components in a Tesserae (C#/h5) app.
---

# Layout & Alignment

Tesserae arranges UI with `Stack` (flexbox) and `Grid` (CSS grid). Alignment,
placement, and sizing are applied through fluent APIs and `IComponent`
extensions. Bring factories into scope with `using static Tesserae.UI;`.

## Create

- `HStack()` / `VStack()` — horizontal / vertical flex container.
- `Stack(Orientation)` — explicit orientation.
- `Grid(params UnitSize[] columns)` — CSS grid with column tracks.

Pass children with `.Children(...)`; append one with `.Add(...)`.

## Stack alignment

- `.AlignItems(...)` / `.AlignContent(...)`
- `.JustifyContent(ItemJustify.Between | Center | ...)` / `.JustifyItems(...)`
- `.AlignItemsCenter()` — shorthand.
- `.CanWrap` / `.IsInline` — wrapping / inline behavior.

Align an individual child (extensions mapping to `Stack.SetAlign`/`SetJustify`):
`.AlignCenter()`, `.AlignEnd()`, `.AlignStretch()`.

## Grid

- `.Columns(...)` / `.Rows(...)` — track templates (`UnitSize[]`).
- `.Gap(...)` / `.RowGap(...)` / `.ColumnGap(...)`.
- Place a child (call **before** adding it): `.GridColumn(start, end)`,
  `.GridRow(start, end)`, `.GridColumnStretch()`. These map to
  `Grid.SetGridColumn`/`SetGridRow`.

## Sizing / spacing helpers

- `.Stretch()` / `.S()` — width and height to 100%; `.WS()` / `.HS()` for one axis.
- `.Grow(int)` / `.Shrink()` / `.NoShrink()` — flex-grow / flex-shrink on stack items.
- `.W(...)` / `.H(...)`, `.P(...)`, `.M(...)` — fixed size, padding, margin.

## Example

```csharp
using static Tesserae.UI;

var toolbar = HStack()
    .AlignItemsCenter()
    .JustifyContent(ItemJustify.Between)
    .Children(Button("Cancel"), Button("Save").Primary());

var grid = Grid(1.fr(), 2.fr())
    .Rows(new[] { 64.px(), 1.fr() })
    .Gap(16.px());
grid.Add(Card(TextBlock("A").TextCenter()).WS());
grid.Add(Card(TextBlock("B").TextCenter()).WS().GridColumn(1, 3)); // span both columns
```

## Related

- Styling (UnitSize, shorthands) — `.styling.md`
- Core Concepts — `.core-concepts.md`
- Full docs & API: `/tesserae/get-started/layout-alignment`
