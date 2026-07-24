---
name: label
description: A non-interactive caption for a form field, with inline layout, auto-width alignment and a required marker. Use when captioning inputs (TextBox, NumberPicker, pickers) in a Tesserae (C#/Transpose) app.
---

# Label

A caption that pairs with input controls. Wires `htmlFor` to the contained input
automatically and offers inline layout plus auto-width so a column of labels
lines up. Extends `TextBlock`, so text-formatting helpers apply.

## Create

- `Label(string text = "")` — text label.
- `Label(IComponent component)` — label whose caption is a component.

Bring the factory into scope with `using static Tesserae.UI;`.

## Key configuration

- `.SetContent(IComponent)` / `.Content` — the control the label captions.
- `.Inline()` — render label and content side by side.
- `.AutoWidth(parentSelector = null, alignRight = false)` — size all sibling inline labels to the widest, so they align.
- `.Required()` / `.IsRequired` — show the required marker.
- `.SetMinLabelWidth(UnitSize)` — fixed minimum label width.
- `.AlignLabelTop()` — top-align the label against tall content.
- Inherited from `TextBlock`: `.Primary()`, `.Secondary()`, `.Tiny()`, `.Disabled()`, `.SemiBold()`.

## Example

```csharp
using static Tesserae.UI;

var form = Stack().Children(
    Label("Username").SetContent(TextBox()),
    Label("Count").Required().SetContent(NumberPicker(1)),
    Stack().Children(
        Label("Name").Inline().AutoWidth().SetContent(TextBox()),
        Label("Email address").Inline().AutoWidth().SetContent(TextBox())
    )
);
```

## Related

- TextBlock — `/tesserae/components/text-block`
- TextBox — `/tesserae/components/text-box`
- Full docs & API: `/tesserae/components/label`
