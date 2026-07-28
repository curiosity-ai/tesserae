---
name: tool-call
description: An inline, accordion-style indicator of an AI tool invocation, a ToolsUsed summary that opens a list-and-detail modal, and ToolCallInspect for showing a call's arguments and response. Use when surfacing tool calls in a chat/assistant UI in a Tesserae (C#/Transpose) app.
---

# ToolCall / ToolsUsed / ToolCallInspect

`ToolCall` is an inline tool-call row (icon + label + chevron) that expands to show its content; the content is built lazily the first time it expands. `ToolsUsed` is a compact "Used N tools" summary that opens a modal listing the tools, with a back/detail slide. `ToolCallInspect` is the ready-made body for a call — its arguments above its response — for use as either one's content.

Both carry an 8px bottom margin, so when you stack a pill above the answer text in a chat message you don't need to add your own top padding on the text.

## Create

`UI.ToolCall(UIcons icon, string text, Func<IComponent> contentFactory = null)` — lazy content.
`UI.ToolCall(UIcons icon, string text, IComponent content)` — eager content.
`UI.ToolsUsed(params ToolCall[] tools)` — summary wrapping several tool calls.
`UI.ToolCallInspect(string arguments = null, string response = null)` — arguments + response body.
`using static Tesserae.UI;`.

## Key configuration

`ToolCall`:

- `.Expand()` / `.Collapse()` / `.Toggle()` / `.Expanded(bool)` — control state.
- `.NotExpandable()` — hide the chevron, lock collapsed.
- `.OnToggle(tc => ...)` — fires on expand/collapse.
- `.SetContent(...)` / `.SetText(...)` / `.SetIcon(...)` — update fields.
- `.SetProgress(string)` / `.SetProgress(IObservable<string>)` / `.ClearProgress()` — a live progress
  line on the header row while the call runs (see `.live-progress.md`). Updates rewrite the text of
  the line already on screen, so a stream of updates never re-renders the call, and expanding it
  still opens the content full width underneath.
- `Progress` — the `LiveProgress` itself, for finer control.
- `IsExpanded`, `HasContent`, `Icon`, `Text` — read state.

Expansion is per instance, so a host that rebuilds its layout into a diffing container (a streaming
chat bubble) must carry it: record it with `.OnToggle(c => open = c.IsExpanded)` and re-apply
`.Expanded()` on the rebuilt call, or the diff collapses the open one on screen and drops the content
it had built.

`ToolsUsed`:

- `.Add(ToolCall)` / `.AddRange(...)` / `.Add(icon, text, factory)` — add tools.
- `.SetSummary(label)` / `.SetTitle(title)` / `.SetIcon(icon)` — customise.
- `.SetProgress(string)` / `.SetProgress(IObservable<string>)` / `.ClearProgress()` / `Progress` —
  same live progress line, on the summary pill.
- `.Show()` / `.Hide()` — open/close the modal.

`ToolCallInspect`:

- `.SetArguments(json)` — a JSON object becomes one name/value row per property; anything else is
  shown verbatim. An empty value hides the section.
- `.SetResponse(text)` — shown in a read-only code block, re-indented when it parses as JSON, so
  handing it the raw payload is enough. An empty value hides the section.
- `.SetError(message)` — an error line above the response, for a call that failed.
- `.SetArgumentsLabel(...)` / `.SetResponseLabel(...)` / `.SetErrorLabel(...)` / `.SetEmptyText(...)` —
  captions, for localisation. Default to "Arguments" / "Response" / "Error".

The arguments and the response scroll independently rather than under one shared scrollbar: the
arguments are capped (at 40vh inline, at half the pane inside a `ToolsUsed` detail view) and the
response claims the rest, so inspecting a long response never scrolls the arguments off screen.

## Example

```csharp
using static Tesserae.UI;

var tools = ToolsUsed(
    ToolCall(UIcons.Search, "Search documentation",
        () => TextBlock("query: tesserae popover").Small()),
    ToolCall(UIcons.FileCode, "Read source file",
        () => TextBlock("Popover.cs").Small())
);
```

Inspecting what a call was given and what it returned:

```csharp
using static Tesserae.UI;

var call = ToolCall(UIcons.FilePdf, "Consult 'report.pdf'",
    () => ToolCallInspect(
        @"{ ""fileUID"": ""WAwweAZJPrs6nE95L25GbX"", ""page"": 188 }",
        @"{ ""pages"": 240, ""extracted"": { ""characters"": 8000 } }"));

var failed = ToolCall(UIcons.Globe, "Fetch https://api.example.com/v1/status",
    () => ToolCallInspect(@"{ ""timeoutMs"": 30000 }")
             .SetError("HTTP 503 - the upstream did not respond within 30s"));
```

## Related

- LiveProgress — `.live-progress.md`
- Chat — `.chat.md`
- Expander — `.expander.md`
- Full docs & API: `/tesserae/components/tool-call`
