---
name: theme-colors
description: The static `Theme` class for switching light/dark mode and overriding primary/background colors at runtime. Use when toggling themes or recoloring a Tesserae (C#/Transpose) app.
---

# Theme Colors

`Theme` manages the app theme via CSS variables: switch between light and dark mode and override the primary/background palette at runtime. (For the raw named palette and the `Color` helper, see the colors skill.)

## Methods

- `Theme.Light()` — switch to light mode.
- `Theme.Dark()` — switch to dark mode (toggles the `tss-dark-mode` body class).
- `Theme.SetPrimary(Color light, Color dark)` — set primary color for both modes. It also drives the
  colors derived from it: `--tss-link-color` (links, `Badge().Info()`) and `--tss-highlight-color`
  (marked search terms, see `omni-result.md`).
- `Theme.SetBackground(Color light, Color dark)` — set background for both modes.
- `Theme.SetHighlight(Color light, Color dark)` — set the highlight color (`--tss-highlight-color`)
  for both modes, so marked text stops following the primary color. What is set here wins over
  `SetPrimary` whatever order the two are called in.
- `Theme.ResetHighlight()` — drop an explicit highlight color and go back to following the primary.

## Properties

- `Theme.IsLight` / `Theme.IsDark` — current mode (bool).
- `Theme.Default`, `Theme.Primary`, `Theme.Secondary`, `Theme.Danger`, `Theme.Success` — color accessors exposing `.Background`, `.Foreground`, `.Border`, etc.
- `Theme.Default.Highlight` — the highlight color as a CSS variable reference, for drawing your own marked text with it.
- `Theme.Fonts.SansSerif` / `Theme.Fonts.Monospace` — the two font stacks the toolkit draws with, as CSS variable references. See `styling.md`.
- `Theme.OnThemeChanged` — event raised on mode change.

## Example

```csharp
using static Tesserae.UI;

var status = TextBlock(Theme.IsDark ? "Dark theme" : "Light theme").Medium();

Stack().Children(
    status,
    HStack().Children(
        Button("Dark").OnClick((s, e) => { Theme.Dark();  status.Text("Dark theme"); }),
        Button("Light").OnClick((s, e) => { Theme.Light(); status.Text("Light theme"); })));

// Runtime recolor
Theme.SetPrimary(Color.FromString("blue"), Color.FromString("lightblue"));
Theme.SetBackground(Color.FromString("white"), Color.FromString("#333"));

// The highlight follows the primary color unless you say otherwise
Theme.SetHighlight(Color.FromString("#b45309"), Color.FromString("#fbbf24"));
```

## Related

- Colors (palette constants + Color helper) — `colors.md`
- Gradients — `gradients.md`
- Full docs & API: `/tesserae/utilities/theme-colors`
