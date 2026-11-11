# WorkerCard

The card an assistant leaves in a chat when it splits a job and hands the parts to other agents: a
header ("Started 3 workers") with an optional note at its end, and a row per worker - the agent's
logo, the task, a quiet line under it (the agent's name, say), an optional Agent column, and a live status (a spinner
while it works). It goes inline in a chat, under the message that announces the work.

## Factory

```csharp
public static WorkerCard WorkerCard(string title = null)
```

## Workers

```csharp
var card = WorkerCard("Started 3 workers")
   .Hint("Each in its own worktree")           // optional, end of the header
   .HeaderIcon(UIcons.CodeFork)                // default
   .SetItems(new[]
    {
        new WorkerCardItem
        {
            Id = "w1",                         // stable key; rows are reconciled by it
            Title = "Meter API calls per workspace",
            Subtitle = "Metering agent",        // the quiet line under the title: who is doing it
            Agent = null,                      // optional column of its own
            Tone = ConversationStateTone.Running, StateLabel = "Working",
            Icon = UIcons.Sparkles,            // or Leading = a small component (a logo)
            Trailing = Button("Review").Primary().LessPadding(),   // optional, after the status
        },
    })
   .OnSelect(item => Open(item.Data));         // makes the rows clickable
```

| Method | What it does |
|---|---|
| `SetItems(items)` | Replaces every worker; unchanged rows keep their DOM. |
| `UpdateItem(item)` | Replaces the worker with the same id, or adds it at the end. |
| `Title(text)` / `Hint(text)` / `HeaderIcon(icon)` | The header. |
| `Items` | What the card lists. |

Tones are `ConversationStateTone`: `Running` (spinner), `Success` (check), `Attention` (warning),
`Danger` (cross), `Idle` / `Muted` (ring). A click on `Trailing` is the button's own and does not
reach `OnSelect`.

Related: [conversation-list](conversation-list.md), [tool-call](tool-call.md).
