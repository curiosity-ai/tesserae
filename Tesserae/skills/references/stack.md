---
name: stack
description: A flexbox container that lays out child components vertically or horizontally with alignment, wrapping, gap, and per-child sizing. Use as the default one-axis layout container in a Tesserae (C#/Transpose) app.
---

# Stack

The workhorse layout container — a flexbox abstraction. Default orientation is vertical;
the cross-axis stretches by default. Use per-child sizing helpers (`.WS()`, `.Grow()`, …)
to control how children claim space.

## Create

`Stack(Orientation orientation = Vertical)`, or the shortcuts `VStack()` (vertical) and
`HStack()` (horizontal). Bring factories into scope with `using static Tesserae.UI;`.
Add children with `.Add(component)` or `.Children(params IComponent[])`.

## Key configuration

- `.Vertical()` / `.Horizontal()` / `.VerticalReverse()` / `.HorizontalReverse()` — orientation.
- `.AlignItems(ItemAlign)` / `.AlignItemsCenter()` — cross-axis alignment of children.
- `.JustifyContent(ItemJustify)` — main-axis distribution (Start, Center, End, Between, Around, Evenly).
- `.Gap(unitSize)` / `.RowGap(...)` / `.ColumnGap(...)` — spacing between children.
- `.Wrap()` / `.NoWrap()`, `.Inline()`, `.OverflowHidden()`, `.NoDefaultMargin()`, `.Relative()`.
- `.Add` / `.Prepend` / `.InsertBefore` / `.InsertAfter` / `.Remove` / `.Replace` / `.Clear` — mutate children.
- On children (from IComponentExtensions): `.WS()`/`.HS()`/`.S()` stretch, `.Grow(int)` claim space, `.Shrink()`/`.NoShrink()`, `.W(...)`/`.H(...)`.

## Example

```csharp
using static Tesserae.UI;

var stack = Stack().Vertical().AlignItemsCenter().NoWrap();
stack.Add(Button("Button 1"));
stack.Add(Button("Button 2"));
return stack.Render();

// Toolbar: push items apart with JustifyContent
// HStack().AlignItemsCenter().JustifyContent(ItemJustify.Between)
//     .Children(Button("Left"), Button("Right"));
```

## Related

- Grid — `/tesserae/collections/grid` (two-axis layout)
- SortableStack — `sortable-stack.md`, ObservableStack — `observable-stack.md`
- Full docs & API: `/tesserae/collections/stack`
