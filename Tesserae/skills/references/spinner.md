---
name: spinner
description: An animated indeterminate loading indicator with an optional positioned label and optional determinate progress. Use when signalling a running background task in a Tesserae (C#/Transpose) app.
---

# Spinner

An animated circle that indicates a task is running. Shows an optional label
that can sit on any side, and can switch from indeterminate spinning to a
determinate progress arc.

## Create

`UI.Spinner(string text = "")` (i.e. `Spinner("Loading...")`) returns a
`Spinner`. Bring factories into scope with `using static Tesserae.UI;`.

## Key configuration

- `.XSmall()` / `.Small()` / `.Medium()` / `.Large()` — size (also `.Size`).
- `.Left()` / `.Right()` / `.Above()` / `.Below()` — label position
  (also `.Position`).
- `.SetText(text)` / `.Text` — the label.
- `.Primary()` / `.Success()` / `.Warning()` / `.Danger()` / `.CustomColor(color)` — colour.
- `.AI(bool strongEffect = false)` — the purple-to-blue AI gradient on the ring and on its label: waiting on a model rather than on a server. The ring becomes a masked conic gradient instead of the svg stroke, rotating at the same speed; the determinate form runs the gradient across the filled part of the sweep. See `ai-variants.md`.
- `.Progress(float percent)` or `.Progress(int position, int total)` — switch
  to a determinate arc.
- `.Indeterminate()` — back to continuous spinning.

## Theming

Every component that waits on something draws this same ring (the `.tss-spinner`
element): Dropdown's loading and search states, SearchBox's busy slot, a Tree
node loading its children, a running Plan step, AutoCompleteBox, OmniBox,
ProgressModal, `ProgressRing.Indeterminate()`, and the in-place spinner of
`Button.ToSpinner` / `OnClickSpinWhile` and `SaveButton` (which add a check or an
x on top). A custom theme that restyles `.tss-spinner` restyles all of them.

The ring is an svg with a faint track and a quarter arc that enters as a dot and
stretches out. A theme tunes it with variables on `.tss-spinner`:

- `--tss-spinner-color` — arc and track (the track at 16%).
- `--tss-spinner-stroke` — thickness in the ring's 48-unit box (4 is 1.33px at 16px).
- `--tss-spinner-arc` — the arc's dash pattern (default `31.4 94.26`, a quarter of 125.66).
- `--tss-spinner-mask` — the ring's band, for painting the element's background instead.

## Example

```csharp
using static Tesserae.UI;

var spinner = Spinner("Loading, please wait...").Medium();

// Determinate progress
var saving = Spinner("Saving...").Below().Success().Progress(40);
```

## Related

- Skeleton (placeholder shapes) — `skeleton.md`
- AI variants — the `.AI()` variant — `ai-variants.md`
- ProgressIndicator — `/tesserae/feedback/progress-indicator`
- Full docs & API: `/tesserae/feedback/spinner`
