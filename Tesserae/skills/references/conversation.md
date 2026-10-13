---
name: conversation
description: An issue-tracker style discussion thread - comment cards with an avatar, role badges, a Markdown body, image attachments and emoji reactions, one-line events on a vertical rail between them, and a Write/Preview reply composer. Use when building a GitHub-issues-like conversation, a ticket history or a review thread in a Tesserae (C#/Transpose) app.
---

# Conversation

A column of `ConversationComment` cards joined by a vertical rail, with `ConversationEvent`
rows for what happened in between (a label added, someone assigned, the thread closed) and an
optional `ConversationComposer` at the foot. Any other `IComponent` can be added as an item and
sits on the rail too. Put a `MetadataPanel` (`metadata-panel.md`) beside it for the
assignees / labels / milestone column.

## Create

- `Conversation()` — the thread. `.Add(item)`, `.Children(params IComponent[])`, `.Remove(item)`,
  `.Clear()`, `.Composer(IComponent)` (pass `null` to remove), `.Items`.
- `ConversationComment(string author, string markdown = null, string timestamp = null, Avatar avatar = null)`
- `ConversationEvent(UIcons icon, string actor, string text, string timestamp = null)`
- `ConversationComposer(Avatar avatar = null, string placeholder = "Add your comment here...", string submitText = "Comment")`

Bring factories into scope with `using static Tesserae.UI;`.

## ConversationComment

- `.Action(string)` — the verb after the author: `"commented"` (default), `"opened"`, ...
- `.Timestamp(string text, string tooltip = null)` — display text such as `"on May 18"`; the tooltip is the full date.
- `.Edited(bool = true, string tooltip = null)` — the "· edited" marker.
- `.Role(string)` / `.ClearRoles()` — outlined pill badges on the right: `"Author"`, `"Member"`, `"Contributor"`.
- `.Menu(params ContextMenu.Item[])` — a "…" button opening a context menu.
- `.Body(string markdown, MarkdownSanitization = Default)` / `.Body(IComponent)` — Markdown is rendered by a `MarkdownBlock`.
  Use `MarkdownSanitization.NoLinksOrEmbeddedContent` for untrusted text.
- `.Attachment(IComponent)` — under the body, e.g. `Image(url).W(320)`.
- `.Reaction(string emoji, int count, bool reacted = false)` — add/update a chip (count `<= 0` removes it).
  Clicking a chip toggles it. `.GetReaction(emoji)` returns `(count, reacted)`.
- `.OnReact((comment, emoji, added) => ...)` — called after a toggle; also shows the add-reaction button,
  whose choices are `.ReactionChoices(params string[])` (default `ConversationComment.DefaultReactions`).
- `.SetAuthor(string)`, `.SetAvatar(Avatar)`, `.Highlighted(bool = true)` (primary frame), `.Author`, `.Markdown`.

## ConversationEvent

- `.Inline(params IComponent[])` / `.Inline(string)` — components or bold text in the sentence (label tags, an assignee).
- `.Detail(IComponent)` — a line under it (a commit, a linked pull request).
- `.Tone(BadgeTone)` — fill the rail icon for state changes: `Success` reopened, `Danger` not planned, `AI`/`Primary` completed.
- `.SetAvatar(Avatar)` (defaults to initials of the actor; `null` hides it), `.Timestamp(text, tooltip)`.

## ConversationComposer

- `.OnSubmit((composer, markdown) => ...)` — submit button or Ctrl/⌘+Enter; the box is cleared first.
- `.Actions(params Button[])` — extra buttons left of submit ("Close issue").
- `.SetText(string)`, `.Text`, `.Focus()`, `.SetAvatar(Avatar)`, `.TextArea` (the underlying `TextArea`).

## Example

```csharp
using static Tesserae.UI;

var conversation = Conversation().Children(
    ConversationComment("Ana Souza", "The rail breaks under a **highlighted** comment.", "on May 18")
        .Action("opened").Role("Author")
        .Attachment(Image("./assets/img/screenshot.png").W(320))
        .Reaction("👍", 4)
        .Menu(ContextMenuItem("Copy link"), ContextMenuItem("Edit"))
        .OnReact((c, emoji, added) => Save(c, emoji, added)),
    ConversationEvent(UIcons.Tags, "Ana Souza", "added", "on May 18")
        .Inline(Tag("bug").Pill().Tint("#d73a4a")),
    ConversationEvent(UIcons.CheckCircle, "Ana Souza", "closed this as completed", "on May 22")
        .Tone(BadgeTone.AI));

var composer = ConversationComposer(Avatar(initials: "ME"));
composer.OnSubmit((c, text) => conversation.Add(ConversationComment("You", text, "just now")));
conversation.Composer(composer);

return HStack().WS().Wrap().AlignItems(ItemAlign.Start).Children(
    conversation.Grow().Style(s => { s.flexBasis = "480px"; s.minWidth = "0"; }).PR(24),
    issueMetadataPanel.W(256).NoShrink()).Render();
```

## Related

- Metadata panel — `metadata-panel.md`
- Markdown block — `markdown-block.md`
- Avatar — `avatar.md`, Image — `image.md`, Badge/Tag — `badge.md`
- Timeline — `timeline.md`, Chat — `chat.md`
