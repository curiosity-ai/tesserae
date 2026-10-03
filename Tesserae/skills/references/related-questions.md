---
name: related-questions
description: A card embedded in a chat transcript offering follow-up questions about one object (a company, a contract, a dataset) - the object's identity on the left, ToolCall-style question rows on the right - with loading, asked, overflow and error states, a compact one-line form, and a group for several objects. Use for suggested follow-ups under an assistant answer in a Tesserae (C#/Transpose) app.
---

# RelatedQuestions / RelatedQuestionsGroup

`RelatedQuestions` sits in a chat transcript, under the answer that mentioned an object, and offers
questions to ask next about it. It is split vertically: on the left the object's identity (an icon
tile, a label, a second line, an optional detail line and key/value facts — the same parts as a
`ContextCard`), on the right the questions, each a row drawn like a `ToolCall` header with a small icon
saying what kind of question it is (a search, a trend, the people involved, a document). Clicking a
question — or pressing Enter or Space on it, since every row is a real button — calls `OnAsk`, where
the app sends it as the next message, and marks it as asked.

The card fills the width it is given. Below about 520px it stacks itself — the identity becomes a
header strip, the questions wrap onto several lines — by a container query on its own width, so a
phone-width chat or a side panel needs nothing configured. Being a size container, its width comes from
its parent, never from its content — it is `width: 100%` — so in a horizontal `Stack` give it an
explicit `.W(...)`.

## Create

`UI.RelatedQuestions(string label, UIcons icon = UIcons.Cube, UIconsWeight weight = UIconsWeight.Regular)`
`UI.RelatedQuestions(string label, IComponent iconOrImage)`
`UI.RelatedQuestionsGroup(params RelatedQuestions[] cards)`
Bring factories into scope with `using static Tesserae.UI;`.

## Key configuration

Identity (left column):

- `.SetLabel(string)` — the object's name. It wraps rather than being cut.
- `.SetSubLabel(string)` — the line below ("Company · ACC-20931"); null hides it.
  `.MonospaceSubLabel()` renders it in the monospace font, for an id or a path.
- `.SetDetail(string)` — a quiet line at the foot of the column ("Hamburg · Key account").
- `.AddFact(string key, string value, bool monospace = false)` / `.ClearFacts()` — key/value pairs for
  an object whose questions read better with a few of its numbers beside them.
- `.SetIcon(UIcons, UIconsWeight)` / `.SetIcon(IComponent)` / `.SetImage(url)` — the tile.
- `.IconBackground(color)` / `.IconForeground(color)` / `.IconTint(color, percent = 14)` /
  `.NoIconBackground()` — tile colours, as on `ContextCard`. Defaults to the theme's primary colours.

Questions (right column):

- `.SetTitle(string)` — the small heading above them ("Ask about this company"); null hides it.
- `.AddQuestion(string text, UIcons icon = UIcons.CommentQuestion, UIconsWeight weight = Regular, object tag = null)`
  — one row. `tag` is any payload (the prompt to send when it differs from the text shown, a query, an id).
- `.AddQuestions(params string[])` — several rows with the default icon.
- `.RemoveQuestion(RelatedQuestions.Question)` / `.ClearQuestions()`.
- `.OnAsk(Action<RelatedQuestions, RelatedQuestions.Question>)` / `.OnAsk(Action<string>)` — called when
  a question is asked. Handlers accumulate.
- `.MarkAskedOnClick(bool = true)` — on by default; turn off to decide yourself (once the message was
  actually sent) and call `.MarkAsked(question)` / `.MarkAsked(text)`. An asked row shows a check and an
  "Asked" tag, and can still be clicked again. `.MarkAsked(q, false)` clears it.
- `.MaxVisible(int)` — show only the first N with a "Show N more" button; `.ShowAll()` opens the rest.
- `.SetTexts(moreFormat, askedText, retryText)` — "Show {0} more" / "Asked" / "Retry", for localisation.
- `Questions` (each `Question` has `Text`, `Icon`, `IsAsked`, `Tag`), `Label`, `SubLabel`, `IsLoading`, `Tag`.

States and layout:

- `.Loading(bool = true, int placeholders = 3)` — placeholder rows while the questions are generated,
  with the identity already drawn. The shimmer stops under reduced motion.
- `.SetError(string message, Action onRetry = null)` / `.ClearError()` — a message in place of the
  questions, with a Retry button when a handler is given.
- `.Stacked(bool = true)` — force the stacked layout at any width.
- `.Compact(bool = true)` — one wrapping line: the object as a chip, then the questions as pills (their
  icons hidden). For a transcript where a full card under every answer would be too much.

`RelatedQuestionsGroup` draws several cards as one bordered card, a row per object, for an answer that
mentions more than one thing. `.Add(card)` / `.Remove(card)` / `.Clear()` / `Cards`;
`.OnAsk(...)` on the group reaches every card, including ones added later. Each card keeps its own
questions, states and handlers.

## Example

```csharp
using static Tesserae.UI;

var followUps = RelatedQuestions("Acme Logistics GmbH", UIcons.Building)
    .SetSubLabel("Company · ACC-20931").MonospaceSubLabel()
    .SetDetail("Hamburg · Key account")
    .SetTitle("Ask about this company")
    .AddQuestion("Which open support tickets mention Acme Logistics?", UIcons.Inbox)
    .AddQuestion("How has their order volume changed this quarter?",  UIcons.ChartLineUp)
    .AddQuestion("Who are our main contacts there?",                  UIcons.Users)
    .AddQuestion("Summarize the latest framework contract with Acme", UIcons.FileSignature)
    .MaxVisible(3)
    .OnAsk(text => chat.Send(text));

// Generated asynchronously: draw the identity now, the questions when they arrive.
var pending = RelatedQuestions("Framework Agreement 2026", UIcons.FileSignature)
    .IconBackground("#f59e0b")
    .SetSubLabel("Contract")
    .AddFact("Renews", "2027-03-31", monospace: true)
    .Loading();

pending.Loading(false).AddQuestion("What are the termination clauses?", UIcons.Search);
pending.SetError("Couldn't suggest questions.", () => Regenerate(pending));

// Several objects mentioned in one answer.
var group = RelatedQuestionsGroup(
    RelatedQuestions("Acme Logistics", UIcons.Building).IconTint("#248efa").SetSubLabel("Company")
        .AddQuestion("Who are our main contacts there?", UIcons.Users),
    RelatedQuestions("shipments", UIcons.Database).IconTint("#16a34a").SetSubLabel("Dataset · 42,109 rows")
        .AddQuestion("Which routes had the most delays last month?", UIcons.ChartHistogram)
).OnAsk((card, q) => chat.Send(q.Text));
```

## Related

- ContextCard — the identity half is drawn from the same parts — `context-card.md`
- ToolCall / ToolsUsed — the rows match a tool call's header — `tool-call.md`
- Chat (ChatArea / ChatMessage) — `chat.md`
- Full docs & API: `/tesserae/components/related-questions`
