---
name: horizontal-separator
description: A thin horizontal divider that can carry centered text or custom content, with left/center/right alignment. Use to separate stacked sections in a Tesserae (C#/h5) app.
---

# HorizontalSeparator

A horizontal rule that optionally shows text or a custom component inline.
Content is center-aligned by default; switch to left or right. Ideal for
breaking a vertical layout into labelled sections.

## Create

`UI.HorizontalSeparator(string text = "")` or `UI.HorizontalSeparator(IComponent component)`.
Bring factories into scope with `using static Tesserae.UI;`.

## Key configuration

- `.Left()` / `.Center()` / `.Right()` — alignment (`.Alignment` property: `Align.Left/Center/Right`).
- `.SetText(string)` / `.Text` — separator label.
- `.SetContent(IComponent)` — replace with custom content.
- `.Primary()` — primary-tone styling.
- `.Background` — CSS background string.

## Example

```csharp
using static Tesserae.UI;

var ui = Stack().Children(
    HorizontalSeparator("Center"),
    HorizontalSeparator("Left").Left(),
    HorizontalSeparator("Right").Right(),
    HorizontalSeparator(
        HStack().Children(
            Icon(UIcons.Plane).AlignCenter().PaddingRight(8.px()),
            TextBlock("Custom").SemiBold().MediumPlus().AlignCenter()
        )
    ).Primary()
);
```

## Related

- TextBlock, Icon
- Stack — `../stack/SKILL.md`
- Full docs & API: `/tesserae/components/horizontal-separator`
