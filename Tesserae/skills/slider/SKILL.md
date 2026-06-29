---
name: slider
description: A range input for picking an integer value between a min and max with a step, horizontal or vertical. Use when a user adjusts a numeric value along a track in a Tesserae (C#/h5) app.
---

# Slider

A range (`<input type="range">`) control for selecting an integer value within
a min/max range at a fixed step. Supports horizontal (default) and vertical
orientations.

## Create

`UI.Slider(int val = 0, int min = 0, int max = 100, int step = 10)` (i.e.
`Slider(val: 50, min: 0, max: 100, step: 5)`) returns a `Slider`.
Bring factories into scope with `using static Tesserae.UI;`.

## Key configuration

- `.SetValue(int)` / `.Value` — current value.
- `.SetMin(int)` / `.SetMax(int)` / `.SetStep(int)` — range and increment.
- `.Horizontal()` / `.Vertical()` — orientation (also `.Orientation`).
- `.Disabled(bool = true)` / `.IsEnabled` — enable/disable.
- `.OnInput((s, e) => ...)` — fires continuously while dragging.
- `.OnChange((s, e) => ...)` — fires when the value is committed.

## Example

```csharp
using static Tesserae.UI;

var slider = Slider(val: 50, min: 0, max: 100, step: 5)
    .Horizontal()
    .OnInput((s, e) => console.log($"Slider value: {s.Value}"));

var vertical = Slider(val: 25, min: 0, max: 50, step: 5).Vertical();
```

## Related

- StepsSlider (snaps to named discrete steps) — `../steps-slider/SKILL.md`
- Full docs & API: `/tesserae/components/slider`
