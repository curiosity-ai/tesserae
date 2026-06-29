---
name: resource-card
description: A card tailored to summarising one resource (model, document, service) with icon, title, subtitle, tags, description, date, and a footer for actions. Use when rendering resource tiles, often inside a SearchableList grid, in a Tesserae (C#/h5) app.
---

# ResourceCard

A `Card` with predefined slots. Empty sections are hidden automatically; the footer appears only when a footer or footer-command is set.

## Create

`UI.ResourceCard()` — returns a `ResourceCard`. Also `new ResourceCard()`.
Bring factories into scope with `using static Tesserae.UI;`.

## Key configuration

Each setter takes a `string` (wrapped in a styled `TextBlock`) or an `IComponent`:

- `.SetIcon(IComponent)` — leading icon (e.g. `Icon(UIcons.Picture, size: TextSize.Large)`).
- `.SetTitle(string | IComponent)` / `.SetSubtitle(string | IComponent)`.
- `.SetTags(params IComponent[])` — small inline tags, e.g. `Badge("...")`.
- `.SetDescription(string | IComponent)` / `.SetDate(string | IComponent)`.
- `.SetFooter(IComponent)` — left-aligned footer content (e.g. a link).
- `.SetFooterCommands(params IComponent[])` — right-aligned footer actions (e.g. buttons).
- `.BackgroundColor(string)` / `.Border(color, size = null)` — card chrome.

## Example

```csharp
using static Tesserae.UI;

var card = ResourceCard()
    .SetIcon(Icon(UIcons.Picture, size: TextSize.Large))
    .SetTitle("seedream-4.0")
    .SetSubtitle("bytedance")
    .SetTags(Badge("Text-to-Image"))
    .SetDescription("ByteDance's image creation model.")
    .SetDate("Apr 8, 2026")
    .SetFooter(Link("https://example.com/terms", "Terms"))
    .SetFooterCommands(Button("Copy ID").SetIcon(UIcons.Copy).NoBorder().NoBackground());
```

## Related

- Card — `.../card/SKILL.md`
- Full docs & API: `/tesserae/components/resource-card`
