---
name: node-view
description: An interactive node-graph editor (wraps BaklavaJS) with typed inputs/outputs, static and dynamic nodes, and save/load state. Use when building flow/graph editors in a Tesserae (C#/h5) app.
---

# NodeView

A visual node-based editor. Define node types with typed input/output interfaces, listen for graph changes, and serialize/restore the graph as JSON. Built on BaklavaJS, so it initializes after the component mounts (calls before mount are queued).

## Create

`UI.NodeView(Action<NodeView.IViewSettings> settings = null)` — returns a `NodeView`. Usually size it to fill: `NodeView().S()`. Bring factories into scope with `using static Tesserae.UI;`.

## Key configuration

- `.DefineNode(typeName, ib => …)` — static node; use `ib.AddInput(name, () => …)` / `ib.AddOutput(name, () => …)`.
- `.DefineDynamicNode(typeName, buildBase, (inputs, outputs, ib) => …)` — outputs/inputs recomputed from current values.
- `.Register<T>()` — register a node type implementing `INodeView` (`IDynamicNodeView` for dynamic).
- `.OnChange(Action<NodeView>)` — fires (debounced) on graph edits.
- `.GetState()` / `.GetJsonState(bool formatted = false)` / `.SetState(json or NodeViewGraphState)` — persistence.
- `NodeView.State()` — a `StateBuilder` to construct graphs programmatically.

Interface builders (`NodeView.Interfaces`): `TextInterface`, `TextInputInterface`, `IntegerInterface`, `NumberInterface`, `CheckboxInterface`, `SelectInterface`, `SliderInterface`, `ButtonInterface`.

## Example

```csharp
using static Tesserae.UI;

var nodeView = NodeView().S();

nodeView.DefineNode("SimpleNode", ib =>
{
    ib.AddInput("inp",  () => NodeView.Interfaces.TextInputInterface("Input", "Initial value"));
    ib.AddOutput("out", () => NodeView.Interfaces.TextInterface("Output", "Result"));
});

nodeView.OnChange(nv => console.log(nv.GetJsonState(true)));
```

## Related

- SplitView (common layout for an editor + JSON pane) — `/tesserae/layout/split-view`
- Full docs & API: `/tesserae/utilities/node-view`
