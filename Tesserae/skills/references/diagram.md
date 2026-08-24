---
name: diagram
description: A flow-chart surface of draggable pill-shaped nodes with arrows computed automatically between connected ones, drawn on a pannable dotted-grid canvas. Use to show a pipeline, a graph of bindings or any node-and-arrow flow in a Tesserae (C#/Transpose) app.
---

# Diagram

`Diagram` draws a flow chart: pill-shaped nodes with an optional icon and label, and
arrows between the ones you connect. You declare the *flow* — the layout is computed
from node sizes and connectivity, arranging the nodes in layers that follow the arrows.

The background is a pannable dotted grid; the arrows live on a canvas behind the nodes
and are redrawn as the user pans, drags a node, or the container resizes (via a
`ResizeObserver`).

For an editable node graph with typed inputs and outputs, use `NodeView` instead
(`node-view.md`); for a static two-axis layout, use `Grid`.

## Create

`UI.Diagram()` — the surface. `UI.DiagramNode(string text = "")` — one node.
Bring factories into scope with `using static Tesserae.UI;`.

Give it a height (`.H(300.px())`); it fills the width it is given.

## Key configuration

Diagram:

- `.Connect(Node from, Node to)` — draw an arrow between the two, adding either to the
  diagram if it isn't there yet. This is the usual way to build one: the nodes come along
  with the connections.
- `.Nodes(params Node[])` / `.Add(Node)` / `.Remove(Node)` / `.Clear()` — manage nodes
  directly, for a node that stands on its own.
- `.AutoArrange()` — recompute the layout for **every** node, including ones the user
  dragged or you pinned with `.At(...)`, and re-centre the view.
- `.DotSpacing(int pixels)` (24 by default, floors at 4) / `.NoDots()` — the background grid.
- `.NotDraggable()` — stop the background from panning. Nodes stay draggable.

Node:

- `.SetText(string)` / `.Text` — the label. A node with no text renders as a circle, which
  is what a junction point wants.
- `.SetIcon(UIcons icon, string color = "", TextSize size = Small, UIconsWeight weight = Regular)`
  / `.SetIcon(Emoji, TextSize)` / `.SetImage(string url)` / `.ClearIcon()` — the mark before
  the label.
- `.Default()` / `.Primary()` / `.Secondary()` / `.Success()` / `.Danger()` — the same tones
  a `Button` has.
- `.Color(string background, string foreground = "", string borderColor = "")` — anything the
  tones don't cover; `background` takes a gradient as happily as a colour.
- `.At(double x, double y)` — pin the node at that position, so the automatic layout leaves
  it alone. Dragging a node pins it the same way.
- `.OnClick(...)` / `.OnContextMenu(...)` — both take `Action` or a `(node, mouseEvent)`
  handler. A click is not raised after a drag, so moving a node never also activates it.
- `.Background` / `.Foreground` — CSS colours of the node itself.

## Example

```csharp
using static Tesserae.UI;

var source = DiagramNode("Source").SetIcon(UIcons.CloudDownloadAlt);
var parse  = DiagramNode("Parse").SetIcon(UIcons.FileCode).Secondary();
var enrich = DiagramNode("Enrich").SetIcon(UIcons.Sparkles).Primary();
var index  = DiagramNode("Search index").SetIcon(UIcons.Search).Success();
var graph  = DiagramNode("Graph").SetIcon(UIcons.ChartNetwork).Danger();

// An icon-only node draws as a circle - a junction rather than a step.
var fanIn  = DiagramNode().SetIcon(UIcons.Bolt, color: "white")
                          .Color("linear-gradient(135deg, #6a11cb, #2575fc)", "white", "transparent");

var pipeline = Diagram()
    .Connect(source, parse)
    .Connect(parse,  enrich)
    .Connect(enrich, index)
    .Connect(enrich, graph)
    .Connect(fanIn,  parse)
    .WS().H(420.px());

// Pinned, and answering clicks.
var entry = DiagramNode("Entry point").SetIcon(UIcons.CursorFinger).Primary()
    .At(260, 40)
    .OnClick(() => Toast().Information("Entry point"));
```

## Related

- NodeView — an editable node graph with typed interfaces — `node-view.md`
- Tree — hierarchy rather than flow — `tree.md`
- Plan — a task list with progress, when the steps are sequential — `plan.md`
- Grid — for a layout rather than a graph — `grid.md`
- Full docs & API: `/tesserae/components/diagram`
