---
name: icon-toggle
description: A segmented control of icon buttons where exactly one is selected at a time, exposing the selection as a typed value. Use when building a view-mode switcher or single-choice icon toolbar in a Tesserae (C#/h5) app.
---

# IconToggle

A row of icon buttons that behaves like a segmented control: exactly one item is
selected. Each item carries a `UIcons` glyph, a tooltip and an arbitrary data
payload of type `T`, surfaced through an observable.

## Create

`IconToggle<T>(params IconToggle<T>.Item[] items)` — builds the toggle from items.
Build items with `IconToggleItem<T>(UIcons icon, string tooltip, T data)`.
Bring the factories into scope with `using static Tesserae.UI;`. The first item
is selected by default.

## Key configuration

- `.Select(T item)` — programmatically select the item carrying that data.
- `.AsObservable()` — `IObservable<T>` of the currently selected payload; observe it to react to changes.
- `IconToggleItem(icon, tooltip, data)` — one entry: glyph, hover tooltip, payload.

## Example

```csharp
using static Tesserae.UI;

public enum ViewMode { List, Grid, Cards }

var toggle = IconToggle<ViewMode>(
    IconToggleItem(UIcons.List, "List view", ViewMode.List),
    IconToggleItem(UIcons.Apps, "Grid view", ViewMode.Grid),
    IconToggleItem(UIcons.Grid, "Card view", ViewMode.Cards)
);

toggle.AsObservable().Observe(mode => Console.WriteLine($"Now: {mode}"));
```

## Related

- ChoiceGroup / Option — `option.md`
- Icon — `icon.md`
- Full docs & API: `/tesserae/components/icon-toggle`
