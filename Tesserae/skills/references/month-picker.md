---
name: month-picker
description: A form input for picking a single calendar month (year + month) via the browser's native month input, surfaced as a typed (year, month) tuple. Use for billing periods, report months or renewal dates in a Tesserae (C#/Transpose) app.
---

# MonthPicker

Picks a year + month using the browser's native month input. The selected value
is a typed `(int year, int month)` tuple, avoiding manual string parsing.

## Create

`new MonthPicker((int year, int month)? monthAndYear)` — pass an initial value or
`null` for empty. This component is constructed directly (not via a `UI.` factory).
Typically wrapped in a `Label`.

## Key configuration

- `.Month` — read the selected `(year, month)` tuple.
- `.OnChange((sender, ev) => ...)` — fires on selection; read `sender.Month`.
- `.SetMin((year, month))` / `.SetMax((year, month))` — constrain the range.
- `.WithBrowserFallback()` — adds a `pattern` attribute for older browsers.
- `.Disabled()` — disable the input.

## Example

```csharp
using System;
using static Tesserae.UI;

var picker = new MonthPicker((DateTime.Today.Year, DateTime.Today.Month))
    .SetMin((DateTime.Today.Year - 1, 1))
    .SetMax((DateTime.Today.Year + 1, 12))
    .OnChange((s, e) => Toast().Information($"Selected: {s.Month.year}-{s.Month.month:D2}"));

var form = VStack().Children(
    Label("Billing month").SetContent(picker)
);
```

## Related

- Label — `label.md`
- DatePicker — `/tesserae/components/date-picker`
- Full docs & API: `/tesserae/components/month-picker`
