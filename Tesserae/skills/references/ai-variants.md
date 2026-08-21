---
name: ai-variants
description: The AI variant shared by Card, TextBlock, Icon, Button, InlineLabel, Skeleton, Badge, Spinner and ProgressIndicator - a subtle purple-to-blue gradient plus the Sparkles glyph, marking what a model produced and the actions that ask it for more. Use when building AI features in a Tesserae (C#/Transpose) app.
---

# AI variants

A model's output, and the buttons that ask for it, are not new components — they are the
components you already have, saying where the content came from. So nine of them carry the
same variant: **`.AI()`**, a quiet purple-to-blue gradient, with **`UIcons.Sparkles`** where a
glyph is wanted.

Each variant is the component's own stylesheet with the colour swapped: same geometry, same
states, same size. Dropping one into a laid-out page moves nothing.

## The set

| Component | Call | What it does |
|---|---|---|
| `Card` | `.AI()` | Faint tint over the card's own background, accent border, soft shadow. Header and footer take one step more tint. |
| `TextBlock` | `.AI()` | Words painted with the gradient. For **short** strings — a title, a heading, a one-line summary. |
| `TextBlock` | `.AISurface()` | Generated prose: the theme's own text colour on a tinted panel with an accent edge down the left. For paragraphs. |
| `Icon` | `.AI()` | Glyph filled with the gradient. `UI.AIIcon()` is the shorthand for a gradient Sparkles. |
| `Button` | `.AI()` | Filled gradient — the AI equivalent of `Primary()`. Takes the Sparkles glyph unless it already has an icon. |
| `Button` | `.AISubtle()` | The quiet form: tint plus accent text, for the second and third AI action on a surface. |
| `InlineLabel` | `.AI()` | Tinted pill in the accent colour, Sparkles as its mark unless it already has one. |
| `Skeleton` | `.AI()` | Tinted placeholder with a purple shimmer — generating, rather than fetching. |
| `Badge` / `Tag` / `Chip` | `.AI()` (`BadgeTone.AI`) | Filled gradient; `.Outline()` gives tint plus accent instead. `UI.AIBadge()` is a Sparkles-led gradient pill. |
| `Spinner` | `.AI()` | The accent colour on the arc and on its label. |
| `ProgressIndicator` | `.AI()` | Bar painted with the gradient, determinate or indeterminate. |

`Button.AI(withSparklesIcon: false)` and `InlineLabel.AI(withSparklesIcon: false)` leave the
label/mark alone when the component already says what it is some other way.

## Example

```csharp
using static Tesserae.UI;

var answer = Card(VStack().WS().Children(
        TextBlock("Brake sensor calibration failed on line 3 in eleven of the last fourteen runs.")
            .AISurface(),
        HStack().WS().Wrap().Gap(6.px()).MT(12).Children(
            InlineLabel("12 sources").AI(),
            InlineLabel("94% confidence").AI(withSparklesIcon: false))))
    .SetTitle(HStack().AlignItemsCenter().Gap(8.px()).Children(
        AIIcon(),
        TextBlock("What went wrong on line 3").SemiBold().AI(),
        AIBadge()))
    .SetFooter(HStack().Gap(8.px()).Children(
        Button("Ask a follow-up").AI(),
        Button("Show sources").AISubtle(withSparklesIcon: false).SetIcon(UIcons.Books)))
    .AI();
```

While the answer is being generated:

```csharp
var pending = Card(VStack().WS().Children(
        Spinner("Reading 12 documents").AI(),
        Skeleton().WS().H(12).AI().MT(12),
        Skeleton().W(80.percent()).H(12).AI().MT(8),
        ProgressIndicator().Progress(35).AI().WS().MT(16)))
    .AI();
```

## Design rules

- Use it to mean *a model made this* or *this asks a model for something* — never as decoration.
- One filled `AI()` button per surface, the way you keep one `Primary()`; the rest go `AISubtle()`.
- `AI()` on a title, `AISurface()` on the paragraph. Gradient words are for short strings.
- Don't nest an AI card inside an AI card — the tints add up and the inner one loses its edge.
- Don't combine `AI()` with `Primary()`, `Success()` or `Danger()` on the same component.
- Don't animate a resting AI surface. The only movement in the set is the `Skeleton` shimmer and
  the `ProgressIndicator` sweep, which were already moving.
- Let a component that already carries a source (an avatar, a logo) keep its own mark rather
  than taking Sparkles.

## Theming it

Every variant reads from the same CSS variables, declared for the light theme and again under
`.tss-dark-mode`, so an app that wants another AI hue overrides these rather than a stylesheet:

- `--tss-ai-from`, `--tss-ai-to` — the two ends of the gradient.
- `--tss-ai-accent` — the single colour a glyph, a border or an arc takes.
- `--tss-ai-gradient`, `--tss-ai-gradient-hover` — the filled form.
- `--tss-ai-gradient-text` — the readable form, for words and glyphs.
- `--tss-ai-surface`, `--tss-ai-surface-strong` — translucent tints, layered over whatever
  background the component already had.
- `--tss-ai-border-color`, `--tss-ai-shadow`, `--tss-ai-shadow-hover`, `--tss-ai-on-fill`.

`Theme.Gradients.AI` is separate and unchanged: a louder three-stop blue→purple→magenta for
when you want a gradient as a feature rather than as a marker.

## Related

- Card — `card.md` · TextBlock — `text-block.md` · Icon — `icon.md` · Button — `button.md`
- InlineLabel — `inline-label.md` · Skeleton — `skeleton.md` · Badge — `badge.md`
- Spinner — `spinner.md` · ProgressIndicator — `progress-indicator.md`
- Gradients — `gradients.md` · Chat — `chat.md` · Tool call — `tool-call.md` · Plan — `plan.md`
- Full docs & API: `/tesserae/ai/ai-variants`
