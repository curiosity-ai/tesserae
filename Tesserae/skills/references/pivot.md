---
name: pivot
description: A horizontal tabbed surface showing one panel at a time, with scrolling, an overflow menu, closeable tabs and keyboard nav. Use when organizing content into switchable tabs in a Tesserae (C#/h5) app.
---

# Pivot

A tab strip with one content panel visible at a time. Tabs scroll horizontally,
overflow into a "more" menu, and can be cycled with arrow keys.

## Create

`Pivot()` — returns a `Pivot`. Add tabs with the `.Pivot(...)` extension. Bring
factories into scope with `using static Tesserae.UI;`.

## Key configuration

- `.Pivot(id, titleCreator, contentCreator, cached = false, closeable = false, onClosed = null)` — add a tab. `titleCreator`/`contentCreator` are `Func<IComponent>`; `cached: true` keeps content alive between switches.
- `PivotTitle("Text")` / `PivotTitle("Text", UIcons.Folder)` — convenient title `Func<IComponent>`.
- `.Host(Modal modal, id, titleCreator, closeable = true, onClosed = null)` — embed a `Modal` as a tab (basis of TabbedModal).
- `.Select(id, refresh = false)` — switch to a tab.
- `.RemoveTab(id)` — remove a tab.
- `.Centered()` / `.Justified()` — tab-strip alignment.
- `.HideIfSingle()` — hide the strip when only one tab exists.
- `.EnableCtrlTabSwitching()` — Ctrl+Alt+Left/Right cycles tabs.
- `.OnNavigate(...)` / `.OnBeforeNavigate(...)` — navigation callbacks; call `e.Cancel()` in before-navigate to block.
- `.SelectedTab` — id of the current tab.

## Example

```csharp
using static Tesserae.UI;

var pivot = Pivot()
    .Pivot("first",  PivotTitle("First"),  () => TextBlock("Content one"))
    .Pivot("second", PivotTitle("Second"), () => TextBlock("Content two"), cached: true)
    .Centered();
```

## Related

- PivotSelector — `pivot-selector.md` (dropdown-driven variant)
- SegmentedPivot — `segmented-pivot.md`
- CardPivot — `card-pivot.md`
- TabbedModal — `tabbed-modal.md`
- Full docs & API: `/tesserae/surfaces/pivot`
