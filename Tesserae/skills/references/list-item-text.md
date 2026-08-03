---
name: list-item-text
description: A two-line list row pairing a bold title with a lighter subtitle, with an optional leading icon on a rounded-square background. Use for list rows, settings entries and notification items in a Tesserae (C#/Transpose) app.
---

# ListItemText

`ListItemText` is a title with one line of secondary context underneath it, and an optional
leading icon shown inside a rounded-square tile. It is the row shape for settings entries,
notification items and any list where a heading alone would not say enough.

The subtitle is optional: omit it (or pass `null`) and the row is a single line, which keeps a
list of mixed rows aligned on the title.

## Create

`UI.ListItemText(string title, string subtitle = null)`
Bring factories into scope with `using static Tesserae.UI;`.

## Key configuration

- `.SetTitle(string)` / `.SetSubtitle(string)` — rewrite either line.
- `.SetIcon(UIcons icon, UIconsWeight weight = UIconsWeight.Regular, TextSize size = TextSize.Medium)`
  — the leading glyph and its tile.
- `.IconForeground(string color)` / `.IconBackground(string color)` — tint the tile. Use theme
  tokens (`Theme.Danger.Foreground`, `Theme.Success.Background`, …) rather than literal colors,
  so the row follows the theme.

## Example

```csharp
using static Tesserae.UI;

var rows = VStack().Children(
    ListItemText("Project roadmap", "Last edited 3 hours ago by Alex"),
    ListItemText("General settings", "Theme, language and notifications")
       .SetIcon(UIcons.Settings),
    ListItemText("Storage almost full", "Free up space to keep syncing")
       .SetIcon(UIcons.TriangleWarning)
       .IconForeground(Theme.Danger.Foreground)
       .IconBackground(Theme.Danger.Background));
```

## Related

- ContextCard — a labelled pill for a document or record — `context-card.md`
- OmniResult — a full search-result row (icon tile, title, excerpt, footer) — `omni-result.md`
- ItemsList / DetailsList — the containers such rows usually go in — `items-list.md`,
  `details-list.md`
- Full docs & API: `/tesserae/components/list-item-text`
