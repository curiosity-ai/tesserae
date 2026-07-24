---
name: number-picker
description: A numeric input control over the native HTML number field, with min/max/step bounds, validation and an int Value. Use when a user must enter or pick a number in a Tesserae (C#/Transpose) app.
---

# NumberPicker

A numeric input backed by the native `<input type="number">`, with fluent
min/max/step configuration. Often paired with a `Label`.

## Create

`NumberPicker(int defaultValue = 0)` — creates the input with a starting value.
Bring the factory into scope with `using static Tesserae.UI;`.

## Key configuration

- `.Value` — read the current value as `int`.
- `.SetMin(int)` / `.SetMax(int)` / `.SetStep(int)` — bounds and increment (or the `Min`/`Max`/`Step` properties).
- `.OnChange(...)` — react to value changes (inherited from `Input`).
- `.Disabled()`, `.Required()` — state (inherited).
- `.Validation(np => np.Value % 2 == 0 ? null : "Pick an even value")` — return an error string or `null`.
- `.Error("...")` / `.IsInvalid()` — show an error message.
- `.Size`, `.Weight`, `.TextAlign`, `.Background`, `.Foreground` — formatting.

## Example

```csharp
using static Tesserae.UI;

var qty = NumberPicker(8).SetMin(5).SetMax(20).SetStep(5);

var form = Stack().Children(
    Label("Quantity").SetContent(qty),
    Label("Even only").SetContent(
        NumberPicker().Validation(np => np.Value % 2 == 0 ? null : "Please choose an even value"))
);
```

## Related

- TextBox — `/tesserae/components/text-box`
- Slider — `/tesserae/components/slider`
- Full docs & API: `/tesserae/components/number-picker`
