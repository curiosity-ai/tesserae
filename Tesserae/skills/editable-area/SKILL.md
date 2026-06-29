---
name: editable-area
description: An inline-editable multi-line text surface that toggles between a read-only label and a textarea on click. Use for editing notes/descriptions in place in a Tesserae (C#/h5) app.
---

# EditableArea

Displays text as a label until clicked, then swaps to a textarea with save/cancel
behavior. Use it for paragraph-style content the user may edit in place
(notes, descriptions). Single-line equivalent: `EditableLabel`.

## Create

`UI.EditableArea(string text = "")` — the editable area.
Bring factories into scope with `using static Tesserae.UI;`.

## Key configuration

- `.OnSave(SaveEditHandler)` — `(EditableArea sender, string newValue) => bool`; return true to accept the edit, false to reject (keeps editing). Blur or click-away triggers save; Escape cancels.
- `.SetText(string)` — set the displayed/edited text.
- `.Size` (`TextSize`), `.Weight` (`TextWeight`), `.TextAlign` (`TextAlign`) — text formatting.
- `.IsEditingMode` — get/set edit mode.
- `.AsObservable()` — current text as `IObservable<string>`.

## Example

```csharp
using static Tesserae.UI;

var editable = EditableArea("Click to edit this paragraph");
editable.Size = TextSize.Medium;
editable.OnSave((sender, newValue) =>
{
    if (string.IsNullOrWhiteSpace(newValue)) { Toast().Warning("Cannot be empty."); return false; }
    Toast().Success("Saved.");
    return true;
});
```

## Related

- EditableLabel — `../editable-label/SKILL.md` (single-line)
- TextBlock, TextBox
- Full docs & API: `/tesserae/components/editable-area`
