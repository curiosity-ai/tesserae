---
name: text-block
description: A display component for rendering styled text. Use when showing static or dynamic labels, headings, or inline text in a Tesserae (C#/Transpose) app.
---

# TextBlock

Renders text with consistent, configurable styling (size, weight, alignment, colour, wrapping). The most common building block for labels and copy.

## Create

`UI.TextBlock(text, treatAsHTML: false, selectable: false, textSize, textWeight, afterText: null)` — returns a `TextBlock`. The simplest form is `TextBlock("Hello")`.
Bring the factories into scope with `using static Tesserae.UI;`. `.Render()` returns the `HTMLElement`.

## Key configuration

Sizes and weights come from `ITextFormating` fluent helpers:

- `.Tiny()` / `.Small()` / `.Medium()` / `.Large()` / `.XLarge()` — text size.
- `.Regular()` / `.SemiBold()` / `.Bold()` — text weight.
- `.NoWrap()` — disable wrapping (sets `CanWrap = false`).
- `.Primary()` / `.Secondary()` / `.Success()` / `.Danger()` / `.Invalid()` — colour variant.
- `.Title(string)` — tooltip (hover) text.

Useful properties:

- `Text` — get/set the plain text.
- `HTML` — get/set inner HTML (when `treatAsHTML`).
- `IsSelectable` — allow text selection.
- `EnableEllipsis` / `EnableBreakSpaces` — overflow behaviour.

## Example

```csharp
using static Tesserae.UI;

var heading = TextBlock("Hello, Tesserae!").Medium().SemiBold();

var note = TextBlock("The quick brown fox.", selectable: true)
    .Large()
    .NoWrap()
    .Title("Example tooltip");
```

## Related

- Label — `.label.md`
- Full docs & API: `/tesserae/components/text-block`
