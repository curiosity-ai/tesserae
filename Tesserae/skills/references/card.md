---
name: card
description: A bordered, shadowed surface that groups related content, with optional title header and footer. Use to present a single topic's content and actions as a self-contained block in a Tesserae (C#/Transpose) app.
---

# Card

`Card` is a surface for grouping content on one topic. It takes a body component and can
add a header (title) and footer on demand. Header/footer/content slots are created lazily
the first time you set them.

## Create

`UI.Card(IComponent content, bool noAnimation = false)`.
Bring factories into scope with `using static Tesserae.UI;`.

## Key configuration

- `.SetTitle(string title, UIcons? icon = null, string iconColor = null, UIconsWeight iconWeight = UIconsWeight.Regular)` — add/replace the header, optionally with an icon drawn just before the title. Without `iconColor` the glyph takes the title's colour.
- `.SetTitle(IComponent)` — a header of your own.
- `.SetContent(IComponent)` — replace the body.
- `.SetFooter(IComponent)` — add/replace the footer.
- `.Compact()` — denser layout.
- `.NoPadding()` — remove inner padding.
- `.BackgroundColor(string)` / `.Background` — surface color.
- `.Border(string color, UnitSize size = null)` — border (default 1px).
- `.HoverColor(bool = true)` — hover overlay.
- `.OnClick((sender, evt) => ...)` or `.OnClick(Action)` — makes the card clickable (sets pointer cursor).

## Example

```csharp
using static Tesserae.UI;

var card = Card(TextBlock("Card body content."))
    .SetTitle("Rejected items", UIcons.CircleXmark, Theme.Colors.Red600)
    .SetFooter(Button("Action").Primary())
    .HoverColor();
```

## Related

- BackgroundArea — `background-area.md`
- UIcons — the glyph set `SetTitle`'s icon comes from — `uicons.md`
- Full docs & API: `/tesserae/components/card`
