---
name: toggle-button
description: A button that maintains an on/off checked state. Use when you need a pressable toolbar-style toggle (bold, mute, pin) in a Tesserae (C#/h5) app.
---

# ToggleButton

A button that keeps a checked state. Created from an existing `Button`, so it inherits all button styling (icons, sizes, colours, hotkeys).

## Create

`button.ToToggle()` — extension on `Button` returning a `ToggleButton`. Build the button first with `UI.Button(...)`, then call `.ToToggle()`.
`using static Tesserae.UI;`.

## Key configuration

- `.Checked(bool = true)` — set the checked state (or get/set the `IsChecked` property).
- `.OnChange((s, e) => ...)` — fires whenever the checked state flips; read `s.IsChecked`.
- `.Disabled(bool = true)` — enable/disable.
- `IsEnabled` — property mirroring the underlying button's enabled state.

## Example

```csharp
using static Tesserae.UI;

var bold = Button("Bold").SetIcon(UIcons.Bold)
    .ToToggle()
    .Checked()
    .OnChange((s, e) => Console.WriteLine($"Bold: {s.IsChecked}"));
```

## Related

- Button — `.../button/SKILL.md`
- Toggle — `.../toggle/SKILL.md`
- Full docs & API: `/tesserae/components/toggle-button`
