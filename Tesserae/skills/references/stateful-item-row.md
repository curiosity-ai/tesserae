---
name: stateful-item-row
description: A full-width list row - a leading visual, a title with a subtitle under it, and trailing controls pinned to the far end - washable in a tone to say what is happening to the row. Use for people pickers, access lists and any list whose rows carry controls and pending state in a Tesserae (C#/Transpose) app.
---

# StatefulItemRow

`StatefulItemRow` is the whole row that `ListItemText` is only the text of: a leading visual
(an `Avatar`, an `Icon`, anything), a title with a quieter line under it, and whatever the
row is acted on with at the far end. It fills its container's width, keeps the trailing
content pinned right, and ellipsizes the text rather than pushing the controls off the edge.

A **tone** washes the row in one of the theme's colors. That is what lets a list show a
change before it has been saved — added reads green, changed blue, on its way out red and
struck through — without every list re-inventing the colours.

## When to reach for this one

Five components in the toolkit draw something row-shaped, and they are not
interchangeable:

| | leading | text | trailing | state |
| --- | --- | --- | --- | --- |
| `ListItemText` | icon square | title + subtitle | — | — |
| `Persona` | avatar | name + two lines | — | — |
| `ContextCard` | icon or image | label + sublabel | remove, chevron | a raw background colour |
| `OmniResult<T>` | icon tile | title + excerpt | command menu | — |
| **`StatefulItemRow`** | any component | title + subtitle | **any components** | **a semantic tone** |

So: `ListItemText` when you only want the two lines and the container owns the row.
`Persona` for a person shown on their own, not in a list. `ContextCard` for one
attached piece of context, where remove-and-chevron is the whole interaction.
`OmniResult` for a search hit — it brings highlighting, a source footer, selection and
a modal it opens as. **`StatefulItemRow` when the row carries controls you choose, or
has a state to show** — that pair is what nothing else covers.

## Create

`UI.StatefulItemRow(IComponent leading = null)` — the leading visual may be left out, and the row
then starts at its text. Bring factories into scope with `using static Tesserae.UI;`.

## Key configuration

- `.SetLeading(IComponent)` — the leading visual; `null` takes it out.
- `.SetTitle(string)` / `.SetTitle(IComponent)` — the bold first line. Null or empty drops it.
  Pass a component for a name with a quieter suffix, or a highlighted search match.
- `.SetSubtitle(string)` / `.SetSubtitle(IComponent)` — the quieter second line. Null or empty
  drops it and the row draws as one line.
- `.Trailing(params IComponent[])` — what the row is acted on with (a badge, a menu, a remove
  button), replacing whatever was there. `.AddTrailing(...)` appends instead.
- `.Tone(RowTone)` — `None` (default), `Primary`, `Success`, `Warning`, `Danger`, `Muted`,
  or the shorthands `.Neutral()`, `.Primary()`, `.Success()`, `.Warning()`, `.Danger()`,
  `.Muted()`. `.CurrentTone` reads it back. `Muted` is not a colour — it fades a row that is
  still worth showing but cannot be acted on.
- `.Struck(bool = true)` — strikes the **title** through, for a row naming something on its way
  out. The subtitle is left alone: it is what still identifies the row while it is going.
- `.Compact(bool = true)` — tighter row, for a dense list.
- `.Interactive(bool = true)` — hover wash and pointer cursor. It only changes how the row
  looks; wire the click with `.OnClick(...)`.
- `.OnClick(Action)` / `.OnClick((row, ev) => ...)` — the row itself is clickable.

## Example

```csharp
using static Tesserae.UI;

var access = VStack().WS().Children(
    StatefulItemRow(Avatar(initials: "DK").Size(AvatarSize.Medium))
       .SetTitle("Dana Kaur")
       .SetSubtitle("dana.kaur@curiosity.ai")
       .Success()
       .Trailing(Badge("Added").Pill().Outline().Success(),
                 Button("Undo").NoBorder().NoBackground()),

    StatefulItemRow(Avatar(initials: "AS").Size(AvatarSize.Medium))
       .SetTitle("Aiko Sato")
       .SetSubtitle("aiko.sato@curiosity.ai")
       .Danger()
       .Struck()
       .Trailing(Badge("Removing").Pill().Outline().Danger()),

    StatefulItemRow(Avatar(initials: "SA").Size(AvatarSize.Small))
       .SetTitle("Sales — DACH")
       .SetSubtitle("24 members")
       .Compact()
       .Interactive()
       .OnClick(() => OpenTeam()));
```

## Related

- ListItemText — only the two lines of text, with no row around them — `list-item-text.md`
- Avatar / Persona — what usually goes in the leading slot — `avatar.md`
- Badge — the pill in the trailing slot — `badge.md`
- Banner — the same tones, on a notice that explains a row — `banner.md`
- ItemsList / DetailsList — what usually holds a column of these — `items-list.md`, `details-list.md`
- Full docs & API: `/tesserae/components/stateful-item-row`
