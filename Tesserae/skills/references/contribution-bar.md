---
name: contribution-bar
description: A single stacked horizontal bar showing how weighted segments add up to a total, with an optional legend. Use when visualising a score breakdown or composition in a Tesserae (C#/Transpose) app.
---

# ContributionBar

A horizontal bar divided into colored segments sized proportionally to their
values, with an optional legend listing each segment's label and value. Useful
for showing how each signal contributes to a similarity/ranking score.

## Create

`UI.ContributionBar()` — empty bar; add segments fluently with `.Add(...)`.
Bring factories into scope with `using static Tesserae.UI;`.

## Key configuration

- `.Add(string label, double value, string color = null)` — append a segment; color defaults to a built-in 8-color palette by position.
- `.Max(double)` — pin the full-width value; when segments sum to less, the remainder is an empty track (default: sum of segments).
- `.ShowLegend(bool = true)` / `.HideLegend()` — toggle the legend below the bar.
- `.ShowValues(bool = true)` — show/hide each segment's numeric value in the legend.
- `.Decimals(int)` — decimal places for formatted values (default 2).
- `.Thickness(UnitSize)` — bar height (default `10px`).
- `ContributionBar.DefaultPalette` — the static color array used for unspecified colors.

## Example

```csharp
using static Tesserae.UI;

var bar = ContributionBar()
    .Max(1.0)
    .Decimals(3)
    .Add("Embedding similarity", 0.52)
    .Add("BM25 keyword",          0.21)
    .Add("Recency",               0.09, Theme.Colors.Orange500);
// place it: bar.WS().Padding(16.px())
```

## Related

- DeltaComponent — `delta-component.md`
- Full docs & API: `/tesserae/components/contribution-bar`
