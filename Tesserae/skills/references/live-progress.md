---
name: live-progress
description: A single line of progress text that is rewritten in place as updates stream in, with a stable tooltip carrying the full text and no entrance animation. Use for the running status of a long task (an AI tool call, an upload, an index build) in a Tesserae (C#/Transpose) app.
---

# LiveProgress

`LiveProgress` is one quiet, ellipsized line of text ("Reading documents · Encoding 57%") built for
updates that arrive many times a second. The line element and its tooltip are created once; an update
only rewrites their text, so nothing fades in, re-mounts or re-animates while the values change.

An empty text hides the line (`display: none`) instead of removing it, so the next update brings the
same element back.

## Create

`UI.LiveProgress(string text = null)`
`using static Tesserae.UI;`.

## Key configuration

- `.SetText(string)` — write the current progress. Call as often as it arrives.
- `.Clear()` — hide the line, keeping the element for the next update.
- `.Stream(IObservable<string>)` — follow an observable; every value published is written into the
  line. The subscription is released when the component leaves the DOM.
- `.StopStreaming()` — stop following the observable, keeping the last text.
- `.WithTooltip(bool = true)` / `.NoTooltip()` — the hover tooltip with the untruncated text is on by
  default (the line is ellipsized to whatever width it is given). It is a tippy popover attached to
  the line itself on the first hover, then updated in place, so it stays correct even while open.
  The line is an `inline-block` that hugs its text — pad it with `margin`, not `padding`, or the
  tooltip centers on the padding box instead of on the text.
- `Text`, `IsEmpty` — read state.

## Example

```csharp
using static Tesserae.UI;

var progress = LiveProgress();

// Imperative: write each update into the line already on screen.
progress.SetText("Reading documents · 12 of 40");

// Or stream it from an observable.
var status = new SettableObservable<string>("Starting");
var line   = LiveProgress().Stream(status);

status.Value = "Encoding 57%";   // rewrites the text, nothing re-renders
status.Value = "";               // hides the line
```

A `ToolCall` / `ToolsUsed` hosts one of these on its own header, which is the usual place for it in a
chat transcript:

```csharp
var call = ToolCall(UIcons.Search, "Search documentation", () => TextBlock("…"));

call.SetProgress("Reading documents · Encoding 57%");   // sits on the header row
call.SetProgress(status);                               // or stream it
call.ClearProgress();                                   // when the call is done
```

Because the line lives inside the header row, expanding the tool call is unaffected — the content
still opens full width underneath.

## Related

- ToolCall / ToolsUsed — `.tool-call.md`
- Chat — `.chat.md`
- ProgressIndicator (determinate bar) — `.progress-indicator.md`
- Observables — `.observables.md`
- Full docs & API: `/tesserae/components/live-progress`
