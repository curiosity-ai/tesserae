---
name: progress-ring
description: A circular (donut-style) progress indicator with determinate value, indeterminate spin, and an optional centre label. Use when showing quota/usage or a compact loading state in a Tesserae (C#/h5) app.
---

# ProgressRing

An SVG ring that fills clockwise to a value, with an optional text label in its centre.

## Create

`UI.ProgressRing(int size = 48, int thickness = 4)` — returns a `ProgressRing` (size = diameter in px). Also `new ProgressRing(size, thickness)`.
Bring factories into scope with `using static Tesserae.UI;`.

## Key configuration

- `.Progress(double current, double total)` — set value and max in one call.
- `.Value` / `.Max` — set/read value and maximum (default max 100).
- `.Indeterminate(bool = true)` — show a continuously spinning ring.
- `.Label(string)` — text in the centre; `.NoLabel()` — clear it.

## Example

```csharp
using static Tesserae.UI;

HStack().Gap(24.px()).AlignItems(ItemAlign.End).Children(
    ProgressRing(64, 6).Progress(25, 100).Label("25%"),
    ProgressRing(64, 6).Progress(75, 100).Label("75%"),
    ProgressRing(48, 4).Indeterminate()
);
```

## Related

- ProgressIndicator — `.progress-indicator.md`
- Full docs & API: `/tesserae/components/progress-ring`
