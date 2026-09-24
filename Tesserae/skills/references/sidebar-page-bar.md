---
name: sidebar-page-bar
description: The bar at the top of the content while a Sidebar renders as a page on a phone - a back button to the sidebar, a brand and the page title. Use when building the phone layout of a Tesserae (C#/Transpose) app with Sidebar.AsPage.
---

# SidebarPageBar

The bar a phone layout puts at the top of the content while its `Sidebar` renders
as a page (`Sidebar.AsPage()`, see `sidebar.md`). Left to right: a back button that
brings the sidebar back (`sidebar.ShowSidebar()`), an optional brand, the title of
the page on screen, and optional commands.

It collapses itself while its sidebar is not a page, so mount it once in the content
area and leave it there — it costs a desktop layout nothing.

## Create

`UI.SidebarPageBar(Sidebar sidebar)` (i.e. `SidebarPageBar(sidebar)`).

## Key configuration

- `.Brand(IComponent)` — shown between the back button and the title (typically the
  app's logo, about 24px). `null` removes it.
- `.SetTitle(string)` / `.Title` — the title of the page on screen. Update it when
  the page changes.
- `.Commands(params IComponent[])` — replaces the commands on the right.

## Example

```csharp
var sidebar = Sidebar().AsPage();
var pageBar = SidebarPageBar(sidebar).Brand(Icon(UIcons.Rocket)).SetTitle("Inbox");

sidebar.AddContent(new SidebarButton("calendar", UIcons.Calendar, "Calendar")
   .OnClick(() => pageBar.SetTitle("Calendar")));   // the sidebar steps aside by itself

var shell = HStack().S().Children(
    sidebar.HS(),
    VStack().HS().W(1).Grow().Children(pageBar, page.H(10).Grow()));
```

## Related

`sidebar.md` (page mode, `ShowContent` / `ShowSidebar`), `navbar.md` (the other
mobile layout: a top bar with a drawer).
