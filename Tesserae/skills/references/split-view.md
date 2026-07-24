---
name: split-view
description: A two-pane container with left/right panels separated by an optionally draggable splitter. Use when building side-by-side resizable layouts in a Tesserae (C#/Transpose) app.
---

# SplitView

A container that arranges two child components side by side, separated by a
splitter. By default the splitter is fixed; call `.Resizable()` to enable
drag-to-resize. Panels can also be pinned to a fixed size on one side.

## Create

`UI.SplitView()` (i.e. `SplitView()`) returns a `SplitView`. Optionally pass a
`UnitSize splitterSize` to the constructor (`new SplitView(8.px())`).
Bring factories into scope with `using static Tesserae.UI;`.

## Key configuration

- `.Left(component, background = "")` / `.Right(component, background = "")` —
  fill the two panes.
- `.Resizable(Action<int> onResizeEnd = null, string backgroundColor = null)` —
  enable the drag handle; callback receives the final pixel width.
- `.NotResizable()` — disable dragging.
- `.LeftIsSmaller(size, maxSize = null, minSize = null)` /
  `.RightIsSmaller(...)` — pin one pane to a fixed/bounded width.
- `.SplitInMiddle()` — reset to a flexible 50/50 split.
- `.Open()` / `.Close()` — show/collapse the pinned smaller pane.
- `.PanelStyle()` — apply the panel visual style.
- Size the whole view with `.W()` / `.H()` (e.g. `.H(60.vh()).W(60.vw())`).

## Example

```csharp
using static Tesserae.UI;

var split = SplitView().H(60.vh()).W(60.vw())
    .LeftIsSmaller(200.px())
    .Left(Stack().Children(TextBlock("Sidebar")).S(), "#e0ffe0")
    .Right(Stack().Children(TextBlock("Content")).S(), "#e0e0ff")
    .Resizable(onResizeEnd: w => console.log($"width={w}px"));
```

## Related

- Stack — `/tesserae/collections/stack`
- Full docs & API: `/tesserae/components/split-view`
