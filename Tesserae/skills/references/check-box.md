---
name: check-box
description: A two-state (checked/unchecked) labeled form control exposing its state as an observable. Use for boolean settings, opt-ins, and multi-select item lists in a Tesserae (C#/h5) app.
---

# CheckBox

`CheckBox` is a labeled boolean toggle. Its checked state is observable via
`AsObservable()`, so you can react to changes or bind it elsewhere.

## Create

`UI.CheckBox(string text = "")`.
Bring factories into scope with `using static Tesserae.UI;`.

## Key configuration

- `.Checked(bool = true)` — set checked state. `.IsChecked` — get/set.
- `.Disabled(bool = true)` — disable interaction. `.IsEnabled` — get/set.
- `.SetText(string)` — label text.
- `.AsObservable()` — `IObservable<bool>` of the checked state.
- Text formatting: `.Size`, `.Weight`, `.TextAlign`.

## Example

```csharp
using static Tesserae.UI;

var options = Stack().Children(
    CheckBox("Unchecked"),
    CheckBox("Checked").Checked(),
    CheckBox("Disabled").Disabled());

var remember = CheckBox("Remember me");
remember.AsObservable().Observe(isChecked => SaveSetting(isChecked));
```

## Related

- ChoiceGroup (single select) — `choice-group.md`
- Toggle — `/tesserae/components/toggle`
- Full docs & API: `/tesserae/components/check-box`
