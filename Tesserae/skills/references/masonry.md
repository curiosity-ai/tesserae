---
name: masonry
description: A Pinterest-style masonry layout that flows variable-height items into a fixed number of equal-width columns. Use when building a tile/card feed with uneven heights in a Tesserae (C#/h5) app.
---

# Masonry

Lays children of varying heights into N equal-width columns, packing them to minimise
gaps (wraps the `masonry-layout` JS library; relayout is debounced). Needs a definite
width — a stretched Masonry collapses and cards overlap.

## Create

`Masonry(int columns, int gutter = 10)` — returns a `Masonry` (`columns` lanes, `gutter` px spacing).
Bring factories into scope with `using static Tesserae.UI;`.

## Key configuration

- `.W(px)` / `.Width(unitSize)` — give it a definite width (required for correct layout).
- `.Add(IComponent)` — append a tile (triggers relayout). Give each tile a real height.
- `.Remove(IComponent)` / `.Replace(new, old)` / `.Clear()` — mutate tiles.
- `.Background` / `.Margin` / `.Padding` — CSS string properties.

## Example

```csharp
using static Tesserae.UI;

// Fixed width so columns can be measured.
var masonry = Masonry(4).W(480);

for (int i = 0; i < 8; i++)
{
    var card = Card(VStack().AlignItemsCenter().JustifyContent(ItemJustify.Center)
                    .Children(TextBlock($"Card {i}").NoWrap()))
               .H(60 + (i % 3) * 30);   // variable height
    masonry.Add(card);
}

return masonry.Render();
```

## Related

- Grid — `/tesserae/collections/grid` (fixed rows/columns)
- Stack — `stack.md`
- Full docs & API: `/tesserae/collections/masonry`
