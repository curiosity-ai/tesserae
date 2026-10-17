---
name: scheduler
description: Scheduler — a calendar of appointments with Day, Week, Month, Agenda and multi-resource Timeline views, drag-to-move, edge resize, drag/double-click to create, external drag-drop, selection, delete and context-menu requests. Use when showing or editing appointments, bookings, shifts or jobs over time in a Tesserae (C#/Transpose) app.
---

# Scheduler

A toolbar (Today, previous/next, the range title, a view switcher) over one of five views:

| View | Shows |
|---|---|
| `Day` | a time grid of `VisibleDays(n)` days from the date (default 1) |
| `Week` | a time grid of the week holding the date (`FirstDayOfWeek`) |
| `Month` | the month as day cells with chips and "+N more" |
| `Agenda` | a list of the next `AgendaDays(n)` days (default 7), grouped by date |
| `Timeline` | one lane per resource over `TimelineDays(n)` days (default 7 = the week) |

**The scheduler never changes the data it shows.** Every gesture raises a request carrying the
proposed change; the application updates its own list and hands it back with `.Appointments(...)`.
An unhandled request changes nothing on screen.

## Create

`UI.Scheduler()` returns a `Scheduler` (`using static Tesserae.UI;`). Give it a height (`.H(600)`,
`.S()` or `.Grow()`): the body scrolls inside it.

## Data

- `SchedulerAppointment` — `Id` (stable; selection is kept by it), `Title`, `Description`, `Start`,
  `End`, `IsAllDay`, `ResourceIds` (`List<object>`), `Color` (CSS), `IsLocked` (no move, resize,
  delete or edit), `IsRecurring` (mark only), `Tag`; `Clone()`.
  An appointment of 24 hours or more, or `IsAllDay`, is drawn in the all-day row. An all-day
  appointment covers `Start.Date` up to, not including, `End` (or just `Start`'s day).
- `SchedulerResource(id, name, color)` — `.Resources(...)`. Appointments naming none of them gather
  in an "Unassigned" column/lane.
- `.Appointments(IEnumerable<SchedulerAppointment>)` — replace the whole list after every change.

## Configuration

- `.View(SchedulerView)`, `.Views(params SchedulerView[])` (what the switcher offers), `.GoTo(date)`,
  `.Previous()`, `.Next()`, `.GoToToday()`, `.Now(DateTime)` (override the clock: today highlight,
  now line, Today button).
- `.FirstDayOfWeek(DayOfWeek)`, `.VisibleDays(n)`, `.AgendaDays(n)`, `.TimelineDays(n)`.
- `.VisibleHours(start, end)`, `.WorkHours(TimeSpan, TimeSpan)` / `.NoWorkHours()`,
  `.SlotMinutes(30)` (grid lines and drag snap; Alt snaps to the minute), `.HourHeight(48)`,
  `.TimelineHourWidth(24)`, `.ColumnMinWidth(90)`, `.MonthMaxAppointments(3)`.
- `.Grouping(SchedulerGrouping.ByResource | ByDate | None)` — resource columns in Day/Week.
- `.TwelveHourClock()`, `.ReadOnly()`, `.SingleClickCreates()`, `.HideToolbar()`,
  `.Commands(params IComponent[])` (toolbar, right side), `.Labels(new SchedulerLabels { … })`.
- `.AppointmentTemplate(Func<SchedulerAppointment, IComponent>)`, `.AppointmentTooltip(...)`, `.NoTooltips()`.

## Requests and events

| Method | Raised when |
|---|---|
| `.OnMove(Action<SchedulerMoveRequest>)` | dragged to a new time/resource. `NewStart`, `NewEnd`, `FromResourceId`, `ToResourceId`, `ChangesResource`, `IsCopy` (Ctrl held) |
| `.OnResize(Action<SchedulerResizeRequest>)` | an edge dragged. `NewStart`, `NewEnd` |
| `.OnCreate(Action<SchedulerSlot>)` | drag across empty time, or double-click a slot (1 h; whole day in the all-day row and Month). `Start`, `End`, `ResourceId`, `IsAllDay`, `Event` |
| `.OnSlotClick(Action<SchedulerSlot>)` | every click on an empty slot |
| `.OnEdit(Action<SchedulerAppointment>)` | double-click or Enter on an appointment |
| `.OnContextMenu(Action<SchedulerAppointment, MouseEvent>)` | right-click; build a `ContextMenu().ShowAt(...)` |
| `.OnDelete(Action<IReadOnlyList<SchedulerAppointment>>)` | Delete/Backspace with a selection |
| `.OnSelectionChanged(...)`, `.OnNavigate(Action<SchedulerView, DateTime>)` | |
| `.OnExternalDrop(Action<SchedulerDropRequest>)` | something made with `Scheduler.MakeDraggable(component, payload, title, duration)` dropped on a slot |

Selection: click, Ctrl+click toggles; `.Select(...)`, `.ClearSelection()`, `SelectedAppointments`.

## Example

```csharp
var list = new List<SchedulerAppointment>();
Scheduler scheduler = null;
void Refresh() => scheduler.Appointments(list.ToList());

scheduler = Scheduler()
    .FirstDayOfWeek(DayOfWeek.Monday)
    .Resources(new SchedulerResource(1, "Alex", "#2f6fde"), new SchedulerResource(2, "Bailey", "#1f9d6b"))
    .OnMove(e =>
    {
        e.Appointment.Start = e.NewStart;
        e.Appointment.End   = e.NewEnd;
        if (e.ChangesResource) e.Appointment.ResourceIds = new List<object> { e.ToResourceId };
        Refresh();
    })
    .OnResize(e => { e.Appointment.Start = e.NewStart; e.Appointment.End = e.NewEnd; Refresh(); })
    .OnCreate(s =>
    {
        list.Add(new SchedulerAppointment { Id = Guid.NewGuid().ToString(), Title = "New", Start = s.Start, End = s.End, IsAllDay = s.IsAllDay,
                                            ResourceIds = s.ResourceId is object ? new List<object> { s.ResourceId } : new List<object>() });
        Refresh();
    })
    .OnDelete(items => { foreach (var a in items) list.Remove(a); Refresh(); })
    .H(640);
```

## Related

`date-picker.md`, `week-picker.md`, `timeline.md`, `activity-calendar.md`, `context-menu.md`, `task-board.md`.
