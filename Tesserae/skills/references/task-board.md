---
name: task-board
description: A Kanban board of named columns holding draggable cards, with drag-and-drop reordering between and within columns. Use when building a Trello-style task/Kanban board in a Tesserae (C#/Transpose) app.
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
- `.RowMode(bool = true)` — switch to horizontal rows instead of columns. Order does not matter: columns added afterwards pick the mode up too.
- `.ReadOnly(bool = true)` — disable all drag-and-drop; the cards drop their grab cursor too.
- `.StretchColumns(bool = true)` — the columns share the board's width (each at least 220px before the board scrolls) instead of being 300px wide each, so a board with a handful of columns shows them all. No effect in row mode.
- `.ReorderColumns(bool = true)` — whether the columns themselves can be dragged into another order (on by default). `ReorderColumns(false)` for a board whose columns are a fixed workflow; cards still move between them.
- `.OnColumnDrop(e => ...)` / `.OnColumnUpdate(e => ...)` — column move callbacks
  (`e` is a `SortableEvent` with `oldIndex` / `newIndex`).
- Size with `.Height(...)`, `.Width(...)`.

TaskBoardColumn:

- `.Cards(params TaskBoardCard[])` / `.AddCard(card)` — set/append cards.
- `.AcceptDrops(bool = true)` — whether cards dragged from another column of the same group may be dropped here. `AcceptDrops(false)` makes a column cards only leave (or are only placed by code), e.g. a "Done" column the workflow moves cards into.
- `.HeaderCommands(params IComponent[])` — components on the right of the column header (a count badge, an add button). Replaces what was there.
- `.OnCardAdd` / `.OnCardRemove` / `.OnCardDrop` / `.OnCardUpdate` — card callbacks.

TaskBoardCard:

- `.Header(IComponent)` / `.Footer(IComponent)` — optional header/footer rows.
- `.Locked(bool = true)` — the card cannot be dragged (it keeps its place; the other cards of its column still move around it). Its content stays clickable. `.IsLocked` reads it back.

Identify a moved card by giving it an id (`.Id("task-42")`) and reading `e.item.id` in the destination column's `OnCardAdd` — the callback is per column, so the column is the one you registered it on.

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

- Cards — `card.md`
- Full docs & API: `/tesserae/lists/task-board`
