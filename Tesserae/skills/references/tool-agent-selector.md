---
name: tool-agent-selector
description: A Dropdown-style trigger button + searchable popup for enabling agents and tools, grouped into "Agents" and "Tools" sections with a selection-count badge. Use when adding a tool/agent picker next to an OmniBox (or any other) chat/search input, including an "@mention" inline picker.
---

# ToolAgentSelector

A trigger button that opens a searchable, checkbox-based popup listing agents and tools under
"Agents" and "Tools" section headers (icon + title + optional description per row). Selecting
items shows a count badge on the trigger. It can also be driven inline — anchored at an explicit
viewport position instead of the trigger — to back an "@mention" style picker inside a chat input
(see `OmniBox.EnableChatMentions` in `omni-box.md`).

## Create

`UI.ToolAgentSelector(string label = "Tools", UIcons icon = UIcons.Tools)` — the trigger + popup.
`UI.ToolAgentSelectorItem(string id, string title, string description = null, UIcons? icon = null)`
— a selectable row. Bring factories into scope with `using static Tesserae.UI;`.

## Key configuration

ToolAgentSelector:

- `.Agents(params Item[])` / `.Tools(params Item[])` — set the items in each section (replaces
  existing).
- `.Compact(bool = true)` — hides item descriptions for a denser list.
- `.SelectedItems` / `.SelectedCount` — current selection, agents first then tools.
- `.OnChange(Action<ToolAgentSelector>)` — fires whenever any item's selection changes.
- `.Show()` / `.Hide()` — anchored below the trigger button (also toggled by clicking the
  trigger, or Enter/Space when it has focus).
- `.ShowInlineAt(double clientX, double clientY)` — shows the popup anchored at an explicit
  viewport position instead of the trigger, without moving focus or toggling the trigger's
  pressed state. Used for an "@mention" picker inside a text input/textarea.
- `.Filter(string text)` — filters visible items by title/description; also drives the popup from
  an inline mention as more is typed after `@`.
- `.MoveHighlight(int direction)` / `.ActivateHighlighted()` — keyboard navigation among currently
  visible items (`+1`/`-1` to move, toggling the highlighted item's selection).

ToolAgentSelector.Item:

- `.Selected(bool = true)` / `.IsSelected` — selection state.
- `.Tag` — arbitrary payload.

## Example

```csharp
using static Tesserae.UI;

var toolAgentSelector = ToolAgentSelector()
    .Agents(
        ToolAgentSelectorItem("deep-researcher", "Deep Researcher", "Multi-step web research with citations", UIcons.Search).Selected(),
        ToolAgentSelectorItem("code-assistant", "Code Assistant", "Plans, writes and debugs code end to end", UIcons.FileCode))
    .Tools(
        ToolAgentSelectorItem("web-search", "Web Search", "Search the live web for fresh results", UIcons.Globe).Selected(),
        ToolAgentSelectorItem("calculator", "Calculator", "Evaluate math expressions", UIcons.Calculator))
    .OnChange(s => Toast().Information($"{s.SelectedCount} tool(s)/agent(s) enabled"));

var chatSample = OmniBox(new OmniBox.Config(OmniBox.Mode.Chat)
{
    PlaceholderChat = "Ask me anything — type @ to mention a tool or agent",
    ChatFooter = new OmniBox.FooterItems { LeftSide = new IComponent[] { toolAgentSelector } }
})
.EnableChatMentions(new OmniBox.ChatMention
{
    OnShow         = (x, y) => toolAgentSelector.ShowInlineAt(x, y),
    OnQueryChanged = text    => toolAgentSelector.Filter(text),
    OnMove         = dir     => toolAgentSelector.MoveHighlight(dir),
    OnCommit       = ()      => toolAgentSelector.ActivateHighlighted(),
    OnHide         = ()      => toolAgentSelector.Hide(),
    IsOpen         = ()      => toolAgentSelector.IsVisible
});
```

## Related

- OmniBox, `OmniBox.EnableChatMentions` — `omni-box.md`
- Dropdown — a single/multi-select combobox with a similar Layer-based popup — `dropdown.md`
- NotificationCenter — the count-badge-on-a-trigger pattern this reuses — `notification-center.md`
- Full docs & API: `/tesserae/components/tool-agent-selector`
