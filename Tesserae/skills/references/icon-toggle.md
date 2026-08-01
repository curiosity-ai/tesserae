---
name: icon-toggle
description: A segmented control of icon buttons where exactly one is selected at a time, exposing the selection as a typed value. Use when building a view-mode switcher or single-choice icon toolbar in a Tesserae (C#/Transpose) app.
---

# IconToggle

A row of icon buttons that behaves like a segmented control: an inset track with the
selected item lifted out of it as a raised pill, and exactly one item selected at a
time. Each item carries a `UIcons` glyph, a tooltip, an optional label and an
arbitrary data payload of type `T`, surfaced through an observable.

## Create

`IconToggle<T>(params IconToggle<T>.Item[] items)` — builds the toggle from items.
Build items with `IconToggleItem<T>(UIcons icon, string tooltip, T data, string text = null)`.
Bring the factories into scope with `using static Tesserae.UI;`. The first item that
isn't disabled is selected on render.

## Key configuration

- `IconToggleItem(icon, tooltip, data, text)` — one entry: glyph, hover tooltip, payload
  and an optional label next to the icon. `.SetText(string)` and `.Disabled(bool = true)`
  configure an item after the fact.
- `.Select(T item)` — programmatically select the item carrying that data; values that
  don't match any item are ignored.
- `.Selected` — the currently selected payload.
- `.AsObservable()` — `IObservable<T>` of the selected payload; observe it (or hand it to
  `DeferSync`) to drive content from the selection.
- `.OnChange((sender, value) => ...)` — fires on every change, but not for the initial
  selection.
- `.Bind(SettableObservable<T>)` — two-way binding: the control follows the observable and
  writes back to it.
- `.Compact()` / `.Large()` — a denser control for toolbars, or a roomier one for a
  page-level switch. The default sits comfortably next to other inline controls.
- `.Vertical()` / `.Horizontal()` — stack the items into a rail instead of a row (every
  item as wide as the widest); horizontal is the default.
- `.FullWidth()` — stretch the track to its container, every item taking an equal share.
- `.Rounded(BorderRadius radius = BorderRadius.Medium)` — reshape the track; the items
  follow along, so `BorderRadius.Full` gives a pill.
- `.Disabled(bool value = true)` — disable the whole control. Items disabled on their own
  stay disabled when it is re-enabled.

## Example

```csharp
using static Tesserae.UI;

public enum ViewMode { List, Grid, Cards }

var toggle = IconToggle<ViewMode>(
    IconToggleItem(UIcons.List, "List view", ViewMode.List),
    IconToggleItem(UIcons.Apps, "Grid view", ViewMode.Grid),
    IconToggleItem(UIcons.Grid, "Card view", ViewMode.Cards)
).Compact();

var panel = VStack().Children(
    toggle,
    DeferSync(toggle.AsObservable(), mode => RenderItems(mode))
);
```

With labels, as a mode selector:

```csharp
IconToggle(
    IconToggleItem(UIcons.Comment, "Ask the assistant", Mode.Chat,   "Chat"),
    IconToggleItem(UIcons.Search,  "Search everything", Mode.Search, "Search")
).FullWidth().OnChange((sender, mode) => Switch(mode));
```

## Related

- ChoiceGroup / Option — `option.md`
- SegmentedPivot (same look, but it owns the content panes) — `segmented-pivot.md`
- OmniBox (uses one as its Search/Chat mode selector) — `omni-box.md`
- Icon — `icon.md`
- Full docs & API: `/tesserae/components/icon-toggle`
