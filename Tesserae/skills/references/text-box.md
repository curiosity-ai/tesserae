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
- `.AI(bool value = true, bool strongEffect = false)` / `.IsAI` — mark the field as holding what a model wrote (a generated title, a suggested value): AI tint and accent edge. Turn it off (`AI(false)`) once the user types over the value. See `ai-variants.md`.
- `await tb.SpinWhileAsync(async () => …)` / `SpinWhileAsync<T>(…)` — a spinner inside the field, at its end, while the call runs; gone when it finishes or throws (the exception propagates). The text box's version of `Button.OnClickSpinWhile`. Overlapping calls share one spinner; the field stays editable; the spinner takes the AI arc while the field is `AI()`. `.IsSpinning` reads it.

Validation / errors (inherited from `Input`):

- `.Error("message")` then `.IsInvalid()` — show an error state. The message line only takes space while it shows; a box with `.Validation(...)` reserves it from the start, so validating never moves the fields below.
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

// A value a model suggests: spin inside the field while it is fetched, mark it AI, and drop the mark once the user types.
var title = TextBox().SetPlaceholder("Suggested from the description");

var suggestion = await title.SpinWhileAsync(() => SuggestTitleAsync(description.Text));
title.Text = suggestion;
title.AI();

title.OnInput((s, e) => { if (e is object) s.AI(false); });   // Text = … raises OnInput with no event
```

## Related

- Label — `label.md`
- AutoCompleteBox (suggestions + async check) — `auto-complete-box.md`
- Full docs & API: `/tesserae/inputs/text-box`
