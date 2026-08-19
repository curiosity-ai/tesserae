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
- `IconModeToggleChat` / `IconModeToggleSearch`, `TooltipModeToggleChat` / `TooltipModeToggleSearch`,
  `TextModeToggleChat` / `TextModeToggleSearch` — the `SearchAndChat` mode selector in the footer (an
  `IconToggle`, see `icon-toggle.md`). It is icon-only unless a text is set, and it takes the box's
  roundness, so `.Rounded(BorderRadius.Full)` gives a pill-shaped selector.
- `ChatHeader` (`IComponent`) — rendered inside the box above the chat input: what the message is being written against, e.g. a compact `ContextCards` group of the attached documents. Swap it later with `.SetChatHeader(component)`, or pass `null` to empty the slot — it takes up no space while empty. For individual cards *below* the input, use `.WithContextToAdd(...)`.
- `GeneratingText` — label shown in the footer while generating (default `"Generating"`); the live elapsed time is appended, e.g. `"Generating, 1m 25s"`.
- `AllowSendWhileGenerating` (default `false`) — when set, typing a message while `IsGenerating` is true sends it (`OnChat` fires) instead of the trigger stopping the reply, so the host can queue it for the turn in flight. The trigger still shows the stop icon while the input is empty and turns back into the send icon as soon as there is text.

OmniBox:
- `.OnSearch((sender, SearchQuery) => ...)` — fires on search; `query.Tokens` hold the parsed tokens.
- `.OnChat((sender, ChatMessage) => ...)`, `.OnStop(...)`, `.OnModelChanged(...)`.
- `.IsGenerating` (bool) — toggles the footer spinner + elapsed-time indicator and swaps the send button for a stop button. `.GeneratingText` (string) — read/write the indicator label; setting it updates the footer live. `.AllowSendWhileGenerating` (bool) — read/write the config flag above.
- `.SearchText` / `.ChatText` / `.SetSearchText(string)` — read/write input text.
- `.ActiveMode` (`Mode`) — which half of a `SearchAndChat` box is showing. Setting it switches the box
  and moves the box's own toggle with it. `.ActiveModeObservable` follows it, and `.NoModeToggle()`
  takes the toggle out of the footer for a host that puts a mode control somewhere of its own (a page
  header) — the box still switches through `.ActiveMode`.
- `.SetChatHeader(IComponent)` — replace (or clear, with `null`) whatever sits above the chat input.
- `.RegisterSnap(SnapHandler)` / `.RegisterFilterSnap(FilterSnapHandler)` — turn recognized input into inline filter chips (search modes only).
- `.AddFilterSnap(FilterSnapHandler, value, trigger = null)` / `.ClearSnaps()` — add or drop active filter chips in code, to open the box already filtered.
- `.WithHistory(Func<Task<SearchQuery[]>>)` — enable the history button.
- `.WithHelp(bool showSyntax = false)` — a `?` button opening a panel that lists the registered filter
  snaps and snaps (with their example values); `showSyntax: true` also documents `AND` / `OR` / `NOT`,
  grouping and quoting (search modes only).
- `.InlineFilterChips` — observable list of `InlineFilterChip`s rendered at the head of the search
  input, for filters the app owns rather than ones the user typed. A chip takes a text (with optional
  background/foreground colors and an `onClick`) or an arbitrary `IComponent`.
- `.SetSearchRightText(string)` — a label at the far end of the search input, e.g. a result count.
- `.SetSearching(bool)` / `.IsSearching` — says a search is running: the magnifier gives its place to a
  spinner, in the same spot, so the box keeps its shape while it runs. The two cross over only after
  ~140ms, so a fast or cached answer never makes the button blink. (`CommandPalette` drives this for you
  while its `OnSearch` is in flight — `command-palette.md`.)
- `.Rounded(BorderRadius radius = BorderRadius.Full)` — rounds the box, and everything meeting its
  outline follows: the search container, the buttons at its ends and the "Ask AI" button. `Full` makes
  the single-row search box a pill — dropping the vertical dividers between its buttons and giving the
  row a taller, roomier 48px (still overridable with `.Height(...)` / `.H(...)`); on the multi-row chat
  layouts, where a stadium shape would curve through the input, it settles for a generously rounded
  rectangle and pulls the footer clear of the corners.
- `.WithAskAI(string text = "Ask AI", UIcons icon = UIcons.Beacon, Action<OmniBox> onClick = null)` — a
  primary-styled button at the end of the search input (search modes only), following the box's
  roundness. The handler gets the OmniBox, so it can read `.SearchText`. In `SearchAndChat` the button
  sits at the end of the footer and hides itself while the box is in chat mode. Calling it again updates
  the button that is there; a null or empty `text` hides it.
- `.SetModels(params ModelOption[])` / `.LockModel(ModelOption)` / `.SetThinkingEffort(ThinkingEffort)`
  — the chat footer's model selector; a locked model shows with a lock and stops opening the popover.
- `.SetKeyboardShortcut(params string[] keys)` — e.g. `("Ctrl", "K")`: a document-level shortcut that
  focuses the box, shown as a hint at the end of the search input (hidden while the input has focus,
  shown again on blur).
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
- `.CurrentSearchQuery` — what the box says right now, parsed: the same `SearchQuery` pressing Enter would
  raise, chips included. For a host that answers as the query is typed rather than on Enter (a
  `CommandPalette`, a search-as-you-type page).

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

A pill-shaped search box with a result count and an "Ask AI" action:

```csharp
var omni = new OmniBox(new OmniBox.Config(OmniBox.Mode.Search) { PlaceholderSearch = "Search…" })
    .Rounded()                                   // BorderRadius.Full — a pill
    .SetSearchRightText("18 results · 0.21s")
    .WithAskAI("Ask AI", UIcons.Beacon, box => AskAI(box.SearchText))
    .OnSearch((s, q) => Search(q.RawQuery));
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
- ContextCards — a compact row of pills naming the attached context, mounted in the box's `ChatHeader` slot — `context-cards.md`
- ContextCard — the cards `WithContextToAdd` renders below the input — `context-card.md`
- OmniResult — the search-result rows a query typed here is answered with — `omni-result.md`
- Full docs & API: `/tesserae/components/omni-box`
