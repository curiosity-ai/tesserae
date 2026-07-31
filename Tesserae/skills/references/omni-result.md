---
name: omni-result
description: A search-result row with an icon tile tinted from one color, a title with a badge, an excerpt whose matched terms are highlighted, a footer naming the source, selectable checkboxes, right-click or button commands, and an optional fanning page preview. Use for search result lists, pickers and file browsers in a Tesserae (C#/Transpose) app.
---

# OmniResult&lt;T&gt;

`OmniResult<T>` is the row one search hit is drawn as. It carries the result it stands for as
`Result`, so a click, selection or command handler shared by a whole list acts on the right hit
without a closure per row:

```
[✓]  [PDF]  BRK-SEN-447 calibration procedure.pdf   3 matches in text                 [pages]  [...]
            Torque the mount to 12 Nm before starting brake sensor work. Full calibration steps …
            ▪ Box · sample-files / pdfs · 2.4 MB · Pius Neuhaus · Apr 12, 2024
```

Everything past the title is optional. Drop the excerpt, the preview, the footer, or all three, and
the row tightens up — the same component covers a two-line picker row and a full result card.

## Create

`UI.OmniResult(result, title = null)` — returns an `OmniResult<T>`, where `T` is whatever the row
stands for. Also `new OmniResult<T>(result, title)`. Bring factories into scope with
`using static Tesserae.UI;`.

## Key configuration

**Title, badge, excerpt**

- `.SetTitle(string)` / `Title` — one line, ellipsized, with the full text as its tooltip.
- `.SetBadge(string)` — the quiet pill next to the title ("3 matches in text"). Null or empty hides it.
- `.SetBadge(IComponent)` — a `Badge` with a tone of its own, a `Spinner`, a small button.
- `.SetText(string)` / `Text` — the excerpt, as **plain text**, ellipsized to two lines.
- `.TextLines(int)` — how many lines the excerpt gets before it is ellipsized.
- `.HighlightWords(params string[])` — mark those words in the excerpt, case-insensitively.
- `.Highlight(Regex)` / `.Highlight(string pattern, bool ignoreCase = true)` — mark every match, e.g.
  the pattern a search backend hands back. Matching runs against the text and each match is wrapped in
  its own element, so an excerpt containing angle brackets renders them instead of obeying them.

**Icon tile**

- `.SetIcon(UIcons icon, string color = null, UIconsWeight weight = Regular)` — the glyph in `color`,
  over a wash computed from that same color: a pale tint under a light theme, a deep one under a dark
  theme. Both variants are written to the element, so flipping the theme at runtime needs no redraw,
  and the computed pair is cached per color (a list drawing one color per file type only pays once).
- `.SetIcon(string text, string color = null)` — a short type name ("PPTX", "CSV") in place of a glyph.
- `.SetIcon(IComponent, string color = null)` — an `Image` thumbnail, an `Avatar`, an emoji.

Pass a literal color (`"#ef4444"`) rather than a CSS variable when you want the tint to track the
theme: a `var(--…)` is resolved once, at the time it is set.

**Footer**

- `.SetSource(string color, string text)` — a small rounded square in that color plus the text, at the
  footer's start. Null or empty text drops it.
- `.SetFooterEntries(params string[])` / `.SetFooterEntries(params IComponent[])` — the metadata after
  the source: a path, a size, an owner, a date. Dots between entries are drawn by CSS, so nothing has
  to interleave separators, and a footer with no source never starts with one.

**Selection**

- `.Selectable(OmniResultSelectionMode mode = OnHoverBeforeIcon)` / `.NotSelectable()`.
  Modes: `OnHoverBeforeIcon`, `OnHoverOverIcon` (on the tile, which fades out under it),
  `AlwaysBeforeIcon`, `ReplacingIcon` (no tile at all). A selected row always shows its checkbox,
  whatever the mode, and the column is reserved either way so revealing it never shifts the row.
- `IsSelected` / `.Selected(bool = true)` / `IsSelectionEnabled`.
- `.OnSelectionChanged(Action<OmniResult<T>, bool>)` — every change, from the user or from code.
- `.OnRangeSelectionRequested(Action<OmniResult<T>>)` — shift-click. A row knows nothing about its
  siblings, so the host list decides what "between" means and selects them itself.
- Ctrl-click toggles the row; Space toggles the focused row; Enter activates it (`OnClick`).
- `IsActive` — styles the row like a hovered one, for a keyboard-driven list.

**Commands**

- `.OnContextMenu(Func<OmniResult<T>, ContextMenu.Item[]> menu, OmniResultCommandsMode mode = RightClickOnly)`
  — the row builds and places the menu itself, at the pointer or under the button.
- `.OnContextMenu(Action<OmniResult<T>> handler, OmniResultCommandsMode mode = RightClickOnly)` — a
  plain handler; it can still place a menu with `.ShowMenu(ContextMenu)`, which uses the pointer
  position when the row was right-clicked and the button when it was pressed.
- `OmniResultCommandsMode`: `RightClickOnly` (no button drawn), `ButtonOnHover`, `ButtonAlwaysVisible`.
  `.CommandsMode(mode)` changes it later without touching the handler.
- `.InlineCommands(params IComponent[])` / `.InlineCommands(OmniResultCommandsVisibility visibility, params IComponent[])`
  — one or two buttons before the `[...]`, `OnHover` (default) or `AlwaysVisible`. The space is
  reserved either way.

**Page preview**

- `.SetPages(PagesStack)` — pinned to the row's end, inside a rail wide enough for the fan (see
  `pages-stack.md`).
- `.PagesFanOnHover(bool = true)` — the stack fans while the whole row is hovered, not only while the
  pointer is over the pages. On by default.

## Example

```csharp
using static Tesserae.UI;

var list  = VStack().WS();
var terms = new[] { "brake sensor", "calibration" };

foreach (var hit in results)
{
    var row = OmniResult(hit, hit.Name)                        // T is whatever `hit` is
        .SetIcon(UIcons.FilePdf, "#ef4444")
        .SetBadge($"{hit.Matches} matches in text")
        .SetText(hit.Excerpt)
        .HighlightWords(terms)
        .SetSource("#0061d5", hit.Source)
        .SetFooterEntries(hit.Path, hit.Size, hit.Owner, hit.Modified)
        .SetPages(PagesStack(5).TotalPages(hit.Pages))
        .Selectable(OmniResultSelectionMode.OnHoverBeforeIcon)
        .OnSelectionChanged((r, isSelected) => RefreshActionBar())
        .OnRangeSelectionRequested(r => SelectRangeTo(r))       // the host owns the range
        .OnClick((r, _) => Open(r.Result))
        .OnContextMenu(r => new[]
        {
            ContextMenuItem(r.Result.Name).Header(),
            ContextMenuItem("Open").OnClick(() => Open(r.Result)),
            ContextMenuItem().Divider(),
            ContextMenuItem("Delete").OnClick(() => Delete(r.Result))
        }, OmniResultCommandsMode.ButtonOnHover)
        .InlineCommands(Button(UIcons.Download).Tooltip("Download").OnClick(() => Download(hit)));

    list.Add(row);
}
```

A picker row — no excerpt, no preview, the checkbox always in place:

```csharp
var pick = OmniResult(file, file.Name)
    .SetIcon("XLSX", "#16a34a")
    .SetSource("#0061d5", "Box")
    .SetFooterEntries(file.Path, file.Size)
    .Selectable(OmniResultSelectionMode.AlwaysBeforeIcon);
```

## Related

- PagesStack — the page preview it takes — `pages-stack.md`
- ContextMenu — the menu the commands open, and its items — `context-menu.md`
- OmniBox — the search input these rows usually answer — `omni-box.md`
- ResourceCard (the larger, tile-shaped resource summary) — `resource-card.md`
- ContextCard (the compact chat-attachment card) — `context-card.md`
- Badge — what `SetBadge(IComponent)` takes — `badge.md`
- CheckBox — the selection control — `check-box.md`
- DetailsList / SearchableList (when the results are really a table or a grid) — `details-list.md`, `searchable-list.md`
- Full docs & API: `/tesserae/components/omni-result`
