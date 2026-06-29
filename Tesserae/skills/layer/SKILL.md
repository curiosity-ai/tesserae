---
name: layer
description: A technical container that renders its content at the end of the document so it escapes overflow:hidden and z-index stacking. Use as the projection primitive behind menus, tooltips and overlays in a Tesserae (C#/h5) app.
---

# Layer

Renders content outside the normal DOM tree (appended at the end of the
document) so it always overlays the rest of the UI without z-index tricks. It's
the base used by surfaces like `Modal`, `Panel` and `ContextMenu`; reach for it
directly only when building a custom overlay.

## Create

`Layer()` — returns a non-derivable `Layer`. `LayerHost()` creates an optional
host to confine layers to a sub-tree. Bring factories into scope with
`using static Tesserae.UI;`.

## Key configuration

- `.Content(IComponent)` — set the projected content.
- `.IsVisible` — get/set visibility.
- `.Host` — assign a `LayerHost` to control where content projects.

## Example

```csharp
using static Tesserae.UI;

var layer = Layer();
layer.Content(
    HStack().Children(
        TextBlock("Rendered in a Layer."),
        Button("Toggle").Primary().OnClick((s, e) => layer.IsVisible = !layer.IsVisible)
    )
);
layer.IsVisible = true;
```

## Related

- Modal — `../modal/SKILL.md`
- Dialog — `../dialog/SKILL.md`
- Full docs & API: `/tesserae/surfaces/layer`
