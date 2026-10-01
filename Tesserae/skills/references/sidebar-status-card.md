---
name: sidebar-status-card
description: A live status card at the top of a Sidebar showing a section's glyph and name, a status line with a coloured dot, and an optional progress bar with a caption. Use when a sidebar should say what its section is doing right now (indexing, syncing, a running job) in a Tesserae (C#/Transpose) app.
---

# SidebarStatusCard

A card for a `Sidebar`'s header that says what the section it heads is doing
right now: the section's glyph and name, a status line ("Index healthy · 2.4M
nodes") with a dot in the status's tone, and an optional progress bar with a
caption for a running job ("Re-index 72% · 4 min left"). Every part can be
changed while it is on screen, so keep it updated from whatever reports the state.

On the collapsed rail it is the glyph with a short progress bar under it, and
the text moves to its tooltip.

## Create

`new SidebarStatusCard(string identifier, UIcons icon, string title, string status = null, UIconsWeight weight = UIconsWeight.Regular)`
is an `ISidebarItem`. Add it with `sidebar.AddHeader(...)`.
Bring factories into scope with `using static Tesserae.UI;`.

## Key configuration

- `.SetTitle(title)` / `.SetStatus(status)` — change the name or the status line
  (null or empty hides the status line).
- `.Success()` / `.Warning()` / `.Danger()` / `.Neutral()` — the status dot's
  colour (neutral is the default).
- `.Progress(percent, caption = null)` — show the progress bar at `percent`
  (0 to 100), with an optional caption under it.
- `.Indeterminate(caption = null)` — an indeterminate bar, for a job that cannot
  say how far along it is.
- `.ClearProgress()` — remove the bar and its caption.
- `.OnClick(action)` — make the card clickable, e.g. to open a status page.
- `.NotSortable()` — keep it out of drag reordering.

## Example

A plain card, with no progress bar and nothing updating it:

```csharp
sidebar.AddHeader(new SidebarStatusCard("govern-status", UIcons.Shield, "Govern", "All policies passing").Success());
```

A live one, updated as a job reports in:

```csharp
using static Tesserae.UI;

var status = new SidebarStatusCard("build-status", UIcons.Database, "Build", "Index healthy · 2.4M nodes")
    .Success()
    .Progress(72, "Re-index 72% · 4 min left");

var sidebar = Sidebar();
sidebar.AddHeader(status);
sidebar.AddContent(new SidebarSeparator("data", "Data"));
sidebar.AddContent(new SidebarButton("data-sources", UIcons.Database, "Data Sources").Selected());

// Later, as the job reports in:
status.Progress(90, "Re-index 90% · 1 min left");
status.SetStatus("Index healthy · 2.4M nodes").Success().ClearProgress();
status.SetStatus("Re-index failed").Danger();
```

## Brand row or status card?

Both head a sidebar. A `SidebarBrand` (`sidebar-brand.md`) says *what* the
section is (glyph, name, a qualifying line) and can carry commands. A
`SidebarStatusCard` says *what it is doing*, and is the one to use when that
changes while the page is open.

## Related

- Sidebar — `sidebar.md`
- SidebarBrand — `sidebar-brand.md`
- Sidenav (icon rail to the left) — `sidenav.md`
- ProgressIndicator — `progress-indicator.md`
- Full docs & API: `/tesserae/components/sidebar-status-card`
