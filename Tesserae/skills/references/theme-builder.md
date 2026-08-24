---
name: theme-builder
description: A fluent builder for every colour CSS variable the toolkit exposes, set independently for light and dark mode and applied as one stylesheet. Use to brand an app, match an existing design system, or ship a per-tenant palette in a Tesserae (C#/Transpose) app.
---

# ThemeBuilder

`Theme.Build()` hands you a builder for the whole palette: every colour variable the
toolkit reads, each taking a **light** value and a **dark** value, applied in one go as a
single `<style>` element.

Use it when you are theming the product. For a one-off change — the primary colour, the
page background — `Theme.SetPrimary(...)` / `Theme.SetBackground(...)` /
`Theme.SetHighlight(...)` (`theme-colors.md`) are the smaller tools, and they leave the
rest of the palette alone.

## Create

`Theme.Build()` — returns a `ThemeBuilder`. Chain setters, finish with `.Apply()`.
`Theme.ResetBuild()` drops the applied theme and goes back to the defaults in
`tss.common.css`. Bring factories into scope with `using static Tesserae.UI;`.

Anything you don't set keeps its default, so a theme is only as long as the parts you
actually want to change. The dark value applies while the document carries `.tss-dark-mode`
(what `Theme.Dark()` / `Theme.Light()` toggle), the light one otherwise — so one `Apply()`
themes both modes, and switching mode afterwards needs nothing further.

## Key configuration

Each setter takes `(Color light, Color dark)`:

- **Surface** — `.DefaultBackground`, `.DefaultBackgroundHover`, `.DefaultBackgroundActive`,
  `.DefaultForeground`, `.DefaultForegroundHover`, `.DefaultForegroundActive`,
  `.DefaultBorder`, `.DarkBorder`, `.DefaultSeparator`, `.InvalidBorder`.
- **Primary** — `.Primary(light, dark)` sets the background, its hover/active shades, the
  border and the shadow together; `.PrimaryBackground`, `.PrimaryBackgroundHover`,
  `.PrimaryBackgroundActive`, `.PrimaryBorder`, `.PrimaryForeground`,
  `.PrimaryForegroundHover`, `.PrimaryForegroundActive`, `.PrimaryShadow` set the pieces.
- **Tones** — `.Danger(light, dark)` and `.Success(light, dark)` (same all-at-once shape as
  `Primary`), plus their `…Background`/`…BackgroundHover`/`…BackgroundActive`/`…Border`/
  `…Foreground`/`…ForegroundHover`/`…ForegroundActive` parts, and
  `.WarningBackground` / `.WarningForeground`.
- **Chrome** — `.SecondaryBackground`, `.SecondaryForeground`, `.SidebarBackground`,
  `.SidebarForeground`, `.DisabledBackground`, `.DisabledForeground`,
  `.TooltipBackground`, `.TooltipForeground`.
- **Accents** — `.Link` (links, `Badge().Info()`), `.Highlight` (marked search terms, see
  `omni-result.md`), `.Slider`, `.SliderActive`, `.SliderDisabled`, `.ProgressBackground`.
- `.SetVariable(string name, Color light, Color dark)` — the escape hatch, for a variable
  the typed setters don't cover. Give the name **without** the `--tss-` prefix
  (`"my-accent-color"`).

Finishing:

- `.Apply()` — render the palette into a `<style>` in the document head. A theme applied
  earlier is removed first, so calls are idempotent, and `Theme.OnThemeChanged` is raised.
- `.ToCss()` — the generated CSS without attaching it: for a preview, for server-side
  rendering, or to persist a tenant's theme and re-apply it on load.
- `Theme.ResetBuild()` — remove the applied theme entirely.

## Example

```csharp
using static Tesserae.UI;

Theme.Build()
    .Primary  (Color.FromString("#0078d4"), Color.FromString("#2899f5"))
    .Link     (Color.FromString("#0078d4"), Color.FromString("#55b3fb"))
    .Highlight(Color.FromString("#0078d4"), Color.FromString("#55b3fb"))
    .DefaultBackground      (Color.FromString("#eaf3fb"), Color.FromString("#0b1320"))
    .DefaultBackgroundHover (Color.FromString("#d6e6f4"), Color.FromString("#101d33"))
    .DefaultForeground      (Color.FromString("#0b3559"), Color.FromString("#e6f1fb"))
    .DefaultBorder          (Color.FromString("#bcd6ea"), Color.FromString("#1f3252"))
    .Apply();

// Keep it: the CSS is a string, so a tenant's palette can be stored and re-applied.
var css = Theme.Build().Primary(Color.FromString("#16a34a"), Color.FromString("#22c55e")).ToCss();

Theme.ResetBuild();   // back to the shipped defaults
```

## Related

- Theme colours — light/dark switching and the focused setters — `theme-colors.md`
- Colors — the palette constants and the `Color` helper — `colors.md`
- Gradients — `gradients.md`
- Custom styles — your own CSS classes on top — `custom-styles.md`
- Full docs & API: `/tesserae/utilities/theme-builder`
