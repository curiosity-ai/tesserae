---
name: sidebar-divider
description: A horizontal rule across a Sidebar, run out to the rail's edges, that goes between any two items in the header, content or footer. Use when drawing a line between Sidebar items (for example above the account row) in a Tesserae (C#/Transpose) app.
---

# SidebarDivider

A line across a `Sidebar`, run out past the sidebar's padding to the rail's own
edges - the same line `.Separated()` draws on an identity row, as an item of its
own. It draws on the open and the collapsed rail alike, never takes part in drag
reordering, and carries `role="separator"`.

In the header and the footer it spans the whole rail. In the content (a scroller)
it stops at the scroller's right edge, so it never adds a horizontal scrollbar.

Every theme package draws it the way it draws the identity-row divider (thicker
ink in Kontrast and Memphis, a groove in Weniger, and so on).

## Create

`new SidebarDivider(string identifier)` — an `ISidebarItem`. `identifier` must be
unique within the sidebar. Add it with `sidebar.AddHeader(...)`,
`sidebar.AddContent(...)` or `sidebar.AddFooter(...)`; it sits where it was added.

## Key configuration

- `.Collapse()` / `.Show()` — hide or reveal the line.

## When to use which

- `SidebarDivider` — a line between two items, anywhere.
- `SidebarSeparator` — a gap, or a caption over a group (no line).
- `.Separated()` on `SidebarBrand` / `SidebarProfile` / `SidebarSectionHeader` —
  the line belongs to that row and moves with it.

## Example

```csharp
using static Tesserae.UI;

var sidebar = Sidebar();
sidebar.AddHeader(new SidebarBrand("brand", UIcons.Rocket, "My App"));
sidebar.AddHeader(new SidebarDivider("header-divider"));

sidebar.AddContent(new SidebarButton("home",  UIcons.Home,   "Home"));
sidebar.AddContent(new SidebarDivider("content-divider"));
sidebar.AddContent(new SidebarButton("files", UIcons.Folder, "Files"));

sidebar.AddFooter(new SidebarButton("manage", UIcons.Settings, "Manage"));
sidebar.AddFooter(new SidebarDivider("footer-divider"));   // the line above the account row
sidebar.AddFooter(new SidebarProfile("profile", "Ada Lovelace", "ada@example.com"));
```

## Related

- Sidebar — `sidebar.md`
- SidebarSeparator (gap or caption) — `sidebar-separator.md`
- SidebarProfile — `sidebar-profile.md`
- Full docs & API: `/tesserae/navigation/sidebar-divider`
