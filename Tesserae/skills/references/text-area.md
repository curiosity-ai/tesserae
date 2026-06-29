---
name: text-area
description: A multi-line text input with placeholder, validation, read-only/disabled states, and optional auto-resize. Use when collecting multi-line text (comments, descriptions) in a Tesserae (C#/h5) app.
---

# TextArea

A multi-line text input (`<textarea>`). Supports placeholder, max length,
required/invalid validation states, read-only/disabled, and grow-to-fit
auto-resize. Exposes its text as an observable.

## Create

`UI.TextArea(string text = "")` (i.e. `TextArea()` / `TextArea("initial")`)
returns a `TextArea`. Bring factories into scope with `using static Tesserae.UI;`.

## Key configuration

- `.SetText(text)` / `.Text` / `.ClearText()` — content.
- `.SetPlaceholder(text)` / `.Placeholder` — placeholder.
- `.OnInput((s, e) => ...)` / `.OnChange((s, e) => ...)` — change handlers.
- `.AsObservable()` — `IObservable<string>` of the text.
- `.Disabled(bool = true)` / `.ReadOnly()` — input state.
- `.Required()` / `.IsInvalid` / `.Error(msg)` — validation display.
- `.MaxLength` — character cap.
- `.NoSpellCheck()` — disable browser spellcheck.
- `.AutoResize(bool allowShrink = true, int? minHeight = null, int? maxHeight = null)`
  — grow with content; clamp with min/max pixel heights.
- `.Focus()` — scroll into view and focus.

## Example

```csharp
using static Tesserae.UI;

var component = VStack().Children(
    Label("Description").SetContent(
        TextArea().SetPlaceholder("Enter a description…").AutoResize(maxHeight: 150)),
    Label("Notes").Required().SetContent(
        TextArea().OnInput((s, e) => console.log(s.Text)))
);
```

## Related

- TextBox (single line) — `/tesserae/components/text-box`
- Full docs & API: `/tesserae/components/text-area`
