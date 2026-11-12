---
name: vertical-separator
description: A thin vertical divider for the items of a horizontal row, stretching to the row's height, optionally with a label or component in the middle. Use to separate side-by-side items in a Tesserae (C#/Transpose) app; the counterpart of HorizontalSeparator.
---

# VerticalSeparator

A vertical rule that divides the items of a row, as `HorizontalSeparator` divides a column. It stretches to the height of the row it
is in, and can carry a short label (the "or" between two halves of a form) or any component in the middle. It is a visual divider,
not a layout: for two resizable panes use `SplitView`.

## Create

`UI.VerticalSeparator(string text = "")` or `UI.VerticalSeparator(IComponent component)`. Bring factories into scope with
`using static Tesserae.UI;`.

## Key configuration

- `.Top()` / `.Center()` / `.Bottom()` — where the label sits on the line (`.Alignment`: `VerticalSeparator.Align.Top/Center/Bottom`).
- `.SetText(string)` / `.Text` — the label. `.SetContent(IComponent)` — custom content.
- `.Primary()` — primary-tone label. `.Background` — CSS background of the label.

A separator with no label is a plain rule. It takes its height from the row (`align-self: stretch`): in a row that centres its items, give the row
a height, or the separator is one line of text tall.

## Example

```csharp
using static Tesserae.UI;

var nav = HStack().Gap(8.px()).AlignItemsCenter().Children(
    TextBlock("Home"), VerticalSeparator(), TextBlock("Docs"), VerticalSeparator(), TextBlock("Blog"));

var either = HStack().H(120).Children(
    VStack().Grow().Children(TextBox().SetPlaceholder("Email"), TextBox().Password()),
    VerticalSeparator("or"),
    VStack().Grow().Children(Button("Email me a link").Primary()));
```

## Related

- HorizontalSeparator — `horizontal-separator.md`
- SplitView — `split-view.md`
- Stack — `stack.md`
