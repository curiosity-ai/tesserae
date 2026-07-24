---
name: link
description: A hyperlink (anchor) component wrapping text or any component, with new-tab, pop-up-window, underline and icon support. Use when adding navigational or external links in a Tesserae (C#/Transpose) app.
---

# Link

An `<a>` element whose content is a string or any `IComponent`. Controls target,
underline, and can open as a sized pop-up window.

## Create

- `Link(string url, string text)` — text link.
- `Link(string url, string text, UIcons icon, bool noUnderline = false)` — link with a leading icon.
- `Link(string url, IComponent content, bool noUnderline = false)` — link wrapping a component.

Bring the factory into scope with `using static Tesserae.UI;`.

## Key configuration

- `.OpenInNewTab()` — sets `target="_blank"`.
- `.AsWindow(string features = null)` — open in a centered pop-up window (default 900×600) instead of navigating.
- `.OnClick(Action)` — run a callback instead of (or before) navigating.
- `.URL` / `.Target` — get/set href and target.
- Inherited `ITextFormating`: `.Size` (`TextSize`), `.Weight` (`TextWeight`), `.TextAlign`.

## Example

```csharp
using static Tesserae.UI;

var stack = Stack().Children(
    Link("https://curiosity.ai", "Visit Curiosity").OpenInNewTab(),
    Link("https://github.com/curiosity-ai/tesserae", "Tesserae on GitHub", UIcons.BrandsGithub)
);
```

## Related

- Button — `/tesserae/components/button`
- ActionButton — `/tesserae/components/action-button`
- Full docs & API: `/tesserae/components/link`
