---
name: sidebar-profile
description: The sidebar row that says who is signed in - picture, name, an optional second line for an e-mail or company, and the settings and logout commands. Use for the account row at the bottom of a rail in a Tesserae (C#/Transpose) app.
---

# SidebarProfile

The row at the bottom of a rail that names the signed-in account: a round
picture, the person's name, an optional second line for whatever identifies the
account beside the name — an e-mail address, a company, a tenant — and the one or
two commands that belong to it.

It is twice the height of an ordinary sidebar row and its commands are drawn at
rest rather than under the pointer, which is why it is a component rather than a
`SidebarButton` dressed up: composing it out of a button and a stack ends in a
stylesheet of your own overriding the rail's heights, paddings and icon sizes.

`SidebarBrand` (`sidebar-brand.md`) is the same shape at the top of the rail, so a
sidebar bracketed by the two reads as one piece of chrome.

## Create

```csharp
new SidebarProfile(string identifier,
                   string name,
                   string subtitle   = null,
                   string pictureUrl = null,
                   string initials   = null)
```

An `ISidebarItem`, so it goes in through `sidebar.AddFooter(...)` (or
`AddHeader` / `AddContent`). `identifier` must be unique within the sidebar.

**The picture falls back to the initials**, so a photo URL that only answers for
accounts that have uploaded one can be passed unconditionally — a URL that 404s
lands on the initials, tinted with the gradient `Avatar` (`avatar.md`) derives
from them. `initials` is only for overriding what is read off the name, which is
the first letter of the first word and of the last: `"M. Okafor"` reads `MO`.

Passing no `subtitle` leaves the row a single line rather than an empty one.

## Key configuration

- `.Settings(Action onClick, string tooltip = null, UIcons icon = UIcons.Settings)` —
  the gear. `.Settings(SidebarCommand)` takes a command you built yourself, for one
  that opens a menu (`SidebarCommand.OnClickMenu`) rather than a settings page.
- `.Logout(Action onClick, string tooltip = null, UIcons icon = UIcons.SignOutAlt)` —
  optional; leave it off where the only way out is a menu item. `.Logout(SidebarCommand)`
  for a command of your own. Passing `null` to either removes it.
- `.Commands(params SidebarCommand[])` — commands drawn *before* those two.
- `.OnClick(...)` — what the row itself opens. The commands are separate buttons,
  so pressing one is not pressing the row.
- `.Separated()` — a divider on the row's outer edge, run out to the sidebar's own
  edges rather than stopping at its padding. Which edge is read from the section
  the row is in: above it in the footer, below it in the header.
- `.Presence(AvatarPresence.Online | Away | Busy | Offline)` — the dot on the picture.
- `.Selected(bool = true)` / `.IsSelected` / `.SelectedStatus` — marks the account's
  own page as the one being shown, in the theme's primary colour.
- `.SetName(...)`, `.SetSubtitle(...)`, `.SetPicture(...)`, `.SetInitials(...)` —
  update the row in place when the account changes.
- `.CommandsOnHover()` — opt into the ordinary row behaviour, where the commands
  wait for the pointer. The default is the other way round: a gear that only appears
  on hover is a gear nobody finds.
- `.Tooltip(...)` — the tooltip on the collapsed rail. The default is the name and
  the second line.
- `.NotSortable()`, `.Class(...)`, `.OnContextMenu(...)`, `.OnRendered(...)`.

## Collapsed

On the collapsed rail the row is the picture alone — no name, no commands — with
the name and the second line in its tooltip. Nothing has to be configured for it.

## Example

```csharp
using static Tesserae.UI;

sidebar.AddFooter(new SidebarProfile("account", user.FullName, user.Email, API.PhotoUrl(user.Uid))
    .Separated()
    .Presence(user.IsOnline ? AvatarPresence.Online : AvatarPresence.Offline)
    .Settings(() => Router.Navigate("#/preferences"))
    .Logout(() => Auth.Logout())
    .OnClick(() => Router.Navigate("#/account")));
```

A menu instead of a gear, for the accounts that keep more than settings behind it:

```csharp
new SidebarProfile("account", user.FullName, user.Email)
    .Settings(new SidebarCommand(UIcons.MenuDots).Tooltip("More").OnClickMenu(() => new ISidebarItem[]
    {
        new SidebarButton("prefs",  UIcons.Settings,   "Preferences").OnClick(OpenPreferences),
        new SidebarButton("theme",  UIcons.MoonStars,  "Switch to dark mode").OnClick(ToggleDarkMode),
        new SidebarButton("logout", UIcons.SignOutAlt, "Log out").OnClick(Auth.Logout),
    }));
```

## Related

- SidebarBrand — `sidebar-brand.md`
- Sidebar — `sidebar.md`
- Avatar / Persona — `avatar.md`
- Full docs & API: `/tesserae/components/sidebar-profile`
