---
name: modal
description: A centered overlay surface that dims the page and hosts arbitrary content, with optional header, footer, command rows, blocking/non-blocking and light-dismiss modes. Use for dialogs, forms and detail overlays in a Tesserae (C#/h5) app.
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

- Dialog — `../dialog/SKILL.md` (prebuilt confirmation buttons)
- Panel — `../panel/SKILL.md` (side drawer)
- Float — `../float/SKILL.md`
- Full docs & API: `/tesserae/surfaces/modal`
