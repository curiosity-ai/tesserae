---
name: dropdown
description: A select-style input for picking one or many values from a list, with search, async loading, and validation. Use when offering a compact list-of-options chooser in a Tesserae (C#/h5) app.
---

# Dropdown

A combobox-style input. Single- or multi-select, with optional search box, async
item loading, custom selection rendering, and validation. Items are
`Dropdown.Item`s created with `UI.DropdownItem(...)`.

## Create

`UI.Dropdown()` (or `UI.Dropdown(string noItemsText)`) — the dropdown.
`UI.DropdownItem(string text, string selectedText = "", UIcons? icon = null)` — an option;
`UI.DropdownItem()` for a divider/header placeholder.
Bring factories into scope with `using static Tesserae.UI;`.

## Key configuration

Dropdown:

- `.Items(params Dropdown.Item[])` — set options (replaces existing).
- `.Items(Func<Task<Item[]>>)` + `.LoadItemsAsync()` — async loading (auto-loaded when opened).
- `.Single()` / `.Multi()` — selection mode.
- `.Searchable(string placeholder = "Search")` — add a search box.
- `.Required()` / `.Disabled()` / `.NoBorder()` / `.NoBackground()` / `.FitContent()`.
- `.Placeholder(string|IComponent)` — empty-state text.
- `.Attach(handler)` — fires on selection change (use for validation: set `.IsInvalid` and `.Error`).
- `.SelectedItems` / `.SelectedText` — current selection. `.AsObservable()` for the selected list.

Dropdown.Item:

- `.Selected()` / `.SelectedIf(bool)` / `.IsSelected` — selection state.
- `.Header()` / `.Divider()` — non-option rows.
- `.Disabled()`, `.SetData<T>(T)` / `.GetDataAs<T>()`, `.OnSelected(...)`.

## Example

```csharp
using static Tesserae.UI;

var dd = Dropdown().Items(
    DropdownItem("Option 1").Selected(),
    DropdownItem("Option 2")
);

dd.Attach(d =>
{
    var ok = d.SelectedItems.Length == 1 && d.SelectedItems[0].Text == "Option 1";
    d.IsInvalid = !ok;
    if (!ok) d.Error = "Please select 'Option 1'";
});
```

## Related

- ChoiceGroup, Toggle — alternative single-choice inputs
- Full docs & API: `/tesserae/components/dropdown`
