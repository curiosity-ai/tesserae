---
name: dictation-targets
description: Find the text field to dictate into from anywhere on the page - the focused TextBox, TextArea or OmniBox, else a visible one. Use for a push-to-talk button, a keyboard shortcut or a voice command that types into whatever field the user is in, in a Tesserae (C#/Transpose) app.
---

# Dictation targets

Every `TextBox`, `TextArea` and `OmniBox` is an `IDictationTarget`, wherever it was built. An application that
transcribes speech (a shortcut, a push-to-talk button outside the field, a voice command) does not hold on to the fields:
it asks `DictationTargets` for the one in use and starts dictating into it. The words come from the same place as the
fields' microphone buttons: the field's own source, else `Dictation.Provider`, else the browser's speech recognition
(see `omni-box.md`, "Dictation").

## `IDictationTarget`

- `Text` - the text in the field (for an `OmniBox`, the input that is showing).
- `IsEditable` - enabled and not read-only.
- `CanDictate` - something can supply words (an `OmniBox` also needs `Config.DictationButton`).
- `IsDictating`.
- `BeginDictation()` - starts dictating after the text already there, **whether or not the field shows a microphone button**. Does nothing when already dictating or `CanDictate` is false.
- `EndDictation()` - stops; what was heard stays.

## `DictationTargets` (static)

- `Current()` - the **focused** target, else the first **visible** one that `IsEditable`; null when there is none.
- `Focused()` - the target holding keyboard focus (focus is anywhere inside its element), or null.
- `Visible()` - targets rendered, not hidden and within the viewport, in document order.
- `All()` - every target on the page, in document order, visible or not.
- `Elements()` / `Of(element)` - the root elements, and the target of one.
- `Register(element, target)` - for a component of your own that implements `IDictationTarget`; call once with its root element.

The queries are on this class rather than on the interface because the netstandard2.0 target does not allow static
interface members.

How it works: each target puts itself in a hidden field of its root element and marks the element with
`data-tss-dictation`, so a query is one `querySelectorAll` over what is on the page right now; a field removed from the
page is not found. The toolkit's fields register on construction, nothing to declare.

## Example

```csharp
using static Tesserae.UI;

// A push-to-talk button that dictates into whatever field the user was in. It must not take focus, or
// Focused() would be the button's page, not the field: stop the mousedown.
var talk = Button("Talk").OnClick((_, __) => DictationTargets.Current()?.BeginDictation());
talk.Render().addEventListener("mousedown", (Event e) => e.preventDefault());

// A text box with no microphone button can still be dictated into, with its own source.
var note = TextBox().SetPlaceholder("Note").DictationSource(MyProvider);
```

## Related

- Dictation (microphone button, providers, `DictationSession`) - `omni-box.md`
- `text-box.md`, `text-area.md`
