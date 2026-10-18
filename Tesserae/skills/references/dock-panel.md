---
name: dock-panel
description: DockPanel and DockPane — an IDE-style docking layout of a document with panes docked to its four edges, tab groups, splitters, auto-hide rails with slide-out flyouts, close/restore, drag-to-redock with drop guides, and layout save/restore as JSON. Use when building a workspace with tool panes around a main area in a Tesserae (C#/Transpose) app.
---

# DockPanel

One document in the middle; `DockPane`s docked to its left, right, top and bottom. Panes on one edge
stack (splitter between); panes on one edge sharing a tab group become tabs of one slot. Every seam is
a splitter. The header's pin sends a pane to a rail on its edge (it slides out over the document when
its rail tab is clicked); the cross closes it and the menu at the document's top right reopens it.
Dragging a header or tab shows markers on the borders (dock to that edge) and a compass over the pane
under the pointer (join as a tab, or go before/after it). Below `CollapseBelow` (600px) every pane goes
to its rail.

## Create

```csharp
var dock = DockPanel()
    .Document(editor)
    .Panes(
        DockPane("explorer", "Explorer", files).Icon(UIcons.Folder).Left().SetSize(240).InTabGroup("left"),
        DockPane("search",   "Search",   search).Icon(UIcons.Search).Left().InTabGroup("left"),
        DockPane("props",    "Properties", props).Right(),
        DockPane("toolbox",  "Toolbox",  toolbox).Right().Unpinned(),
        DockPane("output",   "Output",   output).Bottom().SetSize(160))
    .OnLayoutChanged(json => localStorage.setItem("layout", json))
    .H(600);

var saved = localStorage.getItem("layout");
if (saved is object) dock.LoadLayout(saved);
```

Give the panel a height: it fills the box it is given.

## DockPane (`UI.DockPane(id, title, content)`)

- Id: the stable identity a saved layout refers to.
- Placement: `.Left()` (default) / `.Right()` / `.Top()` / `.Bottom()` / `.DockTo(DockEdge)`,
  `.InTabGroup(name)`, `.SetSize(px)` (width on the sides, height top/bottom; an edge takes its first
  pane's size, default 260), `.SizeLimits(min, max)`, `.SetWeight(w)` (share when stacked).
- State: `.Unpinned()`, `.Closed()`.
- Capabilities: `.NotClosable()`, `.NotPinnable()`, `.NotMovable()`, `.AllowEdges(params DockEdge[])`.
- `.Icon(UIcons)`, `.SetTitle(...)`, `.SetContent(...)`; read back `Edge`, `TabGroup`, `Size`, `IsOpen`, `IsPinned`.

## DockPanel

- `.Document(IComponent)`, `.Panes(...)`, `.Add(pane)`, `.Remove(id)`, `.Pane(id)`, `CurrentPanes`.
- `.EdgePriority(params DockEdge[])` — who owns the corners; default Top, Bottom, Left, Right (top and
  bottom span the full width).
- Commands: `.Open(id)`, `.Close(id)`, `.Pin(id)`, `.Unpin(id)`, `.Activate(id)`, `.Move(id, edge)`, `.ShowFlyout(id)`.
- `.GetLayout()` → JSON; `.LoadLayout(json)` → bool (unknown ids ignored, disallowed edges ignored,
  sizes clamped).
- `.OnLayoutChanged(Action<string>)` (after every change, with the JSON), `.OnCloseRequested(Func<DockPane, bool>)`
  (return false to keep it open), `.OnFlyoutOpened(Action<DockPane>)`.
- `.CollapseBelow(px)` (0 = off), `IsNarrow`, `.RestoreMenu(false)`, `.Labels(new DockPanelLabels { … })`.

The layout is rebuilt on every change by moving each pane's content element (not recreating it), so
content keeps its state; each pane body's scroll position is carried across.

## Related

`split-view.md`, `horizontal-split-view.md`, `grid.md`, `panel.md`, `pivot.md`.
- Full docs & API: `/tesserae/layout/dock-panel`
