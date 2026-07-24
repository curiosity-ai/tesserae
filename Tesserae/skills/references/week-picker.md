---
name: week-picker
description: A native browser week input that surfaces the selection as a (year, weekNumber) tuple. Use when choosing an ISO week for scheduling, sprints, or weekly reports in a Tesserae (C#/Transpose) app.
---

# WeekPicker

Wraps the native HTML `<input type="week">`. The selection is exposed as a typed `(int year, int weekNumber)` tuple via the `Week` property. Derives from `MomentPickerBase` (min/max, disable, change).

## Create

`new WeekPicker((int year, int weekNumber)? week)` — construct directly (no `UI.` factory). Pass an initial week or `null`.

## Key configuration

- `Week` — get the selection as `(int year, int weekNumber)`.
- `.OnChange((s, e) => ...)` — fires on selection; read `s.Week.year` / `s.Week.weekNumber`.
- `.SetMin((year, week))` / `.SetMax((year, week))` — bounds.
- `.Disabled()` — disable the input.

## Example

```csharp
using static Tesserae.UI;

var picker = new WeekPicker((DateTime.Today.Year, 1))
    .SetMin((DateTime.Today.Year, 1))
    .SetMax((DateTime.Today.Year, 52))
    .OnChange((s, e) =>
        Toast().Information($"Selected: {s.Week.year}-W{s.Week.weekNumber}"));

var component = Label("Pick a week:").SetContent(picker);
```

## Related

- Date Picker — `.date-picker.md`
- Time Picker — `.time-picker.md`
- Full docs & API: `/tesserae/components/week-picker`
