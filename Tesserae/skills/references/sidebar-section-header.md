---
name: sidebar-section-header
description: A non-interactive row at the top of a Sidebar naming the section it shows, with the section's glyph, name and an optional line saying what is in it. Use when a Sidenav rail picks the section and the sidebar beside it lists that section's items, in a Tesserae (C#/Transpose) app.
---

# SidebarSectionHeader

The row at the top of a `Sidebar` that names the section the sidebar is showing:
the section's glyph, its name, and an optional second line saying what is in it
("Data, AI and delivery"). It has the shape and height of a `SidebarBrand`, but it
is a heading, not a control. It does not answer the pointer, takes no tab stop and
carries no commands.

On the collapsed rail it is the glyph alone, with the name (and the second line)
in its tooltip.

## Create

`new SidebarSectionHeader(string identifier, UIcons icon, string title, string subtitle = null, UIconsWeight weight = UIconsWeight.Regular)`
is an `ISidebarItem`. Add it with `sidebar.AddHeader(...)`.
Bring factories into scope with `using static Tesserae.UI;`.

## Key configuration

- `.SetTitle(title)` / `.SetSubtitle(subtitle)` — change the name or the second
  line (null or empty leaves a single line).
- `.Separated()` — a divider under the header, run out to the sidebar's edges.
- `.NotSortable()` — keep it out of drag reordering.

## Example

```csharp
using static Tesserae.UI;

var header = new SidebarSectionHeader("section", UIcons.Database, "Build", "Data, AI and delivery").Separated();

var sidebar = Sidebar();
sidebar.AddHeader(header);
sidebar.AddContent(new SidebarSeparator("data", "Data"));
sidebar.AddContent(new SidebarButton("data-sources", UIcons.Database, "Data Sources").Selected());

// Switching section: rename the header and replace the content
header.SetTitle("Govern").SetSubtitle("Policies and audit");
sidebar.ClearContent();
```

## Section header, brand or status card?

All three head a sidebar.

- `SidebarBrand` (`sidebar-brand.md`) is the **application's** row: a logo that
  opens Home and carries commands such as settings or the rail's open/close.
- `SidebarSectionHeader` names the **section** the sidebar shows, and is not
  clickable.
- `SidebarStatusCard` (`sidebar-status-card.md`) says what the section **is
  doing**, when that changes while the page is open.

## Related

- Sidebar — `sidebar.md`
- Sidenav (icon rail to the left) — `sidenav.md`
- SidebarBrand — `sidebar-brand.md`
- SidebarStatusCard — `sidebar-status-card.md`
- SidebarSeparator (groups within the section) — `sidebar-separator.md`
- Full docs & API: `/tesserae/components/sidebar-section-header`
