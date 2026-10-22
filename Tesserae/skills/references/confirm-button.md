---
name: confirm-button
description: A two-step button whose first click arms it (label morphs to a confirmation prompt, danger tone, draining bar) and second click confirms. Use for a small destructive action in a Tesserae (C#/Transpose) app where a dialog is out of proportion.
---

# ConfirmButton

Asks for a second click instead of opening a dialog. The first click arms it:
the label morphs (its width animates) into the confirmation prompt, the button
takes the theme's danger style and a bar drains along its foot. A second click
before the bar runs out confirms and fires `OnConfirmed`. Letting it run out,
pressing Escape or moving focus away disarms it. The prompt is announced to
screen readers.

## Create

`UI.ConfirmButton(string text, string confirmText = "Click again to confirm", string doneText = null)`
returns a `ConfirmButton`. `doneText` is shown after confirming (null keeps `text`).

## Key configuration

- `.OnConfirmed(Action)` — runs on the confirming click.
- `.Timeout(int ms)` — how long it stays armed (default 3000).
- `.ResetAfter(int ms)` — how long the done label shows (default 1500); `0` keeps it until `.Reset()`.
- `.SetIcon(UIcons)`, `.Disabled(bool = true)`, `.Reset()`, `.IsArmed`.

## Example

```csharp
using static Tesserae.UI;

var row = HStack().AlignItemsCenter();
row.Children(
    TextBlock("old-logo.svg").Grow(),
    ConfirmButton("Remove", "Remove?").SetIcon(UIcons.Trash)
        .OnConfirmed(() => row.DismissWithSwipe(() => files.Remove("old-logo.svg"))));
```

## Related

- Press-and-hold confirmation — `hold-button.md`
- Ordinary button — `button.md`
- Dialog — `dialog.md`
- `DismissWithSwipe` and the other feedback extensions — `micro-interactions.md`
- Full docs & API: `/tesserae/commands/confirm-button`
