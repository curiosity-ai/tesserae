---
name: color-palette
description: A grid of named color swatches for picking from a predefined palette. Use when offering users a fixed set of brand/theme colors (not arbitrary colors) in a Tesserae (C#/h5) app.
---

# ColorPalette

A radiogroup of named color swatches. Use it instead of a free color picker when the valid choices are known in advance; opt in to `WithCustomColor()` to also allow an arbitrary color.

## Create

`UI.ColorPalette()` — returns a `ColorPalette`. Bring factories into scope with `using static Tesserae.UI;`.

Add swatches with `ColorPalette.Define(label, hexColor)` entries.

## Key configuration

- `.Swatches(params ColorAndLabel[])` — add many swatches at once (each via `ColorPalette.Define("Blue", "#0078d4")`).
- `.AddSwatch(label, hexColor)` — add one swatch.
- `.SetValue(hexColor)` — select a color programmatically.
- `.WithCustomColor(bool = true)` — append a "custom" swatch that opens the native color input.
- `.OnChange(Action<string>)` — fires with the selected hex string.
- `.Value` — currently selected hex color (read-only).

## Example

```csharp
using static Tesserae.UI;

var palette = ColorPalette()
    .Swatches(
        ColorPalette.Define("Blue",    "#0078d4"),
        ColorPalette.Define("Red",     "#d13438"),
        ColorPalette.Define("Green",   "#107c10"))
    .SetValue("#0078d4")
    .WithCustomColor()
    .OnChange(c => Toast().Information($"Selected: {c}"));
```

Swatch values can be theme colors, e.g. `ColorPalette.Define("Primary", Theme.Primary.Background)`.

## Related

- Colors / Theme — `colors.md`
- Full docs & API: `/tesserae/utilities/color-palette`
