---
name: styling
description: Fluent strongly-typed styling for text formatting, spacing, sizing, UnitSize, and direct DOM styles. Use when styling components — text size/weight, padding/margin, sizes, or inline CSS — in a Tesserae (C#/Transpose) app.
---

# Styling

Tesserae styles components with fluent, strongly-typed APIs. Bring factories
into scope with `using static Tesserae.UI;`. Prefer the fluent helpers first;
drop to direct DOM styling only for one-offs.

## Text formatting

On components implementing `ITextFormating` (`ITextFormatingExtensions`):

- Sizes: `.Tiny()`, `.XSmall()`, `.Small()`, `.Medium()`, `.Large()`.
- Weight: `.Bold()`, `.SemiBold()`.
- Alignment: `.TextCenter()`.
- Explicit: `.SetTextSize(TextSize.Small)`, `.SetTextWeight(TextWeight.SemiBold)`.

## Spacing & sizing (IComponentExtensions)

- Padding/margin: `.Padding(...)`, `.MarginBottom(...)`, etc.
- Size: `.Width(...)`, `.Height(...)`, `.Stretch()`.
- Shorthands (accept `UnitSize` or `int` pixels): `P, PT, PB, PL, PR`,
  `M, MT, MB, ML, MR`, `W, H`, `S` (= `WS().HS()`), `WS`, `HS`.

## UnitSize

CSS sizes via numeric extensions: `16.px()`, `50.percent()`, `1.fr()`,
`100.vw()`, `100.vh()`. Helpers: `UnitSize.Auto()`, `UnitSize.FitContent()`,
and raw `new UnitSize("calc(100% - 32px)")`.

## Direct DOM styling (advanced)

- `.Style(s => s.borderRadius = "12px")` — fluent inline style.
- `component.Render().style.borderRadius = "12px"` — mutate the element directly.

## Example

```csharp
using static Tesserae.UI;

var title = TextBlock("Dashboard").Large().Bold().TextCenter();

var card = Card(TextBlock("Summary"))
    .P(16)        // Padding(16.px())
    .MB(12)       // MarginBottom(12.px())
    .W(320)       // Width(320.px())
    .S();         // Stretch

var button = Button("Download").Style(s => s.borderRadius = "12px");
```

## Related

- Custom Styles (CSS classes) — `.custom-styles.md`
- Layout & Alignment — `.layout-alignment.md`
- Full docs & API: `/tesserae/get-started/styling`
