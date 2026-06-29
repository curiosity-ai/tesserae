---
name: command-palette
description: A keyboard-driven full-screen command launcher (Ctrl/Cmd-K style) with search, nesting, and per-action shortcuts. Use when adding a quick-action/command search overlay to a Tesserae (C#/h5) app.
---

# CommandPalette

A Layer-based overlay that lets users search and invoke actions by keyboard. Supports nested actions (breadcrumbs), per-action shortcuts, sections, and a global Ctrl/Cmd-K toggle bound to a host component's lifetime.

## Create

`new CommandPalette(IComponent host, IEnumerable<CommandPaletteAction> actions = null)` — also via `UI.CommandPalette(host, params CommandPaletteAction[] actions)`. The `host` controls listener lifetime: the global shortcut is attached when host mounts, detached when removed. Bring factories into scope with `using static Tesserae.UI;`.

## Key configuration

CommandPalette:
- `.AddAction(action)` / `.SetActions(actions)` — manage the action list.
- `.Open()` / `.Close()` / `.Toggle()` — control visibility.
- `.Placeholder` — search box hint text.
- `.GlobalShortcutKey` (default `"k"`), `.EnableGlobalShortcut`, `.EnableGlobalActionShortcuts`, `.HideOnAction`.
- `.ActionExecuted` event — fires after an action runs.

`CommandPaletteAction(string id, string name)` properties:
- `Perform` (`Action`), `Subtitle`, `Keywords`, `Section`, `Icon` (`UIcons?`).
- `Shortcut` (`string[]`) — global keys that fire the action directly.
- `ParentId` — set to another action's `Id` to nest under it (parent acts as a submenu).
- `IsEnabled`, `IsVisible`.

## Example

```csharp
using static Tesserae.UI;

var nav  = new CommandPaletteAction("nav", "Navigate");
var home = new CommandPaletteAction("home", "Go to Home") { ParentId = "nav", Perform = () => Toast().Success("Home") };
var help = new CommandPaletteAction("help", "Help Center")
{
    Perform = () => Toast().Success("Help"),
    Shortcut = new[] { "?" }, Section = "Actions", Icon = UIcons.CommentsQuestion
};

CommandPalette palette = null;
var ui = Button("Open").OnClick(() => palette.Open());
palette = new CommandPalette(ui, new[] { nav, home, help });
```

## Related

- Keyboard shortcut chips — `../keyboard-shortcut/SKILL.md`
- Toast — `../toast/SKILL.md`
- Full docs & API: `/tesserae/utilities/command-palette`
