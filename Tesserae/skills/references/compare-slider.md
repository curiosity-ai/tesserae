---
name: compare-slider
description: Two components laid over each other with a draggable divider to compare them (before/after). Use for image edits, two versions of a chart, or light vs dark renderings in a Tesserae (C#/Transpose) app.
---

# CompareSlider

Lays `before` over `after` and clips it at a divider the user drags: `before`
shows to the left, `after` to the right. A click anywhere jumps the divider
there. To assistive technology the divider is a slider; it moves with the arrow
keys (Shift for steps of 10), Home and End. `after` sets the size and `before`
is stretched over it, so the two should be the same shape.

## Create

`UI.CompareSlider(IComponent before, IComponent after, double position = 50)`
returns a `CompareSlider` (position in percent of the width).

## Key configuration

- `.Position` / `.SetPosition(double percent)` — move the divider.
- `.OnPositionChanged(Action<double>)` — runs when it moves.
- `.Labels(string before, string after)` — corner labels that fade as the divider passes them.
- Size with `.MaxWidth(...)`, `.W(...)`.

## Example

```csharp
using static Tesserae.UI;

CompareSlider(Image(originalUrl), Image(editedUrl))
    .Labels("Before", "After")
    .MaxWidth(640.px());
```

## Related

- Carousel — `carousel.md`
- Slider — `slider.md`
- Code diff, for text — `code-diff.md`
- Full docs & API: `/tesserae/media/compare-slider`
