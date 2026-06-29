---
name: file-selector-and-drop-area
description: Two file-input components — FileSelector (dialog-style picker, validatable) and FileDropArea (drag-and-drop zone). Use when users need to pick or drop files in a Tesserae (C#/h5) app.
---

# FileSelector & FileDropArea

`FileSelector` is a text-box-styled single-file picker that supports validation. `FileDropArea` is a drag-and-drop zone that also opens the file dialog on click and can accept multiple files (including dropped directories).

## Create

- `UI.FileSelector()` — returns a `FileSelector`.
- `UI.FileDropArea()` or `UI.FileDropArea(IComponent component)` — returns a `FileDropArea` (the overload wraps your own content as the drop target).

Bring factories into scope with `using static Tesserae.UI;`.

## FileSelector configuration

- `.SetPlaceholder(text)` — text shown when empty.
- `.SetAccepts(accepts)` — accepted types, e.g. `".zip,.rar"` or `"image/*"`.
- `.OnFileSelected((fs, e) => …)` — handler; read `fs.SelectedFile` (`.name`, `.size`).
- `.Required()` / `.NoTextBox()` / `.Reset()`. Implements `ICanValidate` (`Error`, `IsInvalid`, `IsRequired`).

## FileDropArea configuration

- `.Multiple()` — accept multiple files.
- `.SetAccepts(accepts)` — accepted types.
- `.OnFilesDropped((sender, File[] files) => …)` — handler.
- `.SetContent(IComponent)` / `.OpenFileSelection()` / `.Reset()`.

## Example

```csharp
using static Tesserae.UI;

var selector = FileSelector()
    .SetPlaceholder("Select an image")
    .SetAccepts("image/*")
    .OnFileSelected((fs, e) => console.log($"Selected: {fs.SelectedFile.name}"));

var drop = FileDropArea()
    .Multiple()
    .OnFilesDropped((sender, files) =>
    {
        foreach (var f in files) console.log($"Dropped: {f.name}");
    });
```

## Related

- Validator (form validation) — `../validator/SKILL.md`
- Full docs & API: `/tesserae/utilities/file-selector-and-drop-area`
