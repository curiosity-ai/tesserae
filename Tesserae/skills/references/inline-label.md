---
name: inline-label
description: One small piece of metadata on a line of them - an optional mark (a glyph, an image, or a rounded square of colour) followed by optional text, optionally pressable or a real link. Use for the footer of an OmniResult, or any row of small facts, in a Tesserae (C#/Transpose) app.
---

# InlineLabel

An optional mark followed by optional text, sized so a line of them sits on one baseline. It is what an
`OmniResult` footer is made of — "Box · 2.4 MB · Marie Lang · Apr 11, 2024" — and it reads the same
anywhere else a row of small facts belongs.

Every combination is allowed: text alone, a mark alone, or both.

It draws at one of two sizes, decided by where it is rather than by a flag: on its own it is a **compact
button** — a bordered pill at 24px, which is the shape a chip of related things wants — and inside an
`OmniResult` footer the chrome comes off and it takes the footer's own colour and type size. A pressable
label in a footer underlines on hover instead of filling, so one clickable fact doesn't turn the line
into a row of buttons.

```
▪ Box      📁 sample-files / pdfs      2.4 MB      👤 Pius Neuhaus      🔒
```

## Create

`UI.InlineLabel(string text = null)`. Bring factories into scope with `using static Tesserae.UI;`.

## Key configuration

- `.SetText(string)` / `Text` — the text. Null or empty leaves the label as its mark alone.
- `.SetIcon(UIcons icon, UIconsWeight weight = Regular)` — a glyph before the text.
- `.SetIcon(IComponent)` — a component of the host's own (an `Avatar`, an emoji, a `Spinner`), drawn in
  the same box as any other mark.
- `.SetImage(string url)` — an image (a source's logo, a favicon), fitted rather than cropped.
- `.SetColor(string color)` — a small rounded square of that colour.
- `.NoMark()` — text alone.
- `.OnClick(Action<InlineLabel>)` — makes it pressable: it takes a tab stop, answers Enter and Space,
  and the click stops at the label so pressing it never also counts as a click on the row it sits in.
  The usual `OnClick(ComponentEventHandler<InlineLabel, MouseEvent>)` works too.
- `.SetHref(string href, bool openInNewTab = false)` — makes it a real link. The label is an anchor
  either way, so a link is middle-clickable and shows its address in the status bar.

Whatever the mark is it takes one box — 14px on the compact button, 12px in a footer — and the text
ellipsizes rather than wrapping, so a long path gives way to whatever it shares the line with.

## Example

On its own — a line of chips, in a `DetailsGrid` value or anywhere else:

```csharp
using static Tesserae.UI;

DetailsGrid()
    .Row("Folder", InlineLabel(hit.Path).SetIcon(UIcons.Folder).OnClick(_ => OpenFolder(hit.FolderId)))
    .Row("Source", InlineLabel(hit.Source).SetImage(hit.SourceLogo).SetHref(hit.SourceUrl, openInNewTab: true))
    .Row("Labels", HStack().Wrap().Gap(6.px()).Children(
        InlineLabel("brakes").SetColor("#ef4444"),
        InlineLabel("calibration").SetColor("#16a34a")));
```

In a footer, where the same labels draw small:

```csharp
row.SetFooterEntries(
    InlineLabel(hit.Path).SetIcon(UIcons.Folder).OnClick(_ => OpenFolder(hit.FolderId)),
    InlineLabel(hit.Size),
    InlineLabel(hit.Author).SetIcon(UIcons.User),
    InlineLabel(hit.Modified),
    InlineLabel(hit.Source).SetImage(hit.SourceLogo).SetHref(hit.SourceUrl, openInNewTab: true));
```

## Related

- OmniResult — its footer takes an array of these — `omni-result.md`
- Badge — a filled pill, for a status or a count rather than a fact on a line — `badge.md`
- Link — a standalone hyperlink with its own text styling — `link.md`
- Full docs & API: `/tesserae/components/inline-label`
