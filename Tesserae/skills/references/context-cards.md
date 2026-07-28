---
name: context-cards
description: A group of ContextCards behind one summary pill that expands into a list of rows, or a compact wrapping row of pills that collapses everything past the first few behind a "+N more" pill. Use to show what a chat, a search or an answer is scoped to in a Tesserae (C#/Transpose) app.
---

# ContextCards

`ContextCards` holds a set of [`ContextCard`](context-card.md)s and shows them as one thing. It has two
shapes:

- **Grouped** (default) — a summary pill ("Added 5 items to context") that expands into a bordered list
  of rows and collapses back. Same pill shape and chevron as `ToolsUsed`, so a transcript reads as one
  family of disclosures. In the list, cards render as full-width rows with a divider between each and
  the badge and the ✕ in the row rather than hovering over a corner.
- **Compact** (`.Compact()`) — no header, just a wrapping row of pills. The first `MaxVisible` (5 by
  default) are shown; the rest collapse behind a dashed "+N more" pill.

An empty group renders nothing and takes up no space, so it can sit permanently in a layout.

## Create

`UI.ContextCards(params ContextCard[] cards)` / `UI.ContextCards(IEnumerable<ContextCard>)`.
Bring factories into scope with `using static Tesserae.UI;`.

## Key configuration

- `.Add(ContextCard)` / `.AddRange(IEnumerable<ContextCard>)` / `.Remove(card)` / `.Clear()`, plus
  `.Cards` (read-only list) and `.Count`. Adding a card wires its remove button to the group, so its ✕
  detaches it — a handler the caller registered with `ContextCard.OnRemove` still runs.
- `.SetSummary(string)` — the header text. Without one the group summarises itself as
  "Added N items to context", updated as cards come and go.
- `.SetIcon(UIcons)` / `.IconBackground(string)` / `.IconForeground(string)` — the header's icon tile
  (a stack of layers, tinted with the primary color, by default).
- `.Expand()` / `.Collapse()` / `.Toggle()` / `.Expanded(bool = true)`, `.IsExpanded`,
  `.OnToggle(Action<ContextCards>)`. Collapsed is the default.
- `.Compact(bool = true)` — switch to the pill row.
- `.MaxVisible(int)` — how many pills the compact row shows before "+N more" takes over (5).
- `.MoreText(string moreFormat, string lessText = null)` — the wording of that pill; `{0}` is how many
  cards it hides (`"+{0} more"` / `"Show less"`).
- `.OnShowAll(Action)` — hand the pill over to a host that opens the full list its own way (a panel, a
  search scoped to the context) instead of revealing the hidden cards in place.

The header is keyboard reachable and toggles on Enter/Space; it opens on a *tap* rather than a raw
click, for the same reason `ToolsUsed` does — in a live transcript the content around it re-renders and
scrolls under the pointer, so the browser can drop the click between press and release.

## Example

```csharp
using static Tesserae.UI;

// Grouped: one pill that opens into a list of rows.
var sources = ContextCards(
    ContextCard("Q3 revenue model", UIcons.Table).SetSubLabel("finance/q3-model.xlsx · 4 sheets").MonospaceSubLabel().SetBadge("SharePoint").IconTint("#16a34a"),
    ContextCard("Incident 482 postmortem", UIcons.FileInvoice).SetSubLabel("docs/postmortem-482.md").MonospaceSubLabel().SetBadge("Wiki").IconTint("#3b82f6"),
    ContextCard("events.request_log", UIcons.Database).SetSubLabel("warehouse · 2.1M rows").MonospaceSubLabel().SetBadge("Snowflake").IconTint("#10b981"))
   .SetSummary("3 sources for this answer")
   .OnToggle(g => Console.WriteLine(g.IsExpanded));

// Compact: a dense row that ends in "+N more".
var attached = ContextCards().Compact().MaxVisible(3);

attached.Add(ContextCard("Migration plan.docx", UIcons.FileWord).IconTint("#3b82f6").MaxLabelWidth(120.px()));
```

Inside a chat composer, a compact group is what usually goes in `OmniBox`'s slot above the input:

```csharp
var omni = OmniBox(new OmniBox.Config(OmniBox.Mode.Chat) { ChatHeader = attached });

omni.SetChatHeader(attached);   // or hand it over later
```

Both the slot and an empty group collapse to nothing, so a chat with no context looks untouched. For
individual cards *below* the input instead, see `OmniBox.WithContextToAdd` in `omni-box.md`.

## Related

- ContextCard — the card this groups, and every option on it — `context-card.md`
- ToolCall / ToolsUsed — the disclosures this matches — `tool-call.md`
- OmniBox — `ChatHeader` / `WithContextToAdd` — `omni-box.md`
- Chat — `chat.md`
- Full docs & API: `/tesserae/components/context-cards`
