---
name: gradients
description: Predefined theme gradients exposed as CSS-variable strings on `Theme.Gradients`. Use when applying consistent gradient backgrounds in a Tesserae (C#/h5) app.
---

# Gradients

Tesserae ships a set of named gradients on the static `Theme.Gradients` class, each a CSS-variable string that adapts to light/dark mode. Apply them anywhere a background value is accepted (e.g. `.Background(...)`).

## Available gradients

`Theme.Gradients.Lime`, `.Red`, `.Orange`, `.Yellow`, `.Green`, `.Teal`, `.Blue`, `.Purple`, `.Magenta`, and `.AI`.

Each returns a `string` (a CSS `var(...)` reference), so pass it directly to fluent background helpers.

## Example

```csharp
using static Tesserae.UI;

var banner = HStack().NoWrap()
    .Background(Theme.Gradients.AI)
    .Children(Button("AI Feature").Foreground("white").NoBackground());
```

React to mode switches with `Theme.OnThemeChanged += () => …` if you cache the rendered value.

## Related

- Colors / Theme — `colors.md`
- Full docs & API: `/tesserae/utilities/gradients`
