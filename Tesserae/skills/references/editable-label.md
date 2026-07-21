---
name: editable-label
description: An inline-editable single-line text surface that toggles between a read-only label and a textbox on click. Use for editing a title/name field in place in a Tesserae (C#/Transpose) app.
---

# EditableLabel

Displays text as a label until clicked, then swaps to a single-line textbox with
save/cancel. Enter saves, Escape cancels, blur saves. Multi-line equivalent:
`EditableArea`.

## Create

`UI.EditableLabel(string text = "")` — the editable label.
Bring factories into scope with `using static Tesserae.UI;`.

## Key configuration

- `.OnSave(SaveEditHandler)` — `(EditableLabel sender, string newValue) => bool`; return true to accept, false to reject.
- `.SetText(string)` — set the displayed/edited text.
- `.Size` (`TextSize`), `.Weight` (`TextWeight`), `.TextAlign` (`TextAlign`) — text formatting.
- `.IsEditingMode` — get/set edit mode.
- `.AsObservable()` — current text as `IObservable<string>`.

## Example

```csharp
using static Tesserae.UI;

var label = EditableLabel("Click to edit me");
label.Size   = TextSize.Large;
label.Weight = TextWeight.Bold;
label.OnSave((sender, newValue) =>
{
    if (string.IsNullOrWhiteSpace(newValue)) return false; // reject empty
    console.log("Saved: " + newValue);
    return true;
});
```

## Related

- EditableArea — `editable-area.md` (multi-line)
- TextBlock, TextBox
- Full docs & API: `/tesserae/components/editable-label`
