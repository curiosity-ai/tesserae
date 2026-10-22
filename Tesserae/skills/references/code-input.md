---
name: code-input
description: A one-time-code field drawn as one cell per character, with paste and OTP autofill support. Use for verification codes, PINs and short codes in a Tesserae (C#/Transpose) app.
---

# CodeInput

Draws a verification code as a row of cells. Each cell fills with a small pop as
it is typed, the next one carries the caret, and `OnCompleted` fires when the
last is filled. Underneath is one ordinary `<input>` laid over the cells
(`autocomplete="one-time-code"`), so pasting a whole code, browser and phone
autofill, Backspace and screen readers behave as in a plain field. Pasted text
is cleaned (`"24-68 "` becomes `2468`).

## Create

`UI.CodeInput(int length = 6)` returns a `CodeInput` (an `IBindableComponent<string>`).

## Key configuration

- `.OnCompleted(Action<string>)` — runs when every cell is filled.
- `.Invalid(bool = true)` — danger outline and a shake for a refused code; typing again clears it.
- `.Numeric(bool = true)` — digits only (default); `false` accepts letters (shown uppercase).
- `.Masked(bool = true)` — dots instead of characters.
- `.Text`, `.IsComplete`, `.Clear()`, `.Focus()`, `.Disabled(bool = true)`, `AsObservable()`.

## Example

```csharp
using static Tesserae.UI;

var code = CodeInput(6).Focus();
code.OnCompleted(async value =>
{
    if (!await VerifyAsync(value)) code.Invalid();
});
```

## Related

- Text box — `text-box.md`
- Validator — `validator.md`
- Shake and other feedback — `micro-interactions.md`
- Full docs & API: `/tesserae/inputs/code-input`
