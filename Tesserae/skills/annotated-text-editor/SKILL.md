---
name: annotated-text-editor
description: A multi-line text editor that highlights entity spans in place using a debounced async annotator callback. Use when building NLP/entity-tagging inputs or prompt editors that mark up text as the user types in a Tesserae (C#/h5) app.
---

# AnnotatedTextEditor

`AnnotatedTextEditor` is an editable textarea with an overlay that highlights entity
ranges. After the user pauses typing, a debounced async lambda (`Func<string, Task<Entity[]>>`,
default 500ms) is called and returns the entities to highlight. Entities can carry a
label (shown on hover), background, color, and border.

## Create

`UI.AnnotatedTextEditor(Func<string, Task<Entity[]>> annotator, string initialText = null, int debounceMs = 500, string placeholder = null)`.
Bring factories into scope with `using static Tesserae.UI;`.

`AnnotatedTextEditor.Entity(int start, int length, string label = null, string background = null, string color = null, string border = null)`
describes one highlight. Callers must return non-overlapping entities sorted by start.

## Key configuration

- `.SetText(string)` / `.Text` — get/set the content (re-triggers annotation).
- `.ReadOnly(bool = true)` — non-editable but entities still clickable.
- `.Disabled(bool = true)` — disable entirely.
- `.MinHeight(unitSize)` / `.Height(unitSize)` / `.H(px)` — sizing.
- `.OnAnnotationsChanged((s, entities) => ...)` — after each annotation pass.
- `.OnTextChanged((s, text) => ...)` — on input.
- `.OnEntityClick((s, entity, mouseEvent) => ...)` — clicking a highlighted span.
- `.Entities` — current `Entity[]`.

## Example

```csharp
using static Tesserae.UI;

var editor = AnnotatedTextEditor(
        annotator:   AnnotateAsync,           // Func<string, Task<Entity[]>>
        initialText: "Alice met Bob in Berlin.",
        placeholder: "Type some text...")
    .MinHeight(160.px())
    .OnEntityClick((s, entity, e) =>
        Toast().Information(s.Text.Substring(entity.Start, entity.Length)));
```

## Related

- Full docs & API: `/tesserae/components/annotated-text-editor`
- TextBox — `/tesserae/components/text-box`
