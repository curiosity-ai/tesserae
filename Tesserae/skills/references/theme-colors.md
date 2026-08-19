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

## Properties

- `Theme.IsLight` / `Theme.IsDark` — current mode (bool).
- `Theme.Default`, `Theme.Primary`, `Theme.Secondary`, `Theme.Danger`, `Theme.Success` — color accessors exposing `.Background`, `.Foreground`, `.Border`, etc.
- `Theme.Fonts.SansSerif` / `Theme.Fonts.Monospace` — the two font stacks the toolkit draws with, as CSS variable references. See `styling.md`.
- `Theme.OnThemeChanged` — event raised on mode change and on `SetPrimary`/`SetBackground`.
  It is a **static** event, so a component that subscribes must unsubscribe when it is
  removed, or it keeps itself and its DOM alive for the life of the page. Anything that
  reads a theme value into a non-CSS form (a hex string for a canvas or a JS library) has
  to recompute here — see `colors.md` and `wrap-a-javascript-library.md`.

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

- Colors (palette constants + Color helper) — `colors.md`
- Gradients — `gradients.md`
- Full docs & API: `/tesserae/utilities/theme-colors`
