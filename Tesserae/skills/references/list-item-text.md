---
name: list-item-text
description: A two-line list row - a bold title with a lighter subtitle under it, and an optional icon in a rounded, tinted square. Use for settings entries, notification rows and any list row that needs one line of context under its heading in a Tesserae (C#/Transpose) app.
---

# ListItemText

`ListItemText` is the small two-line block a list row is usually made of: a title in
semibold with a quieter second line under it, and optionally a leading icon inside a
rounded square.

It renders text and nothing else — no border, no hover, no click. Put it inside whatever
is doing the listing (`Card`, `ItemsList`, `DetailsList`, a `Stack`) and let that own the
row's behaviour.

## Create

`UI.ListItemText(string title, string subtitle = null)` — a null or empty subtitle leaves
the second line out and the row draws as one line. Bring factories into scope with
`using static Tesserae.UI;`.

## Key configuration

- `.SetTitle(string)` / `.Title` — the bold first line.
- `.SetSubtitle(string)` / `.Subtitle` — the quieter second line. Setting it to null or
  empty hides it.
- `.SetIcon(UIcons icon, UIconsWeight weight = Regular, TextSize size = Medium)` — a
  leading glyph in a rounded square. Calling it again swaps the glyph.
- `.IconForeground(string color)` / `.IconBackground(string color)` — the glyph and the
  square behind it. Both are no-ops until an icon is set, and both take any CSS colour, so
  the theme's own variables are the tidy way to say "this row is a warning".

## Example

```csharp
using static Tesserae.UI;

var rows = VStack().WS().Children(
    ListItemText("Project roadmap", "Last edited 3 hours ago by Alex"),

    ListItemText("General settings", "Theme, language and notifications")
        .SetIcon(UIcons.Settings),

    ListItemText("Backup completed", "All files synced successfully")
        .SetIcon(UIcons.Check)
        .IconForeground("var(--tss-success-foreground-color)")
        .IconBackground("var(--tss-success-background-color)"),

    ListItemText("Storage almost full", "Free up space to keep syncing")
        .SetIcon(UIcons.TriangleWarning)
        .IconForeground("var(--tss-danger-foreground-color)")
        .IconBackground("var(--tss-danger-background-color)"));
```

## Related

- OmniResult — the full search-result row, with an icon tile, excerpt and commands — `omni-result.md`
- ContextCard — the compact card for one attached piece of context — `context-card.md`
- ItemsList / DetailsList — what usually holds a column of these — `items-list.md`, `details-list.md`
- TextBlock — one line of text on its own — `text-block.md`
- Full docs & API: `/tesserae/components/list-item-text`
