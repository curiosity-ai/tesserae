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

- `.Pivot(id, titleCreator, contentCreator, cached = false, closeable = false, onClosed = null, onBeforeClose = null)` — add a tab. `titleCreator`/`contentCreator` are `Func<IComponent>`; `cached: true` keeps content alive between switches (hidden, not torn down, so an editor in it keeps its caret and undo history); `closeable: true` adds the close cross and lets a middle click close the tab; `onBeforeClose` is awaited first and a `false` keeps the tab (the place for an unsaved-changes prompt).
- `PivotTitle("Text")` / `PivotTitle("Text", UIcons.Folder)` — convenient title `Func<IComponent>`. A custom title component gets no padding of its own, so build one from `Button(text).NoBackground().Regular()` if you need to go beyond these; for a tab that shows an unsaved-changes marker, use `TabSaveIndicator.Title(id, "Text")` (`unsaved-changes-guard.md`).
- `.Host(Modal modal, id, titleCreator, closeable = true, onClosed = null)` — embed a `Modal` as a tab (basis of TabbedModal).
- `.Select(id, refresh = false)` — switch to a tab.
- `.RemoveTab(id)` — remove a tab. Removing the selected tab selects its neighbour — the tab that took its place on the strip, or the last one when it was the last.
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

## The close cross, and the unsaved marker that stands in for it

A `closeable: true` tab ends in its close cross, which is simply there — the tab's own
foreground colour, turning `danger` under the pointer. What changes is what occupies that
spot when the tab has unsaved changes:

| Tab state | The spot shows |
| --- | --- |
| clean | the close cross |
| unsaved changes (`TabSaveIndicator.MarkDirty`) | a dot in the cross's place |
| unsaved changes, pointer anywhere on the tab, or the cross keyboard-focused | the close cross again |

The marker replaces the cross rather than sitting beside it, so a tab never resizes when its
editor goes dirty, and the two can't crowd each other. Pointing at the tab always brings the
cross back, so there is always something to close with.

The cross is reachable by keyboard: it takes focus after its tab title, and Enter or Space
closes the tab through the same `onBeforeClose` guard a click goes through — as does a middle
click anywhere on the tab, the gesture every browser's tab strip teaches. On a touch
screen — where nothing hovers, so the cross could never be brought back — the marker sits
beside the label instead. A tab that is *not* closeable has no cross to stand in for, so its
marker sits beside the label too.

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

Selecting and dragging do not compete for the same press, which is what makes a
reorderable strip still feel like a set of buttons:

- **The press selects**, not the release — as in VS Code and in every browser's own tab
  strip. So a click that turns into a drag has already switched to the tab it picked up,
  and a click that merely wobbled cannot be swallowed by the drag.
- **A drag only starts once the pointer has travelled 5px.** Below that the gesture stays
  a click, and the strip does not move under the cursor.
- **On a touch screen the drag waits for the press to be held** (about a quarter of a
  second) instead of for it to travel, since the swipe that would drag a tab is also the
  one that scrolls the strip: a swipe scrolls, a long press picks the tab up, a tap
  selects.

## Related

- PivotSelector — `pivot-selector.md` (dropdown-driven variant)
- SegmentedPivot — `segmented-pivot.md`
- CardPivot — `card-pivot.md`
- TabbedModal — `tabbed-modal.md`
- UnsavedChangesGuard / TabSaveIndicator (warn before losing a dirty tab) — `unsaved-changes-guard.md`
- Full docs & API: `/tesserae/surfaces/pivot`
