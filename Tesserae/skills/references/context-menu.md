---
name: context-menu
description: A click/right-click popup menu of commands with headers, dividers, disabled items and nested submenus. Use when surfacing contextual actions relative to an element or coordinates in a Tesserae (C#/Transpose) app.
---

# ContextMenu

A popup menu of commands shown relative to a target element or at explicit
coordinates. Supports headers, dividers, disabled items and arbitrarily deep
submenus, with arrow-key navigation and Esc to dismiss.

## Create

`ContextMenu()` — returns a `ContextMenu`. Items are built with
`ContextMenuItem(...)`. Bring factories into scope with
`using static Tesserae.UI;`.

## Key configuration

- `.Items(params Item[])` — add the menu entries.
- `ContextMenuItem("Text")` / `ContextMenuItem(IComponent)` — an entry.
- `.OnClick((s, e) => ...)` or `.OnClick(() => ...)` — item click handler.
- `.Divider()` / `.Header()` — turn an item into a divider or section header.
- `.Disabled(bool = true)` — disable an item.
- `.SubMenu(ContextMenu cm)` — attach a nested menu that opens on hover or ArrowRight; nest as deep as needed.
- `.ShowFor(IComponent or HTMLElement, distanceX = 1, distanceY = 1)` — show anchored to a target (auto-hides when the target is removed).
- `.ShowAt(x, y, minWidth)` — show at screen coordinates.
- `.Hide()` / `.OnHide(Action)` — dismiss / hide callback.
- `.SidePanel(IComponent)` — free content beside the rows, inside the same popup (details about the
  target, actions that are not menu rows). Call it before showing the menu. The panel is not a row: the
  arrow keys walk past it and a click inside it does not close the menu, so an action in it calls
  `menu.Hide()` itself. With a panel, the `minWidth` of `ShowAt` (or the target's width for `ShowFor`)
  sizes the rows' column, and the panel takes its own width.

## Behaviour

- **Submenus track the pointer.** Moving from a row towards the submenu it opened keeps it open
  across the rows in between; resting on another row for about 300ms switches to it. Returning to
  the row that opened the submenu does not re-open it. A third level stays open while the pointer
  is anywhere in the branch.
- A submenu opens beside its parent, top-aligned with its row; to the left of the parent when there
  is no room on the right.
- **Keyboard** (deepest open level only): ArrowUp/ArrowDown and Home/End move between rows,
  ArrowRight or Enter open a submenu and focus its first row, ArrowLeft or Escape close one level
  and return to the row that opened it; Escape at the top level closes the menu. Rows built from a
  component are focusable like text rows, and Enter or Space on one clicks the component.
- **Opened from the keyboard** (Enter or Space on the trigger, which then matches `:focus-visible`),
  `ShowFor` / `ShowAt` move the focus to the first row, as the WAI-ARIA menu button pattern does;
  opened with the pointer the focus stays put until an arrow key. A text field being typed in never
  counts, so a menu shown under one does not take the caret.
- When the menu closes (Escape, an item chosen, a click outside) the focus goes back to whatever had
  it when the menu opened, if the focus is not already somewhere else on purpose.

## Example

```csharp
using static Tesserae.UI;

var menu = ContextMenu().Items(
    ContextMenuItem("New").OnClick((s, e) => Toast().Information("New")),
    ContextMenuItem().Divider(),
    ContextMenuItem("Edit").OnClick(() => Toast().Information("Edit")),
    ContextMenuItem("Delete").Disabled()
);

Button btn = null;
btn = Button("Open menu").OnClick((s, e) => menu.ShowFor(btn));
```

### With a side panel

```csharp
var menu = ContextMenu();

menu.Items(
        ContextMenuItem("Open").OnClick(() => Toast().Information("Open")),
        ContextMenuItem("Share").OnClick(() => Toast().Information("Share")))
   .SidePanel(VStack().W(260).P(12).Children(
        TextBlock("report-2026.pdf").SemiBold(),
        Button("Copy link").Link().NoPadding().OnClick(() => { Toast().Success("Copied"); menu.Hide(); })))
   .ShowAt(x, y, 220);
```

## Related

- Dialog — `dialog.md`
- Panel — `panel.md`
- ContextCard — attaches one of these to a card's right-click via `OnContextMenu` — `context-card.md`
- Full docs & API: `/tesserae/commands/context-menu`
