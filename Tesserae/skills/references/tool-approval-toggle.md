---
name: tool-approval-toggle
description: A three-way toggle that sets how an AI tool may run - always allow, needs approval, or blocked - with the selected icon coloured by its meaning. Use when building a settings page where an admin or user decides per tool whether calls run on their own, wait for approval, or are refused, in a Tesserae (C#/Transpose) app.
---

# ToolApprovalToggle

A compact `IconToggle<ToolApprovalMode>` with the three modes an AI tool can run under:

| `ToolApprovalMode` | Icon | Selected colour |
|---|---|---|
| `AlwaysAllow` | `UIcons.CheckCircle` | success |
| `NeedsApproval` | `UIcons.HandPaper` | warning |
| `Blocked` | `UIcons.Ban` | danger |

## Create

`ToolApprovalToggle(ToolApprovalMode selected = NeedsApproval, string alwaysAllowLabel = null, string needsApprovalLabel = null, string blockedLabel = null)`.
The labels are the items' tooltips; null keeps the English `DefaultLabel`. Pass translated
strings when the UI is translated. Bring the factory into scope with `using static Tesserae.UI;`.

## Key configuration

- `.Select(ToolApprovalMode)` / `.Selected` — set or read the mode.
- `.AsObservable()` — `IObservable<ToolApprovalMode>`.
- `.OnChange((sender, mode) => ...)` — fires when the user picks another mode, not for the initial one.
- `.Bind(SettableObservable<ToolApprovalMode>)` — two-way binding.
- `.Disabled(bool value = true)` — show the mode without letting it change.
- `ToolApprovalToggle.IconOf(mode)` / `ToolApprovalToggle.DefaultLabel(mode)` — the same icon and
  English name for a read-only view of the modes, so it matches the toggle.

## Example

```csharp
using static Tesserae.UI;

var rows = VStack().Children(tools.Select(tool =>
    HStack().WS().AlignItemsCenter().Children(
        TextBlock(tool.Name).W(10).Grow(),
        ToolApprovalToggle(tool.Mode, "Always allow".t(), "Needs approval".t(), "Blocked".t())
           .OnChange((_, mode) => SaveMode(tool, mode)))).ToArray());
```

An application with its own mode enum maps it to `ToolApprovalMode` at the edge, so the toggle
and every read-only view share one set of icons.

## Related

- IconToggle (the control underneath) — `icon-toggle.md`
- ToolCall (how a call that waits for approval shows in a chat) — `tool-call.md`
- ToolAgentSelector — `tool-agent-selector.md`
- Full docs & API: `/tesserae/components/tool-approval-toggle`
