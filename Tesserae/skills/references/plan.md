---
name: plan
description: A timeline card that displays a multi-step task with per-step status, progress, substeps and badges, plus header/footer commands and an overall progress bar. Use when showing a running task plan or agent task list in a Tesserae (C#/Transpose) app.
---

# Plan

A card with a title, a timeline of steps (each a status glyph on a connecting rail
plus text), optional substeps, and a footer status strip with a progress bar and a
start/stop button. Drive it imperatively with `.AddTask(...)` or declaratively with
`.SetModel(model)`.

## Create

`UI.Plan(string title)` — returns a `Plan`. Also `new Plan(title)`.
Bring factories into scope with `using static Tesserae.UI;`.

## PlanStatus

Every step, substep and the plan itself carries a `PlanStatus`, which drives the
glyph, the glyph color, the text weight and the text color:

| Status | Glyph | Glyph color | Text |
| --- | --- | --- | --- |
| `Completed` | check-circle | success (green) | semibold, default color |
| `Running` | spinner (rotates) | primary | bold, default color |
| `Active` | circle | primary | bold, default color |
| `Failed` | cross-circle | danger (red) | bold, danger color |
| `Pending` | circle | secondary | semibold, secondary color |
| `Canceled` | ban | warning (orange) | semibold, default color |

`Running` and `Active` are styled alike — use `Active` for work that is in progress
but has nothing streaming in, so the row does not spin. Both promote the step into a
tinted panel (except in compact mode).

## Key configuration

- `.AddTask(string title, bool completed)` — append a step (`Completed` / `Pending`).
- `.AddTask(string title, PlanStatus status, IComponent badge = null)` — append a step
  with an explicit status and an optional badge at the end of its title row.
- `.Title(string)` — update the heading.
- `.FooterMessage(string)` — bold prefix in front of the derived progress summary.
- `.HeaderCommands(params IComponent[])` / `.FooterCommands(params IComponent[])`.
- `.Progress(int position, int total)` / `.Progress(float percent)` / `.Indeterminate()`.
- `.StartStopButton(Action<Button>)`, `.Start()` / `.Stop()`,
  `.ShowStartStopButton()` / `.HideStartStopButton()`.
- `.SetModel(PlanModel)` — see below.

### Density and chrome

- `.Compact(bool = true)` — tighter rows, and every status glyph but the running
  spinner collapses to a plain filled dot in the status color. Safe to call before or
  after the steps are added. Use when the plan is a detail inside something else.
- `.NoHeader(bool = true)` — hide the title strip (the container supplies the heading).
- `.NoFooter(bool = true)` — hide the progress strip.
- `.NoBorder(bool = true)` — drop the border, radius, shadow and background so the
  plan sits flush inside its container.

## Data-driven updates

`.SetModel(PlanModel)` applies a plain data model and reconciles the DOM in place.
Steps and substeps are matched by `Id` (or a positional key when no id is given), so
nodes are reused and a running spinner is not restarted.

```csharp
public sealed class PlanModel
{
    public string Id, Title, CurrentStage, FooterMessage, Searches;
    public PlanStatus Status;
    public float? Progress;                       // 0..1
    public IList<PlanStepModel> Steps;
    public DateTimeOffset? StartedAt, CompletedAt;
}

public sealed class PlanStepModel
{
    public string Id, Title, CurrentStage;
    public int Index;
    public PlanStatus Status;
    public float? Progress;                       // 0..1
    public IList<PlanSubstepModel> Substeps;
    public bool? IsExpanded;                      // null = auto (open when in progress or failed)
    public IComponent Badge;                      // end of the title row
}

public sealed class PlanSubstepModel
{
    public string Id, Title, Message;
    public PlanStatus Status;
}
```

`Plan.Model` returns the last model passed to `SetModel`, or `null` when the plan was
only driven through the fluent API.

## Example

```csharp
using static Tesserae.UI;

var plan = Plan("Nightly build")
    .HeaderCommands(Button("Update").NoBorder().Rounded())
    .AddTask("Restore packages", PlanStatus.Completed, Badge("12s").Success())
    .AddTask("Compile",          PlanStatus.Completed, Badge("1m 04s").Success())
    .AddTask("Run unit tests",   PlanStatus.Running,   Badge("318 / 902").Primary())
    .AddTask("Publish",          PlanStatus.Pending)
    .FooterMessage("Finalizing...")
    .Progress(2, 4)
    .Start()
    .MaxWidth(800.px());
```

Embedded in a card that supplies its own title and frame:

```csharp
var card = Card(
    Plan("")
        .Compact().NoHeader().NoFooter().NoBorder()
        .AddTask("Collect the RFCs",      PlanStatus.Completed)
        .AddTask("Survey the providers",  PlanStatus.Completed)
        .AddTask("Gather sample payloads", PlanStatus.Running, Badge("3 of 8").Primary())
        .AddTask("Write the explanation", PlanStatus.Pending)
        .WS()
).SetTitle("Research progress");
```

The same shape works as the body of a `ResourceCard` via `.SetDescription(...)`.

## Related

- Card — `card.md`
- Resource Card — `resource-card.md`
- Badge — `badge.md`
- ProgressIndicator — `progress-indicator.md`
- Full docs & API: `/tesserae/components/plan`
