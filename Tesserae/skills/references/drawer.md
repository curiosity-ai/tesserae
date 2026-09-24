---
name: drawer
description: A bottom sheet that slides up over a dimmed page, with a grab handle, title, close button, scrolling content and footer, dismissed by dragging it down or tapping outside. Use for lists and short tasks on phones in a Tesserae (C#/Transpose) app, or to show a Modal as a sheet on mobile.
---

# Drawer

A sheet that slides up from the bottom of the screen - the phone's answer to a
`Panel` or a `Modal`. It is as tall as its content, up to 85% of the screen, and
slides back down when hidden. On a screen wider than 768px it stays a 640px column
in the middle rather than spanning the window.

## Create

`Drawer(string title = null)` or `Drawer(IComponent title)`. Bring factories into
scope with `using static Tesserae.UI;`.

## Key configuration

- `.Content(IComponent)` (also a property) / `.SetFooter(IComponent)` (or `.Footer`).
- `.SetTitle(string | IComponent)` — the title beside the close button.
- `.MaxHeight(UnitSize)` — cap the height (default 85% of the screen);
  `.FullHeight()` — always that tall, for content that grows while open.
- `.LightDismiss()` / `.NoLightDismiss()` — tap outside to close (on by default).
- `.NoDragToDismiss()` / `.CanDragToDismiss` — pulling the handle down closes it (on by default).
- `.HideCloseButton()`, `.NoHeader()` (handle only), `.NoContentPadding()`.
- `.Show()` / `.Hide(Action onHidden = null)` / `.OnHide(...)`; two-way bindable
  (`IBindableComponent<bool>`), like `Panel`.

## A Modal that becomes a drawer on mobile

`Modal.DrawerOnMobile()` (see `modal.md`) shows the same modal inside a drawer while
`Theme.IsMobileMode` is on, so an existing modal does not need a second code path.

## Example

```csharp
using static Tesserae.UI;

var drawer = Drawer("Used 12 tools")
   .Content(VStack().WS().Children(rows))
   .SetFooter(HStack().Children(Button("Done").Primary()));

Button("Show tools").OnClick(() => drawer.Show());
```

## Related

`panel.md` (the same idea from a side), `modal.md` (`DrawerOnMobile`), `tool-call.md`
(`ToolsUsed` lists its calls in a drawer on mobile).
