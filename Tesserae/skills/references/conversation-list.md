# ConversationList

The history of a chat surface: a list of conversations with a **pinned** section at the top, a
**search** icon and a **filter** icon (by state and by time) in its header, a **state dot** per
row and the **relative time** each conversation last changed ("now", "9h", "2d", "3w").

It is an `ISidebarItem`: put it in a `Sidebar` and, when the sidebar collapses to its rail, it
leaves a single glyph whose hover (or focus, or click) opens **the same list** in a flyout beside
the rail - same filter, same search, same scroll position.

## Factory

```csharp
public static ConversationList ConversationList(string title = null)
```

## Items

```csharp
new ConversationListItem
{
    Id        = "c1",                       // stable key; rows are reconciled by it
    Title     = "Tokenizer splits URLs with ports",
    Subtitle  = "curiosity-ai/catalyst",     // optional second line
    Time      = DateTimeOffset.Now.AddHours(-9),
    IsPinned  = true,
    State     = "working",                  // key of a state added with AddState; null = idle
    Icon      = UIcons.CodeCompare,         // optional, drawn before the title
    IconColor = Theme.Colors.Green600,      // optional
    Trailing  = Icon(UIcons.TriangleWarning),// optional small component before the time
    Data      = myDto,                      // anything, handed back on select
};
```

| Method | What it does |
|---|---|
| `SetItems(items)` | Replaces every item; unchanged rows keep their DOM. |
| `AddItems(items)` | Appends a page; an id already held is updated in place. |
| `UpdateItem(item)` | Replaces the item with the same id, or adds it at the top. |
| `RemoveItem(id)` / `Clear()` | Removes one / all. |
| `Loading(bool)` | Spinner instead of the empty message while there are no items. |
| `HasMore(bool)` + `OnLoadMore(Action)` | Paging: a sensor at the end fires once per `HasMore(true)`. |
| `Select(id)` | Marks the open conversation and scrolls it into view. |

## Children: agents working inside a conversation

Give an item a `Children` list and its row expands like a sidebar group: the arrow replaces the dot
while the row is hovered (ArrowRight / ArrowLeft on the focused row too), and a count before the time
says how many children there are, with a pulsing dot while one is running (red if one failed). A
null or empty list leaves a plain row.

```csharp
new ConversationListItem
{
    Id = "c0", Title = "Checkout flow", Time = DateTimeOffset.Now,
    Children = new[]
    {
        new ConversationListChild
        {
            Id = "run-1",                       // shares the id space of the conversations
            Title = "Tighten address validation",
            Meta = "opus-5.5",                  // optional: a quiet monospace detail after the name
            Time = DateTimeOffset.Now.AddMinutes(-49),
            Tone = ConversationStateTone.Running, StateLabel = "Running",   // spinner / check / warning / cross / ring
            Icon = UIcons.Sparkles,             // or Leading = a small component (a provider logo)
            Data = myRun,
        },
    },
};

list.OnChildSelect((child, parent) => OpenRun(child.Data))
    .ExpandItem("c0");                          // CollapseItem(id), IsExpanded(id)
```

`Select(childId)` marks the child and opens its conversation. A search matches child titles and
`Meta` too, and opens the conversation it found them in. Selecting a child does not select the
conversation, and updating an item with `UpdateItem` redraws only the child rows that changed.

## States and filters

```csharp
list.AddState("working",  "Working",     UIcons.Spinner,         ConversationStateTone.Running)
    .AddState("input",    "Needs input", UIcons.TriangleWarning, ConversationStateTone.Attention)
    .AddState("done",     "Completed",   UIcons.Check,           ConversationStateTone.Success)
    .AddState("archived", "Archived",    UIcons.Box,             ConversationStateTone.Muted, includedInAll: false);
```

- Tones: `Idle` (hollow ring, the default), `Running` (pulsing primary dot), `Attention`
  (warning dot), `Success` (success dot), `Muted` (faded dashed ring), `Danger` (danger dot).
- `includedInAll: false` keeps that state out of "All" - it shows only when picked (an archive).
- Time filter: `ConversationTimeRange.AnyTime / Today / Past7Days / Past30Days / Older`.
- `FilterByState(key)`, `FilterByTime(range)`, `ClearFilters()`, `OpenSearch()`, `CloseSearch()`,
  `CurrentFilter` (a `ConversationListFilter` with `State`, `TimeRange`, `Search`, `IsDefault`).
- The list always filters what it holds. A host that pages from a server listens to
  `OnFilterChanged(Action<ConversationListFilter>)` and loads what the new filter covers.
- `SearchWith(Func<string, Task<IEnumerable<ConversationListItem>>>)` answers the search from a
  server instead: while something is typed the list shows those results (debounced, stale replies
  dropped).

## Events and chrome

```csharp
list.OnSelect(item => Open(item.Id))
    .ItemMenu(item => new[]
    {
        ContextMenuItem(item.IsPinned ? "Unpin" : "Pin").OnClick(() => TogglePin(item)),
        ContextMenuItem("Archive").OnClick(() => Archive(item)),
    })
    .EmptyMessage("Chats you start will show up here")
    .ClosedLabel("Chats", UIcons.TimePast);   // the rail glyph and its accessible name
```

`ItemMenu` gives every row a "⋯" button (on hover; always visible on touch screens) and a
right-click menu. A menu opened from the flyout keeps the flyout open until it closes.

## In a sidebar

```csharp
var list    = ConversationList("Conversations");
var sidebar = Sidebar();
sidebar.AddContent(new SidebarComponent("history", list, list.RenderClosed()).Grow().NotSortable());
// or: sidebar.AddContent(list);   // it is an ISidebarItem itself
```

## Related

- [chat](chat.md) - the transcript the list opens.
- [sidebar](sidebar.md) - the container it usually lives in.
- [context-menu](context-menu.md) - the items `ItemMenu` returns.
