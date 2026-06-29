---
name: choice-group
description: A radio-style group where exactly one Choice can be selected, in vertical or horizontal layout, exposing the selection as an observable. Use for 2–7 mutually exclusive options that should all stay visible in a Tesserae (C#/h5) app.
---

# ChoiceGroup

`ChoiceGroup` presents mutually exclusive options as radio buttons. Each option is a
nested `ChoiceGroup.Choice`. The selected choice is observable via `AsObservable()`.

## Create

`UI.ChoiceGroup(string label = "")` — the group with a header label.
`new ChoiceGroup.Choice(string text)` — each option (constructed inline).
Bring factories into scope with `using static Tesserae.UI;`.

## Key configuration

ChoiceGroup:

- `.Choices(params ChoiceGroup.Choice[])` — add options.
- `.Horizontal()` / `.Vertical()` — layout (default vertical).
- `.Required()` — mark the group required.
- `.SelectedOption` — the currently selected `Choice`.
- `.AsObservable()` — `IObservable<Choice>` of the selection.

Choice:

- `.Selected()` / `.SelectedIf(bool)` — initial selection.
- `.Disabled(bool = true)` — disable a single option.
- `.OnSelected(Action<Choice>)` — per-option callback.

## Example

```csharp
using static Tesserae.UI;

var group = ChoiceGroup("Pick one")
    .Required()
    .Choices(
        new ChoiceGroup.Choice("Option A").Selected(),
        new ChoiceGroup.Choice("Option B"),
        new ChoiceGroup.Choice("Option C").Disabled());

group.AsObservable().Observe(choice => Toast().Information(choice?.Text));
```

## Related

- CheckBox (multi-select) — `../check-box/SKILL.md`
- Full docs & API: `/tesserae/components/choice-group`
