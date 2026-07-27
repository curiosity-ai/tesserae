---
name: tool-call
description: An inline, accordion-style indicator of an AI tool invocation, plus a ToolsUsed summary that opens a list-and-detail modal. Use when surfacing tool calls in a chat/assistant UI in a Tesserae (C#/Transpose) app.
---

# ToolCall / ToolsUsed

`ToolCall` is an inline tool-call row (icon + label + chevron) that expands to show its content; the content is built lazily the first time it expands. `ToolsUsed` is a compact "Used N tools" summary that opens a modal listing the tools, with a back/detail slide.

Both carry an 8px bottom margin, so when you stack a pill above the answer text in a chat message you don't need to add your own top padding on the text.

## Create

`UI.ToolCall(UIcons icon, string text, Func<IComponent> contentFactory = null)` — lazy content.
`UI.ToolCall(UIcons icon, string text, IComponent content)` — eager content.
`UI.ToolsUsed(params ToolCall[] tools)` — summary wrapping several tool calls.
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

## Related

- LiveProgress — `.live-progress.md`
- Chat — `.chat.md`
- Expander — `.expander.md`
- Full docs & API: `/tesserae/components/tool-call`
