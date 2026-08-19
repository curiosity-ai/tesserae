---
name: dropdown
description: A select-style input for picking one or many values from a list, with search, async loading, and validation. Use when offering a compact list-of-options chooser in a Tesserae (C#/Transpose) app.
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
- `.AddItems(params Dropdown.Item[])` — append options, keeping the current ones and the selection; items whose `Key` is already listed are skipped.
- `.Items(Func<Task<Item[]>>)` + `.LoadItemsAsync()` — async loading (auto-loaded when opened).
- `.Single()` / `.Multi()` — selection mode.
- `.Searchable(string placeholder = "Search")` — add a search box.
- `.SearchAsync(Func<string, Task<Item[]>> searcher, string placeholder = "Search", int debounceMilliseconds = 250)` — lazy loading for lists too large to load up front; see below.
- `.Required()` / `.Disabled()` / `.NoBorder()` / `.NoBackground()` / `.FitContent()`.
- `.Placeholder(string|IComponent)` — empty-state text.
- `.Attach(handler)` — fires on selection change (use for validation: set `.IsInvalid` and `.Error`).
- `.SelectedItems` / `.SelectedText` — current selection. `.AsObservable()` for the selected list.

Dropdown.Item:

- `.Selected()` / `.SelectedIf(bool)` / `.IsSelected` — selection state.
- `.Header()` / `.Divider()` — non-option rows.
- `.Disabled()`, `.SetData<T>(T)` / `.GetDataAs<T>()`, `.OnSelected(...)`.
- `.SetKey(string)` / `.Key` — stable identity, used to dedupe when appending. Defaults to the item's text, so set it whenever the text is not unique.

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

## Lazy search (thousands of options)

`.SearchAsync(...)` turns the built-in search box into a lazy loader. The term the
User types is handed to your callback (debounced, newest term wins), and the items
it returns are **added** to the ones already rendered rather than replacing them —
so the seed items and, above all, the current selection survive every lookup.
Items whose `Key` is already listed are dropped, so a lookup that returns options
the dropdown already knows about does not duplicate them. The normal client-side
filter still runs on top. `SearchAsync` enables the search box itself, so
`.Searchable(...)` is not needed as well.

Seed the dropdown with the first page (plus whatever must be selectable without
searching, such as the current value), and let the callback fill in the rest:

```csharp
var dd = Dropdown()
   .Items(currentUserItem, firstPageItems)          // what is selectable without searching
   .SearchAsync(async term =>
    {
        var found = await API.Users.SearchAsync(term, limit: 100);
        return found.Select(u => DropdownItem(u.Name).SetKey(u.UID)).ToArray();
    }, placeholder: "Search users...");
```

The callback is also called with an empty string when the User clears the box, so
returning the first page for an empty term restores the seed list.

## Related

- ChoiceGroup, Toggle — alternative single-choice inputs
- Full docs & API: `/tesserae/components/dropdown`
