---
name: infinite-scrolling-list
description: A list that lazily loads more items as the user scrolls toward the bottom, rendering as a wrapping stack or a grid depending on the column count. Use when paging through a large/unbounded result set in a Tesserae (C#/h5) app.
---

# InfiniteScrollingList

Appends pages of items as the user scrolls near the bottom. A `VisibilitySensor` at the
end triggers the next-page fetch. One column width (or none) → wrapping `HStack`;
two or more column widths → `Grid`. For very large fixed datasets prefer `VirtualizedList`.

## Create

`InfiniteScrollingList(IComponent[] initialItems, Func<Task<IComponent[]>> getNextPage, params UnitSize[] columns)`
— returns an `InfiniteScrollingList`. Overloads accept a sync `Func<IComponent[]>` and/or omit the initial items.
Bring factories into scope with `using static Tesserae.UI;`.

The `getNextPage` delegate returns the next batch; return an empty array to stop loading.

## Key configuration

- `columns` (params `UnitSize[]`) — pass 2+ widths to lay out as a grid; otherwise a wrapping stack.
- `.WithEmptyMessage(Func<IComponent>)` — placeholder when there are no items.
- `.Height(unitSize)` — required so the scroll container can size and trigger loading.

## Example

```csharp
using static Tesserae.UI;

int page = 0;

return InfiniteScrollingList(
        new IComponent[0],
        async () =>
        {
            await Task.Delay(200);          // fetch next page
            page++;
            return Enumerable.Range(1, 20)
                .Select(i => (IComponent)Card(TextBlock($"Item {page}/{i}")).MinWidth(200.px()))
                .ToArray();
        })
    .Height(500.px())
    .Render();

// Grid layout: pass column widths →  InfiniteScrollingList(init, next, 33.percent(), 33.percent(), 34.percent())
```

## Related

- VirtualizedList — `../virtualized-list/SKILL.md`
- ItemsList — `../items-list/SKILL.md`
- Full docs & API: `/tesserae/collections/infinite-scrolling-list`
