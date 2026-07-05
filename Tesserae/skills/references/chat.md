---
name: chat
description: A chat transcript surface (ChatArea) holding aligned message bubbles (ChatMessage) with avatars, commands, and live-updatable content via DeltaComponent. Use to build chat or assistant UIs with streaming/typing messages in a Tesserae (C#/h5) app.
---

# Chat (ChatArea / ChatMessage)

`ChatArea` is a scrolling transcript built for streaming replies. It follows the live edge
while the reader is at the bottom, stops following the moment they scroll up (new content
arrives off-screen without yanking them back), and shows a scroll-to-latest button to
re-engage. `ChatMessage` is one bubble; its content is wrapped in a `DeltaComponent`, so you
can call `.ReplaceContent(...)` repeatedly to animate streamed/typed text.

## Create

`UI.ChatArea()`.
`UI.ChatMessage(IComponent content, IComponent avatar = null, IComponent commands = null)`.
Bring factories into scope with `using static Tesserae.UI;`.

## Key configuration

ChatArea:

- `.Add(ChatMessage)` — append a message. Auto-scrolls into view **only if the reader is at
  the bottom**; if they scrolled up, the message is appended without moving them. Adding an
  anchored message (see `.ScrollAnchor()`) always re-engages following.
- `.PrependRange(IEnumerable<ChatMessage>)` — insert older messages at the top while keeping
  the reader's scroll position stable (for loading history above the fold).
- `.Clear()` — remove all messages (also resets follow state and start-position).
- `.Background(string)` — bubble background color for all messages.
- `.DefaultScrollPosition(StartPosition)` — where the transcript settles when first populated
  (or re-populated after `Clear`): `Start`, `End` (default), or `LastAnchor` (the last
  anchored turn, with a peek of prior content above it).
- `.PreviousItemPeek(int px)` — pixels of the previous turn kept visible above an anchored
  message when a new turn begins (default 64).
- `.Busy(bool)` — toggles `aria-busy` on the transcript; set `true` while a reply streams.
- `.ScrollToEnd(bool smooth = false)` / `.ScrollToStart(...)` / `.ScrollToMessage(string id,
  bool smooth = true)` — imperative scrolling. `ScrollToEnd` re-engages following.
- `.IsAtBottom` / `.IsFollowingOutput` — read current scroll state.
- `.OnScroll(...)` / `.OnFocus(...)` / `.OnBlur(...)`.

ChatMessage:

- `.LeftAligned()` (default) / `.RightAligned()` — bubble side.
- `.MaxWidth()` / `.FullWidth()` — bubble width.
- `.ScrollAnchor()` — mark this message as a turn boundary. When added it settles near the top
  (with a peek of the previous turn above it) so the reply that follows grows into the screen
  below. Typically set on the user's message.
- `.Animated()` — fade in streamed deltas.
- `.ReplaceContent(IComponent)` — swap content (use for streaming text).
- `.WithReferences(IEnumerable<IComponent>)` — attach reference chips.
- `.KeepVisible()` — keep this message at the live edge while it streams (respects the reader's
  scroll — a no-op if they scrolled away).
- `.Background(string)` — this bubble's background.

## Example

```csharp
using static Tesserae.UI;

var chat = ChatArea().DefaultScrollPosition(ChatArea.StartPosition.LastAnchor);

// Anchor the user's turn so the reply grows into the screen below it.
chat.Add(ChatMessage(TextBlock("Hi!"), Avatar(null, "U")).RightAligned().MaxWidth().ScrollAnchor());

var reply = ChatMessage(TextBlock(""), Avatar(null, "AI")).Animated().MaxWidth();
chat.Add(reply);
chat.Busy(true);
reply.ReplaceContent(TextBlock("Hello, how can I help?")); // call repeatedly to stream
reply.KeepVisible();
chat.Busy(false);
```

## Related

- Avatar — `avatar.md`
- Full docs & API: `/tesserae/components/chat`
