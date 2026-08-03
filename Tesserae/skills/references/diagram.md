---
name: diagram
description: A pannable dotted canvas of draggable boxes joined by auto-routed arrows, with automatic layout. Use for flow charts, pipelines and small graphs in a Tesserae (C#/Transpose) app.
---

# Diagram

`Diagram` is a pannable dotted canvas holding boxes you connect with `Connect(from, to)`. The
arrows are computed and drawn on a background canvas, so they follow the nodes as they are
dragged. `AutoArrange()` positions the nodes from their sizes and how they are connected, which
is usually enough for a graph you did not lay out by hand.

## Create

`UI.Diagram()` — the canvas.
`UI.DiagramNode(string text = "")` — a box to put on it (`Diagram.Node`).
Bring factories into scope with `using static Tesserae.UI;`.

## Key configuration

Diagram:

- `.Nodes(params Node[])` — set the nodes (replaces the current set); `.Add(node)` /
  `.Remove(node)` / `.Clear()` change it afterwards. Removing a node removes the arrows into and
  out of it.
- `.Connect(from, to)` — draw an arrow between two nodes.
- `.AutoArrange()` — lay the nodes out from their sizes and connections.
- `.DotSpacing(int pixels)` / `.NoDots()` — how dense the background grid is, or no grid at all.
- `.NotDraggable()` — pin the nodes where they are, for a diagram meant to be read rather than
  rearranged.

Diagram.Node:

- `.SetText(string)`, `Text`, `Background`, `Foreground`.
- `.SetIcon(UIcons …)` / `.SetIcon(Emoji …)` / `.SetImage(string source)` / `.ClearIcon()`.
- `.Default()` / `.Primary()` / `.Secondary()` — the box's tone.

## Example

```csharp
using static Tesserae.UI;

var ingest = DiagramNode("Ingest");
var enrich = DiagramNode("Enrich").Primary();
var index  = DiagramNode("Index");
var search = DiagramNode("Search").SetIcon(UIcons.Search);

var pipeline = Diagram()
   .Nodes(ingest, enrich, index, search)
   .Connect(ingest, enrich)
   .Connect(enrich, index)
   .Connect(index, search)
   .AutoArrange()
   .S();
```

## Related

- `Diagram` is for a handful of labelled boxes; a large data graph wants a graph view of its own.
- Card — a box with content rather than a label — `card.md`
- TaskBoard — draggable cards in columns — `task-board.md`
- Full docs & API: `/tesserae/components/diagram`
