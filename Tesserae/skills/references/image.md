---
name: image
description: A responsive image element with fit-mode, sizing, fallback source and circle/crop helpers. Use when displaying screenshots, avatars, logos or any bitmap in a Tesserae (C#/h5) app.
---

# Image

An `<img>` wrapper with fluent fit-mode, sizing and shape helpers. Supports a
fallback `src` shown if the primary image fails to load.

## Create

`Image(string source, string fallback = null)` — the image; `fallback` is shown
on load error. Bring the factory into scope with `using static Tesserae.UI;`.
Size with the shared `.Width(...)` / `.Height(...)` helpers (e.g. `.Width(128.px())`).

## Key configuration

- `.Cover()` / `.Contain()` / `.Fill()` / `.ScaleDown()` / `.NoFit()` — `object-fit` mode.
- `.Position(string objectPosition)` — CSS `object-position` (e.g. `"center top"`).
- `.Circle()` / `.Circle(int pixels)` — circular crop, or a fixed corner radius.
- `.Source` — get/set the `src` after creation.
- `.Background` — CSS background behind a transparent image.
- `.Cursor` — CSS cursor.

## Example

```csharp
using static Tesserae.UI;

var logo = Image("/assets/logo.png").Width(128.px()).Height(128.px());

var avatar = Image("/avatars/jane.jpg", fallback: "/avatars/default.png")
    .Cover()
    .Circle()
    .Width(48.px()).Height(48.px());
```

## Related

- Icon — `icon.md`
- Avatar — `/tesserae/components/avatar`
- Full docs & API: `/tesserae/components/image`
