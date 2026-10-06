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
- `.SetBadge(string text, BadgeTone tone = BadgeTone.Neutral)` — a badge after the
  name, such as the environment a build runs in. See below.
- `.SetTitle(...)` / `.SetSubtitle(...)` — update the row in place.
- `.Selected(bool = true)` / `.IsSelected` / `.SelectedStatus`, `.CommandsOnHover()`,
  `.Tooltip(...)`, `.NotSortable()`, `.Class(...)`, `.OnContextMenu(...)`,
  `.OnRendered(...)` — as on `SidebarProfile`.

## The row's height, in both states

A brand or profile row is **48px**, and the collapsed rail draws its picture in a box of the same height
rather than shrinking to the 28px an ordinary row gets — so the rail's chrome is the same piece before and
after it collapses, and a command that becomes the collapsed picture (see `SidebarBrand.WithSidebarControl`)
does not move under the pointer. The commands on the row are squares of that height too, at its right end.

One custom property drives all of it, so a skin changes the height once and the collapsed box, the command
squares and the room the name gives up for them all follow:

```css
.my-brand-row { --tss-sidebar-identity-height: 56px; }
```

## The rail grows to fit the name

The application's name is what the rail is for, so a brand never ellipsizes it:
the row measures its name and second line and raises the sidebar's `min-width` to
whatever the open rail needs to draw both whole — beside the logo and every
command on the row. It only raises the floor: a rail already wide enough keeps the
width it was given, and the closed rail, a page (`AsPage`) and a navbar
(`AsNavbar`) are left alone. Nothing to call; it is measured each time the rail
opens, when the title, the subtitle or the commands change, and once the fonts
have loaded — never while the rail is being resized, since its width does not
change the answer.

The measurement is written to the sidebar as `--tss-sidebar-identity-min-width`
(beside a `tss-sidebar-fits-identity` class) and turned into `min-width` by the
stylesheet; a sidebar without a brand keeps whatever `min-width` your own CSS gives it. A brand on a child sidebar shifted
into another (`Sidebar.ShiftTo`) does not widen the host, and ellipsizes.

## The name gives way to the logo

Where the name is ellipsized anyway (a brand on a shifted child sidebar), a name
cut to `C..` says less than the logo beside it. Once the room shows **less than
half of the text**, the row hides it and is the logo and its commands, which is
what the collapsed rail draws. It comes back by itself as the rail gets wider.
Nothing to call, and no script: it is the stylesheet doing arithmetic on the
text's own width (`calc-size(max-content, ...)` on its `flex-basis`), so it holds
while a rail is dragged or animated.

- Half is taken of the whole text block, name and second line.
- It applies to an open rail only, not to a page (`AsPage`) or a navbar
  (`AsNavbar`), and not to `SidebarProfile`, where a long e-mail would decide for
  the name.
- The words stay in the document, so assistive technology still reads the name.
  There is no tooltip while the text is hidden: a tooltip that appears only then
  would need script to know when.
- A browser without `calc-size()` keeps ellipsizing the name, as before.
- A skin that changes the logo's size or the gap beside it sets
  `--tss-sidebar-brand-logo-size` / `--tss-sidebar-identity-gap`, which the
  arithmetic reads too.

## A badge after the name

```csharp
.SetBadge(string text, BadgeTone tone = BadgeTone.Neutral)   // null or empty text removes it
.SetBadge(Badge badge)                                       // a badge of your own; null removes it
```

For a label that qualifies the name itself rather than adding a second line under
it: the environment a build runs in (`"DEV"`, `"VAL"`), an edition, a pre-release
marker. `tone` is the same `BadgeTone` a `Badge` takes (`Neutral`, `Primary`,
`Success`, `Warning`, `Danger`, `Info`, `AI`), so it follows the theme. The
`Badge` overload is for an outline, an icon or colours outside the tones
(`.Background(...)` / `.Foreground(...)`).

- The name ellipsizes before the badge does, and the rail grows to fit the name
  *and* the badge (see below).
- On the collapsed rail the badge text goes in the logo's default tooltip, after
  the name: `Aurelia Ops (DEV) - Fleet Europe`. With `.WithSidebarControl(...)` the
  logo turns into the open button under the pointer, so the tooltip shown is that
  button's.
- The name and badge share a `.tss-sidebar-identity-title-line` that only exists
  while there is a badge; the badge carries `.tss-sidebar-identity-title-badge`.

```csharp
new SidebarBrand("brand", "Aurelia Ops", "Fleet Europe", logoUrl)
   .SetBadge("DEV", BadgeTone.Warning);
```

## The brand as the rail's open/close control

```csharp
.WithSidebarControl(Action onOpen,
                    Action onClose,
                    string openTooltip  = null,   // "Open Sidebar"
                    string closeTooltip = null,   // "Close Sidebar"
                    UIcons openIcon     = UIcons.SidebarFlip,
                    UIcons closeIcon    = UIcons.Sidebar)
```

The top row is the one thing on the rail that is there in both states, so it is
where an application that has an open/close control usually wants it:

- **While the sidebar is open** the control is the *last* command on the row, so
  closing the rail is the rightmost thing on it — after the gear and after
  anything `.Commands(...)` added.
- **While the sidebar is closed** there is no room for a command beside the logo,
  so the logo *is* the control: it stands as the brand at rest and turns into the
  open button under the pointer.

The default glyphs are the rail itself — `UIcons.Sidebar` to close it and
`UIcons.SidebarFlip`, its mirror, to open it — so the control says which thing it
acts on rather than which direction something moves.

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
- SidebarSectionHeader (a section's name, not clickable) — `sidebar-section-header.md`
- SidebarStatusCard — `sidebar-status-card.md`
- Sidebar — `sidebar.md`
- Full docs & API: `/tesserae/components/sidebar-brand`
