---
name: auto-complete-box
description: A free-text input that suggests values as the user types and can check the typed value asynchronously. Use when a field has a known but open set of good values (model ids a server lists, tags, domains) in a Tesserae (C#/Transpose) app.
---

# AutoCompleteBox

A single-line text input with a suggestion list that narrows as you type, plus an optional debounced
asynchronous check (`Input.CheckAsync`) whose answer is shown as an icon and a short label at the end of the box. The text stays free: a suggestion is
a shortcut, not a constraint. If the value must be one of a list, use a `Dropdown().Searchable()` instead.

Extends `Input`, so it has `.Text`, `.Disabled()`, `.Required()`, `.Validation(...)`, `.Bind(...)`,
`.OnChange(...)`, `.OnInput(...)`, `.OnBlur(...)` like a `TextBox`.

## Create

`UI.AutoCompleteBox(string text = "")` — returns an `AutoCompleteBox`. `using static Tesserae.UI;` brings it into scope.

## Key configuration

- `.SetPlaceholder(string)` — placeholder text.
- `.Suggestions(params string[])` — a fixed list.
- `.Suggestions(Func<Task<string[]>> loader)` — loaded once, the first time the list is needed (focus or
  first keystroke), and cached. The component does the filtering (prefix matches first, then
  substring, case-insensitive), so the loader returns everything.
- `.ReloadSuggestions()` — drop the cached answer, after a setting the list depends on has changed.
- `.ReadOnly()` — read-only box; it shows no suggestions.
- `.CheckAsync(Func<string, Task<InputCheck>> check, int debounceMs = 400)` — the asynchronous check every input
  has (see `text-box.md`): a Verified / warning / Not verified label inside the box at its end, with an optional
  tooltip; an invalid answer turns the border red. It is what a field with an open set of values usually wants.

A pick (click, Enter, or Tab on a highlighted row) sets the text and raises the change event, so one
`.OnChange(...)` handler sees a pick and an edit alike (an edit raises it on blur).

Keyboard: ArrowDown/ArrowUp open the list and move through it, Enter picks the highlighted row, Escape
closes it. Tab moves to the next field as usual, picking the highlighted row first if there is one; with
none highlighted the typed text is kept. The input is a `role="combobox"` over a `role="listbox"`.

## Example

```csharp
using static Tesserae.UI;

var models = AutoCompleteBox(currentModel)
   .SetPlaceholder("The model id this endpoint answers to")
   .Suggestions(async () => await LoadModelIdsAsync())
   .CheckAsync(async id => (await LoadModelIdsAsync()).Contains(id)
        ? InputCheck.Valid("Listed", tooltip: "This endpoint lists the model id.")
        : InputCheck.Invalid(tooltip: $"This endpoint does not list \"{id}\"."))
   .OnChange((box, _) => Save(box.Text));
```

## Related

- TextBox — `text-box.md`
- Dropdown — `dropdown.md`
- Picker — `picker.md`
- Full docs & API: `/tesserae/inputs/auto-complete-box`
