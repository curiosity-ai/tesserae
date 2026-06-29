---
name: progress-modal
description: A blocking modal that shows a title, dynamic message, and either a spinner or a progress bar (determinate or indeterminate), with an optional cancel button. Use to report on a long-running operation in a Tesserae (C#/h5) app.
---

# ProgressModal

A blocking modal for long-running work. Starts as a spinner and can switch to a
percentage bar or an indeterminate bar, with a live message and optional cancel.

## Create

`ProgressModal()` — returns a `ProgressModal`. Bring factories into scope with
`using static Tesserae.UI;`.

## Key configuration

- `.Title(string or IComponent)` / `.Message(string or IComponent)` — header / status text.
- `.ProgressSpin()` — show the spinner (initial state).
- `.Progress(float percent)` or `.Progress(int position, int total)` — switch to a determinate bar.
- `.ProgressIndeterminated()` — switch to an indeterminate bar.
- `.WithCancel(Action<Button> onCancel, Action<Button> btnCancel = null)` — add a cancel button.
- `.Show()` / `.Hide()` / `.ShowEmbedded()`.

## Example

```csharp
using static Tesserae.UI;

var modal = ProgressModal().Title("Uploading").Message("Starting...").ProgressSpin();
modal.WithCancel(b => modal.Hide());
modal.Show();

for (int i = 0; i <= 100; i += 20)
{
    await Task.Delay(500);
    modal.Progress(i).Message($"Uploading {i}%");
}
modal.ProgressIndeterminated().Message("Finishing...");
await Task.Delay(1000);
modal.Hide();
```

## Related

- Modal — `../modal/SKILL.md` (underlying surface)
- ProgressIndicator — `/tesserae/components/progress-indicator`
- Spinner — `/tesserae/components/spinner`
- Full docs & API: `/tesserae/surfaces/progress-modal`
