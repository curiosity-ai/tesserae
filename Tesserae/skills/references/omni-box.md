---
name: omni-box
description: A search/chat input that parses boolean operators (AND, OR, NOT, parentheses, quotes), supports inline filter chips, autocomplete suggestions and a chat mode with model selection. Use when building a unified search-and-chat bar in a Tesserae (C#/h5) app.
---

# OmniBox

A configurable input that switches between a search interaction and a chat
interaction. In search mode it parses and visually highlights logical operators
(`AND`/`&&`, `OR`/`||`, `NOT`/`!`/`-`, parentheses, quotes) and supports inline
filter chips plus async suggestions. Constructed from a `Config` object.

## Create

`OmniBox(OmniBox.Config config)` — pass a `new OmniBox.Config(Mode mode, Mode? initialMode = null)`
where `Mode` is `Search`, `Chat` or `SearchAndChat`. Bring the factory into scope
with `using static Tesserae.UI;`. (`UI.OmniBox(config)` is also available.)

## Key configuration

Config (set via object initializer):
- `PlaceholderSearch` / `PlaceholderChat`, `ExpandOnFocus`, `TokenIgnoreCase`.
- `SuggestionsFetcher = async input => OmniBoxSuggestionItem[]` — autocomplete source.
- `IconSearch` / `IconChat` / `IconStop`, `SearchFooter` / `ChatFooter` (`FooterItems`).

OmniBox:
- `.OnSearch((sender, SearchQuery) => ...)` — fires on search; `query.Tokens` hold the parsed tokens.
- `.OnChat((sender, ChatMessage) => ...)`, `.OnStop(...)`, `.OnModelChanged(...)`.
- `.SearchText` / `.ChatText` / `.SetSearchText(string)` — read/write input text.
- `.RegisterSnap(SnapHandler)` / `.RegisterFilterSnap(FilterSnapHandler)` — turn recognized input into inline filter chips (search modes only).
- `.WithHistory(Func<Task<SearchQuery[]>>)` — enable the history button.
- `.Focus()`.
- `OmniBox.ParseQuery(string, bool tokenIgnoreCase = false)` — static parser returning a `SearchQuery`.

## Example

```csharp
using static Tesserae.UI;

var config = new OmniBox.Config(OmniBox.Mode.SearchAndChat)
{
    PlaceholderSearch = "Search…",
    PlaceholderChat   = "Ask anything…",
    SuggestionsFetcher = async input => new[]
    {
        new OmniBox.OmniBoxSuggestionItem("recent: invoices"),
        new OmniBox.OmniBoxSuggestionItem("recent: contracts"),
    }
};

var omni = new OmniBox(config)
    .OnSearch((s, q) => Console.WriteLine($"Search: {q.Tokens.Count} tokens"))
    .OnChat((s, m) => Console.WriteLine("Chat sent"));
```

## Related

- TextBox — `/tesserae/components/text-box`
- SearchBox — `/tesserae/components/search-box`
- Full docs & API: `/tesserae/components/omni-box`
