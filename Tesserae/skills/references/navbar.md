---
name: navbar
description: A hierarchical vertical navigation tree (Nav) with nested sections, icons, badges and selection tracking; also the top-bar form of a Sidebar via AsNavbar(). Use when building app navigation in a Tesserae (C#/h5) app.
---

# Navbar / Nav

`Nav` is a vertical, hierarchical navigation tree of `NavLink`s with arbitrarily
deep nesting, selection and expansion state. A horizontal navigation bar is
produced from a `Sidebar` via `.AsNavbar()` (header items render inline, the rest
in a drawer).

## Create

- `Nav()` — an empty nav tree; add links with `.Links(...)`.
- `new NavLink(string text)` / `new NavLink(IComponent content)` — a node.
- `Sidebar().AsNavbar()` — top-bar navigation populated with `SidebarButton`/`SidebarSeparator` items.

Bring the factories into scope with `using static Tesserae.UI;`.

## Key configuration

Nav:
- `.Links(params NavLink[])` — add top-level links.
- `.SelectedLink` — the currently selected `NavLink`.
- `.Compact()`, `.NoLinkStyle()`, `.SelectMarkerOnRight()` — appearance.
- `.InlineContent(IComponent)` — non-link content inside the nav.

NavLink:
- `.Links(params NavLink[])` — nested children.
- `.LinksAsync(Func<Task<NavLink[]>>)` — lazy-load children on expand.
- `.OnSelected(handler)` / `.OnExpanded(handler)` — events.
- `.Selected()`, `.Expanded()`, `.SelectedOrExpandedIf(bool)`, `.CanSelectAndExpand()`.
- `.Icon` (icon class), `.Text` / `.SetText(...)`.

Sidebar-as-navbar: `.AddHeader(item)`, `.AddContent(item)`, `.AddFooter(item)`.

## Example

```csharp
using static Tesserae.UI;

var navbar = Sidebar().AsNavbar();
navbar.AddHeader(new SidebarButton("brand", UIcons.Rocket, "My App").Primary());
navbar.AddHeader(new SidebarButton("dashboard", UIcons.Dashboard, "Dashboard"));
navbar.AddContent(new SidebarButton("profile", UIcons.User, "Profile"));
navbar.AddContent(new SidebarSeparator("sep1"));
navbar.AddFooter(new SidebarButton("about", UIcons.Info, "About"));

var page = VStack().H(500.px()).Children(navbar, TextBlock("Content").Padding(16.px()));
```

## Related

- Sidebar — `/tesserae/components/sidebar`
- Menu — `menu.md`
- Full docs & API: `/tesserae/components/navbar`
