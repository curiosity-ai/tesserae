---
name: toggle
description: A switch control for an on/off (boolean) setting with an auto-updating label. Use when a setting should take effect immediately on flip in a Tesserae (C#/h5) app.
---

# Toggle

A physical switch for binary settings. The on/off label updates automatically with the state, and the checked state is exposed as an `IObservable<bool>`.

## Create

`UI.Toggle()` — no labels.
`UI.Toggle(string text)` — same text shown for on and off.
`UI.Toggle(string onText, string offText)` — distinct on/off labels.
`UI.Toggle(IComponent onText, IComponent offText)` — custom label components.
`using static Tesserae.UI;`.

## Key configuration

- `.Checked(bool = true)` — set the checked state (or get/set `IsChecked`).
- `.SetText(string)` — override the auto on/off label with fixed text.
- `.Disabled(bool = true)` — enable/disable (or `IsEnabled`).
- `.AsObservable()` — returns `IObservable<bool>` tracking the checked state.
- `.Small()` / `.SemiBold()` etc. — `ITextFormating` size/weight.

Pair with `UI.Label(...).SetContent(toggle)` to give it a leading caption.

## Example

```csharp
using static Tesserae.UI;

var notifications = Toggle("On", "Off").Checked();
notifications.AsObservable().Observe(on => Console.WriteLine($"Notifications: {on}"));

var component = Stack().Children(
    Label("Notifications").Inline().SetContent(notifications),
    Toggle().Disabled()
);
```

## Related

- ToggleButton — `.../toggle-button/SKILL.md`
- CheckBox — `.../check-box/SKILL.md`
- Full docs & API: `/tesserae/components/toggle`
