---
name: command-palette
description: A keyboard-driven full-screen command launcher (Ctrl/Cmd-K style) with search, nesting, per-action shortcuts, and rows of your own - search results drawn as OmniResults - above the actions. Use when adding a quick-action or search overlay to a Tesserae (C#/Transpose) app.
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
- `.SetResults(...)` / `.OnSearch(...)` / `.ResultActivated` — rows of your own, below.
- `.CurrentQuery` — what is typed in the search box right now, trimmed.

`CommandPaletteAction(string id, string name)` properties:
- `Perform` (`Action`), `Subtitle`, `Keywords`, `Section`, `Icon` (`UIcons?`).
- `Shortcut` (`string[]`) — global keys that fire the action directly.
- `ParentId` — set to another action's `Id` to nest under it (parent acts as a submenu).
- `IsEnabled`, `IsVisible`.

## Rows of your own

A palette that has to *answer* a question rather than list commands puts its own rows above the actions.
A row is any `IComponent` — usually the `OmniResult` (`omni-result.md`) a search page would draw, so a
result looks the same wherever it is shown.

- `.SetResults(IEnumerable<CommandPaletteResult>)` (also `params`) — replace the rows. They are shown as
  given: the palette does not filter them, because whoever produced them for a query already knows which
  ones answer it. The actions beneath them are still filtered as usual.
- `.OnSearch(Func<string, Task<IEnumerable<CommandPaletteResult>>> search, int debounceMs = 200)` — have
  the rows refreshed as the query changes. Debounced, and an answer that arrives after a newer query was
  typed is dropped, so a slow search never overwrites a faster one behind it.
- `new CommandPaletteResult(IComponent component, Action activate = null)` — the row and what Enter does
  with it, plus a `Section` heading like an action's. With no `activate` the row is only clickable, which
  is what a component that already answers its own click wants.

Rows join the arrow-key walk like any other item, and the palette closes after one is activated
(`HideOnAction`). The last row is usually the way out — "show all results" — onto the page that can show
what the palette only had room to preview.

```csharp
palette.OnSearch(async query =>
{
    var hits = await SearchAsync(query);

    var rows = hits.Take(5).Select(hit => new CommandPaletteResult(
        OmniResult(hit, hit.Title).SetIcon(hit.Extension, hit.Color).Highlight(query),
        () => Open(hit)) { Section = "Results" }).ToList();

    rows.Add(new CommandPaletteResult(TextBlock($"Show all results for \"{query}\""),
                                      () => Router.Navigate(SearchUrl(query))));

    return rows;
});
```

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

- OmniResult — what a row of your own usually is — `omni-result.md`
- Keyboard shortcut chips — `keyboard-shortcut.md`
- Toast — `toast.md`
- Full docs & API: `/tesserae/utilities/command-palette`
