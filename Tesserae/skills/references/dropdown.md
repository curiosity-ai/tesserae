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
- `.Attach(handler)` — fires on every selection change, including each toggle inside a multi-select popup (use for validation: set `.IsInvalid` and `.Error`).
- `.OnChange(handler)` — fires once, when the popup closes, with the selection the User settled on (for a single-select dropdown that is as soon as an option is picked, since picking one closes the popup).
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

dd.OnChange((d, _) => console.log($"picked {d.SelectedText}"));
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

## Rich item content

An item's content is any `IComponent` — an avatar and two lines of text, a badge, a colour swatch, a
sparkline — and the box shows a **clone** of it, laid out inline and comma-separated on a single
~32px row. So whatever an option renders in the list, the box has to render on one clipped line.

The second argument to `UI.DropdownItem` is the escape hatch: it is what the box shows once the
option is selected, and it is worth giving whenever the list content is taller or wider than one
row. Content taller than the row is the one case the box cannot rescue — a separator next to a
two-line block has nowhere good to sit.

```csharp
DropdownItem(
    // In the list: avatar, name, email.
    HStack().AlignItemsCenter().Gap(8.px()).Children(
        Avatar(initials: "AP").Size(AvatarSize.Small),
        VStack().Children(TextBlock("Ana Pereira").Small(), TextBlock("ana@example.com").Tiny().Secondary())),
    // In the box: avatar and first name, one line.
    HStack().AlignItemsCenter().Gap(4.px()).Children(
        Avatar(initials: "AP").Size(AvatarSize.XSmall),
        TextBlock("Ana").Small()))
   .SetKey("ana@example.com");
```

### Content that loads asynchronously

The copy the box shows is taken when the item becomes selected and is kept in step with the row
afterwards, so a `Defer` (or an image, or anything bound to an observable) that resolves later shows
up in the box too.

A `Defer` normally waits to be **mounted** before it loads, and an option's row is not in the
document until the dropdown is first opened — so a selected option would otherwise sit in the box as
a loading placeholder with nothing to make it resolve. Being selected is what settles it: its content
is on show in the box, so the dropdown asks it to load whether or not the list has ever been opened.

That covers a `Defer` that *is* the item's content. One nested deeper — a `Defer` inside a `Stack`
inside the item — is not reachable this way and still waits for the list to be opened. Giving the
item an explicit short form sidesteps the whole question, since a short form is a live component
mounted in the box and resolves there on its own:

```csharp
DropdownItem(
    Defer(async () => await LoadTheWholeRow(id),  loadMessage: Skeleton().Animated().W(120).H(16)),
    Defer(async () => await LoadJustTheLabel(id), loadMessage: Skeleton().Animated().W(60).H(12)))
   .SetKey(id)
   .Selected();
```

To take over the box entirely instead — a count, a pile of avatars — use
`.WithCustomSelectionRender(items => ...)`. That replaces the clones altogether, separators
included, so you own the whole presentation.

## Related

- ChoiceGroup, Toggle — alternative single-choice inputs
- Full docs & API: `/tesserae/components/dropdown`
