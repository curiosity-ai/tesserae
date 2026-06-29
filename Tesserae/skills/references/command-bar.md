---
name: command-bar
description: A horizontal toolbar of command buttons split into near (left) and far (right) sections. Use when building a page or item action bar in a Tesserae (C#/h5) app.
---

# CommandBar

A horizontal bar of commands anchored to the top of a surface. Items go in the
primary (left) section; "far" items align to the right. Items are usually
`CommandBarItem`s but any `IComponent` works (e.g. a `SearchBox`).

## Create

`UI.CommandBar(params IComponent[] items)` — the bar. `UI.CommandBarItem(string text = null, UIcons? icon = null)` — a single button.
Bring factories into scope with `using static Tesserae.UI;`.

## Key configuration

CommandBar:

- `.AddItem(item)` / `.AddItems(params)` / `.Items(params)` — add to the left section.
- `.AddFarItem(item)` / `.AddFarItems(params)` / `.FarItems(params)` — add to the right section.

CommandBarItem:

- `.OnClick(Action)` — click handler.
- `.SetText(text)` / `.Text` — label.
- `.SetIcon(UIcons)` / `.Icon` — icon.
- `.Primary(bool = true)` — primary tone.
- `.Disabled(bool = true)` / `.IsEnabled` — enable/disable.

## Example

```csharp
using static Tesserae.UI;

var bar = CommandBar(
    CommandBarItem("New", UIcons.Plus).Primary().OnClick(() => Toast().Success("New")),
    CommandBarItem("Edit", UIcons.Edit).OnClick(() => Toast().Success("Edit")),
    CommandBarItem("Delete", UIcons.Trash).OnClick(() => Toast().Success("Delete"))
).FarItems(
    SearchBox().SetPlaceholder("Search...").Width(200.px()),
    CommandBarItem("Settings", UIcons.Settings).OnClick(() => Toast().Information("Settings"))
);
```

## Related

- Button — `button.md` (alternative for standalone actions)
- Full docs & API: `/tesserae/components/command-bar`
