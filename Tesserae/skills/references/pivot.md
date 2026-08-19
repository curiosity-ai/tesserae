---
name: pivot
description: A horizontal tabbed surface showing one panel at a time, with scrolling, an overflow menu, closeable tabs and keyboard nav. Use when organizing content into switchable tabs in a Tesserae (C#/Transpose) app.
---

# Pivot

A tab strip with one content panel visible at a time. Tabs scroll horizontally,
overflow into a "more" menu, and can be cycled with arrow keys.

## Create

`Pivot()` — returns a `Pivot`. Add tabs with the `.Pivot(...)` extension. Bring
factories into scope with `using static Tesserae.UI;`.

## Key configuration

- `.Pivot(id, titleCreator, contentCreator, cached = false, closeable = false, onClosed = null)` — add a tab. `titleCreator`/`contentCreator` are `Func<IComponent>`; `cached: true` keeps content alive between switches.
- `PivotTitle("Text")` / `PivotTitle("Text", UIcons.Folder)` — convenient title `Func<IComponent>`. A custom title component gets no padding of its own, so build one from `Button(text).NoBackground().Regular()` if you need to go beyond these; for a tab that shows an unsaved-changes "*", use `TabSaveIndicator.Title(id, "Text")` (`unsaved-changes-guard.md`).
- `.Host(Modal modal, id, titleCreator, closeable = true, onClosed = null)` — embed a `Modal` as a tab (basis of TabbedModal).
- `.Select(id, refresh = false)` — switch to a tab.
- `.RemoveTab(id)` — remove a tab.
- `.Centered()` / `.Justified()` — tab-strip alignment.
- `.HideIfSingle()` — hide the strip when only one tab exists.
- `.EnableCtrlTabSwitching()` — Ctrl+Alt+Left/Right cycles tabs.
- `.Reorderable(reorderable = true)` — let the user drag tab titles to reorder the strip. Pass `false` to turn dragging off again (the current order is kept).
- `.OnReorder(...)` — raised after a user drag; the event carries `TabId`, `OldIndex`, `NewIndex` and `TabIds` (every id in the new order). Not raised by `.MoveTab(...)`.
- `.MoveTab(id, newIndex)` — reorder programmatically (index is clamped into range), e.g. to restore a saved order.
- `.TabIds` — the tab ids in strip order.
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

## Reordering by drag

`.Reorderable()` makes the tab titles draggable along the strip. Clicking a tab
still selects it and the close button still closes it — only a drag reorders.
Use `.OnReorder(...)` to persist the order, and `.MoveTab(...)` to restore it:

```csharp
var pivot = Pivot()
    .Reorderable()
    .OnReorder((s, e) => SaveTabOrder(e.TabIds));

foreach (var id in LoadTabOrder().Select((id, i) => (id, i)))
{
    pivot.MoveTab(id.id, id.i);
}
```

## Related

- PivotSelector — `pivot-selector.md` (dropdown-driven variant)
- SegmentedPivot — `segmented-pivot.md`
- CardPivot — `card-pivot.md`
- TabbedModal — `tabbed-modal.md`
- UnsavedChangesGuard / TabSaveIndicator (warn before losing a dirty tab) — `unsaved-changes-guard.md`
- Full docs & API: `/tesserae/surfaces/pivot`
