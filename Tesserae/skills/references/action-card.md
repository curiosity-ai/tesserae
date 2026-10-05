---
name: action-card
description: A card offering actions about one object (a company, a contract, a dataset) - the object's identity on the left, ToolCall-style action rows on the right, each carrying typed data for the handler - with loading, overflow and error states, a compact one-line form, and a group for several objects. Use for next steps or suggested follow-ups beside an object in a Tesserae (C#/Transpose) app.
---

# ActionCard / ActionCardGroup

`ActionCard<TData>` offers actions about one object - what to do next with it, or what to ask about it
(under an assistant answer, say). It is split vertically: on the left the object's identity (an icon
tile, a label, a second line, an optional detail line and key/value facts — the same parts as a
`ContextCard`), on the right the actions, each a row drawn like a `ToolCall` header with a small icon
saying what kind of action it is (a search, a trend, the people involved, a document). Clicking one — or
pressing Enter or Space on it, since every row is a real button — calls `OnAction` with the item, whose
`Data` is a `TData` the app chose when adding it, so the handler needs no cast or lookup.

The card fills the width it is given. Below about 520px it stacks itself — the identity becomes a
header strip, the actions wrap onto several lines — by a container query on its own width, so a
phone-width chat or a side panel needs nothing configured. Being a size container, its width comes from
its parent, never from its content — it is `width: 100%` — so in a horizontal `Stack` give it an
explicit `.W(...)`.

## Create

`UI.ActionCard<TData>(string label, UIcons icon = UIcons.Cube, UIconsWeight weight = UIconsWeight.Regular)`
`UI.ActionCard<TData>(string label, IComponent iconOrImage)`
`UI.ActionCardGroup<TData>(params ActionCard<TData>[] cards)` — `TData` is inferred from the cards.
Bring factories into scope with `using static Tesserae.UI;`.

## Key configuration

Identity (left column):

- `.SetLabel(string)` — the object's name. It wraps rather than being cut.
- `.SetSubLabel(string)` — the line below ("Company · ACC-20931"); null hides it.
  `.MonospaceSubLabel()` renders it in the monospace font, for an id or a path.
- `.SetDetail(string)` — a quiet line at the foot of the column ("Hamburg · Key account").
- `.AddFact(string key, string value, bool monospace = false)` / `.AddFact(string key, IComponent value)` /
  `.ClearFacts()` — key/value pairs for an object whose actions read better with a few of its numbers
  beside them; the `IComponent` overload takes a tag, a link or an icon with text as the value.
- `.SetIcon(UIcons, UIconsWeight)` / `.SetIcon(IComponent)` / `.SetImage(url)` — the tile.
- `.IconBackground(color)` / `.IconForeground(color)` / `.IconTint(color, percent = 14)` /
  `.NoIconBackground()` — tile colours, as on `ContextCard`. Defaults to the theme's primary colours.

Actions (right column):

- `.SetTitle(string)` — the small heading above them ("Ask about this company"); null hides it.
- `.AddAction(string text, UIcons icon = UIcons.Bolt, UIconsWeight weight = Regular, TData data = default)`
  — one row. `data` is typed by the card's `TData` (the prompt to send when it differs from the text
  shown, a query, an id) and comes back as `item.Data`, no cast.
- `.AddActions(params string[])` — several rows with the default icon.
- `.RemoveAction(ActionCard<TData>.Item)` / `.ClearActions()`.
- `.OnAction(Action<ActionCard<TData>, ActionCard<TData>.Item>)` / `.OnAction(Action<string>)` — called
  with the item (or its text) when an action is activated. Handlers accumulate.
- `.MaxVisible(int)` — show only the first N with a "Show N more" button; `.ShowAll()` opens the rest.
- `.SetTexts(moreFormat, retryText)` — "Show {0} more" / "Retry", for localisation.
- `Actions` (each `Item` has `Text`, `Icon`, `Data`), `Label`, `SubLabel`, `IsLoading`, `Tag`.

States and layout:

- `.Loading(bool = true, int placeholders = 3)` — placeholder rows while the actions are generated,
  with the identity already drawn. The shimmer stops under reduced motion.
- `.SetError(string message, Action onRetry = null)` / `.ClearError()` — a message in place of the
  actions, with a Retry button when a handler is given.
- `.Stacked(bool = true)` — force the stacked layout at any width.
- `.Compact(bool = true)` — one wrapping line: the object as a chip, then the actions as pills (their
  icons hidden). For a transcript where a full card under every answer would be too much.

`ActionCardGroup` draws several cards as one bordered card, a row per object, for an answer that
mentions more than one thing. `.Add(card)` / `.Remove(card)` / `.Clear()` / `Cards`;
`.OnAction(...)` on the group reaches every card, including ones added later. Each card keeps its own
actions, states and handlers.

## Example

```csharp
using static Tesserae.UI;

var followUps = ActionCard<string>("Acme Logistics GmbH", UIcons.Building)
    .SetSubLabel("Company · ACC-20931").MonospaceSubLabel()
    .SetDetail("Hamburg · Key account")
    .SetTitle("Ask about this company")
    .AddAction("Which open support tickets mention Acme Logistics?", UIcons.Inbox, data: "tickets:open account:ACC-20931")
    .AddAction("How has their order volume changed this quarter?",  UIcons.ChartLineUp)
    .AddAction("Who are our main contacts there?",                  UIcons.Users)
    .AddAction("Summarize the latest framework contract with Acme", UIcons.FileSignature)
    .MaxVisible(3)
    .OnAction((card, item) => chat.Send(item.Data ?? item.Text));

// Generated asynchronously: draw the identity now, the actions when they arrive.
var pending = ActionCard<string>("Framework Agreement 2026", UIcons.FileSignature)
    .IconBackground("#f59e0b")
    .SetSubLabel("Contract")
    .AddFact("Renews", "2027-03-31", monospace: true)
    .Loading();

pending.Loading(false).AddAction("What are the termination clauses?", UIcons.Search);
pending.SetError("Couldn't suggest actions.", () => Regenerate(pending));

// Several objects mentioned in one answer.
var group = ActionCardGroup(
    ActionCard<string>("Acme Logistics", UIcons.Building).IconTint("#248efa").SetSubLabel("Company")
        .AddAction("Who are our main contacts there?", UIcons.Users),
    ActionCard<string>("shipments", UIcons.Database).IconTint("#16a34a").SetSubLabel("Dataset · 42,109 rows")
        .AddAction("Which routes had the most delays last month?", UIcons.ChartHistogram)
).OnAction((card, item) => chat.Send(item.Text));
```

## Related

- ContextCard — the identity half is drawn from the same parts — `context-card.md`
- ToolCall / ToolsUsed — the rows match a tool call's header — `tool-call.md`
- Chat (ChatArea / ChatMessage) — `chat.md`
- Full docs & API: `/tesserae/components/action-card`
