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

## Fonts

Tesserae draws with exactly two font stacks, and every rule that sets a
`font-family` names one of them with no inline fallback list:

- `--tss-sansserif-font-family` — everything but code. Defaults to
  `"Plus Jakarta Sans", "Inter", "Segoe UI", …`; Tesserae does not ship the font
  files, so an app that wants the first name in that list serves its own
  `@font-face` for it.
- `--tss-monospace-font-family` — code, paths, identifiers.

Override either one on `:root` (or on any sub-tree) and the whole UI follows,
form controls included. From C#, reference them as `Theme.Fonts.SansSerif` /
`Theme.Fonts.Monospace` rather than repeating the `var(...)` literal:

```css
:root {
    --tss-sansserif-font-family: "Plus Jakarta Sans", "Inter", sans-serif;
    --tss-monospace-font-family: "Monaspace Neon", ui-monospace, monospace;
}
```

```csharp
TextBlock(path).Style(s => s.fontFamily = Theme.Fonts.Monospace);
```

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
