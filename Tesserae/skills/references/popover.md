---
name: popover
description: A manually-triggered, anchored overlay surface that shows arbitrary IComponent content next to an anchor. Use when building menus, dropdowns, or click-triggered transient surfaces in a Tesserae (C#/Transpose) app.
---

# Popover

A reusable overlay positioned against an anchor (delegated to Tippy/Popper). Unlike `.Tooltip(...)` (hover), a popover is shown imperatively with `ShowFor(...)` and stays open until `Hide()`, an outside click, or Escape. It is the primitive under menus, comboboxes, and pickers.

## Create

`UI.Popover()` or `UI.Popover(IComponent content)` — returns a `Popover` (not a component; you do not render it directly).
Bring factories into scope with `using static Tesserae.UI;`.

## Key configuration

- `.Content(IComponent)` — content to display (required before showing).
- `.ShowFor(IComponent anchor)` / `.ShowFor(HTMLElement anchor)` — show anchored to a target.
- `.Hide()` — close; `.IsVisible` — current state.
- `.Placement(TooltipPlacement)` — preferred side (e.g. `TooltipPlacement.BottomStart`); auto-flips if no room.
- `.Arrow(bool = true)` — show the pointer arrow.
- `.MaxWidth(int pixels)` — surface max width (default 350).
- `.HideOnClickOutside(bool = true)` / `.HideOnEscape(bool = true)` — dismissal behaviour (both on by default).
- `.OnShown(Action)` / `.OnHidden(Action)` — lifecycle callbacks.
- `.OnBeforeHide(Func<bool>)` — return `false` to cancel a hide.
- `.DelayShow(ms)` / `.DelayHide(ms)`, `.Animation(TooltipAnimation)`, `.Theme(string)`.

## Example

```csharp
using static Tesserae.UI;

var popover = Popover()
    .Content(Stack().Children(
        TextBlock("Hello from the popover").SemiBold(),
        TextBlock("Dismisses on outside click.").Small()
    ).Padding(12.px()))
    .Placement(TooltipPlacement.BottomStart)
    .Arrow();

var anchor = Button("Show popover").OnClick(b => popover.ShowFor(b));
```

## Related

- Menu — `.menu.md`
- Full docs & API: `/tesserae/components/popover`
