---
name: sidebar
description: A collapsible side-navigation panel with header, scrollable middle, and footer sections holding sidebar items (buttons, separators, nav groups, pivots). Use when building app navigation that can collapse to icons in a Tesserae (C#/Transpose) app.
---

# Sidebar

A vertical navigation panel that can be open (icons + labels) or closed (icons
only). Items go into three sections — header, middle content, footer — and
implement `ISidebarItem` (`SidebarButton`, `SidebarSeparator`, `SidebarNav`,
`SidebarPivot`, `SidebarText`, ...). Can also render as a top navbar, or as a
page of its own on a phone.

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
- `.AsNavbar(bool = true)` / `.IsNavbar` — render horizontally as a top bar with a
  hamburger drawer, or (passing `false`) back as a vertical sidebar. Both directions
  work, so an app can switch on a window resize — see *Responsive* below.
- `.AsPage(bool = true)` / `.IsPage` / `.PageMode` — the phone layout: the sidebar
  fills its container and takes turns with the content after it — see *Sidebar as a
  page* below.
- `.ShowContent()` / `.ShowSidebar()` / `.IsShowingContent` / `.ShowingContent` —
  which of the two a page-mode sidebar has on screen.
- `.Secondary()` — use the secondary background colour.
- `.Sortable(bool)` — enable/disable drag reordering.
- `.Search(term)` — filter searchable middle items.
- `.OnSortingChanged(d => ...)`, `.GetCurrentSorting()`, `.LoadSorting(d)` —
  persist item order.

## An item's id is its identity

The `id` every item is constructed with is how the sidebar knows *which row this
is*, so make it name the thing (`"space-" + space.Uid`) and keep it stable across
rebuilds — it is also what `.GetCurrentSorting()` / `.LoadSorting(d)` persist a
reader's drag-reordering against, so an id that changes loses that reader's
placement for the row.

Two behaviours follow, and both are what let a screen be rebuilt from data that
changed:

- **Adding an id that is already there replaces the item, in place.** A rebuild
  hands `.AddContent(...)` / `SidebarNav.Add(...)` freshly built items, and each
  one takes over its row — keeping its position and its place in the order —
  rather than being dropped as a duplicate. Handing back the *same* item is still
  nothing to do, so a rebuild that changed nothing costs no re-render.
- **Removing matches on the id, not on the object.** `.RemoveContent(item)` /
  `SidebarNav.Remove(item)` take a freshly built item carrying the right id; you
  do not need a reference to the one that is actually up there.

```csharp
//Re-run whenever the data changes: each row is replaced by the one built from what it now says.
foreach (var space in spaces)
{
    var item = new SidebarButton("space-" + space.Uid, space.Icon, space.Name, CommandsFor(space));

    if (space.Pinned) { nav.Remove(item); sidebar.AddContent(item); }
    else              { sidebar.RemoveContent(item); nav.Add(item); }
}
```

Common item types: `SidebarButton(id, UIcons icon, text)` (`.Selected()`,
`.OnClick(...)`, `.Primary()`, `.Danger()`, `.Rounded()`,
`.SetKeyboardShortcut("Ctrl", "Shift", "O")`, `.ShortcutOnlyOnHover()`,
`.Tooltip(...)`),
`SidebarSeparator(id, text)`, `SidebarNav(id, icon, text, initiallyCollapsed)`,
`SidebarText(id, text)`,
`SidebarSearchBox(id, placeholder)` (`.OnSearch(...)`, `.OnClick(...)`,
`.SetKeyboardShortcut("Ctrl", "K")`, `.Rounded()`),
`SidebarComponent(id, component)`,
`SidebarBrand(id, title, subtitle, logoUrl)` and
`SidebarProfile(id, name, subtitle, pictureUrl)`.

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

## The two ends of the rail

The row that says what the application is and the row that says who is signed in
are components of their own — `SidebarBrand` (`sidebar-brand.md`) and
`SidebarProfile` (`sidebar-profile.md`). Both are the same shape: twice the height
of an ordinary row, a picture against two lines of text, and their commands drawn
at rest rather than under the pointer, because a gear that only appears on hover
is one nobody finds. Reach for them instead of a `SidebarButton` with a stylesheet
on top — that is the thing they replace.

```csharp
sidebar.AddHeader(new SidebarBrand("brand", App.Name, workspace.Name, App.LogoUrl)
    .Separated()
    .Configure(() => Router.Navigate("#/manage"))
    .WithSidebarControl(onOpen:  () => sidebar.IsClosed = false,
                        onClose: () => sidebar.IsClosed = true));

sidebar.AddFooter(new SidebarProfile("account", user.FullName, user.Email, user.PhotoUrl)
    .Separated()
    .Settings(() => Router.Navigate("#/preferences"))
    .Logout(Auth.Logout));
```

`.Separated()` runs a divider out to the sidebar's own edges — below the row in the
header, above it in the footer. `SidebarBrand.WithSidebarControl(...)` makes the
brand the rail's open/close control: the last command on the row while it is open,
and the logo itself — swapped for the open button under the pointer — once it is
closed and there is no room for a command beside it.

## The shortcut that presses a button

`SidebarButton.SetKeyboardShortcut("Ctrl", "Shift", "O")` shows the shortcut as a
chip at the button's far end *and* answers it for as long as the button is on
screen, so the chip is a promise rather than a note. The keys are the ones
`KeyboardShortcut` displays (`keyboard-shortcut.md`), so `Ctrl` is the platform's
command modifier: the chip above reads Ctrl+Shift+O and triggers on ⌘⇧O on a Mac.

```csharp
chatBar.AddHeader(new SidebarButton("new-chat", UIcons.Edit, "New chat")
    .Rounded()
    .SetKeyboardShortcut("Ctrl", "Shift", "O")
    .OnClick(StartNewChat));
```

The key presses the button, so anything hooked to it — the click handler, an
`href` wrapper — is reached by the keyboard exactly as it is by the pointer. Only
the open button carries the chip; the closed rail has room for the glyph and
nothing else. A collapsed button holds no shortcut either, because there is
nothing on screen for the key to press.

`.AsSearchBox(keys)` shows the same chip *without* binding the key: the palette or
page it opens owns that key, and answers it from inside a text field too, where a
button's shortcut steps aside. Pass no keys and call `.SetKeyboardShortcut(...)`
when the button itself should answer.

`.ShortcutOnlyOnHover()` — on both `SidebarButton` and `SidebarSearchBox` — keeps
the chip out of sight until the pointer is on the row or something inside it has
focus, so a row reached by tabbing shows its key too. It is for a rail where most
rows carry one: all the chips at once read as a column of noise, while the chip on
the row being pointed at still gets the key discovered. The binding is untouched
and the chip's room stays reserved, so no label re-flows as it fades in. Pass
`false` for the default, a chip that is always there.

```csharp
sidebar.AddContent(new SidebarButton("home", UIcons.Home, "Home")
    .SetKeyboardShortcut("Ctrl", "Shift", "H")
    .ShortcutOnlyOnHover()
    .OnClick(GoHome));
```

A `.Selected()` item is outlined in the theme's primary color and filled with a
wash of it, rather than with the grey a hover uses — so where you are still
reads while the pointer is somewhere else in the list. It follows
`Theme.SetPrimary(...)`, so an app's own brand color is what marks its current
page.

## A selection is never hidden

A collapsed `SidebarNav` **expands itself when a child (or a nested descendant)
becomes selected**, and scrolls the item into view — so the row that marks the
current page can't sit invisible inside a closed group. Only selection *changes*
trigger it: the user can still collapse the group by hand while the same item
stays selected. Observe selection yourself through `SidebarButton.SelectedStatus`
/ `SidebarNav.SelectedStatus` (`IObservable<bool>`), and opt a group out of the
automatic expansion with `.KeepCollapsedOnSelection()`.

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
- `.ShortcutOnlyOnHover(bool = true)` — hide that chip until the box is hovered
  or holds the caret; the key still works either way.
- `.Rounded(BorderRadius = Full)` — render as a full bordered, rounded "pill".
- `.Text` / `.SetText(text)` — read or replace what is in the box. Setting it
  does not raise `.OnSearch(...)`, so a caller that clears the box decides for
  itself what to do about the results.
- `.Focus()` — put the caret in the box.

It is an `ISidebarItem`, so it is normally added to a `Sidebar`; `.RenderOpen()`
gives you the box as an `IComponent` when the list it filters is drawn by a
component of your own rather than by the sidebar itself.

## Commands on a row

A `SidebarCommand` is a small icon button that lives on the right of a
`SidebarButton` — rename, pin, close, search. Pass them to the button's
constructor after the text:

```csharp
new SidebarButton("workspace", new ImageIcon(logoUrl), "Technical Support",
    new SidebarCommand(UIcons.Search).OnClick(() => palette.Open()).Tooltip("Search"),
    new SidebarCommand(UIcons.AngleLeft).OnClick(() => sidebar.ShiftBack()))
```

They are drawn over the row rather than in it, and appear while the pointer is
on that row (or while it is selected). On a hovered row a command is a bare
glyph — the row is already painted with the hover colour, so a command carrying
the same fill would disappear into it — and the command's *own* hover is a step
deeper than the row's, toward the text colour, so it darkens in a light theme and
lightens in a dark one.

Where the commands are permanent the label is laid out beside them: a row with
`.CommandsAlwaysVisible()`, or a selected row, keeps room for exactly as many
commands as it has, so a long name truncates with an ellipsis before the strip
rather than running under it.

On **hover** the label keeps the full width of the rail and nothing re-flows as
the pointer travels the list. Instead the label fades out into the row where the
commands begin — a gradient in the row's own colour, so it works in either theme
— and the icons sit on clean background. A name short enough to end before the
commands is untouched.

Two custom properties tune that, both set **on the row** (not on the command
strip, since the label's own reservation reads the inset too):

```css
.my-brand-row {
    --tss-sidebar-commands-inset: 12px;  /* move the strip in from the edge   */
    --tss-sidebar-label-fade: 28px;      /* how long the label's fade-out is  */
}
```

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

## Responsive: sidebar on desktop, navbar on mobile

`Theme.EnableMobileDetection()` adds and removes the `tss-mobile` class as the
viewport crosses a breakpoint, and the mobile stylesheet turns a `.tss-page-layout`
shell into a column with the navbar pinned to the top. Follow
`Theme.OnMobileModeChanged` and switch the whole shell, rather than picking a layout
once at startup — the stylesheet reshapes what is on the page either way, so a row
built for desktop is still a row in C# when the CSS starts drawing it as a column:

```csharp
Theme.EnableMobileDetection(breakpoint: 768);

var sidebar = Sidebar();
var content = VStack().S().ScrollY().Children(/* ... */);

// The sidebar gets no inline width: .tss-sidebar sizes it on desktop and the
// navbar rules override both axes on mobile.
var shell = HStack().Class("tss-page-layout").S().Children(sidebar.HS(), content);

void ApplyLayoutMode(bool isMobile)
{
    sidebar.AsNavbar(isMobile);

    // The 1px is the flex basis the grow expands from, so it has to sit on the axis
    // the container measures — and the other axis has to be restated, or the value
    // left over from the previous mode becomes a real size on the cross axis.
    if (isMobile) { shell.Vertical();   content.WS().H(1).Grow(); }
    else          { shell.Horizontal(); content.HS().W(1).Grow(); }
}

ApplyLayoutMode(Theme.IsMobileMode);
Theme.OnMobileModeChanged += () => ApplyLayoutMode(Theme.IsMobileMode);
```

## Sidebar as a page (phones)

`.AsPage()` is the other mobile layout: no rail and no drawer — the sidebar is a
page of its own that takes turns with the content. While it is a page it is always
open (the closed state is kept for when it stops being one, and the brand's
close-the-rail command is hidden), it fills its container, and everything *after*
it in that container is hidden.

- Picking a row steps it aside by itself (`.ShowContent()`): a click on a
  `SidebarButton` row, including one in a shifted child sidebar. Commands, a nav
  group's header and arrow, search boxes and the brand/profile rows do not — they
  act on the sidebar rather than leave it. A Ctrl/Cmd/Shift-click opens a new tab
  and leaves the page where it is.
- Something the sidebar cannot see — a route change, a list inside a
  `SidebarComponent` — calls `.ShowContent()` itself.
- `SidebarPageBar(sidebar)` (see `sidebar-page-bar.md`) goes at the top of the
  content: a back button that calls `.ShowSidebar()`, a brand and the page title.
  It collapses itself while its sidebar is not a page, so it can stay mounted.
- A group (`SidebarNav`) with children does not expand in place: pressing its header or
  arrow opens its children as a panel over the sidebar (`calc(100% - 48px)` wide, from
  the right) above a dark backdrop that closes it. The panel's title presses the group's
  own header when it has an `OnClick`; a group inside the panel stacks another panel.
- A sidebar the app has `Collapse()`d leaves the content on its own.

```csharp
Theme.EnableMobileDetection(breakpoint: 768);

var sidebar = Sidebar();
var pageBar = SidebarPageBar(sidebar).Brand(Image(logoUrl).W(24).H(24));
var content = VStack().Children(pageBar, page.H(10).Grow()).HS().W(1).Grow();
var shell   = HStack().S().Children(sidebar.HS(), content);

void ApplyLayoutMode(bool isMobile)
{
    sidebar.AsPage(isMobile);
    if (isMobile) sidebar.ShowContent(); // open on whatever the address says
}

ApplyLayoutMode(Theme.IsMobileMode);
Theme.OnMobileModeChanged += () => ApplyLayoutMode(Theme.IsMobileMode);
```

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

- SidebarBrand — `sidebar-brand.md`
- SidebarProfile — `sidebar-profile.md`
- SidebarSeparator — `sidebar-separator.md`
- Sidenav (icon-only rail) — `sidenav.md`
- Full docs & API: `/tesserae/components/sidebar`
