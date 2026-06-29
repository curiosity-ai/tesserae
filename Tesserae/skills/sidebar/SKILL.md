---
name: sidebar
description: A collapsible side-navigation panel with header, scrollable middle, and footer sections holding sidebar items (buttons, separators, nav groups, pivots). Use when building app navigation that can collapse to icons in a Tesserae (C#/h5) app.
---

# Sidebar

A vertical navigation panel that can be open (icons + labels) or closed (icons
only). Items go into three sections — header, middle content, footer — and
implement `ISidebarItem` (`SidebarButton`, `SidebarSeparator`, `SidebarNav`,
`SidebarPivot`, `SidebarText`, ...). Can also render as a top navbar.

## Create

`UI.Sidebar(bool sortable = false)` (i.e. `Sidebar()`) returns a `Sidebar`.
Pass `sortable: true` to allow drag-reordering of middle items.
Bring factories into scope with `using static Tesserae.UI;`.

## Key configuration

- `.AddHeader(item)` / `.AddContent(item)` / `.AddFooter(item)` — place an
  `ISidebarItem` in the top / scrollable middle / bottom section.
- `.InsertAfterContent(item, addAfter)` — insert relative to an existing item.
- `.RemoveContent(item)`, `.ClearContent()`, `.Clear()` — remove items.
- `.Closed(bool = true)` / `.Toggle()` / `.IsClosed` — collapse to icon rail.
- `.AsNavbar()` — render horizontally as a top bar with a hamburger drawer.
- `.Secondary()` — use the secondary background colour.
- `.Sortable(bool)` — enable/disable drag reordering.
- `.Search(term)` — filter searchable middle items.
- `.OnSortingChanged(d => ...)`, `.GetCurrentSorting()`, `.LoadSorting(d)` —
  persist item order.

Common item types: `SidebarButton(id, UIcons icon, text)` (`.Selected()`,
`.OnClick(...)`, `.Danger()`, `.Tooltip(...)`), `SidebarSeparator(id, text)`,
`SidebarNav(id, icon, text, initiallyCollapsed)`, `SidebarText(id, text)`.

## Example

```csharp
using static Tesserae.UI;

var sidebar = Sidebar();
sidebar.AddHeader(new SidebarText("title", "My App"));
sidebar.AddContent(new SidebarButton("home", UIcons.Home, "Home").Selected());
sidebar.AddContent(new SidebarSeparator("sep", "Tools"));
sidebar.AddContent(new SidebarButton("search", UIcons.Search, "Search")
    .OnClick(() => Toast().Information("Search")));
sidebar.AddFooter(new SidebarButton("settings", UIcons.Settings, "Settings"));

var app = HStack().WS().Children(sidebar.HS(), VStack().Grow().HS());
```

## Related

- SidebarSeparator — `../sidebar-separator/SKILL.md`
- Sidenav (icon-only rail) — `../sidenav/SKILL.md`
- Full docs & API: `/tesserae/components/sidebar`
