---
name: tutorial-modal
description: A split-layout modal with an explanation/illustration column on the left and a content/form column on the right, plus footer commands. Use for onboarding, guided steps or multi-field intro forms in a Tesserae (C#/h5) app.
---

# TutorialModal

A `Modal` with a two-column layout: a left panel for a title, help text and an
illustration, and a right panel for content (often an input form) and footer
commands.

## Create

`TutorialModal(string title, string helpText)` (or `TutorialModal()`). Bring
factories into scope with `using static Tesserae.UI;`.

## Key configuration

- `.SetTitle(string)` / `.SetHelpText(string, treatAsHTML = false)` — left-column text.
- `.SetImageSrc(string imageSrc, UnitSize padding)` / `.SetImage(Image, UnitSize padding)` — left-column illustration.
- `.SetContent(IComponent)` — right-column body.
- `.SetFooterCommands(params IComponent[])` — footer buttons.
- `.Width(UnitSize)` / `.W(...)` and `.Height(UnitSize)` / `.H(...)` — sizing (defaults 800x500px).
- `.LightDismiss()` / `.NoLightDismiss()`, `.ContentPadding(string)`, `.Border(color, size = null)`.
- `.Show()` / `.Hide(Action onHidden = null)` / `.OnShow(...)` / `.OnHide(...)` / `.ShowEmbedded()`.

## Example

```csharp
using static Tesserae.UI;

TutorialModal modal = null;
modal = TutorialModal("Welcome", "Enter your details to proceed")
    .SetImageSrc("./assets/img/tutorial.svg", 16.px())
    .SetContent(VStack().S().ScrollY().Children(
        Label("Username").SetContent(TextBox().SetPlaceholder("Username")),
        Label("Password").SetContent(TextBox().SetPlaceholder("Password"))))
    .SetFooterCommands(
        Button("Cancel").OnClick((s, e) => modal.Hide()),
        Button("Submit").Primary().OnClick((s, e) => modal.Hide()))
    .Width(800.px()).Height(500.px());
modal.Show();
```

## Related

- Modal — `../modal/SKILL.md` (underlying surface)
- Dialog — `../dialog/SKILL.md`
- Panel — `../panel/SKILL.md`
- Full docs & API: `/tesserae/surfaces/tutorial-modal`
