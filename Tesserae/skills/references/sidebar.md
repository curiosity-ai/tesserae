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
`SidebarText(id, text)`,
`SidebarSearchBox(id, placeholder)` (`.OnSearch(...)`, `.OnClick(...)`,
`.SetKeyboardShortcut("Ctrl", "K")`, `.Rounded()`),
`SidebarComponent(id, component)`.

A search that answers somewhere else — in a `CommandPalette`
(`command-palette.md`), on a search page — is a **button dressed as a field**
rather than a box: `SidebarButton.AsSearchBox("Ctrl", "K")` gives it the field's
rounded outline, its muted label and the shortcut at the far end, and it stays a
button, so there is no caret with nothing to do.

```csharp
sidebar.AddHeader(new SidebarButton("search", UIcons.Search, "Search everything")
    .AsSearchBox("Ctrl", "K")
    .OnClick(() => palette.Open()));
```

`SidebarSearchBox` is the real input, for filtering in place (`.OnSearch(...)`).
It also takes `.OnClick(...)` — which makes it read-only and hands presses, and
its `.SetKeyboardShortcut(...)` key, to the handler — but a button is the simpler
thing when nothing is ever typed into it.

A `.Selected()` item is outlined in the theme's primary color and filled with a
wash of it, rather than with the grey a hover uses — so where you are still
reads while the pointer is somewhere else in the list. It follows
`Theme.SetPrimary(...)`, so an app's own brand color is what marks its current
page.

## A component of your own in the sidebar

`new SidebarComponent(id, component, closedComponent = null)` stands where a
sidebar item would and draws whatever you hand it — a chat history, a tree of
spaces, a filter form, a model picker. The component keeps its own state and
only asks the sidebar for a place to stand.

- The **closed** (icon-rail) state takes a component of its own, because almost
  nothing worth hosting fits a 48px rail. Passing none — the default — leaves
  the item out of the rail entirely, which is usually what a list wants.
- `.Grow()` lets it take the leftover height of the middle section, for a
  component that scrolls its own content.
- `.NotSortable()` keeps it out of drag reordering, which a hosted region
  normally wants.

```csharp
chatBar.AddContent(new SidebarComponent("history", chatHistory).Grow().NotSortable());
```

## SidebarSearchBox

`new SidebarSearchBox(id, placeholder)` is a search input for the header that
filters searchable items. Configure it fluently:

- `.OnSearch(term => sidebar.Search(term))` — run on every keystroke.
- `.SetKeyboardShortcut("Ctrl", "K")` — show a shortcut chip (renders ⌘K on
  macOS, Ctrl+K elsewhere) and focus the box when the shortcut is pressed.
- `.Rounded(BorderRadius = Full)` — render as a full bordered, rounded "pill".
- `.Text` / `.SetText(text)` — read or replace what is in the box. Setting it
  does not raise `.OnSearch(...)`, so a caller that clears the box decides for
  itself what to do about the results.
- `.Focus()` — put the caret in the box.

It is an `ISidebarItem`, so it is normally added to a `Sidebar`; `.RenderOpen()`
gives you the box as an `IComponent` when the list it filters is drawn by a
component of your own rather than by the sidebar itself.

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
