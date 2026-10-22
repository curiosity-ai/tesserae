---
name: flip-card
description: A card with two faces that turns over in 3D to show the other one. Use for a summary and its details, or a question and its answer, in a Tesserae (C#/Transpose) app.
---

# FlipCard

Holds two components and turns over in 3D to show the other. Both faces share
one grid cell, so the card is as tall as its taller face and nothing around it
moves when it flips. The face turned away is `aria-hidden` and `inert` (out of
the tab order). Under reduced motion the faces crossfade.

## Create

`UI.FlipCard(IComponent front, IComponent back)` returns a `FlipCard`. Size it
with the usual helpers (`.W(260)`, `.WS()`).

## Key configuration

- `.Flip()` / `.Flip(bool showBack)` / `.IsFlipped` — turn it from code.
- `.FlipOnClick(bool = true)` — turn on a click anywhere on it (not on a button, link or field inside), and on Enter/Space; the card becomes focusable with `role=button` and `aria-pressed`.
- `.Vertical(bool = true)` — turn top over bottom.
- `.OnFlip(Action<bool>)` — runs when it turns, with whether the back is showing.

## Example

```csharp
using static Tesserae.UI;

FlipCard(
    VStack().Children(TextBlock("Team plan").SemiBold(), TextBlock("$24 per seat").Secondary()),
    VStack().Children(TextBlock("Included").SemiBold(), TextBlock("Single sign-on")))
   .FlipOnClick().W(260);
```

## Related

- Card — `card.md`
- Accordion / expander, for details that push content down — `accordion.md`, `expander.md`
- Full docs & API: `/tesserae/layout/flip-card`
