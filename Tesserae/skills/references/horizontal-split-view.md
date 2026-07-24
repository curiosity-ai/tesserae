---
name: horizontal-split-view
description: A two-pane container split into top and bottom areas by a thin divider that can be made draggable. Use for stacked editor/preview or header/body layouts in a Tesserae (C#/Transpose) app.
---

# HorizontalSplitView

Divides a container into a top pane and a bottom pane separated by a splitter
bar. The divider can be made draggable to redistribute space at runtime. (For a
left/right split, use `SplitView`.)

## Create

`HorizontalSplitView(UnitSize splitterSize = null)` — returns a
`HorizontalSplitView` (splitter defaults to 8px). Bring factories into scope
with `using static Tesserae.UI;`.

## Key configuration

- `.Top(IComponent, background = "")` / `.Bottom(IComponent, background = "")` — set the two panes.
- `.Resizable(Action<int> onResizeEnd = null)` — enable the drag handle; callback gets the new pane height in px.
- `.NotResizable()` — disable dragging.
- `.SplitInMiddle()` — default 50/50 split.
- `.TopIsSmaller(size, maxTop = null, minTop = null)` / `.BottomIsSmaller(size, maxBottom = null, minBottom = null)` — pin one pane to a fixed size; the other grows.
- `.Open()` / `.Close()` — show/collapse the smaller pane (only valid after `TopIsSmaller`/`BottomIsSmaller`).
- `.PanelStyle()` — panel-style border treatment.
- Combine with sizing helpers like `.WS()` and `.H(...)` for overall dimensions.

## Example

```csharp
using static Tesserae.UI;

var view = HorizontalSplitView()
    .Top(Stack().S().Children(TextBlock("Editor")))
    .Bottom(Stack().S().Children(TextBlock("Preview")))
    .Resizable(h => Toast().Information($"Top is now {h}px"))
    .WS().H(300);
```

## Related

- Full docs & API: `/tesserae/surfaces/horizontal-split-view`
- SplitView (left/right variant) — `/tesserae/surfaces/`
