---
name: cron-editor
description: A UI for editing cron expressions, with a simple daily/weekly/monthly schedule editor and a raw-cron fallback. Use when letting users schedule tasks in a Tesserae (C#/h5) app.
---

# CronEditor

An editor for cron expressions. Shows a human-readable description that opens
into a simplified time/day picker, with a fallback to a raw cron text input for
advanced expressions. Value is a `(string cron, bool enabled)` tuple.

## Create

`UI.CronEditor(string initialCron = "0 12 * * *", bool initialEnabled = true)` — the editor.
Bring factories into scope with `using static Tesserae.UI;`.

## Key configuration

- `.Value` — get/set the `(string cron, bool enabled)` tuple (setting it updates the UI).
- `.OnChange(ComponentEventHandler<CronEditor>)` — fires when the expression or enabled state changes; read `sender.Value`.
- `.DaysEnabled(bool = true)` — show/hide the day-of-week selector.
- `.MinuteInterval(int)` — step between selectable times in the time dropdown (default 60).
- `.ShowEnableCheckbox(bool)` — show/hide the enable toggle.
- `.AsObservable()` — the value as `IObservable<(string cron, bool enabled)>`.

## Example

```csharp
using static Tesserae.UI;

var editor = CronEditor("0 9 * * *");
var text   = TextBlock("Runs: " + editor.Value.cron);

editor.OnChange(s => text.Text("Runs: " + s.Value.cron +
    (s.Value.enabled ? "" : " (disabled)")));

// seed a preset
editor.Value = ("*/15 * * * *", editor.Value.enabled);

var ui = VStack().Children(editor, text);
```

## Related

- Dropdown — `../dropdown/SKILL.md` (used internally for frequency/time)
- Full docs & API: `/tesserae/components/cron-editor`
