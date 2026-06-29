---
name: option
description: A single radio-style choice (ChoiceGroup.Choice, created via Choice(...)) that lives inside a ChoiceGroup for mutually-exclusive selection. Use when building radio-button groups where exactly one option is active in a Tesserae (C#/h5) app.
---

# Option (Choice)

An "Option" is a single mutually-exclusive choice. The factory is `Choice(...)`
and the underlying type is `ChoiceGroup.Choice`. Options are placed inside a
`ChoiceGroup`, which manages shared selection and exposes the selected choice as
an observable.

## Create

- `Choice(string label = "")` — one radio option.
- `ChoiceGroup(string label = "")` — the container; add options with `.Choices(...)`.

Bring the factories into scope with `using static Tesserae.UI;`.

## Key configuration

Choice:
- `.Selected()` / `.SelectedIf(bool)` / `.IsSelected` — selection.
- `.Disabled(bool = true)` / `.IsEnabled` — enabled state.
- `.OnSelected(handler)` — fires when this option becomes selected.
- `.Text` / `.SetText(string)` — label text.

ChoiceGroup:
- `.Choices(params Choice[])` — add options.
- `.AsObservable()` — `IObservable<Choice>` of the selected option.
- `.SelectedOption` — current selection.
- `.Horizontal()` / `.Vertical()` — layout. `.Required()`.

## Example

```csharp
using static Tesserae.UI;

var selected = TextBlock("Selected: none");

var group = ChoiceGroup("Favorite environment").Choices(
    Choice("Development").Selected(),
    Choice("Staging"),
    Choice("Production").Disabled()
);

group.AsObservable().Observe(c => selected.Text("Selected: " + (c?.Text ?? "none")));

var ui = Stack().Children(selected, group);
```

## Related

- ChoiceGroup — `/tesserae/components/choice-group`
- CheckBox — `/tesserae/components/check-box`
- Full docs & API: `/tesserae/components/option`
