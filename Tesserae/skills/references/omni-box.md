---
name: omni-box
description: A search/chat input that parses boolean operators (AND, OR, NOT, parentheses, quotes), supports inline filter chips, autocomplete suggestions and a chat mode with model selection. Use when building a unified search-and-chat bar in a Tesserae (C#/Transpose) app.
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
- `ChatHeader` (`IComponent`) — rendered inside the box above the chat input: what the message is being written against, e.g. a `ContextBar` of the attached documents. Swap it later with `.SetChatHeader(component)`, or pass `null` to empty the slot — it takes up no space while empty.
- `GeneratingText` — label shown in the footer while generating (default `"Generating"`); the live elapsed time is appended, e.g. `"Generating, 1m 25s"`.

OmniBox:
- `.OnSearch((sender, SearchQuery) => ...)` — fires on search; `query.Tokens` hold the parsed tokens.
- `.OnChat((sender, ChatMessage) => ...)`, `.OnStop(...)`, `.OnModelChanged(...)`.
- `.IsGenerating` (bool) — toggles the footer spinner + elapsed-time indicator and swaps the send button for a stop button. `.GeneratingText` (string) — read/write the indicator label; setting it updates the footer live.
- `.SearchText` / `.ChatText` / `.SetSearchText(string)` — read/write input text.
- `.SetChatHeader(IComponent)` — replace (or clear, with `null`) whatever sits above the chat input.
- `.RegisterSnap(SnapHandler)` / `.RegisterFilterSnap(FilterSnapHandler)` — turn recognized input into inline filter chips (search modes only).
- `.WithHistory(Func<Task<SearchQuery[]>>)` — enable the history button.
- `.EnableChatMentions(ChatMention)` — turns typing `@` at a word boundary in the chat input into an
  "@mention" style picker (chat/search-and-chat modes only). `ChatMention` is a set of UI-agnostic
  callbacks (`OnShow(x, y)`, `OnQueryChanged(text)`, `OnMove(direction)`, `OnCommit()`, `OnHide()`,
  `IsOpen()`) — wire them up to any anchored popup, e.g. a `ToolAgentSelector`'s
  `ShowInlineAt`/`Filter`/`MoveHighlight`/`ActivateHighlighted`/`Hide`/`IsVisible` (see
  `tool-agent-selector.md`). Arrow Up/Down, Enter/Tab and Escape are forwarded to the callbacks
  while the picker is open; a `true` return from `OnCommit` removes the typed `@mention` text.
- `.Focus()`.
- `.CaretClientX()` — the viewport x of the text caret in whichever input is focused, clamped to that input's bounds, or `double.NaN` when there is nothing to measure. Used to point something at where the user is typing, e.g. the `PixelAvatar` companion walking over to the caret.
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

- Perching an animated pixel-art cat on top of the box — `pixel-avatar.md`

- TextBox — `/tesserae/components/text-box`
- SearchBox — `/tesserae/components/search-box`
- ToolAgentSelector — the tool/agent picker `EnableChatMentions` is commonly wired to — `tool-agent-selector.md`
- ContextBar — bubbles naming the context attached to the chat, mounted in the box's `ChatHeader` slot — `context-bar.md`
- Full docs & API: `/tesserae/components/omni-box`
