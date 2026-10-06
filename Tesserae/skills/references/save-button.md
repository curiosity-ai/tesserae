---
name: save-button
description: A Button variant that drives itself through save states (pending, verifying, saving, saved, error) with matching icon, colour, and spinner. Use when wiring an async save action to a single button in a Tesserae (C#/Transpose) app.
---

# SaveButton

A button that encapsulates the visual states of a save operation. Click handlers only fire while the button is in `PendingSave`.

The button keeps one width in every state: it is sized by its Verifying label (spinner + `verifying` text, "Verifying..." by default), so it does not resize as it moves through a save. Any label or `message` wider than that is ellipsized; configure a longer `verifying:` text to make the button wider.

## Create

`SaveButton()` (or `new SaveButton()`).
Bring other factories into scope with `using static Tesserae.UI;`.

`SaveButton.State`: `NothingToSave`, `PendingSave`, `Verifying`, `Saving`, `Saved`, `Error`.

## Key configuration

- `.Configure(save:, verifying:, saving:, saved:, error:, saveHover:, saveIcon:, saveHoverIcon:, pendingPrimary:)` — customise per-state text/icons (all optional).
- `.SetState(State, string message = null)` — set state imperatively; convenience: `.Pending()`, `.Verifying()`, `.Saving()`, `.Saved()`, `.Error()`, `.NothingToSave()`.
- `.OnClick(Action)` — handler (only runs in `PendingSave`).
- `.OnClickSpinWhile(Func<Task>, text, onError)` — async handler that shows the `Saving` state (or `text`) while awaiting. If the handler sets no state itself, the button returns to `PendingSave` afterwards; an exception shows `Error` and a toast (or calls `onError` instead), then is rethrown so it is logged to the console.
- `.VerifyingWhile(Func<Task<State>>, text, onError)` — run an async check and apply the returned state, auto-handling errors.

## Example

```csharp
using static Tesserae.UI;

SaveButton saveButton = null;
saveButton = SaveButton()
    .Configure(saved: "All changes saved!")
    .OnClick(async () =>
    {
        saveButton.SetState(SaveButton.State.Verifying);
        await Task.Delay(500);
        saveButton.SetState(SaveButton.State.Saving);
        await Task.Delay(1000);
        saveButton.SetState(SaveButton.State.Saved);
    });
```

## Related

- Button — `button.md`
- SavingToast — `saving-toast.md`
- Full docs & API: `/tesserae/components/save-button`
