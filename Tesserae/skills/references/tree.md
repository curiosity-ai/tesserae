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
- `.Compact(bool = true)` — compact density (22px rows, 13px text, 8px indent), matching a code editor's file explorer. On a phone (`body.tss-mobile`) every row is a 44px touch target whatever the density, so a compact tree is only compact on a desktop.
- `.OnSelected((s, item) => ...)` — fires when the selected item changes; `SelectedItem` holds the last one picked.
- `.OnSelectionChanged((s, items) => ...)` — fires with everything selected; one call per gesture, even when a range or a cascade moved many rows.
- `.ClearSelection()` / `.SelectAll()` — move the whole selection from code; `SelectedItems` reads it back, in tree order.
- `.Clear()` / `.Replace(newItem, oldItem)` — manage nodes.
- `.FocusItem(item)` — move keyboard focus to a row, opening the folders above it.
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

## Keyboard

The tree follows the WAI-ARIA tree pattern, with nothing to configure. The tree is one tab stop: Tab
lands on the row that last had focus (else the selected row, else the first), and a row reached from
the keyboard shows its commands, so Tab goes on to them.

- **Up / Down** — previous / next row on screen (collapsed and filtered-out rows are skipped).
- **Home / End** — first / last row.
- **Right** — opens a closed folder (an `ItemsAsync` one starts loading); on an open folder, moves to its first child.
- **Left** — closes an open folder; otherwise moves to the parent.
- **Enter** — what a plain click on the row does: opens or closes it and runs its `OnClick`.
- **Space** — toggles the focused row's selection (`Single` or `Multiple`); **Shift+Space** selects the range up to it in `Multiple`.
- **Shift+Up / Shift+Down** (and **Ctrl+Shift+Home / End**) — move and extend a `Multiple` selection from the anchor, like a shift-click.
- **Ctrl (or cmd) + A** — `SelectAll()` in `Multiple`.
- **\*** — opens every sibling of the focused row.
- **Typing** — focuses the next row whose text starts with the letters typed.

Moving focus never changes the selection by itself. `tree.FocusItem(item)` moves focus from code,
opening the folders above the item.

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
