---
name: icon
description: A single visual glyph rendered from the bundled UIcons set or an Emoji, with size, weight, color and tooltip control. Use when adding inline icons to reinforce actions or labels in a Tesserae (C#/h5) app.
---

# Icon

Renders one glyph using the strongly typed `UIcons` set or an `Emoji`. No raw
HTML or CSS classes needed.

## Create

- `Icon(UIcons icon, UIconsWeight weight = Regular, TextSize size = Small, string color = null)` — a UIcon glyph.
- `Icon(Emoji icon, TextSize size = Medium)` — an emoji glyph.
- `Icon(UIcons icon, string color)` — glyph with a foreground color.

Bring the factories into scope with `using static Tesserae.UI;`.

## Key configuration

- `.SetIcon(UIcons icon, ...)` / `.SetIcon(Emoji icon, ...)` — swap the glyph later.
- `.Foreground(color)` / `.Foreground` — set the icon color (e.g. `Theme.Primary.Background`).
- `.Size` — `TextSize` (e.g. `TextSize.Large`).
- `.Weight` — `TextWeight` of the glyph.
- `.SetTitle(string)` / `.Title` — native hover tooltip.
- `.Tooltip(...)` — richer tooltip (extension; see Button/utilities).

## Example

```csharp
using static Tesserae.UI;

var saveIcon = Icon(UIcons.Disk, size: TextSize.Large)
    .Foreground(Theme.Primary.Background)
    .SetTitle("Save");

var rocket = Icon(Emoji.Rocket, TextSize.Large);

HStack().AlignItemsCenter().Children(saveIcon, TextBlock("Save"), rocket);
```

## Related

- UIcons — `/tesserae/utilities/uicons`
- IconToggle — `icon-toggle.md`
- Full docs & API: `/tesserae/components/icon`
