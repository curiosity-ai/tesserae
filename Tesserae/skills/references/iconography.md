---
name: iconography
description: Strongly-typed icon and emoji set (UIcons + Emoji enums) plus helpers to render them in Tesserae components. Use when adding icons or emoji to components, buttons, or commands in a Tesserae (C#/h5) app.
---

# Icons & Emoji

Tesserae exposes a typed icon set via the `UIcons` enum (Fluent-inspired) and an
`Emoji` enum, rendered through the `Icon(...)` factory and accepted by many
components. Bring factories into scope with `using static Tesserae.UI;`.

## Key APIs / patterns

- `Icon(UIcons icon, UIconsWeight weight = Regular, TextSize size = Small)` —
  standalone icon component. Defaults to Regular weight, Small size.
- `Icon(Emoji emoji, TextSize size)` — render an emoji glyph as a component.
- `Icon(UIcons icon, string color)` — set a CSS color (overload used in samples).
- `.SetIcon(UIcons icon, color: ...)` / `.SetIcon(Emoji emoji)` — on `Button`
  (and other icon-accepting components), set the icon inline.
- `UIconHelper.AsUIcon(string cssClass)` — wrap any CSS class as a `UIcons`-like
  value, for custom icon fonts (e.g. Font Awesome).

Enum values are converted to the appropriate CSS class or glyph at render time.

## Example

```csharp
using static Tesserae.UI;

var rocket   = Icon(UIcons.Rocket);                                     // Regular, Small
var settings = Icon(UIcons.Settings, UIconsWeight.Regular, TextSize.Small);
var party    = Icon(Emoji.ConfettiBall, TextSize.Medium);

var save     = Button("Save").SetIcon(UIcons.Disk, color: Theme.Primary.Foreground);
var celebrate = Button("Yay").SetIcon(Emoji.ConfettiBall);

// Custom icon font (import its CSS in h5.json first):
var faRocket = Icon(UIconHelper.AsUIcon("fa-solid fa-rocket"));
```

## Notes

- `UIcons` is used throughout the library (sidebar buttons, dropdowns, toolbars).
- For custom icon fonts, import the font/CSS files via your `h5.json` resources.

## Related

- Button — `.button.md`
- Project Setup — `.project-setup.md`
- Full docs & API: `/tesserae/get-started/iconography`
