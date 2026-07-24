---
name: observable-stack
description: A Stack that renders a list of items and reconciles its DOM in place when an observable source changes, touching only inserted/removed/moved rows. Use when rendering a dynamic list where preserving scroll/focus matters in a Tesserae (C#/Transpose) app.
---

# ObservableStack

Renders items from an observable source into a host `Stack` and keeps it in sync. On
each change it diffs by item identity, so unchanged rows (and their scroll position and
focus) are left untouched — unlike rebuilding the whole stack.

## Create

`new ObservableStack<T>(IObservable<IReadOnlyList<T>> source, Func<T, IComponent> renderItem, Stack host = null)`
— construct directly (no static factory). `ObservableList<T>` implements the source
interface, so pass one and mutate it to drive updates. `renderItem` runs once per
newly-inserted item. Bring factories into scope with `using static Tesserae.UI;`.

## Key configuration

- Mutate the backing `ObservableList<T>` (`.Add`, `.RemoveAt`, `.ReplaceAll`, …) to reconcile.
- `.Horizontal()` / `.Vertical()` / `.HorizontalReverse()` / `.VerticalReverse()` — orientation.
- `.AlignItems(ItemAlign)` / `.AlignItemsCenter()` / `.JustifyContent(ItemJustify)` — flex alignment.
- `.Wrap()` / `.NoWrap()`, `.NoDefaultMargin()`, `.OverflowHidden()` — layout tweaks.
- `.WS()` / `.Clear()` — stretch width / remove all rows.

## Example

```csharp
using static Tesserae.UI;

var items = new ObservableList<string>("Item 1", "Item 2", "Item 3");

var stack = new ObservableStack<string>(
    items,
    item => Card(TextBlock(item)).WS().MB(4.px()));

// Mutating the list updates only the affected rows:
items.Add("Item 4");
items.RemoveAt(0);

return stack.Render();
```

## Related

- Stack — `stack.md`
- Observables — `observables.md`
- SortableStack — `sortable-stack.md`
- Full docs & API: `/tesserae/collections/observable-stack`
