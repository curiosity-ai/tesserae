---
name: tree
description: A hierarchical tree view with expand/collapse, selection, keyboard nav, and sync or async child loading. Use when displaying nested/hierarchical data (file trees, org charts) in a Tesserae (C#/Transpose) app.
---

# Tree

A vertically-stacked tree of `Tree.Item` nodes. Nodes expand/collapse to reveal children, support selection, and can load children synchronously or lazily.

## Create

`new Tree()` — construct directly (no `UI.` factory). Add nodes with `.Items(...)`.
`new Tree.Item(string text = null, UIcons? icon = null, params TreeCommand[] commands)` — a node.
`using static Tesserae.UI;` for `UIcons` and helpers.

## Key configuration

`Tree`:

- `.Items(params Tree.Item[])` — add top-level nodes.
- `.Selectable(TreeSelectionMode = Multiple)` — turn on item selection: `None`, `Single` (one at a time), or `Multiple`.
- `.SelectionEnabled(bool = true)` — shorthand for `Single` / `None`.
- `.NotSelectable()` — take selection away again, unselecting whatever was selected.
- `.CascadeSelection(bool = true)` — selecting a folder selects everything inside it, unselecting it unselects them, and a folder only part of which is picked is drawn half-selected (`IsPartiallySelected`).
- `.Compact(bool = true)` — compact density (22px rows, 13px text, 8px indent), matching a code editor's file explorer.
- `.OnSelected((s, item) => ...)` — fires when the selected item changes; `SelectedItem` holds the last one picked.
- `.OnSelectionChanged((s, items) => ...)` — fires with everything selected; one call per gesture, even when a range or a cascade moved many rows.
- `.ClearSelection()` / `.SelectAll()` — move the whole selection from code; `SelectedItems` reads it back, in tree order.
- `.Clear()` / `.Replace(newItem, oldItem)` — manage nodes.
- `.Filter(item => bool)` / `.Filter("text")` / `.ClearFilter()` — show only the matching rows and the folders leading to them (see below); `IsFiltered` says whether one is in force.

`Tree.Item`:

- `.Items(params Tree.Item[])` — add children.
- `.ItemsAsync(async () => Tree.Item[])` — lazy-load children on first expand (shows a spinner).
- `.Expanded(bool = true)` / `.Selected(bool = true)` — initial state.
- `.Selectable(bool = true)` — say whether the row can be picked at all; one that cannot shows no checkbox and is skipped by ranges, cascades and `SelectAll`.
- `.OnSelected(...)` / `.OnSelectionChanged((item, isSelected) => ...)` / `.OnExpanded(...)` / `.OnCollapsed(...)` — node events.
- `.CommandsAlwaysVisible(bool)` — keep row commands visible (not hover-only).
- `.IconColor(color)` — tint the row's icon (a danger colour on a file that fails to compile, say), leaving the text alone; null goes back to the theme's colour.
- `Text`, `Icon`, `IsExpanded`, `IsSelected`, `IsPartiallySelected`, `IsSelectable`, `HasChildren`, `Children`, `Parent`, `IsFilteredOut` — read/write state.

## Selection gestures

`TreeSelectionMode.Multiple` gives a tree the gestures of a list of search results, with the checkbox
of every row on show:

- **Checkbox click** — picks that one row.
- **Ctrl (or cmd) click on a row** — the same, without opening or expanding it.
- **Shift-click on a row** — picks everything between the last row picked and this one, unselecting
  what falls outside. The range runs over the rows on screen, so a collapsed folder counts as one row
  (and, on a cascading tree, brings its contents with it).
- **A plain click** is left alone: it expands the row and runs its `OnClick`, so a tree can both drive
  a details pane and carry a selection.

## Filtering

`.Filter(predicate)` is a view over the tree, not a change to it. A row that matches keeps its
subtree as it was; a folder on the way to a match stays visible and is opened so the match can be
seen; everything else is hidden. Items added while a filter is active are filtered as they arrive,
and `.ClearFilter()` shows everything again and closes the folders the filter opened — the user's own
expansion is what comes back. Opening a folder for the filter raises no `OnExpanded`, so code that
persists what the user expanded is not misled. `.Filter("text")` is the common case, a
case-insensitive match on each row's `Text`; an empty text clears the filter.

```csharp
var tree   = new Tree().Compact().Items(...);
var search = SearchBox("Filter...").SearchAsYouType().OnSearch((s, term) => tree.Filter(term));
```

## Example

```csharp
using static Tesserae.UI;

var tree = new Tree().Compact().SelectionEnabled().Items(
    new Tree.Item("src", UIcons.Folder).Expanded().Items(
        new Tree.Item("index.tsx", UIcons.File).Selected(),
        new Tree.Item("Button.tsx", UIcons.File)
    ),
    new Tree.Item("Lazy folder", UIcons.Folder).ItemsAsync(async () =>
    {
        await Task.Delay(500);
        return new[] { new Tree.Item("child.cs", UIcons.File) };
    })
).OnSelected((s, item) => console.log(item.Text));
```

Picking several files out of a folder tree, folders included:

```csharp
var files = new Tree().Compact().Selectable(TreeSelectionMode.Multiple).CascadeSelection().Items(
    new Tree.Item("config", UIcons.Folder).Expanded().Items(
        new Tree.Item("search.cs", UIcons.File),
        new Tree.Item("upload.cs", UIcons.File),
        new Tree.Item("workspace.json", UIcons.File).Selectable(false)   // nothing to do with this one
    )
).OnSelectionChanged((t, selected) =>
{
    var picked = selected.Where(i => !i.HasChildren).ToArray();
    console.log(picked.Length + " files selected");
});
```

## Related

- Components overview — `/tesserae/components/`
- Full docs & API: `/tesserae/components/tree`
