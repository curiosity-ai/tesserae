---
name: auto-complete-box
description: A free-text input that suggests values as the user types and can check the typed value asynchronously. Use when a field has a known but open set of good values (model ids a server lists, tags, domains) in a Tesserae (C#/Transpose) app.
---

# AutoCompleteBox

A single-line text input with a suggestion list that narrows as you type, plus an optional debounced
asynchronous check whose answer is shown as an icon at the end of the box. The text stays free: a suggestion is
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
- `.MaxSuggestions(int)` — cap on rows shown (default 50).
- `.OnSuggestionSelected((box, value) => ...)` — a pick by click or Enter. A pick also raises the change
  event, so one `.OnChange(...)` handler sees both a pick and an edit (an edit raises it on blur).
- `.CheckAsync(Func<string, Task<AutoCompleteCheck>> check, int debounceMs = 400)` — runs after each edit
  (debounced) and once for the initial text. Return `AutoCompleteCheck.Valid(msg)`,
  `.Warning(msg)`, `.Invalid(msg)` or `.None()`. The answer is an icon inside the box at its end
  (check, warning triangle, cross; a spinner while it runs), with the message as its hover title
  and announced to screen readers. Only the answer for the current text is shown. An empty box is
  not checked.
- `.Recheck()` — run the check now (e.g. after a setting it depends on changed). `.LastCheck` holds the
  last answer.

The indicator is separate from `.Error` / `.Validation(...)`, which keep working on the same box.

Keyboard: ArrowDown/ArrowUp open the list and move through it, Enter picks the highlighted row, Escape
and Tab close it. The input is a `role="combobox"` over a `role="listbox"`.

## Example

```csharp
using static Tesserae.UI;

var models = AutoCompleteBox(currentModel)
   .SetPlaceholder("The model id this endpoint answers to")
   .Suggestions(async () => await LoadModelIdsAsync())
   .CheckAsync(async id => (await LoadModelIdsAsync()).Contains(id)
        ? AutoCompleteCheck.Valid("Listed by this endpoint")
        : AutoCompleteCheck.Warning($"This endpoint does not list \"{id}\""))
   .OnChange((box, _) => Save(box.Text));
```

## Related

- TextBox — `text-box.md`
- Dropdown — `dropdown.md`
- Picker — `picker.md`
- Full docs & API: `/tesserae/inputs/auto-complete-box`
