---
name: search-box
description: A single-line search input with a leading magnifier, clear button, and search-on-Enter or debounced-as-you-type semantics. Use when adding a search field that fires a query callback in a Tesserae (C#/Transpose) app.
---

# SearchBox

A search input that fires its `OnSearch` callback on Enter (default) or after every keystroke when `SearchAsYouType()` is set. Supports an optional global keyboard shortcut chip.

## Create

`UI.SearchBox(string placeholder = "")` — returns a `SearchBox`. Also `new SearchBox(placeholder)`.
Bring factories into scope with `using static Tesserae.UI;`.

## Key configuration

- `.OnSearch((sender, value) => ...)` — fired with the query text.
- `.SearchAsYouType()` — fire (debounced) on each keystroke rather than only on submit.
- `.Underlined()` / `.NotUnderlined()` — underline style.
- `.SetText(string)` / `.Text`, `.SetPlaceholder(string)` / `.Placeholder`.
- `.SetIcon(UIcons)` / `.NoIcon()` — leading glyph.
- `.SetKeyboardShortcut(params string[] keys)` — global focus shortcut + visible chip, e.g. `SetKeyboardShortcut("Ctrl", "K")` (renders ⌘K on macOS).
- `.OnShortcut(Action)` — what that shortcut does *instead* of focusing the box, for a box that
  stands for a search happening somewhere else (a `CommandPalette`, a search page). Pressing the key
  works the same whether or not the box already holds the caret, which focusing does not.
- `.Clear()` — empties the box, focuses it and fires `OnSearch` with an empty query. This is what the
  trailing clear button does; the button itself appears whenever the box has text and needs no setup.
- `.Busy(bool = true)` / `.IsBusy` — shows a spinner beside the clear button while the box is waiting
  on the search it asked for. Set it when the query goes out and clear it when it answers, **including
  when it fails** — a spinner that never stops is worse than none. The box stays editable while busy,
  so a slow search can be retyped or cleared; say what went wrong where the results would have been,
  not in the box.
- `.Focus()`, `.Disabled(bool = true)`, `.Height(UnitSize)` / `.H(int)`.

## Example

```csharp
using static Tesserae.UI;

var search = SearchBox("Search")
    .Underlined()
    .SearchAsYouType()
    .SetKeyboardShortcut("Ctrl", "K")
    .OnSearch((sender, value) => console.log($"Searched: {value}"));
```

Waiting on a server, with the failure handled:

```csharp
var results = VStack();

var search = SearchBox("Search people")
    .SearchAsYouType()
    .OnSearch((sender, value) => RunSearchAsync(sender, value).FireAndForget());

async Task RunSearchAsync(SearchBox box, string query)
{
    box.Busy();

    try
    {
        var found = await API.SearchAsync(query);
        results.Children(found.Select(Row).ToArray());
    }
    catch (Exception)
    {
        results.Children(TextBlock("Could not search right now.").Secondary(),
                         Button("Try again").Link().OnClick(() => RunSearchAsync(box, query).FireAndForget()));
    }
    finally
    {
        box.Busy(false);
    }
}
```

## Related

- TextBox — `text-box.md`
- Full docs & API: `/tesserae/components/search-box`
