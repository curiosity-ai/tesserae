---
name: theme-colors
description: The static `Theme` class for switching light/dark mode and overriding primary/background colors at runtime. Use when toggling themes or recoloring a Tesserae (C#/h5) app.
---

# Theme Colors

`Theme` manages the app theme via CSS variables: switch between light and dark mode and override the primary/background palette at runtime. (For the raw named palette and the `Color` helper, see the colors skill.)

## Methods

- `Theme.Light()` — switch to light mode.
- `Theme.Dark()` — switch to dark mode (toggles the `tss-dark-mode` body class).
- `Theme.SetPrimary(Color light, Color dark)` — set primary color for both modes.
- `Theme.SetBackground(Color light, Color dark)` — set background for both modes.

## Properties

- `Theme.IsLight` / `Theme.IsDark` — current mode (bool).
- `Theme.Default`, `Theme.Primary`, `Theme.Secondary`, `Theme.Danger`, `Theme.Success` — color accessors exposing `.Background`, `.Foreground`, `.Border`, etc.
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
```

## Related

- Colors (palette constants + Color helper) — `../colors/SKILL.md`
- Gradients — `../gradients/SKILL.md`
- Full docs & API: `/tesserae/utilities/theme-colors`
