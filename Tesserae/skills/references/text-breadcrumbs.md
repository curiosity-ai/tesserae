---
name: text-breadcrumbs
description: A breadcrumb trail rendering a path as clickable text links. Use when showing hierarchical navigation/location in a Tesserae (C#/h5) app.
---

# TextBreadcrumbs

A container that lays out a series of `TextBreadcrumb` links separated by chevrons, giving one-click access to higher levels of a navigation hierarchy.

## Create

`UI.TextBreadcrumbs()` — returns the container. Add items with `.Items(...)`.
`UI.TextBreadcrumb(text)` — a single clickable crumb.
Bring the factories into scope with `using static Tesserae.UI;`.

## Key configuration

`TextBreadcrumbs`:

- `.Items(params TextBreadcrumb[])` — add the crumbs (fluent).
- `.Add(TextBreadcrumb)` — append one crumb (separators are inserted automatically).
- `.Clear()` / `.Replace(newCrumb, oldCrumb)` — manage crumbs.
- `.Small()` / `.SemiBold()` etc. — `ITextFormating` size/weight.

`TextBreadcrumb`:

- `.OnClick((s, e) => ...)` — handle a click on the crumb.

## Example

```csharp
using static Tesserae.UI;

var message = TextBlock("Selected: nothing");

var breadcrumbs = TextBreadcrumbs().Items(
    TextBreadcrumb("Home").OnClick((s, e) => message.Text = "Home"),
    TextBreadcrumb("Products").OnClick((s, e) => message.Text = "Products"),
    TextBreadcrumb("Electronics").OnClick((s, e) => message.Text = "Electronics")
);

var ui = Stack().Children(message, breadcrumbs);
```

## Related

- Stack — `.stack.md`
- Full docs & API: `/tesserae/components/text-breadcrumbs`
