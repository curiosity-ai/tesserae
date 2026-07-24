---
name: date-time-picker
description: A form input for picking a single date together with a time-of-day, backed by the native datetime-local input. Use when collecting a precise date+time in a Tesserae (C#/Transpose) app.
---

# DateTimePicker

A date+time form input wrapping the browser's native
`<input type="datetime-local">`. Shares the validation/step/min/max helpers with
`DatePicker` (both extend `MomentPickerBase`).

## Create

`UI.DateTimePicker(DateTime? dateTime = null)` — picker, optionally with a default value.
Bring factories into scope with `using static Tesserae.UI;`.

## Key configuration

- `.DateTime` — gets the selected `DateTime`.
- `.SetMin(DateTime)` / `.SetMax(DateTime)` — bounds.
- `.SetStep(int)` — step increment.
- `.Disabled()` / `.Required()` — state.
- `.Validation(Func<DateTimePicker,string>)` — custom validator; return null when valid, else a message.
- `.Error(string)` / `.IsInvalid()` — manual error.
- `.WithBrowserFallback()` — add a regex `pattern` for older browsers.

## Example

```csharp
using static Tesserae.UI;

var picker = DateTimePicker(DateTime.Now.AddDays(2))
    .Validation(p => p.DateTime <= DateTime.Now.AddMonths(2)
        ? null : "Pick a time within 2 months");

var ui = Stack().Children(Label("Starts at").SetContent(picker));
```

## Related

- DatePicker — `date-picker.md`
- DateRangePicker — `date-range-picker.md`
- Full docs & API: `/tesserae/components/date-time-picker`
