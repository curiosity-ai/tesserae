---
name: context-menu
description: A click/right-click popup menu of commands with headers, dividers, disabled items and nested submenus. Use when surfacing contextual actions relative to an element or coordinates in a Tesserae (C#/h5) app.
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
- `.SubMenu(ContextMenu cm)` — attach a nested menu that opens on hover.
- `.ShowFor(IComponent or HTMLElement, distanceX = 1, distanceY = 1)` — show anchored to a target (auto-hides when the target is removed).
- `.ShowAt(x, y, minWidth)` — show at screen coordinates.
- `.Hide()` / `.OnHide(Action)` — dismiss / hide callback.

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

## Related

- Dialog — `dialog.md`
- Panel — `panel.md`
- Full docs & API: `/tesserae/surfaces/context-menu`
