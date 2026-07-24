---
name: color-picker
description: A native browser color input wrapped as a Tesserae component, exposing the value as both a hex string and a Color. Use when users need to choose a color in a Tesserae (C#/Transpose) app.
---

# ColorPicker

`ColorPicker` wraps the browser's native `<input type="color">`. It defaults to black
unless given a preset `Color`, and exposes the value as a `Color` (`.Color`) or the hex
string (`.Text`, inherited from `Input`).

## Create

`UI.ColorPicker(Color color = null)`.
Bring factories into scope with `using static Tesserae.UI;`.

## Key configuration

- `.Color` — get/set the selected color as a `Color`.
- `.SetColor(Color)` — set the color (updates the hex text).
- `.Text` — the current hex value (e.g. `"#ff0000"`), from the base `Input`.
- `.OnChange((sender, args) => ...)` — react to selection changes.
- Sizing helpers apply: `.W(48)`, `.H(48)`.

## Example

```csharp
using static Tesserae.UI;

var picker = ColorPicker().W(48).H(48);
picker.OnChange((sender, args) =>
    Toast().Information($"Selected: {picker.Text}"));
```

## Related

- Button — `button.md`
- Full docs & API: `/tesserae/components/color-picker`
