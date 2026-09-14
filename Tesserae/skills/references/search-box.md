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
- `.Busy(bool = true)` / `.IsBusy` — while the box is waiting on the search it asked for, a spinner
  stands where the clear button does. Set it when the query goes out and clear it when it answers,
  **including when it fails** — a spinner that never stops is worse than none. The box stays editable
  while busy.
- `.OnCancel((sender, value) => ...)` — what to do when the user calls off a running search, and by
  registering it you say one *can* be called off: a pointer on a busy box then swaps the spinner for a
  cancel button, same place and size. Pressing it raises this **and nothing else** — the box is not
  emptied and no search is raised, so a handler that wants either does it itself (`sender.Clear()`
  empties the box and raises `OnSearch` with the empty query). Without a handler there is no cancel
  button and a busy box keeps its clear button.
- `.Failed(int millisecondsVisible = 5000)` / `.ClearFailure()` / `.IsFailed` — says the search did not
  answer: the box is outlined in the danger colour with a warning glyph in the spinner's place, and takes
  itself down after the given time (pass `0` to leave it up until `.ClearFailure()`). Nothing else takes
  it down — it describes one search, so call `.ClearFailure()` where you start the next one. Still say
  *what* went wrong where the results would have been; the box only says that something did.
  This is not `.IsInvalid`, which is for a query the user has to fix and stays until they do.
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

CancellationTokenSource cts = null;

var search = SearchBox("Search people")
    .SearchAsYouType()
    .OnSearch((sender, value) => RunSearchAsync(sender, value).FireAndForget())
    .OnCancel((sender, value) =>
    {
        // Only what this app means by cancelling - the box raised the event and did nothing else.
        cts?.Cancel();
        sender.Busy(false);
    });

async Task RunSearchAsync(SearchBox box, string query)
{
    cts?.Cancel();
    cts = new CancellationTokenSource();

    box.ClearFailure();
    box.Busy();

    try
    {
        var found = await API.SearchAsync(query, cts.Token);
        results.Children(found.Select(Row).ToArray());
    }
    catch (Exception)
    {
        box.Failed();
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
