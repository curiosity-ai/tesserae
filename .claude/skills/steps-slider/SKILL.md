---
name: steps-slider
description: A generic slider that snaps to a fixed set of named, ordered values instead of a continuous range. Use when a user picks from a small ordered set (sizes, tiers, priorities) in a Tesserae (C#/h5) app.
---

# StepsSlider

A typed slider that only lands on one of the discrete values you provide. Wraps
a `Slider` whose range maps to the indices of your steps, so the value is always
a real `T`, not a raw number.

## Create

`new StepsSlider<T>(params T[] steps)` where `T : IEquatable<T>` (e.g.
`new StepsSlider<string>("XS", "S", "M", "L", "XL")`). There is no `UI.*`
factory — use `new`. Bring factories into scope with `using static Tesserae.UI;`.

## Key configuration

- `.SetValue(T)` / `.Value` — current step value.
- `.OnChange(Action<T>)` — fires when the selected step changes (on drag + commit).
- `.Horizontal()` / `.Vertical()` — orientation (also `.Orientation`).
- `.Disabled(bool = true)` / `.IsEnabled` — enable/disable.
- `.Comparer(IEqualityComparer<T>)` — custom equality for matching values.

## Example

```csharp
using static Tesserae.UI;

var size = new SettableObservable<string>("M");

var sizeSlider = new StepsSlider<string>("XS", "S", "M", "L", "XL")
    .OnChange(v => { size.Value = v; Toast().Information($"Size: {v}"); });

var component = VStack().Children(
    Label("Size").SetContent(sizeSlider),
    DeferSync(size, v => TextBlock(v).SemiBold()));

// Numeric steps
var pct = new StepsSlider<int>(0, 25, 50, 75, 100).OnChange(v => console.log(v));
```

## Related

- Slider (continuous range) — `../slider/SKILL.md`
- Full docs & API: `/tesserae/components/steps-slider`
