---
name: sidebar-separator
description: A visual separator for grouping items inside a Sidebar, optionally with a label. Use when separating or labelling groups of Sidebar items in a Tesserae (C#/h5) app.
---

# SidebarSeparator

A thin divider added between `Sidebar` items to visually group them. Pass a
text to render a labelled group header instead of a plain line.

## Create

`new SidebarSeparator(string identifier, string text = null)` — an
`ISidebarItem` you add to a `Sidebar`. `identifier` must be unique within the
sidebar; `text` (optional) turns it into a labelled separator.

It is added to a sidebar via `sidebar.AddContent(...)` (or `AddHeader` /
`AddFooter`). Bring factories into scope with `using static Tesserae.UI;`.

## Key configuration

- `identifier` (ctor) — unique key for ordering/sorting.
- `text` (ctor) — optional caption; omit for a plain line.
- `.Collapse()` / `.Show()` — hide or reveal the separator.
- `.IsSelected` — selection state (inherited from `ISidebarItem`; rarely used).

## Example

```csharp
using static Tesserae.UI;

var sidebar = Sidebar();
sidebar.AddContent(new SidebarButton("home", UIcons.Home, "Home"));
sidebar.AddContent(new SidebarSeparator("sep1"));                 // plain line
sidebar.AddContent(new SidebarButton("profile", UIcons.User, "Profile"));
sidebar.AddContent(new SidebarSeparator("sep2", "More Options")); // labelled
sidebar.AddContent(new SidebarButton("settings", UIcons.Settings, "Settings"));
```

## Related

- Sidebar — `sidebar.md`
- Full docs & API: `/tesserae/components/sidebar-separator`
