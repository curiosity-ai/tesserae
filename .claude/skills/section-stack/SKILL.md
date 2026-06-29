---
name: section-stack
description: A vertical Stack that adds sections and titles with staggered mount animations and optional card styling. Use to present grouped, animated content blocks in a Tesserae (C#/h5) app.
---

# SectionStack

A vertical `Stack` subclass that lays out content as distinct sections, each
animating in with a staggered transition. Sections can be card-styled.

## Create

`SectionStack()` — returns a `SectionStack`. Bring factories into scope with
`using static Tesserae.UI;`.

## Key configuration

- `.AddAnimated(IComponent component, bool grow = false, bool shrink = false, string customPadding = "", bool cardStyle = true)` — add a section (card-styled by default).
- `.AddAnimatedTitle(IComponent component)` — add a section title row.
- `.Secondary()` — apply the secondary tone.
- `.Clear()` — remove all sections.
- Inherits `Stack` sizing/layout helpers (`.S()`, `.WS()`, `.Children(...)`, etc.).

## Example

```csharp
using static Tesserae.UI;

var sections = SectionStack();
sections.AddAnimatedTitle(TextBlock("Overview"));
for (int i = 1; i <= 3; i++)
{
    sections.AddAnimated(Stack().Children(
        TextBlock($"Section {i}").MediumPlus().SemiBold(),
        TextBlock("Sample content.")));
}
```

## Related

- Panel — `../panel/SKILL.md`
- Dialog — `../dialog/SKILL.md`
- Full docs & API: `/tesserae/surfaces/section-stack`
