---
name: background-area
description: A full-bleed themed container that hosts a single centered child, used as an app shell for splash, sign-in, or onboarding surfaces. Use when you need a full-screen background behind a centered card in a Tesserae (C#/h5) app.
---

# BackgroundArea

`BackgroundArea` fills its parent with a themed background and centers one child on top.
For the common "centered card on a background" shell, use the `CenteredCardWithBackground`
helper, which wraps the content in a `Card` for you.

## Create

`UI.BackgroundArea(IComponent content)` — bare background container.
`UI.CenteredCardWithBackground(IComponent content)` — content wrapped in a centered card.
Related helpers: `UI.CenteredWithBackground(content)`,
`UI.ZeroPaddingCenteredCardWithBackground(content, int outerPadding = 32)`.
Bring factories into scope with `using static Tesserae.UI;`.

## Key configuration

- `.Content(IComponent)` — replace the hosted content.
- `.Background` — CSS background color of the area.
- Combine with sizing helpers like `.S()` (the helpers above already call `.S()`).

## Example

```csharp
using static Tesserae.UI;

var shell = CenteredCardWithBackground(
    Stack().Children(
        TextBlock("Welcome").XLarge().SemiBold(),
        TextBlock("Sign in to continue.").Small()
    ).Padding(24.px()));

document.body.appendChild(shell.Render());
```

## Related

- Card — `../card/SKILL.md`
- Full docs & API: `/tesserae/components/background-area`
