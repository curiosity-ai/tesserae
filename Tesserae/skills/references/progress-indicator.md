---
name: progress-indicator
description: A horizontal progress bar with determinate (0–100%) or indeterminate state. Use when showing the completion of a long-running operation in a Tesserae (C#/Transpose) app.
---

# ProgressIndicator

A thin horizontal bar that fills to a percentage, or animates continuously when the amount of work is unknown.

## Create

`UI.ProgressIndicator()` — returns a `ProgressIndicator`. Also `new ProgressIndicator()`.
Bring factories into scope with `using static Tesserae.UI;`.

## Key configuration

- `.Progress(float percent)` — set fill to a 0–100 percentage (clamped).
- `.Progress(int position, int total)` — set fill from a fraction.
- `.Indeterminated()` — switch to the animated indeterminate state.
- `.Foreground` — bar colour (CSS).
- Width is set with the standard sizing helpers, e.g. `.Width(400.px())` / `.WS()`.

## Example

```csharp
using static Tesserae.UI;

Stack().Children(
    Label("30%").SetContent(ProgressIndicator().Progress(30).Width(400.px())),
    Label("Full").SetContent(ProgressIndicator().Progress(100).Width(400.px())),
    Label("Indeterminate").SetContent(ProgressIndicator().Indeterminated().Width(400.px()))
);
```

## Related

- ProgressRing — `.progress-ring.md`
- Full docs & API: `/tesserae/components/progress-indicator`
