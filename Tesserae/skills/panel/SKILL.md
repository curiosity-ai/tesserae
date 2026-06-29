---
name: panel
description: A side-anchored slide-in drawer with a title, content, footer and configurable size/side, light-dismiss and blocking modes. Use for property inspectors, detail views and side forms in a Tesserae (C#/h5) app.
---

# Panel

A drawer that slides in from one side of the screen. Has a title bar with close
button, a content area and a footer, with several preset sizes and a near/far
side.

## Create

`Panel(string title = null)` or `Panel(IComponent title)`. Bring factories into
scope with `using static Tesserae.UI;`.

## Key configuration

- `.Content(IComponent)` (also a settable property) / `.SetFooter(IComponent)` (or `.Footer` property).
- Sizes: `.Small()`, `.Medium()`, `.Large()`, `.LargeFixed()`, `.ExtraLarge()`, `.FullWidth()` (or the `.Size` property with `PanelSize`).
- Side: `.Far()` / `.Near()` (or `.Side` with `PanelSide`).
- `.LightDismiss()` / `.NoLightDismiss()` — click-outside to close.
- `.Blocking()` / `.NonBlocking()` — block or allow page interaction.
- `.Dark()` / `.HideCloseButton()`.
- `.Show()` / `.Hide(Action onHidden = null)` / `.OnHide(...)`.

## Example

```csharp
using static Tesserae.UI;

Panel panel = null;
panel = Panel("Details").LightDismiss()
    .Content(Stack().Children(TextBlock("Panel content.")))
    .SetFooter(HStack().Children(
        Button("Cancel").OnClick((s, e) => panel.Hide()),
        Button("Save").Primary().OnClick((s, e) => panel.Hide())
    ));
panel.Medium().Far();
panel.Show();
```

## Related

- Modal — `../modal/SKILL.md`
- Dialog — `../dialog/SKILL.md`
- Full docs & API: `/tesserae/surfaces/panel`
