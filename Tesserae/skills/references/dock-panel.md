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
- `.Show(id)` brings a pane into view whatever its state (opens it, selects its tab, slides it out from its rail);
  `.Toggle(id)` shows it, or hides it if it is in view; `IsShown(id)` says whether it can be seen right now.
- **The bar along the top:** `.Title(component | string)` (left), `.PaneToggles()` (a toggle per pane, after the
  title, kept in step with the panes; hides the restore menu) and `.HeaderCommands(params IComponent[])` (right).
  The bar is only drawn while it has one of the three. Use it when the panel fills a page or a tab, so the name of
  what is open, the panes and the save/close commands share one line:

  ```csharp
  DockPanel().Document(editor).Panes(settings, output)
     .Title("Program.cs")
     .PaneToggles()
     .HeaderCommands(Button("Save").SetIcon(UIcons.Disk), Button().SetIcon(UIcons.Cross));
  ```
- `PaneToggle(id)` returns a `Button` (pane icon + title) for a toolbar outside the panel that toggles the pane and
  stays pressed (`aria-pressed`, `tss-dockpanel-toggle-on`) while it is in view. Start secondary panes `.Closed()`,
  hide the restore menu with `.RestoreMenu(false)`, and put the toggles in the app's own header:

  ```csharp
  var dock = DockPanel().Document(editor).Panes(
      DockPane("settings", "Settings", settings).Left(),
      DockPane("output", "Output", output).Bottom().Closed()).RestoreMenu(false);
  var bar = HStack().Children(dock.CurrentPanes.Select(p => (IComponent)dock.PaneToggle(p.Id).Compact().NoBackground()).ToArray());
  ```
- `.GetLayout()` → JSON; `.LoadLayout(json)` → bool (unknown ids ignored, disallowed edges ignored,
  sizes clamped).
- `.OnLayoutChanged(Action<string>)` (after every change, with the JSON), `.OnCloseRequested(Func<DockPane, bool>)`
  (return false to keep it open), `.OnFlyoutOpened(Action<DockPane>)`.
- `.CollapseBelow(px)` (0 = off), `IsNarrow`, `.RestoreMenu(false)`, `.Labels(new DockPanelLabels { … })`
  (it also holds the keyboard texts: `Resize`, `And`, `MovePane`, `MoveLeft/Right/Top/Bottom`, `TabWith`).
- `.KeyboardResize(bool = true)` and `.KeyboardMove(bool = true)` — the keyboard support below is on by default; pass
  `false` to leave splitters (or moving panes) to the pointer. Hosts that used to make splitters focusable themselves
  (a MutationObserver adding `tabindex`) should drop that code.

## Keyboard

- **Splitters** (between regions, between stacked panes, and a slid-out flyout's) are tab stops with
  `role="separator"`, `aria-orientation`, `aria-valuenow/min/max` (+ `aria-valuetext`) and a name ("Resize Explorer",
  "Resize Explorer and Outline"). Arrow keys along the splitter's axis resize by 16 px (Shift: 64 px), Home / End go to the
  smallest / largest, Escape moves the focus into the document. A region's values are pixels; the split between two
  stacked panes is a percentage (the first pane's share). The size is applied in place (nothing is rebuilt), so the focus
  stays and a held key keeps resizing; `GetLayout()` is current at once, and `OnLayoutChanged` fires once the keys rest
  (300 ms), on key up or when the focus leaves. A rebuild (a move, a tab switch) puts the focus back on the same splitter.
  There is no collapse of a region, so Enter / Space do nothing.
- **Tabs** are `role="tab"` in a `tablist` with a roving tabindex; Left / Right switch (wrapping), Home / End go to the
  first / last, and the selected tab has `aria-controls` the `tabpanel` (which is `aria-labelledby` the tab).
- **Moving a pane** (unless `NotMovable()`): a single pane's header, and any tab, take the focus. Alt+Shift+Arrow docks the
  pane to the edge in that direction (out of its tab group if it is on that edge already, honouring `AllowEdges`);
  Enter / Space on a header, the context-menu key or Shift+F10 on a header or tab opens a menu: Move to left / right / top /
  bottom, and "Tab with <pane>" for every other slot. The focus follows the pane. Pointer and touch behaviour is unchanged.

The layout is rebuilt on every change by moving each pane's content element (not recreating it), so
content keeps its state; each pane body's scroll position is carried across.
A pane that is not on screen (closed, on its rail) keeps its content mounted in a hidden holder inside
the panel, so `WhenMounted`/`WhenRemoved`-scoped work in it (socket subscriptions, validators, editors)
keeps running and the pane comes back exactly as it was left.

## Related

`split-view.md`, `horizontal-split-view.md`, `grid.md`, `panel.md`, `pivot.md`.
- Full docs & API: `/tesserae/layout/dock-panel`
