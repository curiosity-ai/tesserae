---
name: hold-button
description: A button that must be pressed and held to confirm, with a fill that sweeps across while held. Use for a destructive or irreversible action in a Tesserae (C#/Transpose) app, instead of a confirmation dialog.
---

# HoldButton

A button that acts only after being pressed and held: a fill sweeps across it
while it is held, letting go early drains the fill back from wherever it got to,
and holding to the end swaps the icon for a check and fires `OnConfirmed`. The
hold *is* the confirmation, so no dialog is needed. Works with the pointer and
the keyboard (hold Space or Enter).

## Create

`UI.HoldButton(string text, int durationMs = 1200)` returns a `HoldButton`.

## Key configuration

- `.OnConfirmed(Action)` — runs when a hold completes.
- `.Duration(int ms)` — how long to hold (minimum 200).
- `.ConfirmedText(string)` — label once confirmed ("Deleted"); defaults to the label.
- `.ResetAfter(int ms)` — how long the confirmed state shows (default 1500); `0` keeps it (and the button disabled) until `.Reset()`.
- `.Primary()` / `.Danger()` — tone. `.SetIcon(UIcons)` — leading icon (replaced by the check when confirmed).
- `.Disabled(bool = true)`, `.Reset()`, `.IsConfirmed`, `.Text`.

## Example

```csharp
using static Tesserae.UI;

HoldButton("Hold to delete", 1600).Danger().SetIcon(UIcons.Trash)
    .ConfirmedText("Deleted")
    .OnConfirmed(() => DeleteProject());
```

## Related

- Two-click confirmation in place — `confirm-button.md`
- Ordinary button — `button.md`
- Confirmation dialog, for expensive mistakes — `dialog.md`
- Shake / Pop and the other feedback extensions — `micro-interactions.md`
- Full docs & API: `/tesserae/commands/hold-button`
