---
name: colors
description: The Theme color system — named palette constants, the Color helper, and runtime light/dark theming. Use when styling components with consistent colors or overriding the theme in a Tesserae (C#/h5) app.
---

# Colors

Tesserae applies colors through CSS variables exposed on the static `Theme` class, plus a `Color` value helper for parsing and constructing colors. Use these instead of hardcoded hex strings so components stay consistent in light and dark mode.

## Theme color accessors

Semantic accessors expose `.Background`, `.Foreground`, `.Border`, etc. as CSS-variable strings:

- `Theme.Default`, `Theme.Primary`, `Theme.Secondary`, `Theme.Danger`, `Theme.Success`.

Apply them with component fluent helpers:

```csharp
using static Tesserae.UI;

var badge = Badge("New")
    .Background(Theme.Primary.Background)
    .Foreground(Theme.Primary.Foreground);
```

## Palette constants

`Theme.Colors.*` holds the raw named palette: hue families (`Lime`, `Red`, `Orange`, `Yellow`, `Green`, `Teal`, `Blue`, `Purple`, `Magenta`) each with shades `100`–`1000` (plus `250`/`850`), and `Neutral0`–`Neutral1100` / `DarkNeutral0`–`DarkNeutral1100`. Gradients live on `Theme.Gradients.*` (see gradients skill).

## Light / dark mode

- `Theme.Light()` / `Theme.Dark()` — switch mode (toggles the `tss-dark-mode` body class).
- `Theme.IsLight` / `Theme.IsDark` — current mode.
- `Theme.OnThemeChanged += () => …` — react to mode changes.

## Overriding the theme

- `Theme.SetPrimary(Color light, Color dark)` — regenerate primary CSS vars for both modes.
- `Theme.SetBackground(Color light, Color dark)` — same for background.

```csharp
Theme.SetPrimary(Color.FromString("#0063B1"), Color.FromString("#2899F5"));
Theme.SetBackground(Color.FromString("#FFFFFF"), Color.FromString("#1B1A19"));
```

## Working with Color

- `Color.FromString("rgba(16,110,190,1)")` / `Color.FromString("blue")` — parse hex/rgb/named.
- `Color.FromArgb(0, 120, 212)` — construct from components.
- `Color.EvalVar(...)` — resolve a CSS variable to a concrete color.
- `.ToHex()` / `.ToRGB()` — serialize. `HSLColor` exposes `.Luminosity` for contrast checks.

## Related

- Theme switching detail — `../theme-colors/SKILL.md`
- Gradients — `../gradients/SKILL.md`
- ColorPalette picker — `../color-palette/SKILL.md`
- Full docs & API: `/tesserae/utilities/colors`, `/tesserae/get-started/colors`
