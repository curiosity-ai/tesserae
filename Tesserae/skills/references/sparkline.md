---
name: sparkline
description: A compact inline SVG area chart that shows the shape of a trend without axes or labels. Use when embedding a tiny trend chart beside a metric, table cell, or dashboard card in a Tesserae (C#/Transpose) app.
---

# Sparkline

A small SVG line+area chart for showing a trend at a glance. It has no axes,
labels, or interactivity — just the curve. Takes a `double[]` of values plus
optional width, height, and colour.

## Create

`UI.Sparkline(double[] data, double width = 100, double height = 30, string color = "")`
(i.e. `Sparkline(new double[]{ 10, 14, 18 }, width: 150, height: 40)`) returns a
`Sparkline`. An empty `color` defaults to the theme primary.
Bring factories into scope with `using static Tesserae.UI;`.

## Key configuration

- `data` — the series; single-point arrays render flat, empty arrays render nothing.
- `width` / `height` — SVG viewbox size (the element itself stretches to fill).
- `color` — stroke + gradient colour; accepts CSS values, `Theme.Colors.*`,
  or CSS vars like `"var(--tss-danger-background-color)"`.
- Sizing helpers (`.WS()`, `.W()`, `.PT()`, ...) apply to the wrapper.

The chart re-renders when re-added to a container, so swap in a fresh instance
(e.g. `host.Replace(newChart, oldChart)`) to update it.

## Example

```csharp
using static Tesserae.UI;

var row = HStack().AlignItemsCenter().Children(
    TextBlock("Revenue").W(120.px()),
    Sparkline(new double[]{ 10, 14, 18, 15, 22, 28, 25, 35, 40, 50 },
              width: 150, height: 40, color: Theme.Colors.Green600)
);
```

## Related

- Cards (dashboard tiles to host sparklines) — `card.md`
- Full docs & API: `/tesserae/components/sparkline`
