---
name: float
description: An absolute-positioned overlay that anchors a child to a corner or edge of a relative-positioned parent. Use when overlaying a small element (badge, button, label) at a fixed spot inside a container in a Tesserae (C#/Transpose) app.
---

# Float

Positions a single child at one of nine anchor points within its parent. The
parent must establish a positioning context — call `.Relative()` on the
containing `Stack` or `Grid`.

## Create

`Float(IComponent child, Float.Position position)` — returns a `Float`. Bring
factories into scope with `using static Tesserae.UI;`.

## Key configuration

- `Float.Position` values: `TopLeft`, `TopMiddle`, `TopRight`, `LeftCenter`, `Center`, `RightCenter`, `BottomLeft`, `BottonMiddle` (note spelling), `BottomRight`.
- The parent container must be `.Relative()` for the anchor to position correctly.

## Example

```csharp
using static Tesserae.UI;

var area = Stack().Relative().WidthStretch().Height(400.px()).Children(
    Float(Button("Top-left"),     Float.Position.TopLeft),
    Float(Button("Centered"),     Float.Position.Center),
    Float(Button("Bottom-right"), Float.Position.BottomRight)
);
```

## Related

- Panel — `panel.md`
- Modal — `modal.md`
- Full docs & API: `/tesserae/surfaces/float`
