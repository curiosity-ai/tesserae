---
name: layer
description: A technical container that renders its content at the end of the document so it escapes overflow:hidden and z-index stacking. Use as the projection primitive behind menus, tooltips and overlays in a Tesserae (C#/Transpose) app.
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

## Stacking

`Layers` hands out the z-indices. A custom overlay appended to the body yourself takes one with
`Layers.PushLayer(element)`; `Layers.AboveCurrent()` is the value to sit just above whatever is
showing (that is what popovers use). `Layers.PushAlwaysOnTop(element)` is for page chrome that lives
outside the body box — an edge-to-edge `Toast().Banner()` — and keeps it above layers pushed later.
Don't invent a z-index of your own.

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

## Blocking the page behind a layer

A layer that blocks the page overrides `LocksPageScroll` (`protected virtual bool`, `false` by
default) and the base class stops the page behind it scrolling for as long as it is shown:

```csharp
protected override bool LocksPageScroll => !IsNonBlocking;   // what Modal and Panel do
```

`false` is the default because most layers are not blocking — a dropdown, a context menu, a toast
or a picker's suggestion list must leave the page scrollable. Call `UpdatePageScrollLock()` if a
property of yours changes the answer while the layer is already open.

**Never write `document.body.style.overflow` from a component.** `Layers` owns the lock: it
remembers whatever the application had on the body and restores exactly that once the last locking
layer has gone, so several overlays can be open at once, in any closing order, and an app shell
that declares "the body never scrolls" keeps that declaration.

## Related

- Modal — `modal.md`
- Dialog — `dialog.md`
- Full docs & API: `/tesserae/surfaces/layer`
