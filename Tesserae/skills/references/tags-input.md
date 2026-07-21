---
name: tags-input
description: A free-form multi-value field where the user types short string values ("tags"/"chips") confirmed with a delimiter key, removable by backspace or click. Use when collecting an open-ended list of labels, categories, or keywords in a Tesserae (C#/Transpose) app.
---

# TagsInput

A field for assembling a list of short strings. The user types a value and
presses `Enter` (or a delimiter like `,`) to turn it into a chip; backspace on
an empty field or clicking the chip's `×` removes it. For values from a fixed
set, prefer `Picker<T>` instead.

## Create

`UI.TagsInput()` for an empty field, or `UI.TagsInput(params string[] initialTags)`
(i.e. `TagsInput("docs", "ui")`) to pre-populate. Returns a `TagsInput`.
Bring factories into scope with `using static Tesserae.UI;`.

## Key configuration

- `.Tags` — current `IReadOnlyList<string>` in insertion order.
- `.AsObservable()` — reactive `IObservable<IReadOnlyList<string>>` for binding.
- `.Add(tag)` / `.Remove(tag)` / `.Clear()` — programmatic edits.
- `.SetPlaceholder(text)` — inline entry placeholder.
- `.AllowingDuplicates()` / `.AllowDuplicates` — permit repeats (default off).
- `.WithMaxTags(int)` / `.MaxTags` — cap the count.
- `.WithDelimiters(params char[])` — extra commit keys (default `,`; empty array
  = Enter only).
- `.WithNormalizer(Func<string,string>)` — transform/reject values (default `Trim`).
- `.OnTagAdded(h)` / `.OnTagRemoved(h)` / `.OnChange(h)` — change callbacks.

## Example

```csharp
using static Tesserae.UI;

var tags = TagsInput("docs", "tesserae", "ui")
    .SetPlaceholder("Add a tag…")
    .WithMaxTags(8)
    .OnChange(() => console.log("changed"));

DeferSync(tags.AsObservable(), list => TextBlock(string.Join(", ", list)));
```

## Related

- Picker (fixed-set multi-select) — `/tesserae/components/picker`
- TextBox — `/tesserae/components/text-box`
- Full docs & API: `/tesserae/components/tags-input`
