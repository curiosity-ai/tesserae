---
name: validator
description: Coordinates validation across multiple form components, tracking user interaction and updating visual state. Use when validating forms or groups of inputs in a Tesserae (C#/h5) app.
---

# Validator

Centralizes validation for components implementing `ICanValidate` (e.g. `TextBox`, `Dropdown`, `FileSelector`). It only shows errors for fields the user has touched — until a full revalidation (form submit) is forced. Register rules via each component's `.Validation(...)` method passing the validator.

## Create

`UI.Validator()` — returns a `Validator`. Bring factories into scope with `using static Tesserae.UI;`.

## Key configuration

- Register via the input's fluent method: `textBox.Validation(tb => error-or-null, validator)`.
- `.OnValidation(validity => …)` — fires with a `ValidationState` (`Invalid` / `NoInvalidComponentFoundSoFar` / `EveryComponentIsValid`); use to enable/disable submit.
- `.AreCurrentValuesAllValid()` — check all components without showing errors (good for initial submit-button state).
- `.Revalidate()` / `.IsValid` — force-validate everything (shows errors), returns bool.
- `.ResetState()` — clear all validation state.
- `.RegisterFromCallback(Func<bool> isInvalid, Action onRevalidation)` — rule not tied to a component.
- `.Debounce(delayInMs)` / `.Debounce(delayInMs, maxDelayInMs)`.

Built-in rule helpers live on `Validation` (e.g. `Validation.NonZeroPositiveInteger(tb)`).

## Example

```csharp
using static Tesserae.UI;

var validator = Validator().OnValidation(v =>
    console.log(v == ValidationState.Invalid ? "Invalid!" : "Valid!"));

var name = TextBox("").Required();
name.Validation(tb => string.IsNullOrWhiteSpace(tb.Text) ? "Name cannot be empty" : null, validator);

var age = TextBox("").Required();
age.Validation(tb => Validation.NonZeroPositiveInteger(tb) ?? "Age must be a positive integer", validator);

var submit = Button("Validate").OnClick((s, b) => validator.Revalidate());
```

## Related

- TextBox — `/tesserae/components/text-box`
- Dropdown — `/tesserae/components/dropdown`
- Full docs & API: `/tesserae/utilities/validator`
