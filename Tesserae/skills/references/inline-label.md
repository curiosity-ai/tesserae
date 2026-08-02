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
`OmniResult` footer the chrome comes off and it takes the footer's own colour and type size, so one
clickable fact doesn't turn a line of facts into a row of buttons.

```
▪ Box      📁 sample-files / pdfs      2.4 MB      👤 Pius Neuhaus      🔒
```

## Create

`UI.InlineLabel(string text = null)` — a label you already know the content of.
`UI.InlineLabel(Func<InlineLabel, Task> load)` — a fact it has to look up (see below).
Bring factories into scope with `using static Tesserae.UI;`.

## A fact it has to look up

Built from a task, a label draws as a **skeleton rectangle** while the task runs, then shows whatever the
task set on it. If the task ends **without setting any text or mark**, the label takes itself out of the
document — and the slot it was standing in with it, so the line it belonged to closes up instead of
keeping a gap for something that turned out not to exist:

| Where it sits | What is removed |
|---|---|
| A `Stack` (the usual row of labels) | the `tss-stack-item` wrapper the stack put it in |
| An `OmniResult` footer | the whole footer entry, so no orphan separator dot is left |
| A `DetailsGrid` value, alone | the **whole row**, label cell included — a labelled blank says nothing |
| A `DetailsGrid` value, beside others | only its own slot; the row keeps the labels that did resolve |
| Anywhere else | the label itself |

A task that throws is treated as one that found nothing: the label removes itself, and the exception is
reported the way any fire-and-forget task's is.

```csharp
row.SetFooterEntries(
    InlineLabel(hit.Size),
    InlineLabel(async label =>
    {
        var author = await LookUpAuthorAsync(hit.Id);   // nothing set -> the entry disappears
        if (author is object) label.SetText(author.Name).SetIcon(UIcons.User);
    }));
```

## Key configuration

- `.SetText(string)` / `Text` — the text. Null or empty leaves the label as its mark alone.
- `.SetIcon(UIcons icon, UIconsWeight weight = Regular, string color = null)` — a glyph before the text,
  in a colour of its own when one is given (a node type's accent, a source's brand) and in the label's
  own colour otherwise.
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
ellipsizes rather than wrapping, so a long path gives way to whatever it shares the line with. What a
label had to cut belongs in `.Tooltip(...)`: the full path, what a code stands for, the date behind
"2 days ago".

**Hover.** A pressable or linked label lifts its background on hover, in a footer as well as on its own.
Only a label with an `href` also underlines — one that merely runs a handler is a button, and
underlining it would promise an address it doesn't have.

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
- Button — an action, or a link that looks like a button (`Button(text, href)`) — `button.md`
- Full docs & API: `/tesserae/components/inline-label`
