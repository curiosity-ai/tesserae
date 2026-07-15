---
name: tool-call
description: An inline, accordion-style indicator of an AI tool invocation, plus a ToolsUsed summary that opens a list-and-detail modal. Use when surfacing tool calls in a chat/assistant UI in a Tesserae (C#/h5) app.
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
- `IsExpanded`, `HasContent`, `Icon`, `Text` — read state.

`ToolsUsed`:

- `.Add(ToolCall)` / `.AddRange(...)` / `.Add(icon, text, factory)` — add tools.
- `.SetSummary(label)` / `.SetTitle(title)` / `.SetIcon(icon)` — customise.
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

- Chat — `.chat.md`
- Expander — `.expander.md`
- Full docs & API: `/tesserae/components/tool-call`
