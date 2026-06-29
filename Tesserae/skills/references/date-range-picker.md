---
name: date-range-picker
description: A composite picker for a contiguous "from → to" date range, made of two synced DatePickers. Use when collecting a start/end date range in a Tesserae (C#/h5) app.
---

# DateRangePicker

Two `DatePicker`s joined by a separator. They stay in sync: setting "from"
raises the "to" picker's minimum, and setting "to" lowers the "from" maximum.

## Create

`UI.DateRangePicker(DateTime? from = null, DateTime? to = null)` — the range picker.
Bring factories into scope with `using static Tesserae.UI;`.

## Key configuration

- `.From` / `.To` — currently selected dates as `DateTime?` (null when unset).
- `.SetFrom(DateTime)` / `.SetTo(DateTime)` — set programmatically.
- `.OnChange(Action<DateRangePicker>)` — fires when either date changes (a `(self, Event)` overload also exists).
- `.FromPicker` / `.ToPicker` — the inner `DatePicker`s for advanced config (min/max bounds, validation).

## Example

```csharp
using static Tesserae.UI;

var range = DateRangePicker(DateTime.Today, DateTime.Today.AddDays(7));
range.OnChange(r =>
{
    if (r.From is DateTime f && r.To is DateTime t)
        Console.WriteLine($"{f:d} to {t:d}");
});
```

## Related

- DatePicker — `date-picker.md`
- DateTimePicker — `date-time-picker.md`
- Full docs & API: `/tesserae/components/date-range-picker`
