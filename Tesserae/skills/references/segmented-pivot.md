---
name: segmented-pivot
description: A pivot styled as a connected segmented (pill) control for switching between a few closely related views, with horizontal scrolling and an overflow menu when the segments don't fit. Use when toggling related views or filters in limited space in a Tesserae (C#/h5) app.
---

# SegmentedPivot

A tabbed surface whose tabs render as a single connected segmented control.
Best for a small number of closely related views or filters.

When the segments are wider than the available space, the strip scrolls
horizontally (mouse wheel or the chevron buttons that appear on each side) and
an overflow (⋯) button lists every segment in a menu — the same overflow
mechanics as `Pivot`.

## Create

`SegmentedPivot()` — returns a `SegmentedPivot`. Add tabs with the
`.SegmentedPivot(...)` extension. Bring factories into scope with
`using static Tesserae.UI;`.

## Key configuration

- `.SegmentedPivot(id, titleCreator, contentCreator, cached = false)` — add a segment. `titleCreator`/`contentCreator` are `Func<IComponent>`.
- `SegmentTitle("Text")` / `SegmentTitle("Text", UIcons.Rocket)` — convenient title `Func<IComponent>`.
- `.Select(id, refresh = false)` — switch segment.
- `.OnNavigate(...)` / `.OnBeforeNavigate(...)` — callbacks; `e.Cancel()` blocks navigation.
- `.RefreshPivotSizes()` — re-evaluate the scroll/overflow controls after the container is resized in a way a `ResizeObserver` can't observe.
- `.SelectedTab` — id of the current segment.

## Example

```csharp
using static Tesserae.UI;

var pivot = SegmentedPivot()
    .SegmentedPivot("o", SegmentTitle("Overview"),  () => TextBlock("Overview content"))
    .SegmentedPivot("l", SegmentTitle("Logs"),      () => TextBlock("Logs content"))
    .SegmentedPivot("a", SegmentTitle("Analytics"), () => TextBlock("Analytics content"));
```

## Related

- Pivot — `pivot.md`
- CardPivot — `card-pivot.md`
- Full docs & API: `/tesserae/surfaces/segmented-pivot`
