---
name: sidebar-brand
description: The sidebar row at the top of a rail that says what the application is - logo, name, an optional second line, a configuration command, and optionally the rail's own open/close control. Use for the brand row of a Tesserae (C#/Transpose) app shell.
---

# SidebarBrand

The row at the top of a rail that says what the application is: a logo, its
name, an optional second line for whatever qualifies that name — the workspace,
the tenant, the environment — and the command that opens its configuration.

It is the same shape and height as `SidebarProfile` (`sidebar-profile.md`), so a
rail bracketed by the two reads as one piece of chrome, and it is a component
rather than a `SidebarButton` dressed up for the same reason: the row is taller
than the ones under it, carries two lines of text, and draws its commands at rest.

## Create

```csharp
new SidebarBrand(string identifier, string title, string subtitle = null, string logoUrl = null)
new SidebarBrand(string identifier, UIcons icon,  string title, string subtitle = null, UIconsWeight weight = UIconsWeight.Regular)
new SidebarBrand(string identifier, Emoji  icon,  string title, string subtitle = null)
new SidebarBrand(string identifier, ISidebarIcon logo, string title, string subtitle = null)
```

An `ISidebarItem`, so it goes in through `sidebar.AddHeader(...)`. `identifier`
must be unique within the sidebar. An image, a glyph and an emoji all take the
same square beside the name — the box is sized by the stylesheet, so a logo of
any aspect is drawn `contain`ed inside it rather than stretched. An
`ISidebarIcon` is cloned for the collapsed rail, since one element cannot be in
two places.

Passing no `subtitle` leaves the row a single line rather than an empty one.

## Key configuration

- `.Configure(Action onClick, string tooltip = null, UIcons icon = UIcons.Settings)` —
  the gear. `.Configure(SidebarCommand)` takes a command you built yourself, for one
  that opens a menu (`SidebarCommand.OnClickMenu`); `null` removes it.
- `.WithSidebarControl(onOpen, onClose, ...)` — make the brand the rail's own
  open/close control. See below.
- `.Commands(params SidebarCommand[])` — commands drawn *before* the gear: a search,
  a back arrow, whatever else the top of the rail carries.
- `.OnClick(...)` — what the row itself opens, usually Home.
- `.Separated()` — a divider on the row's outer edge, run out to the sidebar's own
  edges rather than stopping at its padding. In the header that is the bottom edge.
- `.SetTitle(...)` / `.SetSubtitle(...)` — update the row in place.
- `.Selected(bool = true)` / `.IsSelected` / `.SelectedStatus`, `.CommandsOnHover()`,
  `.Tooltip(...)`, `.NotSortable()`, `.Class(...)`, `.OnContextMenu(...)`,
  `.OnRendered(...)` — as on `SidebarProfile`.

## The brand as the rail's open/close control

```csharp
.WithSidebarControl(Action onOpen,
                    Action onClose,
                    string openTooltip  = null,   // "Open Sidebar"
                    string closeTooltip = null,   // "Close Sidebar"
                    UIcons openIcon     = UIcons.AngleDoubleRight,
                    UIcons closeIcon    = UIcons.AngleDoubleLeft)
```

The top row is the one thing on the rail that is there in both states, so it is
where an application that has an open/close control usually wants it:

- **While the sidebar is open** the control is the *last* command on the row, so
  closing the rail is the rightmost thing on it — after the gear and after
  anything `.Commands(...)` added.
- **While the sidebar is closed** there is no room for a command beside the logo,
  so the logo *is* the control: it stands as the brand at rest and turns into the
  open button under the pointer.

It reports the two intentions rather than driving a sidebar itself, because the
rail a brand sits on is not always the one it opens — an application shifts
between rails (`Sidebar.ShiftTo`) and rebuilds them — so the caller is the one
that knows which `Sidebar` to toggle and what else follows from it (remembering
the state, redrawing a drag handle, …).

## Example

```csharp
using static Tesserae.UI;

var sidebar = Sidebar();

sidebar.AddHeader(new SidebarBrand("brand", App.Name, workspace.Name, App.LogoUrl)
    .Separated()
    .Configure(() => Router.Navigate("#/manage"), "Manage application")
    .WithSidebarControl(onOpen:  () => { sidebar.IsClosed = false; Remember(false); },
                        onClose: () => { sidebar.IsClosed = true;  Remember(true);  })
    .OnClick(() => Router.Navigate("#/home")));
```

## Related

- SidebarProfile — `sidebar-profile.md`
- Sidebar — `sidebar.md`
- Full docs & API: `/tesserae/components/sidebar-brand`
