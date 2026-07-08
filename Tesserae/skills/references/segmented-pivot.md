---
name: segmented-pivot
description: A pivot styled as a connected segmented (pill) control for switching between a few closely related views. Use when toggling related views or filters in limited space in a Tesserae (C#/h5) app.
---

# SegmentedPivot

A tabbed surface whose tabs render as a single connected segmented control.
Best for a small number of closely related views or filters.

## Create

`SegmentedPivot()` — returns a `SegmentedPivot`. Add tabs with the
`.SegmentedPivot(...)` extension. Bring factories into scope with
`using static Tesserae.UI;`.

## Key configuration

- `.SegmentedPivot(id, titleCreator, contentCreator, cached = false)` — add a segment. `titleCreator`/`contentCreator` are `Func<IComponent>`.
- `SegmentTitle("Text")` / `SegmentTitle("Text", UIcons.Rocket)` — convenient title `Func<IComponent>`.
- `.Select(id, refresh = false)` — switch segment.
- `.OnNavigate(...)` / `.OnBeforeNavigate(...)` — callbacks; `e.Cancel()` blocks navigation.

## Sizing tab content

The content area lays out the active tab like a `Stack` with a single child, so
panel-filling content expands to the full available height instead of collapsing.
Size the content to fill with `.S()` / `.HS()`, `.Grow()`, or its own
`height: 100%`; content that is behind an intermediate wrapper (e.g. a `Defer`)
fills as long as the outer piece carries the fill sizing. Fixed- or
intrinsic-height content keeps its natural height and the pane scrolls.

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
