---
name: text-box
description: A single-line text input. Use when collecting string input in forms or any field in a Tesserae (C#/h5) app.
---

# TextBox

A single-line text input with placeholder, read-only, password, and validation support. Extends `Input`, so it inherits `.Text`, `.Disabled()`, `.Error()`, `.IsInvalid()`, `.Validation(...)`, and change events.

## Create

`UI.TextBox(text = "")` — returns a `TextBox`. `using static Tesserae.UI;` brings it into scope.

## Key configuration

- `.SetPlaceholder(string)` — placeholder text (or set the `Placeholder` property).
- `.ReadOnly()` — make the field read-only.
- `.Password()` — switch to password (masked) mode.
- `.Disabled()` — disable the input (from `Input`).
- `.NoBorder()` — remove the border.
- `.NoMinWidth()` — drop the default minimum width.
- `.UnlockHeight()` — allow the input to fill its container's height.
- `MaxLength` — property capping the character count.

Validation / errors (inherited from `Input`):

- `.Error("message")` then `.IsInvalid()` — show an error state.
- `.Validation(tb => tb.Text.Length == 0 ? "Empty" : null)` — live validator.

## Example

```csharp
using static Tesserae.UI;

var stack = Stack().Children(
    TextBlock("Name:"),
    TextBox().SetPlaceholder("Enter your name"),
    TextBlock("Password:"),
    TextBox().Password(),
    TextBox().Validation(tb => tb.Text.Length == 0 ? "Required" : null)
);
```

## Related

- Label — `.label.md`
- Full docs & API: `/tesserae/components/text-box`
