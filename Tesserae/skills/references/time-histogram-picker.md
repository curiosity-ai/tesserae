---
name: time-histogram-picker
description: A histogram with draggable thumbs for selecting a time range over a set of timestamps. Use when filtering data by date/time range visually in a Tesserae (C#/Transpose) app.
---

# TimeHistogramPicker

Turns a `DateTime[]` (or pre-aggregated buckets) into adaptive histogram bars and lets users narrow the selected range by dragging the left/right thumbs or using arrow keys.

## Create

`UI.TimeHistogramPicker(DateTime[] values, int maxBuckets = 80)` — builds buckets from raw timestamps (a private sorted copy is made, so input order doesn't matter).
`UI.TimeHistogramPicker(TimeHistogramBucket[] buckets)` — use pre-computed buckets (e.g. counts from a backend); each is `new TimeHistogramBucket(start, end, count)`.
`using static Tesserae.UI;`.

## Key configuration

- `.OnRangeChanged((from, to, count) => ...)` — fires when the selection changes; `from`/`to` are `DateTime`, `count` is the number of values in range.
- `.SetValues(DateTime[])` / `.SetBuckets(TimeHistogramBucket[])` — replace the data.
- `.SetRange(from, to)` — set the selected range programmatically.
- `.MaxBuckets(int)` — change bucket resolution (raw-values mode only).
- `.WithCustomTimeRender(Func<DateTime,string>)` — format axis/tooltip labels.
- `.ShowBucketTooltipOnHover(bool)` / `.ShowCountOnHover(bool)` — per-bar tooltip.
- `.Disabled()` — disable interaction.

Read the selection via `SelectedFrom`, `SelectedTo`, `SelectedCount`.

## Example

```csharp
using static Tesserae.UI;

var values = new[] { DateTime.Now.AddMinutes(-15), DateTime.Now.AddMinutes(30), DateTime.Now.AddHours(2) };

var picker = TimeHistogramPicker(values, 12)
    .WithCustomTimeRender(d => d.ToString("MMM d, HH:mm"))
    .OnRangeChanged((from, to, count) =>
        Console.WriteLine($"{from:g} - {to:g} ({count} values)"));
```

## Related

- Date Picker — `.date-picker.md`
- Full docs & API: `/tesserae/components/time-histogram-picker`
