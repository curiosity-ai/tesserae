---
name: date-picker
description: A form input for picking a single calendar date, backed by the browser's native date input. Use when collecting a date from the user in a Tesserae (C#/h5) app.
---

# DatePicker

A single-date form input wrapping the browser's native `<input type="date">`.
Supports min/max bounds, step, required/disabled state, and validation. Built on
`MomentPickerBase`, so the validation/step/min/max helpers below are shared with
`DateTimePicker`.

## Create

`UI.DatePicker(DateTime? date = null)` — picker, optionally with a default date.
Bring factories into scope with `using static Tesserae.UI;`.

## Key configuration

- `.Date` — gets the selected `DateTime`.
- `.SetMin(DateTime)` / `.SetMax(DateTime)` — bounds (`.Min` / `.Max` properties also exist).
- `.SetStep(int)` — step increment.
- `.Disabled()` / `.Required()` — state.
- `.Validation(Func<DatePicker,string>)` — custom validator; return null when valid, else an error message. Built-ins: `Validation.NotInTheFuture`, `Validation.NotInThePast`, `Validation.BetweenRange(picker, from, to)`.
- `.Error(string)` / `.IsInvalid()` — set a manual error.
- `.WithBrowserFallback()` — add a regex `pattern` for graceful degradation on older browsers.

## Example

```csharp
using static Tesserae.UI;

var picker = DatePicker(DateTime.Now.AddDays(2))
    .SetMax(DateTime.Now.AddMonths(2))
    .Validation(p => p.Date <= DateTime.Now.AddMonths(2)
        ? null : "Pick a date within 2 months");

var ui = Stack().Children(Label("Due date").SetContent(picker));
```

## Related

- DateTimePicker — `date-time-picker.md`
- DateRangePicker — `date-range-picker.md`
- Full docs & API: `/tesserae/components/date-picker`
