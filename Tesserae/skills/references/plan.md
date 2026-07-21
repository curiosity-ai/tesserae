---
name: plan
description: A card that displays a multi-step task with checkable sub-tasks, header/footer commands, and an overall progress bar. Use when showing a running task plan or agent task list in a Tesserae (C#/Transpose) app.
---

# Plan

A `Card`-based panel with a title, a list of tasks (each a circle/check icon + text), a footer message, and a progress indicator with an optional start/stop button.

## Create

`UI.Plan(string title)` — returns a `Plan`. Also `new Plan(title)`.
Bring factories into scope with `using static Tesserae.UI;`.

## Key configuration

- `.AddTask(string title, bool completed)` — append a task row (filled check if completed).
- `.Title(string)` — update the heading.
- `.FooterMessage(string)` — text shown in the footer.
- `.HeaderCommands(params IComponent[])` / `.FooterCommands(params IComponent[])` — set the header/footer action area.
- `.Progress(int position, int total)` / `.Progress(float percent)` — set determinate progress.
- `.Indeterminate()` — animated indeterminate progress bar.
- `.StartStopButton(Action<Button>)` — wire the start/stop toggle; `.Start()` shows a play icon, `.Stop()` shows a stop icon.
- `.ShowStartStopButton()` / `.HideStartStopButton()` — toggle the button's visibility.

## Example

```csharp
using static Tesserae.UI;

var plan = Plan("Database Migration")
    .HeaderCommands(Button("Update").NoBorder().Rounded())
    .AddTask("Backup database", true)
    .AddTask("Run schema update", true)
    .AddTask("Migrate data", false)
    .FooterMessage("Finalizing...")
    .Progress(2, 3)
    .Stop()
    .MaxWidth(800.px());
```

## Related

- ProgressIndicator — `.progress-indicator.md`
- Full docs & API: `/tesserae/components/plan`
