---
name: task-board
description: A Kanban board of named columns holding draggable cards, with drag-and-drop reordering between and within columns. Use when building a Trello-style task/Kanban board in a Tesserae (C#/h5) app.
---

# TaskBoard

A Kanban board with draggable columns and cards. A `TaskBoard` holds
`TaskBoardColumn`s; each column holds `TaskBoardCard`s. Cards drag between and
within columns; columns can be reordered. Drag callbacks report the moves.

## Create

- `UI.TaskBoard()` — the board.
- `UI.TaskBoardColumn(string title, string sortableGroup = "taskboard")` — a column.
- `UI.TaskBoardCard(IComponent content)` — a card wrapping any content.

Bring factories into scope with `using static Tesserae.UI;`.

## Key configuration

TaskBoard:

- `.Columns(params TaskBoardColumn[])` / `.AddColumn(col)` — set/append columns.
- `.RowMode(bool = true)` — switch to horizontal rows instead of columns.
- `.ReadOnly(bool = true)` — disable all drag-and-drop.
- `.OnColumnDrop(e => ...)` / `.OnColumnUpdate(e => ...)` — column move callbacks
  (`e` is a `SortableEvent` with `oldIndex` / `newIndex`).
- Size with `.Height(...)`, `.Width(...)`.

TaskBoardColumn:

- `.Cards(params TaskBoardCard[])` / `.AddCard(card)` — set/append cards.
- `.OnCardAdd` / `.OnCardRemove` / `.OnCardDrop` / `.OnCardUpdate` — card callbacks.

TaskBoardCard:

- `.Header(IComponent)` / `.Footer(IComponent)` — optional header/footer rows.

## Example

```csharp
using static Tesserae.UI;

var board = TaskBoard()
    .OnColumnDrop(e => console.log($"col {e.oldIndex} -> {e.newIndex}"))
    .Columns(
        TaskBoardColumn("To Do").Cards(
            TaskBoardCard(TextBlock("Research personas"))
                .Header(Badge("Design").Primary())
                .Footer(Avatar(initials: "JD").Size(AvatarSize.XSmall)),
            TaskBoardCard(TextBlock("Draft mockups"))),
        TaskBoardColumn("In Progress").Cards(
            TaskBoardCard(TextBlock("Build TaskBoard").SemiBold())),
        TaskBoardColumn("Done").Cards(
            TaskBoardCard(TextBlock("Project plan"))));

var component = board.Height(600.px());
```

## Related

- Cards — `../card/SKILL.md`
- Full docs & API: `/tesserae/components/task-board`
