---
name: saving-toast
description: A Toast variant that shows a "saving…" indicator and swaps to a success or error message, plus a Task extension that wraps an await in one. Use when giving feedback for a background save/async operation in a Tesserae (C#/h5) app.
---

# SavingToast

Wraps the standard `Toast` to standardise "Saving…", "Saved", and "Error" feedback (icons, colours, durations). The saving state stays up indefinitely until you call `Saved(...)` or `Error(...)`.

## Create

`UI.SavingToast(string initialMessage = null)` or `new SavingToast(initialMessage)` — returns a `SavingToast`.
Bring factories into scope with `using static Tesserae.UI;`.

## Key configuration

- `.Saving(string message = null, string title = "Saving...")` — show the indeterminate saving toast (indefinite).
- `.Saved(string message = null, string title = "Saved")` — swap to success; auto-dismisses after `MinimumDisplayTime`.
- `.Error(string message = null, string title = "Error", bool untilDismissed = false)` — swap to error; pass `untilDismissed: true` to keep it open with a close button.
- `.MinimumDisplayTime` — `TimeSpan` the saved/error state stays visible (default 5s).

### Task extension

`task.WithSavingToast(savingMessage, savedMessage, errorMessage)` — awaits a `Task`/`Task<T>` while showing a SavingToast that resolves to Saved or Error automatically (rethrows on failure).

## Example

```csharp
using static Tesserae.UI;

// Manual:
var toast = SavingToast("Saving data...");
toast.Saving();
await Task.Delay(2000);
toast.Saved("All done!");

// Or wrap an await:
await SaveAsync().WithSavingToast(savedMessage: "Saved!");
```

## Related

- SaveButton — `.save-button.md`
- Full docs & API: `/tesserae/components/saving-toast` and `/tesserae/utilities/saving-toast`
