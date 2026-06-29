---
name: teaching
description: A guided onboarding walkthrough that highlights UI elements one at a time with a tooltip, advancing on a Next click or after a fixed delay. Use when introducing first-time users to features in a Tesserae (C#/h5) app.
---

# Teaching

A sequential walkthrough: each step anchors a tooltip to a target component and
either waits for a Next click or auto-advances after 5/10 seconds. Steps fire as
their target mounts, so it works on dynamically rendered UI.

## Create

`UI.Teaching()` (i.e. `Teaching()`) returns a `Teaching`. It is not a visual
component you render — you add steps and start it. Bring factories into scope
with `using static Tesserae.UI;`.

## Key configuration

- `.AddStep(IComponent showFor, IComponent tooltip, TooltipAnimation animation =
  ShiftToward, TooltipPlacement placement = Top, StepType stepType = NextButton)`
  — highlight `showFor` with `tooltip`.
- `StepType` — `NextButton` (wait for click), `After5seconds`, `After10seconds`.
- `.RunNow()` — start immediately.
- `.RunIf(Func<bool>)` — start only if the condition is true (re-checked per step).
- `.OnComplete(Action)` — callback when the last step finishes.
- `.FirstDelay(ms)` / `.StepDelay(ms)` — delay before first step / between steps.

## Example

```csharp
using static Tesserae.UI;

var btn1 = Button("Feature A").Primary();
var btn2 = Button("Feature B");
var btn3 = Button("Feature C");

Button("Start Walkthrough").SetIcon(UIcons.Play).OnClick(() =>
    Teaching()
        .AddStep(btn1, TextBlock("Step 1: click Next."), stepType: Teaching.StepType.NextButton)
        .AddStep(btn2, TextBlock("Step 2: auto-advances in 5s."), stepType: Teaching.StepType.After5seconds)
        .AddStep(btn3, TextBlock("Step 3: you're done!"))
        .OnComplete(() => Toast().Success("Walkthrough complete!"))
        .RunNow());
```

## Related

- Stepper (in-page wizard) — `stepper.md`
- Full docs & API: `/tesserae/components/teaching`
