---
name: sortable-stack
description: A Stack whose child items can be reordered by drag-and-drop, reporting the new order via a callback. Use when the visual order matters and must be persisted or reacted to in a Tesserae (C#/h5) app.
---

# SortableStack

Wraps a `Stack` and makes its children draggable (via SortableJS). Each item carries a
stable string identifier; when the user finishes a drag, the new ordering is reported as
an array of identifiers.

## Create

`SortableStack(Stack.Orientation orientation = Vertical)`, or the shortcuts
`VSortableStack()` / `HSortableStack()` — return a `SortableStack`.
Bring factories into scope with `using static Tesserae.UI;`.

## Key configuration

- `.Add(string identifier, IComponent component)` — append a draggable item (identifier is the ordering key).
- `.OnSortingChanged(Action<string[]>)` — fired with the current identifier order after each reorder.
- `.LoadSorting(string[] order)` — apply a saved order (call after all items are added).
- `.Remove(string identifier)` / `.Clear()` — remove one / all items.
- `.Children(params SortableStackItem[])` — set all items at once (`SortableStackItem { Identifier, Component }`).
- `.AlignItemsCenter()`, `.JustifyContent(ItemJustify)`, `.Wrap()` / `.NoWrap()`, `.NoDefaultMargin()`, `.WS()` — layout.

## Example

```csharp
using static Tesserae.UI;

var sortable = VSortableStack().NoDefaultMargin().WS();
sortable.Add("alpha", Card(TextBlock("Alpha")).WS().MB(4.px()));
sortable.Add("beta",  Card(TextBlock("Beta")).WS().MB(4.px()));
sortable.Add("gamma", Card(TextBlock("Gamma")).WS().MB(4.px()));

sortable.OnSortingChanged(order =>
    Console.WriteLine(string.Join(", ", order))); // e.g. "beta, alpha, gamma"

return sortable.Render();
```

## Related

- Stack — `stack.md`
- ObservableStack — `observable-stack.md`
- Full docs & API: `/tesserae/collections/sortable-stack`
