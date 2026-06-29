---
name: items-list
description: A simple, non-virtualised list of arbitrary components, laid out as a wrapping stack (one column) or a grid (multiple columns), backed by an observable item list. Use when displaying a small set of items in a Tesserae (C#/h5) app.
---

# ItemsList

Renders a small set of `IComponent`s. One column width (or none) → wrapping scroll
stack; two or more → grid. Items are stored in an `ObservableList<IComponent>`, so
mutating that list updates the view. For large datasets use `VirtualizedList`/`DetailsList`.

## Create

`ItemsList(IComponent[] items, params UnitSize[] columns)` — returns an `ItemsList`.
An overload takes an `ObservableList<IComponent>` directly.
Bring factories into scope with `using static Tesserae.UI;`.

## Key configuration

- `columns` (params `UnitSize[]`) — 2+ widths → grid layout; otherwise a wrapping stack.
- `.Items` — the backing `ObservableList<IComponent>`; `.Add(...)` / `.Clear()` / `.ReplaceAll(...)` to mutate live.
- `.WithEmptyMessage(Func<IComponent>)` — placeholder rendered when the list is empty.
- `.Height(unitSize)` — fixes height; the container scrolls past it.

## Example

```csharp
using static Tesserae.UI;

var items = Enumerable.Range(1, 12).Select(n =>
    (IComponent)Card(TextBlock($"Contact {n}"))).ToArray();

var list = ItemsList(items, 100.percent())   // single column → stack
    .Height(320.px())
    .WithEmptyMessage(() =>
        BackgroundArea(Card(TextBlock("Empty list").Padding(16.px()))).WS().HS().MinHeight(100.px()));

// Grid: ItemsList(items, 25.percent(), 50.percent(), 25.percent())
```

## Related

- VirtualizedList — `virtualized-list.md` (large datasets)
- SearchableList — `searchable-list.md`
- Full docs & API: `/tesserae/collections/items-list`
