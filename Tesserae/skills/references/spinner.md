---
name: spinner
description: An animated indeterminate loading indicator with an optional positioned label and optional determinate progress. Use when signalling a running background task in a Tesserae (C#/h5) app.
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
- `.Primary()` / `.Success()` / `.Danger()` / `.CustomColor(color)` — colour.
- `.Progress(float percent)` or `.Progress(int position, int total)` — switch
  to a determinate arc.
- `.Indeterminate()` — back to continuous spinning.

## Example

```csharp
using static Tesserae.UI;

var spinner = Spinner("Loading, please wait...").Medium();

// Determinate progress
var saving = Spinner("Saving...").Below().Success().Progress(40);
```

## Related

- Skeleton (placeholder shapes) — `skeleton.md`
- ProgressIndicator — `/tesserae/components/progress-indicator`
- Full docs & API: `/tesserae/components/spinner`
