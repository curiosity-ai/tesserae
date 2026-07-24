---
name: dialog
description: A blocking modal dialog with a title, body and prebuilt action-button rows (Ok / OkCancel / YesNo / ...), plus awaitable variants. Use when prompting the user for a confirmation or quick decision in a Tesserae (C#/Transpose) app.
---

# Dialog

A blocking modal (built on `Modal`) for quick confirmations and small forms.
It can show custom command buttons or one of the prebuilt button rows, which
each have an awaitable counterpart that resolves with the user's `Response`.

## Create

`Dialog(IComponent content = null, IComponent title = null, bool centerContent = true)`,
or string overloads `Dialog(title)` / `Dialog(title, content)`. Bring factories
into scope with `using static Tesserae.UI;`.

## Key configuration

- `.Title(IComponent)` / `.Content(IComponent)` — set header / body.
- `.Commands(params IComponent[])` — custom command-button row.
- `.Ok(...)`, `.OkCancel(...)`, `.YesNo(...)`, `.YesNoCancel(...)`, `.RetryCancel(...)` — prebuilt button rows that wire up callbacks **and call `.Show()`**.
- `.OkAsync()`, `.OkCancelAsync()`, `.YesNoAsync()`, ... — awaitable variants returning `Task<Dialog.Response>` (`Ok`/`Cancel`/`Yes`/`No`/`Retry`).
- `.Show()` / `.Hide(Action onHidden = null)` — show / hide manually.
- `.Draggable()` / `.Dark()` — drag handle / dark theme.
- `.Height(UnitSize)` / `.MinHeight(UnitSize)` — sizing.

## Example

```csharp
using static Tesserae.UI;

// Custom commands
Dialog dialog = null;
dialog = Dialog(TextBlock("Confirm"))
    .Content(TextBlock("Proceed with the change?"))
    .Commands(
        Button("Confirm").Primary().OnClick((c, e) => dialog.Hide()),
        Button("Cancel").OnClick((c, e) => dialog.Hide())
    );
dialog.Show();

// Awaitable prebuilt row
var answer = await Dialog("Delete?", "This cannot be undone.").YesNoAsync();
if (answer == Dialog.Response.Yes) { /* ... */ }
```

## Related

- Modal — `modal.md` (underlying surface)
- Panel — `panel.md`
- Full docs & API: `/tesserae/surfaces/dialog`
