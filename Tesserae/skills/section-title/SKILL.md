---
name: section-title
description: A heading that labels a section of a form or page with an icon, title, optional subtitle, and trailing action commands. Use when introducing a section in a form, card, or SectionStack in a Tesserae (C#/h5) app.
---

# SectionTitle

A vertical stack rendering an icon + bold title row (commands pushed to the right) with an optional subtitle line beneath.

## Create

`UI.SectionTitle(UIcons icon, string title, string subtitle, params IComponent[] commands)` — returns a `SectionTitle`. Pass `null`/empty subtitle to omit the subtitle line. Also `new SectionTitle(...)`.
Bring factories into scope with `using static Tesserae.UI;`.

## Key configuration

The content is set entirely through the constructor (icon, title, subtitle, trailing commands). Beyond that:

- `.Margin` / `.Padding` — CSS spacing (it implements `IHasMarginPadding`, so margin/padding helpers apply).

## Example

```csharp
using static Tesserae.UI;

var stack = Stack().Children(
    SectionTitle(UIcons.User, "Profile", "Manage your account details",
        Button("Save").Primary()),
    TextBlock("Section body goes here.")
);
```

## Related

- SectionStack — `/tesserae/surfaces/section-stack`
- Full docs & API: `/tesserae/components/section-title`
