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
- `GeneratingText` — label shown in the footer while generating (default `"Generating"`); the live elapsed time is appended, e.g. `"Generating, 1m 25s"`.

OmniBox:
- `.OnSearch((sender, SearchQuery) => ...)` — fires on search; `query.Tokens` hold the parsed tokens.
- `.OnChat((sender, ChatMessage) => ...)`, `.OnStop(...)`, `.OnModelChanged(...)`.
- `.IsGenerating` (bool) — toggles the footer spinner + elapsed-time indicator and swaps the send button for a stop button. `.GeneratingText` (string) — read/write the indicator label; setting it updates the footer live.
- `.SearchText` / `.ChatText` / `.SetSearchText(string)` — read/write input text.
- `.RegisterSnap(SnapHandler)` / `.RegisterFilterSnap(FilterSnapHandler)` — turn recognized input into inline filter chips (search modes only).
- `.WithHistory(Func<Task<SearchQuery[]>>)` — enable the history button.
- `.WithHelp(bool showSyntax = false)` — a `?` button opening a panel that lists the registered filter
  snaps and snaps (with their example values); `showSyntax: true` also documents `AND` / `OR` / `NOT`,
  grouping and quoting (search modes only).
- `.InlineFilterChips` — observable list of `InlineFilterChip`s rendered at the head of the search
  input, for filters the app owns rather than ones the user typed. A chip takes a text (with optional
  background/foreground colors and an `onClick`) or an arbitrary `IComponent`.
- `.SetSearchRightText(string)` — a label at the far end of the search input, e.g. a result count.
- `.SetModels(params ModelOption[])` / `.LockModel(ModelOption)` / `.SetThinkingEffort(ThinkingEffort)`
  — the chat footer's model selector; a locked model shows with a lock and stops opening the popover.
- `.SetKeyboardShortcut(params string[] keys)` — e.g. `("Ctrl", "K")`: a document-level shortcut that
  focuses the box, shown as a hint at the end of the search input.
- `.Disabled(bool value = true)` — keeps the content but stops taking input.
- `.EnableChatMentions(ChatMention)` — turns typing `@` at a word boundary in the chat input into an
  "@mention" style picker (chat/search-and-chat modes only). `ChatMention` is a set of UI-agnostic
  callbacks (`OnShow(x, y)`, `OnQueryChanged(text)`, `OnMove(direction)`, `OnCommit()`, `OnHide()`,
  `IsOpen()`) — wire them up to any anchored popup, e.g. a `ToolAgentSelector`'s
  `ShowInlineAt`/`Filter`/`MoveHighlight`/`ActivateHighlighted`/`Hide`/`IsVisible` (see
  `tool-agent-selector.md`). Arrow Up/Down, Enter/Tab and Escape are forwarded to the callbacks
  while the picker is open; a `true` return from `OnCommit` removes the typed `@mention` text.
- `.WithContextToAdd(params ContextCard[])` — the context that will go with the next message, rendered
  as a wrapping row of `ContextCard`s **inside the box**, just below the input and above the footer
  (chat/search-and-chat modes only; it hides itself while the box is in search mode, and the row is
  invisible while empty). `.AddContext(card)` appends one, `.RemoveContext(card)` / `.ClearContext()`
  take them out, `.ContextToAdd` (read-only list) and `.HasContextToAdd` read the current state. Each
  card's (x) is wired to the row, so removing a card from the box needs no extra code — a handler the
  caller registered with `ContextCard.OnRemove` still runs, which is where the underlying context gets
  dropped. Cards survive sending: clear the row from `OnChat`. See `context-card.md`.
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

Context attached to the next message, shown inside the box below the input:

```csharp
var omni = new OmniBox(new OmniBox.Config(OmniBox.Mode.Chat))
    .WithContextToAdd(
        ContextCard("Kindersonnenschutzmittel-NEU.pdf", UIcons.FilePdf).SetSubLabel("PDF").IconBackground("#ef4444"))
    .OnChat((s, m) =>
    {
        Send(m.Text, s.ContextToAdd.Select(c => c.Label));   // read it before clearing
        s.ClearContext();                                    // the box keeps it until told otherwise
    });

// Later, e.g. from an attach button or a FileDropArea:
omni.AddContext(ContextCard("Q3-forecast.xlsx", UIcons.FileExcel).SetSubLabel("Spreadsheet").IconBackground("#16a34a"));
```

## Related

- Perching an animated pixel-art cat on top of the box — `pixel-avatar.md`

- TextBox — `/tesserae/components/text-box`
- SearchBox — `/tesserae/components/search-box`
- ToolAgentSelector — the tool/agent picker `EnableChatMentions` is commonly wired to — `tool-agent-selector.md`
- Full docs & API: `/tesserae/components/omni-box`
