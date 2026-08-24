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
`UI.DropdownItem(string text, string selectedText = "", UIcons? icon = null)` — a plain option.
`UI.DropdownItem(IComponent content, IComponent selectedContent)` — an option drawing components you
have already built: one for the row, one for the box. They must be different instances; pass the same
one twice, or `null` for the box, and it falls back to copying the row and says so on the console.
`UI.DropdownItem(Func<IComponent> content, Func<IComponent> selectedContent = null)` — the same from
recipes, so one recipe can serve both, and the box's is built only if the option is selected.
See **Rich item content**.
`UI.DropdownItem(IComponent content)` — **obsolete**. Draws that one component in the row and a
`cloneNode` copy of it in the box. The copy has no event listeners, no mount registration and no
component identity, so a `Defer` inside it never loads and nothing in it ever reacts. It still works,
so existing code keeps running; the compiler warns, and the fix is the `Func<IComponent>` overload.
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

An option is drawn **twice**: as a row in the open list, and — when it is selected — in the closed
box, laid out inline and comma-separated on a single ~32px row. A component exists at exactly one
place in the DOM, so each place needs its own. There are two ways to say that.

**Pass both components** when you have them and they are cheap. They must be different instances —
the same one in both places does not draw twice, it moves out of the row:

```csharp
DropdownItem(
    HStack().AlignItemsCenter().Gap(8.px()).Children(          // the row
        Avatar(initials: "AP").Size(AvatarSize.Small),
        VStack().Children(TextBlock("Ana Pereira").Small(), TextBlock("ana@example.com").Tiny().Secondary())),
    HStack().AlignItemsCenter().Gap(4.px()).Children(          // the box
        Avatar(initials: "AP").Size(AvatarSize.XSmall),
        TextBlock("Ana").Small()))
   .SetKey("ana@example.com");
```

**Pass a recipe** to have one description serve both, and to build the box's only if the option is
ever selected — the better choice for expensive content, for long lists, and whenever the row and
the box should look the same:

```csharp
DropdownItem(() => BadgeRow(status));                             // one recipe, used for both
DropdownItem(() => FullRow(id), () => ShortChip(id));             // a recipe for each
```

Give the box its own, shorter form whenever the row is taller or wider than one line — a two-line
block, a chart. Content taller than the row is the one thing the box cannot lay out well.

Two consequences of the box having its own instance, both worth knowing:


- **It is live.** It mounts, so a `Defer` in it loads — at any depth, not just at the top — it
  animates, and it reacts. (This is why it is a factory: a *copy* of the row would be inert.)
- **Its state is its own.** Put something interactive in an option and the list's and the box's will
  not share state — give the box a read-only short form if that matters.
- **A single recipe runs twice**, so whatever it does happens twice. Share expensive work rather than
  repeating it:

```csharp
var load = LoadOnceAsync(id);   // started once, outside the factory

DropdownItem(() => Defer(async () => Render(await load),
                         loadMessage: Skeleton().Animated().W(120).H(16)))
   .SetKey(id)
   .Selected();                 // fills itself in without the list ever being opened
```

To take over the box entirely instead — a count, a pile of avatars — use
`.WithCustomSelectionRender(items => ...)`. That replaces the per-item elements altogether,
separators included, so you own the whole presentation.

## Related

- ChoiceGroup, Toggle — alternative single-choice inputs
- Full docs & API: `/tesserae/components/dropdown`
