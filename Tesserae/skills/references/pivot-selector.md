---
name: pivot-selector
description: A pivot variant that drives tab navigation through a Dropdown instead of a tab strip, with optional command buttons. Use when you have many tabs or a mobile-first layout in a Tesserae (C#/Transpose) app.
---

# PivotSelector

A tabbed surface where the tab picker is a `Dropdown` rather than a horizontal
strip. Suits a large number of tabs or narrow viewports.

## Create

`PivotSelector()` — returns a `PivotSelector`. Add tabs with the `.Pivot(...)`
extension. Bring factories into scope with `using static Tesserae.UI;`.

## Key configuration

- `.Pivot(id, title, contentCreator, cached = false)` — add a tab with a string title (`contentCreator` is `Func<IComponent>`).
- `.Pivot(id, titleCreator, contentCreator, cached = false)` — add a tab with a `Func<IComponent>` title (e.g. a styled button).
- `.Pivot(params (string id, string title, Func<IComponent> contentCreator)[] tabs)` — add many tabs at once.
- `.SetCommands(params IComponent[])` — buttons shown beside the dropdown.
- `.Select(id, refresh = false)` — switch tabs.
- `.OnNavigate(...)` / `.OnBeforeNavigate(...)` — callbacks; `e.Cancel()` blocks navigation.

## Sizing tab content

The content area lays out the active tab like a `Stack` with a single child, so
panel-filling content expands to the full available height instead of collapsing.
Size the content to fill with `.S()` / `.HS()`, `.Grow()`, or its own
`height: 100%`; fixed- or intrinsic-height content keeps its natural height and
the pane scrolls.

## Example

```csharp
using static Tesserae.UI;

var selector = PivotSelector()
    .SetCommands(Button().SetIcon(UIcons.Add).NoBorder().NoBackground().OnClick(() => {}))
    .Pivot("tab1", "Overview", () => Card(TextBlock("Overview").P(32)))
    .Pivot("tab2", "Details",  () => Card(TextBlock("Details").P(32)));
```

## Related

- Pivot — `pivot.md` (tab-strip variant)
- SegmentedPivot — `segmented-pivot.md`
- Full docs & API: `/tesserae/surfaces/pivot-selector`
