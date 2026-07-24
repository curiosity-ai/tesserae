---
name: badge
description: Small inline label tokens — Badge (status indicator), Tag (categorization), and Chip (removable) — sharing tone, shape, icon, and outline/filled styling. Use for status pills, tags, and dismissible chips in a Tesserae (C#/Transpose) app.
---

# Badge / Tag / Chip

Three token components share one fluent API (`TokenBase<T>`): `Badge` for static status
indicators, `Tag` for categorization labels, and `Chip` for interactive/removable tokens.
They differ only in default styling.

## Create

`UI.Badge(string text = null)`, `UI.Tag(string text = null)`, `UI.Chip(string text = null)`.
Bring factories into scope with `using static Tesserae.UI;`.

## Key configuration

Tone (color): `.Primary()`, `.Success()`, `.Warning()`, `.Danger()`, `.Info()`,
`.Neutral()` (default), or `.Tone(BadgeTone)`.

Shape/style:

- `.Pill(bool = true)` — rounded pill shape.
- `.Outline(bool = true)` / `.Filled(bool = true)` — mutually exclusive fill styles.
- `.SetIcon(string)` — leading icon (use `Icon.Transform(UIcons.X, UIconsWeight.Regular)`).
- `.SetText(string)` — change the label.
- `.Background(string)` / `.Foreground(string)` — override colors.

Removable (Chip etc.):

- `.Removable(bool = true)` — show a remove button.
- `.OnRemove(Action<T>)` — handle removal (also enables the button).

## Example

```csharp
using static Tesserae.UI;

var row = HStack().Children(
    Badge("Primary").Primary(),
    Tag("Metadata").SetIcon(Icon.Transform(UIcons.Tags, UIconsWeight.Regular)).Outline(),
    Chip("Removable").Filled().OnRemove(c => Toast().Success("Removed")));
```

## Related

- Full docs & API: `/tesserae/components/badge`
- Button — `button.md`
