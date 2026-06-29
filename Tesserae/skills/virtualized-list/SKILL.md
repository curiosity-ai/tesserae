---
name: virtualized-list
description: A list that renders only the pages of items currently in (or near) the viewport, recycling DOM as the user scrolls, for very large datasets. Use when displaying thousands of equal-height items in a Tesserae (C#/h5) app.
---

# VirtualizedList

Breaks the dataset into pages and keeps only a small "materialized window" of pages in the
DOM, swapping pages in/out as you scroll. This keeps DOM size constant for huge lists.
Items should have a consistent height so scroll position is computed correctly.

## Create

`VirtualizedList(int rowsPerPage = 4, int columnsPerRow = 4)` — returns a `VirtualizedList`.
Bring factories into scope with `using static Tesserae.UI;`.
Configuration is via the constructor params plus the fluent methods below.

## Key configuration

- `.WithListItems(IEnumerable<IComponent>)` — set the items (the full dataset; only visible pages render).
- `.WithEmptyMessage(Func<IComponent>)` — placeholder shown when the collection is empty.
- `.Height(unitSize)` — sets the scroll viewport height (required for scrolling).
- Constructor `rowsPerPage` / `columnsPerRow` — page granularity and grid columns.

## Example

```csharp
using static Tesserae.UI;

var list = VirtualizedList()
    .WithListItems(Enumerable.Range(1, 5000).Select(n => (IComponent)new Row(n)))
    .Height(400.px());

// Empty state instead:
// VirtualizedList()
//     .WithEmptyMessage(() => TextBlock("No data to display"))
//     .WithListItems(Enumerable.Empty<IComponent>());

return list.Render();
```

## Related

- DetailsList — `../details-list/SKILL.md` (typed, sortable, virtualised table)
- InfiniteScrollingList — `../infinite-scrolling-list/SKILL.md` (lazy paging)
- ItemsList — `../items-list/SKILL.md` (small, non-virtualised)
- Full docs & API: `/tesserae/collections/virtualized-list`
