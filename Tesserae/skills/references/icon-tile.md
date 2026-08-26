---
name: icon-tile
description: The rounded, tinted square that leads a row - a glyph, a few letters like PPTX, or a small component - drawn over a wash computed from one colour, light or dark theme. Use to mark what a result, banner, metric or list row is in a Tesserae (C#/Transpose) app.
---

# IconTile

`IconTile` is the rounded, tinted square that leads a row: a glyph, a few letters
("PPTX", "CSV") or a small component of the host's own, drawn over a pale wash of
one colour.

It is the tile `OmniResult` puts in front of every search result, `Banner` in front
of its message and `Metric` beside its value — one shape, one way of tinting it,
wherever something needs to be marked with what it is.

The host passes **one** colour: the one the glyph keeps. The tile behind it is
computed from that colour — a light wash of it under a light theme, a deep one
under a dark theme, with the glyph lifted until it reads — and cached, so a list
drawing the same handful of colours pays for each of them once. A host therefore
only ever picks the colour that *means* something ("red is an error"), never the
four that draw it.

## Create

- `UI.IconTile(UIcons icon, string color = null, UIconsWeight weight = UIconsWeight.Regular)`
- `UI.IconTile(string text, string color = null, TextSize? size = null)`
- `UI.IconTile(IComponent iconOrImage, string color = null)`

Also `new IconTile()` for an empty one. Bring factories into scope with
`using static Tesserae.UI;`.

## Key configuration

- `.SetIcon(UIcons, color, weight)` — a glyph. A null colour leaves the tile neutral.
- `.SetIcon(string text, color, TextSize?)` — a few letters in place of a glyph, drawn uppercase and bold. The word is measured and drawn smaller when it is wider than the tile, so "PPTX" or "PARQUET" fits instead of being clipped, and three letters keep the full size at any tile size. Passing a `TextSize` pins the size and opts out of that fitting.
- `.SetIcon(IComponent, color)` — an `Image` thumbnail, an `Avatar`, an emoji. An image fills the tile (`object-fit: cover`).
- `.Size(UnitSize)` — how big the tile is (34px square by default). The glyph follows it.
- `.GlyphSize(UnitSize)` — pin the glyph size instead of letting it follow the tile.
- `.Rounded(UnitSize)` — corner radius (8px by default). `.Circular()` for a circle.
- `.Tint(string color)` — re-tint without touching what is on the tile. Null or empty puts it back to neutral.

## Example

```csharp
using static Tesserae.UI;

// A file type, spelled out - four letters are shrunk just enough to fit the square
var pptx = IconTile("PPTX", "#f97316");

// A glyph, bigger and rounder, for a KPI card
var inbox = IconTile(UIcons.Inbox, Theme.Colors.Purple600).Size(44.px()).Rounded(12.px());

// A thumbnail
var avatar = IconTile(Avatar(initials: "PN")).Circular();
```

## Related

- OmniResult — leads every search-result row with one — `omni-result.md`
- Banner — leads its notice with one — `banner.md`
- Metric — puts one beside the value — `metric.md`
- Icon — the plain glyph without a tile — `icon.md`
- Full docs & API: `/tesserae/components/icon-tile`
