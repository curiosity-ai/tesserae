---
name: tree
description: A hierarchical tree view with expand/collapse, selection, keyboard nav, and sync or async child loading. Use when displaying nested/hierarchical data (file trees, org charts) in a Tesserae (C#/h5) app.
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
- `.SelectionEnabled(bool = true)` — turn on item selection.
- `.OnSelected((s, item) => ...)` — fires when selection changes; `SelectedItem` holds the current.
- `.Clear()` / `.Replace(newItem, oldItem)` — manage nodes.

`Tree.Item`:

- `.Items(params Tree.Item[])` — add children.
- `.ItemsAsync(async () => Tree.Item[])` — lazy-load children on first expand (shows a spinner).
- `.Expanded(bool = true)` / `.Selected(bool = true)` — initial state.
- `.OnSelected(...)` / `.OnExpanded(...)` / `.OnCollapsed(...)` — node events.
- `.CommandsAlwaysVisible(bool)` — keep row commands visible (not hover-only).
- `Text`, `Icon`, `IsExpanded`, `IsSelected`, `HasChildren` — read/write state.

## Example

```csharp
using static Tesserae.UI;

var tree = new Tree().SelectionEnabled().Items(
    new Tree.Item("src", UIcons.Folder).Expanded().Items(
        new Tree.Item("index.tsx", UIcons.File).Selected(),
        new Tree.Item("Button.tsx", UIcons.File)
    ),
    new Tree.Item("Lazy folder", UIcons.Folder).ItemsAsync(async () =>
    {
        await Task.Delay(500);
        return new[] { new Tree.Item("child.cs", UIcons.File) };
    })
).OnSelected((s, item) => Console.WriteLine(item.Text));
```

## Related

- Components overview — `/tesserae/components/`
- Full docs & API: `/tesserae/components/tree`
