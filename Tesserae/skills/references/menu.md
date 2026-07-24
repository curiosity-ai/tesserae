---
name: menu
description: A popover menu of clickable items, headers, dividers and arbitrarily deep submenus, anchored to a trigger element. Use when building dropdown or action menus opened on an explicit click in a Tesserae (C#/Transpose) app.
---

# Menu

A menu surface built on the `Popover` primitive (so positioning, click-outside
dismissal and animation are inherited). Use it for dropdown / action menus shown
on a button click. For right-click context menus use `ContextMenu` instead.

## Create

`Menu()` — an empty menu; add entries with `.Items(...)`. Build entries with the
factories:

- `MenuItem(string text)` / `MenuItem(string text, UIcons icon)` — a clickable row.
- `MenuHeader(string text)` — a non-interactive section header.
- `MenuDivider()` — a thin separator.

Bring the factories into scope with `using static Tesserae.UI;`.

## Key configuration

Menu:
- `.Items(params Item[])` — add entries in order.
- `.ShowFor(IComponent anchor)` / `.ShowFor(HTMLElement)` — open anchored to a trigger.
- `.Hide()` / `.IsVisible` — close / query state.
- `.Placement(TooltipPlacement)` — default `BottomStart`.
- `.OnHidden(Action)` — callback after dismissal.

Item:
- `.OnClick(Action)` — activation handler (closes the whole menu stack).
- `.SubMenu(Menu submenu)` — nested submenu, opened on hover/focus; nest arbitrarily deep.
- `.Disabled(bool = true)` — non-interactive row.

## Example

```csharp
using static Tesserae.UI;

var menu = Menu().Items(
    MenuHeader("File"),
    MenuItem("New",  UIcons.AddDocument).OnClick(() => New()),
    MenuItem("Open", UIcons.FolderOpen).OnClick(() => Open()),
    MenuDivider(),
    MenuItem("Export").SubMenu(Menu().Items(
        MenuItem("PDF").OnClick(() => ExportPdf()),
        MenuItem("CSV").OnClick(() => ExportCsv())))
);

var trigger = Button("File").OnClick(b => menu.ShowFor(b));
```

## Related

- Popover — `/tesserae/components/popover`
- ContextMenu — `/tesserae/surfaces/context-menu`
- Full docs & API: `/tesserae/components/menu`
