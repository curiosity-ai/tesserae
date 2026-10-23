---
name: target-bar
description: A thin bar measuring a value against a target, with a tick at the target and a caption of state and figure under it. Use for run-rate, quota or SLA progress in KPI tables in a Tesserae (C#/Transpose) app.
---

# TargetBar

The bar is filled up to the value; a tick marks the target on the same scale; a line under it names the
state on the left and the figure on the right. The scale runs from zero to `max` (150 by default), so a
value past its target (231% of a run rate) still shows how far over it went while the tick stays at 100.

## Create

`UI.TargetBar(double value, double target = 100, double max = 150)`. Bring factories into scope with
`using static Tesserae.UI;`.

## Key configuration

- `.Value(double)` / `.Target(double)` / `.Max(double)` — the scale.
- `.Label(string)` / `.ValueText(string)` — caption left and right.
- `.Color(string)`, `.Success()`, `.Warning()`, `.Danger()`, `.Neutral()` — the fill colour.
- `.Thickness(UnitSize)` — bar thickness, 4px by default.
- `.NoValue(label, valueText)` — an empty track with no tick, for a figure with no target.

## Example

```csharp
using static Tesserae.UI;

TargetBar(84).Warning().Label("Watch").ValueText("84%");
TargetBar(0).NoValue("Context");
```

## Related

- DataTable — `data-table.md`
- ProgressIndicator — `progress-indicator.md`
