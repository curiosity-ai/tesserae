---
name: emoji
description: A strongly typed `Emoji` enum (backed by Emoji.css) rendered through the Icon component. Use when adding emoji glyphs with compile-time safety in a Tesserae (C#/Transpose) app.
---

# Emoji

Tesserae ships an auto-generated `Emoji` enum covering the Emoji.css set. Render an emoji by passing an enum value to the `Icon` factory — there is no separate `Emoji()` component.

## Create

`UI.Icon(Emoji emoji, TextSize size = TextSize.Medium)` — returns an `Icon`. Bring factories into scope with `using static Tesserae.UI;`.

(There is also `UI.Icon(Emoji icon, string color, TextSize size, TextWeight weight)`-style sizing via the same Icon component.)

## Key points

- `Emoji.Smile`, `Emoji.Rocket`, `Emoji.HeartEyes`, … — discover values via IntelliSense on the `Emoji` enum.
- Style the resulting `Icon` like any component: `.Foreground(...)`, sizing helpers, etc.
- For UI glyphs (non-emoji) use `UIcons` instead (see uicons skill).

## Example

```csharp
using static Tesserae.UI;

var big = Icon(Emoji.Smile, size: TextSize.Large);

var labelled = Stack().Children(
    Label("Rocket:").Inline().SetContent(Icon(Emoji.Rocket)),
    Label("Heart:").Inline().SetContent(Icon(Emoji.HeartEyes)));
```

## Related

- UIcons (icon glyphs) — `uicons.md`
- Full docs & API: `/tesserae/utilities/emoji`
