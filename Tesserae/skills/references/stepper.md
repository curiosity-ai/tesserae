---
name: stepper
description: A multi-step wizard that shows numbered steps with titles, swaps content per step, and provides Back/Next navigation. Use when guiding the user through a sequential flow (forms, onboarding) in a Tesserae (C#/Transpose) app.
---

# Stepper

A wizard that breaks a process into ordered steps. It renders a numbered header,
shows one step's content at a time, and provides Back/Next buttons that track
progress. Clicking a header step jumps to it.

## Create

`UI.Stepper(params StepperStep[] steps)` (i.e. `Stepper(Step(...), Step(...))`)
returns a `Stepper`. Build steps with `UI.Step(string title, IComponent content,
string description = null)`. Bring factories into scope with
`using static Tesserae.UI;`.

## Key configuration

Stepper:

- `.AddStep(title, content, description = null)` / `.AddSteps(params steps)` —
  append steps.
- `.Next()` / `.Previous()` / `.SetStep(int index)` — navigate programmatically.
- `.CurrentStepIndex` / `.CurrentStep` — current position / `StepperStep`.
- `.OnStepChange(s => ...)` — callback when the active step changes; `s` is the
  `Stepper` (read `s.CurrentStepIndex`, `s.CurrentStep.Title`).

`StepperStep` has `Title`, `Description`, `Content`.

## Example

```csharp
using static Tesserae.UI;

var wizard = Stepper(
    Step("Personal Info", Stack().Children(
        Label("Name").SetContent(TextBox().SetPlaceholder("John Doe")))),
    Step("Preferences", Stack().Children(
        Toggle(onText: TextBlock("Yes"), offText: TextBlock("No")))),
    Step("Review", Stack().Children(
        CheckBox("I agree to the terms"),
        Button("Finish").Primary().MT(16)))
).OnStepChange(s => Toast().Information($"Step {s.CurrentStepIndex + 1}: {s.CurrentStep.Title}"));
```

## Related

- Steps (Neko doc component) and StepsSlider — `steps-slider.md`
- Full docs & API: `/tesserae/components/stepper`
