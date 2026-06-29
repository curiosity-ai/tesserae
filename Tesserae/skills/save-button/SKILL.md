---
name: save-button
description: A Button variant that drives itself through save states (pending, verifying, saving, saved, error) with matching icon, colour, and spinner. Use when wiring an async save action to a single button in a Tesserae (C#/h5) app.
---

# SaveButton

A button that encapsulates the visual states of a save operation. Click handlers only fire while the button is in `PendingSave`.

## Create

`new SaveButton()` — there is no `UI.SaveButton` factory; construct directly.
Bring other factories into scope with `using static Tesserae.UI;`.

`SaveButton.State`: `NothingToSave`, `PendingSave`, `Verifying`, `Saving`, `Saved`, `Error`.

## Key configuration

- `.Configure(save:, verifying:, saving:, saved:, error:, saveHover:, saveIcon:, saveHoverIcon:, pendingPrimary:)` — customise per-state text/icons (all optional).
- `.SetState(State, string message = null)` — set state imperatively; convenience: `.Pending()`, `.Verifying()`, `.Saving()`, `.Saved()`, `.Error()`, `.NothingToSave()`.
- `.OnClick(Action)` — handler (only runs in `PendingSave`).
- `.OnClickSpinWhile(Func<Task>)` — async handler that shows the spinner while awaiting.
- `.VerifyingWhile(Func<Task<State>>, text, onError)` — run an async check and apply the returned state, auto-handling errors.

## Example

```csharp
using static Tesserae.UI;

SaveButton saveButton = null;
saveButton = new SaveButton()
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

- Button — `.../button/SKILL.md`
- SavingToast — `.../saving-toast/SKILL.md`
- Full docs & API: `/tesserae/components/save-button`
