---
name: text-box
description: A single-line text input. Use when collecting string input in forms or any field in a Tesserae (C#/Transpose) app.
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
- `.NoBorder()` — remove the border, in every state (no hover edge, no focus ring); for an input set inside a box that draws its own edge.
- `.NoMinWidth()` — drop the default minimum width.
- `.UnlockHeight()` — allow the input to fill its container's height.
- `MaxLength` — property capping the character count.

Validation / errors (inherited from `Input`):

- `.Error("message")` then `.IsInvalid()` — show an error state. The message line only takes space while it shows; a box with `.Validation(...)` reserves it from the start, so validating never moves the fields below.
- `.Validation(tb => tb.Text.Length == 0 ? "Empty" : null)` — live validator.

Asynchronous check (inherited from `Input`, so `AutoCompleteBox` and `NumberPicker` have it too):

- `.CheckAsync(Func<string, Task<InputCheck>> check, int debounceMs = 400)` — runs after each edit (debounced) and
  once for the initial text; an empty box is not checked, and only the answer for the current text is shown.
  Return `InputCheck.Valid(message = "Verified", tooltip = null)`, `.Warning(message, tooltip = null)`,
  `.Invalid(message = "Not verified", tooltip = null)` or `.None()`. The answer is drawn inside the box at its end:
  an icon (check, warning triangle, cross; a spinner with "Checking…" while it runs) and the message as a one-line
  label, announced to screen readers. `tooltip` is a plain-text explanation shown over the indicator. An invalid
  answer turns the border red without touching `.IsInvalid`, so `.Validation(...)` keeps working on the same box.
- `.Recheck()` — run the check now (after a setting it depends on changed). `.LastCheck` holds the last answer.

## Example

```csharp
using static Tesserae.UI;

var stack = Stack().Children(
    TextBlock("Name:"),
    TextBox().SetPlaceholder("Enter your name"),
    TextBlock("Password:"),
    TextBox().Password(),
    TextBox().Validation(tb => tb.Text.Length == 0 ? "Required" : null),
    TextBox().CheckAsync(async user => await UserExistsAsync(user)
        ? InputCheck.Valid()
        : InputCheck.Invalid(tooltip: $"No user called \"{user}\""))
);
```

## Related

- Label — `label.md`
- AutoCompleteBox (suggestions + async check) — `auto-complete-box.md`
- Full docs & API: `/tesserae/inputs/text-box`
