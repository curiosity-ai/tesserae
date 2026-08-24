---
name: tool-call
description: An inline, accordion-style indicator of an AI tool invocation, a ToolsUsed summary that opens a list-and-detail modal or expands its tools inline, and ToolCallInspect for showing a call's arguments and response. Use when surfacing tool calls in a chat/assistant UI in a Tesserae (C#/Transpose) app.
---

# ToolCall / ToolsUsed / ToolCallInspect

`ToolCall` is an inline tool-call row (icon + label + chevron) that expands to show its content; the content is built lazily the first time it expands. `ToolsUsed` is a compact "Used N tools" summary that opens a modal listing the tools, with a back/detail slide — or, with `.Inline()`, expands into the calls right where it stands. `ToolCallInspect` is the ready-made body for a call — its arguments above its response — for use as either one's content.

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
  line on the header row while the call runs (see `live-progress.md`). Updates rewrite the text of
  the line already on screen, so a stream of updates never re-renders the call, and expanding it
  still opens the content full width underneath.
- `Progress` — the `LiveProgress` itself, for finer control.
- `.AddAction(UIcons icon, string tooltip, Action)` (also `Action<ToolCall>`) / `.ClearActions()` — an
  icon button hanging off the right of the call, outside the chip and out of flow (so a call is the
  same width with or without one). It shows at a third of its strength until the pointer is anywhere
  on the call, and at full strength on hover, focus, or on a touch screen. Clicking it runs the
  handler only, never expanding or collapsing the call. Use it for a way into what the call stands
  for that isn't its content — opening the run it started, retrying it.
- `IsExpanded`, `HasContent`, `Icon`, `Text` — read state.

Expansion is per instance, so a host that rebuilds its layout into a diffing container (a streaming
chat bubble) must carry it: record it with `.OnToggle(c => open = c.IsExpanded)` and re-apply
`.Expanded()` on the rebuilt call, or the diff collapses the open one on screen and drops the content
it had built. Progress reaches such a host only through a rebuild, so the update that finishes the
call needs one too — clear the progress in the state the layout is built from and diff once more, or
the line on screen keeps the last text it was given.

`ToolsUsed`:

- `.Add(ToolCall)` / `.AddRange(...)` / `.Add(icon, text, factory)` — add tools.
- `.SetSummary(label)` / `.SetTitle(title)` / `.SetIcon(icon)` — customise.
- `.SetProgress(string)` / `.SetProgress(IObservable<string>)` / `.ClearProgress()` / `Progress` —
  same live progress line, on the summary pill.
- `.AddAction(UIcons icon, string tooltip, Action)` (also `Action<ToolsUsed>`) / `.ClearActions()` —
  the same icon button, beside the summary pill, for an action that belongs to the group rather than
  to one of its calls (opening the run whose calls it lists, say). It stays next to the pill whether
  or not the group is expanded, and does not open or fold it.
- `.Inline()` — render the tools in place instead of in the modal (see below).
- `.Show()` / `.Hide()` — open/close (the modal, or the inline list when `.Inline()` is set).
- `.Expand()` / `.Collapse()` / `.Toggle()` / `.Expanded(bool)` / `.OnToggle(tu => ...)` —
  the inline list's state; without `.Inline()` expanding opens the modal instead.
- `IsInline`, `IsExpanded` — read state.

### Inline instead of a modal

`.Inline()` turns the summary into an accordion: it expands underneath itself into the list of
`ToolCall`s, each one opening its own content inline the way a standalone call does — the calls
themselves are what the list holds, so a reference you kept still drives the row on screen. Use it in a
transcript where sending the reader to a modal for a one-line result is too much ceremony, and keep the
modal for groups whose bodies need the room. A call added while the list is open joins it in place, so
a live transcript can append as the calls come in.

```csharp
var tools = ToolsUsed(
    ToolCall(UIcons.Terminal, "Bash dotnet build",
        () => TextBlock("Build succeeded.").BreakSpaces()),
    ToolCall(UIcons.Eye, "Read README.md",
        () => TextBlock("# Needle").BreakSpaces())
).Inline().Expanded();

tools.Add(ToolCall(UIcons.Globe, "Fetch /v1/items", () => ToolCallInspect("{}", "{}")));
```

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

- LiveProgress — `live-progress.md`
- Chat — `chat.md`
- Expander — `expander.md`
- Full docs & API: `/tesserae/components/tool-call`
