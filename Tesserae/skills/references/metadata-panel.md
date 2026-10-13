---
name: metadata-panel
description: The narrow facts column beside an issue conversation - titled sections (assignees, labels, milestone, participants, ...) divided by hairlines, each with an optional edit gear and an empty-state line. Use when building a GitHub-issues-like sidebar or any scan-able column of item properties in a Tesserae (C#/Transpose) app.
---

# MetadataPanel

A stack of `MetadataSection`s. Each section has a small title, an optional gear button,
and either its content or a muted empty text ("No one assigned"). Usually placed beside a
`Conversation` (`conversation.md`) at about 256px wide.

## Create

- `MetadataPanel()` — `.Add(section)`, `.Children(params MetadataSection[])`, `.Remove(section)`,
  `.AriaLabel(string)` (default `"Details"`), `.Sections`.
  `.Compact(bool = true)` / `.IsCompact` — less space between and inside sections, smaller avatars.
- `MetadataSection(string title, string emptyText = null)` — empty text defaults to `"None yet"`.

Bring factories into scope with `using static Tesserae.UI;`.

## MetadataSection content helpers (all append, all fluent)

- `.Person(string name, Avatar avatar = null, string secondary = null)` — avatar + name (+ muted login).
- `.Labels(params IComponent[])` — a wrapping row of tags; use `Tag(text).Pill().Tint(color)` for user-coloured labels.
- `.Item(UIcons icon, string title, string subtitle = null, string iconColor = null, Action onClick = null)` —
  a milestone, linked pull request, project. `onClick` makes the title a link-styled button.
- `.Avatars(IEnumerable<Avatar>, int max = 10, int? total = null)` — participants with a trailing `+N`.
- `.Text(string)`, `.Content(params IComponent[])` (anything, e.g. a `ProgressIndicator` or a `Button`).
- `.SetContent(params IComponent[])` replaces, `.Clear()` empties, `.Empty(string)` sets the empty text, `.IsEmpty`.
- `.OnEdit(Action<IComponent> onEdit)` — shows a gear; clicking it or the title calls `onEdit(gearButton)`,
  so you can anchor a menu: `.OnEdit(anchor => menu.ShowFor(anchor))`.

## Example

```csharp
using static Tesserae.UI;

var labels = MetadataSection("Labels", "No labels")
    .Labels(Tag("bug").Pill().Tint("#d73a4a"), Tag("dark mode").Pill().Tint("#5319e7"));

labels.OnEdit(anchor => ContextMenu().Items(
        ContextMenuItem("good first issue").OnClick(() => labels.SetContent().Labels(Tag("good first issue").Pill().Tint("#7057ff"))),
        ContextMenuItem("Clear labels").OnClick(() => labels.Clear()))
    .ShowFor(anchor));

var panel = MetadataPanel().Children(
    MetadataSection("Assignees", "No one assigned").Person("Marco Ricci", Avatar(initials: "MR"), "marcor"),
    labels,
    MetadataSection("Milestone", "No milestone")
        .Item(UIcons.Flag, "v2026.6", "Due by June 30 · 82% complete")
        .Content(ProgressIndicator().Progress(82).Height(4.px())),
    MetadataSection("Development", "No branches or pull requests")
        .Item(UIcons.CodePullRequest, "Keep the rail above highlighted comments", "org/repo#1490", Theme.Success.Background),
    MetadataSection("Participants")
        .Avatars(new[] { Avatar(initials: "AS"), Avatar(initials: "MR") }, max: 5, total: 12));

return panel.W(256).Render();
```

## Related

- Conversation — `conversation.md`
- Property grid — `property-grid.md`
- Avatar — `avatar.md`, Badge/Tag — `badge.md`
