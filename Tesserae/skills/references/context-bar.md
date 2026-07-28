---
name: context-bar
description: A row of small bubbles naming the context something is scoped to - the files a chat can read, the records a search is limited to, the sources an answer cites - with an ellipsized name that keeps the file extension readable, an optional remove button, and overflow collapsed behind a "+N more" button. Use in a Tesserae (C#/Transpose) app to show attached context above a composer or under a reply.
---

# ContextBar

`ContextBar` is a wrapping row of small bubbles that say what something is scoped to. Each bubble
carries an icon, a name, and optionally a remove button. It is the indicator that survives closing
whatever panel the context was picked in: a chat that can read three documents keeps saying so above
its composer.

Two details it handles that a hand-rolled chip row usually gets wrong:

- **The extension stays readable.** The name is ellipsized at a narrow width (80px by default), but a
  trailing file extension is held outside that width, so a bubble reads `Quarterly repo….pdf` rather
  than `Quarterly repor…`. The truncation is measured once the bubble is in the DOM, so the ellipsis
  lands right where the name stops and the extension butts up against it.
- **Overflow costs nothing.** Only the first `MaxVisible` bubbles are put in the DOM. The rest collapse
  into a `+N more` button, so a host can hand over everything it has without paying to render it.

An empty bar renders nothing and takes up no space, so it can sit permanently in a layout.

## Create

`UI.ContextBar(params ContextBar.Item[] items)`
`UI.ContextBarItem(string name, UIcons icon = UIcons.File, bool keepExtensionVisible = true)`
`using static Tesserae.UI;`.

## Key configuration

On the bar:

- `.Items(params Item[])` — set the bubbles, replacing any it had.
- `.Add(Item)` / `.Remove(Item)` / `.Clear()` — incremental updates.
- `.MaxVisible(int)` — how many bubbles render before the rest collapse. `3` by default.
- `.OnShowAll(Action)` — what the `+N more` button does, typically opening the full context (a search
  restricted to it, a list, a panel). Without a handler the button still reports the count but is not
  clickable.
- `.MoreText(string format)` — the button's wording; `{0}` is the number of hidden bubbles.
  `"+{0} more"` by default.
- `Count`, `IsEmpty` — read state.

On a bubble (`ContextBar.Item`):

- `.OnClick(Action<Item>)` — makes the bubble interactive (keyboard included); typically opens what it
  names.
- `.OnRemove(Action<Item>, string tooltip = "Remove")` — adds the ✕, which stays quiet until its bubble
  is hovered (or it takes focus) and turns red on its own hover, so a row reads as names rather than as
  a row of delete buttons. Activating it never also fires `OnClick`. The handler decides what removal
  means — call `bar.Remove(item)` to drop it from the row.
- `.SetName(string)` / `.SetIcon(UIcons)` / `.IconColor(string)` — update a bubble in place, e.g. once
  an async lookup resolves its real label.
- `.MaxNameWidth(UnitSize)` — widen or narrow where the name is cut.
- `Name`, `Tag` — the current name, and an arbitrary payload (the record's id, say).
- Being an `IComponent`, `.Tooltip("…")` works on a bubble — worth attaching the untruncated name.

## Example

```csharp
using static Tesserae.UI;

var bar = ContextBar().MaxVisible(3);

foreach (var doc in chat.Documents)
{
    var bubble = ContextBarItem(doc.FileName, IconFor(doc)).Tooltip(doc.FileName);

    bubble.Tag = doc.Id;
    bubble.OnClick(_ => OpenPreview(doc));
    bubble.OnRemove(i =>
    {
        bar.Remove(i);
        DetachFromChat(doc.Id);
    });

    bar.Add(bubble);
}

// The host owns what "show all" means.
bar.OnShowAll(() => OpenContextPanel(chat.Documents));
```

Inside a chat composer, hand it to the `OmniBox`'s header slot so it sits above the text being typed,
within the box's own border:

```csharp
var box = OmniBox(new OmniBox.Config(OmniBox.Mode.Chat)
{
    PlaceholderChat = "Ask about the attached documents",
    ChatHeader      = bar
}).WS();

box.SetChatHeader(bar);   // or hand it over later
box.SetChatHeader(null);  // and take it away again
```

The slot collapses while it is empty, and an empty bar renders nothing, so a chat with no context looks
exactly as it would without either.

The same bubbles work as citations under a reply, since `ChatMessage.WithReferences` takes any
components:

```csharp
message.WithReferences(sources.Select(s => ContextBarItem(s.Title, UIcons.FilePdf).OnClick(_ => Open(s))));
```

## Related

- OmniBox (the chat composer it usually sits above) — `.omni-box.md`
- Chat / ChatMessage (`WithReferences`) — `.chat.md`
- ToolAgentSelector (picking tools & agents in the same composer) — `.tool-agent-selector.md`
- Badge (a static label, no removal or overflow) — `.badge.md`
- TagsInput (free-text tags the user types) — `.tags-input.md`
- Full docs & API: `/tesserae/components/context-bar`
