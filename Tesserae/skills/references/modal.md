---
name: modal
description: A centered overlay surface that dims the page and hosts arbitrary content, with optional header, footer, command rows, blocking/non-blocking and light-dismiss modes. Use for dialogs, forms and detail overlays in a Tesserae (C#/Transpose) app.
---

# Modal

A centered overlay window over a dimmed page. Supports a header, footer, command
rows, dragging, dark mode, and blocking or non-blocking interaction. It backs
higher-level surfaces like `Dialog`, `ProgressModal` and `TutorialModal`.

## Create

`Modal(IComponent header = null)` or `Modal(string header)`. Bring factories
into scope with `using static Tesserae.UI;`.

## Key configuration

- `.Content` (property) / `.SetHeader(IComponent)` / `.SetFooter(IComponent)` — content regions.
- `.SetHeaderCommands(...)` / `.SetFooterCommands(...)` / `.SetLeftHeaderCommands(...)` / `.SetLeftFooterCommands(...)` — command buttons.
- `.Width(UnitSize)` / `.Height(UnitSize)` / `.ContentHeight(UnitSize)` — sizing.
- `.LightDismiss()` / `.NoLightDismiss()` — click-outside (and Esc) to close.
- `.Blocking()` / `.NonBlocking()` — block or allow interaction with the page beneath.
- `.CenterContent()`, `.NoPadding()`, `.NoContentPadding()`, `.NoHeader()`, `.NoFooter()`, `.NoAnimation()` — layout tweaks.
- `.Draggable()` / `.Dark()` / `.ShowCloseButton()` / `.HideCloseButton()`.
- `.Show()`, `.ShowAt(fromTop, fromLeft, fromRight, fromBottom)`, `.ShowAsync()` (Task), `.ShowEmbedded()` (return as an embeddable `IComponent`).
- `.Hide(Action onHidden = null)`, `.OnShow(...)`, `.OnHide(...)`.
- `.DrawerOnMobile(bool = true)` — while `Theme.IsMobileMode` is on, show this same modal
  (header, commands, footer, close button) in a `Drawer` sliding up from the bottom
  instead of centred on the page. Decided at every `Show()`; `OnShow` / `OnHide` and
  the bound value fire as usual, `.IsShowingInDrawer` says which one is up (`IsVisible`
  is only ever the dialog). See `drawer.md`.
- `.WithPixelAvatar(avatar | key + design, anchor)` — perch an animated pixel cat on one of the modal's own edges and get the modal back to go on configuring. See `pixel-avatar.md`.

## Example

```csharp
using static Tesserae.UI;

Modal modal = Modal(TextBlock("Sample Modal"))
    .LightDismiss()
    .Width(50.vw())
    .Height(50.vh())
    .SetFooter(TextBlock("Footer"))
    .CenterContent();
modal.Show();
```

## Related

- ModalStack — `modal-stack.md` (a deck of modals, when one opens another)
- Dialog — `dialog.md` (prebuilt confirmation buttons)
- Panel — `panel.md` (side drawer)
- ShortcutGuide — `shortcut-guide.md` (a modal listing the app's keyboard shortcuts)
- Float — `float.md`
- PixelAvatar — `pixel-avatar.md` (a cat perched on the modal's top edge)
- Full docs & API: `/tesserae/surfaces/modal`
