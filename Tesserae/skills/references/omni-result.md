---
name: omni-result
description: A search-result row with an icon tile tinted from one color, an optional identifier before the title, a badge, an excerpt whose matched terms are highlighted, a rich content preview, a footer naming the source, selectable checkboxes, right-click or button commands, a fanning page preview, and a modal it can open as. Use for search result lists, pickers and file browsers in a Tesserae (C#/Transpose) app.
---

# OmniResult&lt;T&gt;

`OmniResult<T>` is the row one search hit is drawn as. It carries the result it stands for as
`Result`, so a click, selection or command handler shared by a whole list acts on the right hit
without a closure per row:

```
[✓]  [PDF]  JR-2214 › BRK-SEN-447 calibration.pdf   3 matches in text                 [pages]  [...]
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

**Identifier, title, badge, excerpt, content**

- `.SetId(string)` / `Id` — an identifier before the title (an issue number, a ticket key, a row
  number), drawn quietly and followed by a chevron pointing at the title. Null or empty drops both.
- `.SetTitle(string)` / `Title` — one line, ellipsized, with the full text as its tooltip.
- `.SetTitle(IComponent, string text = null)` — the escape hatch for a title that genuinely isn't
  text, such as one built from fields an administrator configured. The `text` alongside it stays the
  row's `Title`, so the tooltip and the modal header still have something to say; `Highlight` does not
  reach inside a component title.
- `.SetBadge(string)` — the quiet pill next to the title ("3 matches in text"). Null or empty hides it.
- `.SetBadge(IComponent)` — a `Badge` with a tone of its own, a `Spinner`, a small button.
- `.SetText(string)` / `Text` — the excerpt, as **plain text**, ellipsized to two lines.
- `.TextLines(int)` — how many lines the excerpt gets before it is ellipsized.
- `.SetContent(IComponent)` — a rich preview under the excerpt, in the text column: a thumbnail, a
  quoted message, a table of the fields that matched. Null takes it away.
- `.ContentMaxHeight(UnitSize)` — caps how tall that preview may grow, fading the overflow out rather
  than cutting it off. Null un-caps it.
- `.HighlightWords(params string[])` — mark those words in the **title and the excerpt**,
  case-insensitively. The marks and the badge share one pair of colors, from the `--tss-highlight-color`
  token (the same value as `--tss-link-color`, so `Theme.SetPrimary` moves both), and the excerpt
  itself is a quiet grey.
- `.Highlight(Regex)` / `.Highlight(string pattern, bool ignoreCase = true)` — mark every match, e.g.
  the pattern a search backend hands back. Matching runs against the text and each match is wrapped in
  its own element, so text containing angle brackets renders them instead of obeying them.

**Icon tile**

- `.SetIcon(UIcons icon, string color = null, UIconsWeight weight = Regular)` — the glyph in `color`,
  over a wash computed from that same color: a pale tint under a light theme, a deep one under a dark
  theme. Both variants are written to the element, so flipping the theme at runtime needs no redraw,
  and the computed pair is cached per color (a list drawing one color per file type only pays once).
- `.SetIcon(string text, string color = null)` — a short type name ("PPTX", "CSV") in place of a glyph.
- `.SetIcon(IComponent, string color = null)` — an `Image` thumbnail, an `Avatar`, an emoji.
- `.SetIconBadge(IComponent badge, OmniResultBadgeCorner corner = BottomRight)` — a marker pinned to a
  corner of the tile, drawn outside its clipping: where the result came from, that it is pinned.
  Corners: `TopLeft`, `TopRight`, `BottomLeft`, `BottomRight`. Null clears that corner.

Pass a literal color (`"#ef4444"`) rather than a CSS variable when you want the tint to track the
theme: a `var(--…)` is resolved once, at the time it is set.

**Footer**

The source leads the line and the metadata follows it, and all of it is `InlineLabel`s
(`inline-label.md`) — so the source answers the pointer the same way the entries beside it do.

- `.SetSource(string color, string text, Action<OmniResult<T>> onClick = null)` — a small rounded square
  in that color plus the text, at the footer's start. Null or empty text drops it. Given a handler the
  source becomes clickable — scoping the search to it is the usual thing to do — with its own tab stop,
  Enter/Space and a hover background; the click never also counts as opening the result.
- `.SetSource(IComponent marker, string text, Action<OmniResult<T>> onClick = null)` — the same, with a
  marker of the host's own (the source's logo, an avatar) in place of the colored square.
- `.OnSourceClick(Action<OmniResult<T>>)` — the same handler on its own (null makes the source plain
  text again). `SetSource` only replaces the handler when it is given one, so the two compose in either
  order.
- `.SetFooterEntries(params InlineLabel[])` — the metadata after the source: a path, a size, an owner,
  a date. Each entry is an `InlineLabel` (`inline-label.md`), so it can carry a mark (a glyph, an image,
  a square of colour), be pressable, or be a real link, and they are all drawn at one size. Dots between
  entries are drawn by CSS, so nothing has to interleave separators, and a footer with no source never
  starts with one. The whole line — text and glyphs alike — is drawn in the secondary text colour, so a
  glyph given an accent of its own elsewhere is muted here; the source's colour square keeps its colour,
  being nothing but a colour.
- `.SetFooterEntries(params string[])` — the same, as plain text.
- `.AddFooterEntry(IComponent)` — one more entry at the end of the line, as a component of your own
  rather than a label: a badge, a chip, a small control. Same box, same separating dot.

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
- `CommandsEvent` — the pointer event that last asked for the commands (null when they were asked for
  from the keyboard), for a host that shows a command surface of its own rather than a `ContextMenu`.

**Contribution bar**

- `.SetContributionBar(ContributionBar)` / `Contribution` — a `ContributionBar` under the footer,
  spanning the text column (so it lines up with the title and the excerpt rather than running under the
  icon and the pages rail). The row's place for a relevance breakdown: how much of the score came from
  the title, the content, recency, how often the document is opened. Its own toggle never counts as a
  click on the row, so `Collapsable()` works inside a row of results. Pass null to take it away.

**Page preview**

- `.SetPages(PagesStack)` — pinned to the row's end, inside a rail wide enough for the fan (see
  `pages-stack.md`).
- `.PagesFanOnHover(bool = true)` — the stack fans while the whole row is hovered, not only while the
  pointer is over the pages. On by default.

**Opening as a modal**

A row can carry the full view of the thing it stands for and open as a modal showing it, so the list
and the detail are one object rather than two that have to be kept in step.

- `.SetModalContent(IComponent)` / `.SetModalContent(Func<OmniResult<T>, Task<IComponent>>)` — what the
  modal shows; the `Func` overload builds it on open, so content nobody asks for is never paid for.
  Null makes the row modal-less again.
- `HasModalContent` — whether it has any, so "this result has no preview" is one check.
- `.ModalSize(UnitSize width, UnitSize height)` — the size it opens at. `Auto` by default.
- `.ModalKeepsIcon(bool = true)` — keeps the icon tile in the modal's header, before the identifier
  and the title, so an opened result still shows what kind of thing it is. Everything the tile
  carries comes with it: the glyph or thumbnail, its tint, and any corner badges. Off by default.
- `.ModalKeepsFooter(bool = true)` — keeps the footer (the source and the metadata beside it) as a
  second line under the title, so where a result came from is still said once it is open. A clickable
  source stays clickable. Off by default.
- `.SetModalHeader(Func<OmniResult<T>, IComponent>)` — replaces the default header (the same
  identifier, chevron and title the row shows, plus whatever the two options above kept) with one
  built from the result — for a header that also carries commands or status beside the title. Null
  goes back to the default.
- `.ModalTitle()` — that default header on its own, to build around.

Both options **copy** what the row drew rather than moving it, so opening a result never takes the
tile or the footer out of the row behind it.
- `.ToModal()` — a `Modal` with that header and content, at that size, or null when the row has no
  modal content. Dismissal, bounds and how it is shown are still the caller's.
- `.CurrentModal` — the modal `ToModal()` last built, or null.
- `.GetModalContentAsync()` — the content on its own, for a host that shows it somewhere other than in
  a modal: a side panel, a page, a pane.

**The modal's chrome**

`ToModal()` puts a standard set of commands at the end of the header and, along the bottom, the
keyboard shortcuts the modal actually answers. Every one of them is opt-in except close and
full-screen, so a modal never offers something the host didn't wire up:

```
[tile]  JR-2214 › BRK-SEN-447 calibration.pdf   [Open in Box ▾]  ‹ 2 of 7 ›  [...]  [⤢]  [✕]
        ▪ Box · sample-files / pdfs · 2.4 MB · Pius Neuhaus · Apr 12, 2024
        …
        Esc Close   ← → Navigate results   Ctrl+↵ Open in source   Shift+↵ Open in a new tab
```

- `.OpenInSource(string name, Action<bool> onOpen, UIcons? icon = null)` — the named button that opens
  the result where it actually lives ("Open in Dropbox", "Reveal in folder"). The `bool` says the user
  asked for a new tab (they shift-clicked, or pressed Shift+Enter).
- `.OpenInSource(string name, Func<T, Uri> url, UIcons? icon = null)` — the same, for a source that is
  an address computed from the result. An address is always opened in a new tab.
- Both take a `Func<IComponent>` instead of a `UIcons?` when the mark is the source's own logo; it is a
  factory so that showing an action twice never moves one element between two places.
- Call it more than once for several: the first stays the button, the rest hang off an arrow beside it
  that opens them as a menu. `.NoOpenInSource()` clears them; `OpenActions` / `CanOpenInSource` read
  them; `.Open(bool inNewTab = false)` runs the primary one from code.
- `.ModalNavigation(Action<OmniResult<T>> onPrevious, Action<OmniResult<T>> onNext, int position = 0, int count = 0)`
  — an `InlinePagination` (`inline-pagination.md`): the ‹ › chevrons with "2 of 7" between them when a
  position and count are given (both 1-based). A null handler greys its chevron out, which is how the
  first and last result say so.
- `.ModalCommands(Action<OmniResult<T>>)` — the `[...]` button; read `CommandsEvent` in the handler to
  place a command surface of the host's own where the user clicked. Null leaves the button out.
- `.ModalFullScreen(Action<OmniResult<T>>)` — what `[⤢]` does; without one it grows the modal to fill
  the window and back. `.NoModalFullScreen()` leaves the button out.
- `.ModalShortcuts(bool = true)` — the shortcut hints along the bottom. On by default.
- `.ModalHeaderCommands()` / `.ModalShortcutsBar()` — those two pieces on their own, to build around.
- `.ModalTitle()` — the default header on its own, to build around.
- `.SetModalHeader(Func<OmniResult<T>, IComponent>)` — replaces the default header (identifier, chevron,
  title, plus whatever `ModalKeepsIcon`/`ModalKeepsFooter` kept) with one built from the result. Null
  goes back to the default; the header commands and the footer are unaffected. `HasModalHeader` says
  whether one was set, so a caller applying a default can tell whether anyone got there first.

Keys the modal answers: **Esc** closes it (left to `ModalStack` when it is one of its sheets),
**←/→** step through the results, **Ctrl+Enter** opens it at its source and **Shift+Enter** opens it
in a new tab — the last two only when an open-in-source action is set. None of them fire while the
focus is in a text field.

```csharp
row.ModalKeepsIcon()
   .ModalKeepsFooter()
   .OpenInSource("Open in Box", inNewTab => OpenInBox(row.Result, inNewTab), UIcons.ArrowUpRightFromSquare)
   .OpenInSource("Open on the web", hit => new Uri(hit.WebUrl), UIcons.Globe)
   .ModalCommands(r => CommandPalette.ShowFor(r.Result, r.CommandsEvent))
   .ModalFullScreen(r => Navigate(Routes.For(r.Result)))
   .ModalNavigation(_ => Step(-1), _ => Step(+1), position: index + 1, count: total);

var modal = row.ToModal();

if (modal is object)
{
    modal.MinWidth(60.vw()).MaxHeight(95.vh());

    ModalStack.Push(row.Result.Id, row.Title, modal);   // or modal.LightDismiss().Show()
}
```

## Example

```csharp
using static Tesserae.UI;

var list  = VStack().WS();
var terms = new[] { "brake sensor", "calibration" };

foreach (var hit in results)
{
    var row = OmniResult(hit, hit.Name)                        // T is whatever `hit` is
        .SetIcon(UIcons.FilePdf, "#ef4444")
        .SetIconBadge(Image(hit.SourceLogo), OmniResultBadgeCorner.BottomRight)
        .SetId(hit.Reference)                                  // "JR-2214 › the title"
        .SetBadge($"{hit.Matches} matches in text")
        .SetText(hit.Excerpt)
        .HighlightWords(terms)
        .SetSource("#0061d5", hit.Source, r => ScopeSearchTo(r.Result.Source))   // clickable source
        .SetFooterEntries(hit.Path, hit.Size, hit.Owner, hit.Modified)
        .SetContributionBar(ContributionBar()
            .Add("Title match", hit.TitleScore)
            .Add("Content match", hit.ContentScore)
            .Add("Recency", hit.RecencyScore)
            .Max(100)
            .Decimals(0)
            .Collapsable())                                                      // one line until asked
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
        .InlineCommands(Button(UIcons.Download).Tooltip("Download").OnClick(() => Download(hit)))
        .SetModalContent(async r => await BuildFullViewAsync(r.Result))                  // opens as a modal
        .ModalSize(80.vw(), 80.vh())
        .ModalKeepsIcon()                                                                // tile in the header
        .ModalKeepsFooter();                                                             // source line under the title

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

## Selecting text

A row is a click target rather than something you read a sentence out of, so its text — title, excerpt,
footer — isn't selectable; dragging across a list never leaves half an excerpt highlighted. The modal is
where a result is actually read: there the **title** is selectable, the rest of the header is not, and the
content you passed to `SetModalContent` is untouched, so a document, a transcript or a details grid
selects and copies normally.

## Related

- ModalStack — the deck a result's modal is usually pushed onto — `modal-stack.md`
- InlinePagination — the previous/next control in its modal header — `inline-pagination.md`
- InlineLabel — what its footer is a line of — `inline-label.md`
- DetailsGrid — the metadata block that usually fills the modal's head — `details-grid.md`
- PagesStack — the page preview it takes — `pages-stack.md`
- ContextMenu — the menu the commands open, and its items — `context-menu.md`
- OmniBox — the search input these rows usually answer — `omni-box.md`
- ResourceCard (the larger, tile-shaped resource summary) — `resource-card.md`
- ContextCard (the compact chat-attachment card) — `context-card.md`
- Badge — what `SetBadge(IComponent)` takes — `badge.md`
- ContributionBar — the score breakdown under the footer — `contribution-bar.md`
- CheckBox — the selection control — `check-box.md`
- DetailsList / SearchableList (when the results are really a table or a grid) — `details-list.md`, `searchable-list.md`
- Full docs & API: `/tesserae/components/omni-result`
