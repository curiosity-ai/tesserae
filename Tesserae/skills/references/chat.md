---
name: chat
description: A chat transcript surface (ChatArea) holding aligned message bubbles (ChatMessage) with avatars, commands, and live-updatable content via DeltaComponent. Use to build chat or assistant UIs with streaming/typing messages in a Tesserae (C#/h5) app.
---

# Chat (ChatArea / ChatMessage)

`ChatArea` is a scrolling transcript that auto-scrolls to new messages (unless the user
scrolled up). `ChatMessage` is one bubble; its content is wrapped in a `DeltaComponent`,
so you can call `.ReplaceContent(...)` repeatedly to animate streamed/typed text.

## Create

`UI.ChatArea()`.
`UI.ChatMessage(IComponent content, IComponent avatar = null, IComponent commands = null)`.
Bring factories into scope with `using static Tesserae.UI;`.

## Key configuration

ChatArea:

- `.Add(ChatMessage)` — append a message (auto-scrolls into view).
- `.Clear()` — remove all messages.
- `.Background(string)` — bubble background color for all messages.
- `.OnScroll(...)` / `.OnFocus(...)` / `.OnBlur(...)`.

ChatMessage:

- `.LeftAligned()` (default) / `.RightAligned()` — bubble side.
- `.MaxWidth()` / `.FullWidth()` — bubble width.
- `.ReplaceContent(IComponent)` — swap content (animated; use for streaming text).
- `.WithReferences(IEnumerable<IComponent>)` — attach reference chips.
- `.KeepVisible()` — re-scroll the area to keep this message in view.
- `.Background(string)` — this bubble's background.

## Example

```csharp
using static Tesserae.UI;

var chat = ChatArea();
chat.Add(ChatMessage(TextBlock("Hi!"), Avatar(null, "U")).RightAligned().MaxWidth());

var reply = ChatMessage(TextBlock(""), Avatar(null, "AI")).MaxWidth();
chat.Add(reply);
reply.ReplaceContent(TextBlock("Hello, how can I help?")); // call repeatedly to stream
reply.KeepVisible();
```

## Related

- Avatar — `avatar.md`
- Full docs & API: `/tesserae/components/chat`
