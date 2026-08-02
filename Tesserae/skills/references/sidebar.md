---
name: sidebar
description: A collapsible side-navigation panel with header, scrollable middle, and footer sections holding sidebar items (buttons, separators, nav groups, pivots). Use when building app navigation that can collapse to icons in a Tesserae (C#/Transpose) app.
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
- `.ShiftTo(childSidebar)` / `.ShiftBack()` / `.IsShifted` — slide into a nested
  sidebar (see below).
- `.AsNavbar()` — render horizontally as a top bar with a hamburger drawer.
- `.Secondary()` — use the secondary background colour.
- `.Sortable(bool)` — enable/disable drag reordering.
- `.Search(term)` — filter searchable middle items.
- `.OnSortingChanged(d => ...)`, `.GetCurrentSorting()`, `.LoadSorting(d)` —
  persist item order.

Common item types: `SidebarButton(id, UIcons icon, text)` (`.Selected()`,
`.OnClick(...)`, `.Primary()`, `.Danger()`, `.Rounded()`, `.Tooltip(...)`),
`SidebarSeparator(id, text)`, `SidebarNav(id, icon, text, initiallyCollapsed)`,
`SidebarText(id, text)`, `SidebarSearchBox(id, placeholder)`.

## SidebarSearchBox

`new SidebarSearchBox(id, placeholder)` is a search input for the header that
filters searchable items. Configure it fluently:

- `.OnSearch(term => sidebar.Search(term))` — run on every keystroke.
- `.SetKeyboardShortcut("Ctrl", "K")` — show a shortcut chip (renders ⌘K on
  macOS, Ctrl+K elsewhere) and focus the box when the shortcut is pressed.
- `.Rounded(BorderRadius = Full)` — render as a full bordered, rounded "pill".

## Rounded (pill) style

`SidebarButton.Rounded(BorderRadius = Full)` and
`SidebarSearchBox.Rounded(BorderRadius = Full)` render the item with rounded
corners (a full pill by default; pass `BorderRadius.Small`/`Medium`/`Full`).
Combine `.Primary().Rounded()` on a button for a prominent call-to-action.

## Shift into a child sidebar

When navigating into an interface that has its own navigation (a chat view, a
project workspace, ...), shift the sidebar into a second `Sidebar` instead of
rebuilding the items. The child slides in horizontally from the right, and the
panel that ends up out of view is set to `display: none` once the animation is
over, so it can't be tabbed or read into.

- `.ShiftTo(childSidebar)` — mount the child sidebar and slide into it. Calling
  it with a different sidebar replaces the mounted one.
- `.ShiftBack()` — slide back into the main sidebar.
- `.IsShifted` / `.ShiftedSidebar` — current state and mounted child.
- `.OnShiftChanged(isShifted => ...)` — run when the sidebar shifts, e.g. to swap
  the content area alongside it.

Only one depth level is supported: a child sidebar can't shift again. The child
is rendered inside the hosting sidebar and follows its open/closed state, so
`.Toggle()` on the main sidebar collapses both. Shifting is ignored in
`.AsNavbar()` mode.

```csharp
var sidebar = Sidebar();
var chatBar = Sidebar();

sidebar.AddContent(new SidebarButton("assistant", UIcons.Sparkles, "AI assistant",
        new SidebarCommand(UIcons.AngleRight))
    .CommandsAlwaysVisible()
    .OnClick(() => sidebar.ShiftTo(chatBar)));

chatBar.AddHeader(new SidebarButton("back", UIcons.AngleLeft, "AI assistant")
    .OnClick(() => sidebar.ShiftBack()));
chatBar.AddContent(new SidebarSeparator("today", "Today"));
chatBar.AddContent(new SidebarButton("chat-1", UIcons.Comment, "Brake sensor calibration"));

sidebar.OnShiftChanged(isShifted => Router.Navigate(isShifted ? "#/chat" : "#/home"));
```

## Example

```csharp
using static Tesserae.UI;

var sidebar = Sidebar();

sidebar.AddHeader(new SidebarButton("new-doc", UIcons.Plus, "New document")
    .Primary().Rounded().OnClick(() => Toast().Success("New document")));

sidebar.AddHeader(new SidebarSearchBox("search", "Search docs, parts, records...")
    .Rounded()
    .SetKeyboardShortcut("Ctrl", "K")
    .OnSearch(term => sidebar.Search(term)));

sidebar.AddContent(new SidebarButton("home", UIcons.Home, "Home").Selected());
sidebar.AddContent(new SidebarSeparator("sep", "Tools"));
sidebar.AddFooter(new SidebarButton("settings", UIcons.Settings, "Settings"));

var app = HStack().WS().Children(sidebar.HS(), VStack().Grow().HS());
```

## Related

- SidebarSeparator — `sidebar-separator.md`
- Sidenav (icon-only rail) — `sidenav.md`
- Full docs & API: `/tesserae/components/sidebar`
