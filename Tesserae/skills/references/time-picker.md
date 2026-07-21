---
name: time-picker
description: A native browser time input that stores its value as a DateTimeOffset. Use when collecting a time-only value (schedules, reminders, cut-offs) in a Tesserae (C#/Transpose) app.
---

# TimePicker

A wrapper over the native HTML `<input type="time">`. Selection is exposed as a `DateTimeOffset` via the `Time` property. Derives from `MomentPickerBase`, so it inherits min/max, disable, and change handling.

## Create

`new TimePicker(DateTimeOffset? time = null)` — there is no `UI.` factory; construct it directly. Pass an initial value or `null`.

## Key configuration

- `Time` — get the selected time as a `DateTimeOffset`.
- `.OnChange((s, e) => ...)` — fires when the user picks a time; read `s.Time`.
- `.SetMin(...)` / `.SetMax(...)` — bounds (from `MomentPickerBase`).
- `.Disabled()` — disable the input.
- `.WithBrowserFallback()` — adds a `pattern` attribute (`HH:mm`) for older browsers.

## Example

```csharp
using static Tesserae.UI;

var now = DateTimeOffset.Now;

var component = Stack().Children(
    Label("Standard").SetContent(new TimePicker()),
    Label("Pre-selected").SetContent(new TimePicker(now)),
    new TimePicker(now).OnChange((s, e) =>
        Toast().Information($"Selected: {s.Time:HH:mm:ss}"))
);
```

## Related

- Date Picker — `.date-picker.md`
- Date Time Picker — `.date-time-picker.md`
- Full docs & API: `/tesserae/components/time-picker`
