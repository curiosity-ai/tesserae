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

## Example

```csharp
using static Tesserae.UI;

var pivot = SegmentedPivot()
    .SegmentedPivot("o", SegmentTitle("Overview"),  () => TextBlock("Overview content"))
    .SegmentedPivot("l", SegmentTitle("Logs"),      () => TextBlock("Logs content"))
    .SegmentedPivot("a", SegmentTitle("Analytics"), () => TextBlock("Analytics content"));
```

## Related

- Pivot — `../pivot/SKILL.md`
- CardPivot — `../card-pivot/SKILL.md`
- Full docs & API: `/tesserae/surfaces/segmented-pivot`
