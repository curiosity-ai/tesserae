---
name: uicons
description: A strongly typed `UIcons` enum (Flaticon UIcons set) rendered through the Icon component. Use when adding icon glyphs with compile-time safety in a Tesserae (C#/Transpose) app.
---

# UIcons

Tesserae ships an auto-generated `UIcons` enum for the UIcons (Flaticon) glyph set. Render an icon by passing an enum value to the `Icon` factory; many components also accept `UIcons` directly (e.g. `Button().SetIcon(UIcons.Folder)`).

## Create

`UI.Icon(UIcons icon, UIconsWeight weight = Regular, TextSize size = TextSize.Small, string color = null)` — returns an `Icon`. Also `UI.Icon(UIcons icon, string color)`. Bring factories into scope with `using static Tesserae.UI;`.

## Key points

- `UIcons.Camera`, `UIcons.Settings`, `UIcons.User`, `UIcons.Folder`, `UIcons.Bell`, … — discover values via IntelliSense on the `UIcons` enum.
- Passed directly to component helpers like `Button(UIcons.Camera)`, `.SetIcon(UIcons.Copy, color: "white")`, and in `CommandPaletteAction.Icon`.
- Color via the `color:` argument or `.Foreground(...)` on the resulting `Icon`.
- For emoji glyphs, use the `Emoji` enum instead (see emoji skill).
- Need the raw CSS class of an icon (to build a class string by hand, say)? Call
  `icon.ToCssClass()`, not `icon.ToString()`. `UIcons` has thousands of members and the runtime
  implements `Enum.ToString()` as a scan over all of them — about 80µs a call, which shows up
  immediately in a list or table. `ToCssClass()` returns the same string from a lookup table, and
  `Emoji` has the same extension for the same reason.
- A handful of glyphs are drawn slightly off centre in the font, so the bundled webfonts carry a
  correction baked into those glyph outlines: an icon lands optically centred in whatever box you put
  it in, at any size, with no styling on your part. Icons meant to overlap (a checkbox on a square, a
  slashed variant on its base icon) are held to the same offset, so swapping one for the other never
  shifts.

## Example

```csharp
using static Tesserae.UI;

var big = Icon(UIcons.Camera, size: TextSize.Large);

var rows = Stack().Children(
    Label("Settings:").Inline().SetContent(Icon(UIcons.Settings)),
    Label("User:").Inline().SetContent(Icon(UIcons.User, color: "blue")));
```

## Related

- Emoji — `emoji.md`
- Full docs & API: `/tesserae/utilities/uicons`
