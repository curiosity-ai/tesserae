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
- `.Tint(string color)` — tint with any CSS colour (a translucent fill and border, text mixed with the page foreground so it reads in light and dark), the way an issue tracker draws a user-coloured label: `Tag("bug").Pill().Tint("#d73a4a")`. `null` removes it.
- `.SetIcon(string)` — leading icon (use `Icon.Transform(UIcons.X, UIconsWeight.Regular)`).
- `.SetText(string)` — change the label.
- `.Dot(string color = null)` / `.NoDot()` — a small status dot before the text, in the given CSS colour (or the text's own colour): `Chip("19 on track").Pill().Dot(Theme.Colors.Green700)`.
- `.Background(string)` / `.Foreground(string)` — override colors.
- `.AI()` (`BadgeTone.AI`) — the AI tone: a filled purple-to-blue gradient, or the tint plus accent when combined with `.Outline()`. `.AI(strongEffect: true)` asks for the louder form a custom theme may draw. `UI.AIBadge(text, strongEffect)` is the shorthand for a Sparkles-led gradient pill. See `ai-variants.md`.

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

- Full docs & API: `/tesserae/text/badge`
- Button — `button.md`
- StatefulItemRow — the list row these usually sit at the end of — `stateful-item-row.md`
- AI variants — the `BadgeTone.AI` tone — `ai-variants.md`
