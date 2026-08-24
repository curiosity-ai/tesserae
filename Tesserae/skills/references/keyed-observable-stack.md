---
name: keyed-observable-stack
description: A Stack driven by an observable list of keyed components that reconciles against the live DOM - matching rows by their Identifier, re-rendering one only when its ContentHash changes, and moving the rest. Use for a server-driven or streaming list that reorders in a Tesserae (C#/Transpose) app.
---

# KeyedObservableStack

`KeyedObservableStack` renders an `ObservableList<IComponentWithID>` and, on every change,
reconciles the DOM instead of rebuilding it: rows are matched by their string
`Identifier`, a matched row is re-rendered only when its `ContentHash` changed, surviving
rows are moved into the new order, dropped rows are removed and new ones inserted. Changes
are debounced by default.

Arbitrary reorders are the point: shuffling the middle of the list moves the existing
elements rather than rebuilding the span, so scroll position, focus and any DOM state in
the untouched rows survive.

**Which of the two observable stacks?**

| | `KeyedObservableStack` | `ObservableStack<T>` (`observable-stack.md`) |
|---|---|---|
| Item | an `IComponentWithID` — its own component | any `T`, rendered by a factory you pass |
| Matched by | the `Identifier` string | reference identity |
| Re-render | when `ContentHash` changes | never — a row refreshes itself |
| Reorder | any permutation | diffs a common prefix/suffix, so an interior reorder rebuilds that span |

Reach for this one when the items *are* rendered components carrying a stable key and a
cheap content hash; for data objects with self-managing rows, `ObservableStack<T>` is the
lighter fit.

## Create

`new KeyedObservableStack(ObservableList<IComponentWithID> items, Stack.Orientation orientation = Vertical, bool debounce = true)`
— construct directly (there is no `UI.` factory). Pass `debounce: false` when every change
must land in the same frame.

Each item implements `IComponentWithID`, which is `IComponent` plus two strings:

```csharp
public interface IComponentWithID : IComponent
{
    string Identifier  { get; }   // stable per item, and unique in the list
    string ContentHash { get; }   // changes exactly when the rendered content would
}
```

`Identifier` is what survives a reorder; `ContentHash` is what decides a re-render. A hash
that never changes leaves a stale row on screen; one that changes every time throws away
the win.

## Key configuration

- `.Horizontal()` / `.Vertical()` / `.HorizontalReverse()` / `.VerticalReverse()` — orientation
  (also `.StackOrientation`).
- `.AlignItems(ItemAlign)` / `.AlignItemsCenter()` / `.AlignContent(...)` /
  `.JustifyContent(ItemJustify)` / `.JustifyItems(...)` — alignment.
- `.Wrap()` / `.NoWrap()`, `.Inline()`, `.OverflowHidden()`, `.NoDefaultMargin()`,
  `.Relative()` — the same layout tweaks a `Stack` takes.
- `.Clear()` — empty the rendered stack.
- `.OnMouseOver(...)` / `.OnMouseOut(...)`, `.Background` / `.Margin` / `.Padding`.
- Mutating the `ObservableList` is what drives it: `.Add`, `.RemoveAt`, `.ReplaceAll`, …

## Example

```csharp
using static Tesserae.UI;

public class Row : IComponentWithID
{
    public Row(string id, string name) { Id = id; Name = name; }

    public string Id   { get; }
    public string Name { get; set; }

    public string Identifier  => Id;                                  // survives reordering
    public string ContentHash => Id + "|" + Name;                     // re-render trigger

    public HTMLElement Render() =>
        Card(HStack().WS().AlignItemsCenter().Children(
            TextBlock(Name).SemiBold().Grow(),
            TextBlock(Id).Small())).Render();
}

var items = new ObservableList<IComponentWithID>();
items.ReplaceAll(new IComponentWithID[] { new Row("1", "Alpha"), new Row("2", "Beta") });

var stack = new KeyedObservableStack(items);

// Reordering moves the existing elements; only a row whose hash changed is re-rendered.
items.ReplaceAll(items.Value.Reverse().ToArray());
```

## Related

- ObservableStack — the reference-identity sibling — `observable-stack.md`
- Observables — `ObservableList<T>` and friends — `observables.md`
- Stack — the container this renders as — `stack.md`
- Chat — the streaming transcript this shape suits — `chat.md`
- Full docs & API: `/tesserae/collections/keyed-observable-stack`
