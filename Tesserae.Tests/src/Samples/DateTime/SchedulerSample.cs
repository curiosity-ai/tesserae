using System;
using System.Collections.Generic;
using System.Linq;
using static Transpose.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = SampleGroup.DateTime, Order = 90, Icon = UIcons.Calendars, Description = "Day, week, month, agenda and resource timeline")]
    public class SchedulerSample : IComponent, ISample
    {
        private readonly IComponent _content;

        private readonly List<SchedulerAppointment> _appointments = new List<SchedulerAppointment>();
        private readonly Scheduler                  _scheduler;
        private readonly Scheduler                  _readOnly;
        private readonly TextBlock                  _log = TextBlock("Drag, resize, double-click or right-click an appointment.").Small().Secondary();
        private          int                        _nextId = 100;

        private static readonly SchedulerResource[] Technicians =
        {
            new SchedulerResource(1, "Alex Moreno",  "#2f6fde"),
            new SchedulerResource(2, "Bailey Chen",  "#1f9d6b"),
            new SchedulerResource(3, "Casey Okafor", "#c2571a"),
        };

        public SchedulerSample()
        {
            var today = SampleDate.Today;
            var week  = today.AddDays(-(int)today.DayOfWeek + 1); // Monday of the sample week

            Add("Site survey",       week.AddHours(9),               90,  1, "Measure the plant room before the install.");
            Add("Boiler install",    week.AddDays(1).AddHours(8),    240, 2, "Two-person job, bring the long ladder.");
            Add("Customer call",     week.AddDays(1).AddHours(10),   30,  1);
            Add("Safety briefing",   week.AddDays(1).AddHours(10.5), 60,  3, recurring: true);
            Add("Parts pickup",      week.AddDays(2).AddHours(7.5),  45,  3);
            Add("Inspection",        week.AddDays(2).AddHours(13),   120, 1);
            Add("Inspection report", week.AddDays(2).AddHours(14),   90,  2);
            Add("Quarterly review",  week.AddDays(3).AddHours(15),   60,  1, locked: true, description: "Locked: booked by the office.");
            Add("Emergency repair",  week.AddDays(4).AddHours(11),   150, 3);
            Add("Team lunch",        week.AddDays(4).AddHours(12),   60,  2, color: "#8b5cf6");
            Add("Night maintenance", week.AddDays(3).AddHours(22),   300, 2, description: "Runs past midnight.");

            _appointments.Add(new SchedulerAppointment { Id = _nextId++, Title = "Trade show",  Start = week.AddDays(2), End = week.AddDays(4), IsAllDay = true, ResourceIds = new List<object> { 1, 2 }, Color = "#0e7490" });
            _appointments.Add(new SchedulerAppointment { Id = _nextId++, Title = "Annual leave", Start = week.AddDays(4), End = week.AddDays(7), IsAllDay = true, ResourceIds = new List<object> { 3 } });
            _appointments.Add(new SchedulerAppointment { Id = _nextId++, Title = "Unassigned job", Start = week.AddDays(1).AddHours(14), End = week.AddDays(1).AddHours(15) });

            for (var i = 0; i < 4; i++) Add("Follow-up " + (i + 1), week.AddDays(8 + i * 3).AddHours(9 + i), 60, 1 + i % 3);

            _scheduler = Scheduler()
               .Now(SampleDate.Now)
               .GoTo(today)
               .FirstDayOfWeek(DayOfWeek.Monday)
               .Resources(Technicians)
               .Grouping(SchedulerGrouping.None)
               .WorkHours(TimeSpan.FromHours(8), TimeSpan.FromHours(18))
               .OnMove(e =>
                {
                    if (e.IsCopy)
                    {
                        var copy = e.Appointment.Clone();
                        copy.Id = _nextId++;
                        copy.Start = e.NewStart;
                        copy.End = e.NewEnd;
                        copy.ResourceIds = MoveResource(copy.ResourceIds, e.FromResourceId, e.ToResourceId);
                        _appointments.Add(copy);
                        Log($"Copied “{copy.Title}” to {Describe(e.NewStart)}");
                    }
                    else
                    {
                        e.Appointment.Start = e.NewStart;
                        e.Appointment.End = e.NewEnd;
                        e.Appointment.ResourceIds = MoveResource(e.Appointment.ResourceIds, e.FromResourceId, e.ToResourceId);
                        Log($"Moved “{e.Appointment.Title}” to {Describe(e.NewStart)}" + (e.ChangesResource ? $" ({NameOf(e.ToResourceId)})" : ""));
                    }
                    Refresh();
                })
               .OnResize(e =>
                {
                    e.Appointment.Start = e.NewStart;
                    e.Appointment.End = e.NewEnd;
                    Log($"Resized “{e.Appointment.Title}” to {Describe(e.NewStart)} – {e.NewEnd.Hour:00}:{e.NewEnd.Minute:00}");
                    Refresh();
                })
               .OnCreate(slot => OpenEditor(null, slot))
               .OnEdit(a => OpenEditor(a, null))
               .OnDelete(list =>
                {
                    foreach (var a in list) _appointments.Remove(a);
                    Log($"Deleted {list.Count} appointment(s)");
                    Refresh();
                })
               .OnContextMenu((a, e) => ShowMenu(a, e))
               .OnExternalDrop(drop =>
                {
                    _appointments.Add(new SchedulerAppointment
                    {
                        Id = _nextId++, Title = drop.Title, Start = drop.Start, End = drop.End, IsAllDay = drop.IsAllDay,
                        ResourceIds = drop.ResourceId is object ? new List<object> { drop.ResourceId } : new List<object>(),
                    });
                    Log($"Scheduled “{drop.Title}” at {Describe(drop.Start)}");
                    Refresh();
                })
               .OnNavigate((view, date) => Log($"Showing {view} around {date.Day}/{date.Month}"));

            _readOnly = Scheduler()
               .Now(SampleDate.Now)
               .GoTo(today)
               .FirstDayOfWeek(DayOfWeek.Monday)
               .View(SchedulerView.Timeline)
               .Views(SchedulerView.Timeline, SchedulerView.Agenda)
               .VisibleHours(7, 20)
               .TimelineHourWidth(28)
               .Resources(Technicians)
               .ReadOnly();

            Refresh();

            var queue = VStack().Gap(6.px()).Children(
                SampleSubTitle("Unscheduled"),
                QueueItem("Boiler service", TimeSpan.FromHours(2)),
                QueueItem("Leak check",     TimeSpan.FromMinutes(45)),
                QueueItem("Quote visit",    TimeSpan.FromHours(1)));

            var grouping = HStack().AlignItemsCenter().Gap(4.px()).Children(
                TextBlock("Resource columns:").Small(),
                Button("Shared").OnClick(() => _scheduler.Grouping(SchedulerGrouping.None)),
                Button("By resource").OnClick(() => _scheduler.Grouping(SchedulerGrouping.ByResource)),
                Button("By date").OnClick(() => _scheduler.Grouping(SchedulerGrouping.ByDate)));

            var options = HStack().AlignItemsCenter().Gap(16.px()).Children(
                grouping,
                Toggle("12-hour clock").OnChange((s, _) => _scheduler.TwelveHourClock(s.IsChecked)),
                Toggle("Read-only").OnChange((s, _) => _scheduler.ReadOnly(s.IsChecked)));

            _content = SectionStack().Secondary()
               .S()
               .SampleTitle(typeof(SchedulerSample), UIcons.Calendars, "A calendar of appointments over days, weeks, months and resources")
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                        TextBlock("The Scheduler shows appointments in five views: a time grid of one or more days or a whole week, a month grid, an agenda list and a timeline with one lane per resource. Appointments can be selected (Ctrl+click adds to the selection), dragged to a new time or resource (hold Ctrl to copy, Alt to snap to the minute), resized by their edges, created by dragging across empty time or double-clicking a slot, and dropped in from another part of the page."),
                        TextBlock("The scheduler never changes the data it shows. Every gesture is reported as a request - OnMove, OnResize, OnCreate, OnDelete - and the application updates its own list and hands it back with Appointments(...)."))).SetTitle("Overview")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                        TextBlock("Keep the appointments in your own list and route every request through one refresh that hands the list back. Give each appointment a stable Id so the selection survives the refresh. Use resources for people, rooms or machines: the week view splits its columns by them and the timeline draws a lane each. Lock appointments the user may not change, and use ReadOnly() for a calendar that is only to be looked at."))).SetTitle("Best Practices")))
               .FlatSection(Card(VStack().WS().Children(
                        options.MB(8),
                        HStack().WS().Gap(12.px()).Children(
                            _scheduler.Grow().H(640),
                            queue.W(170).NoShrink()),
                        _log.MT(8))).SetTitle("Usage"))
               .FlatSection(Card(VStack().WS().Children(
                        TextBlock("A read-only timeline of the same week, limited to 07:00 - 20:00, offering only the Timeline and Agenda views.").Small().Secondary().MB(8),
                        _readOnly.WS().H(320))).SetTitle("Read-only timeline"))
               .SeeAlso(typeof(DatePickerSample), typeof(DateRangePickerSample), typeof(WeekPickerSample), typeof(ActivityCalendarSample), typeof(TimelineSample));
        }

        private void Add(string title, DateTime start, int minutes, int resource, string description = null, bool recurring = false, bool locked = false, string color = null)
        {
            _appointments.Add(new SchedulerAppointment
            {
                Id          = _nextId++,
                Title       = title,
                Description = description,
                Start       = start,
                End         = start.AddMinutes(minutes),
                ResourceIds = new List<object> { resource },
                IsRecurring = recurring,
                IsLocked    = locked,
                Color       = color
            });
        }

        private void Refresh()
        {
            _scheduler.Appointments(_appointments.ToList());
            _readOnly.Appointments(_appointments.ToList());
        }

        private void Log(string text) => _log.Text = text;

        private static List<object> MoveResource(List<object> ids, object from, object to)
        {
            var list = (ids ?? new List<object>()).Where(id => !Equals(id, from)).ToList();
            if (to is object && !list.Any(id => Equals(id, to))) list.Add(to);
            return list;
        }

        private static string NameOf(object resourceId) => Technicians.FirstOrDefault(r => Equals(r.Id, resourceId))?.Name ?? "Unassigned";

        private static string Describe(DateTime t) => $"{t.Day}/{t.Month} {t.Hour:00}:{t.Minute:00}";

        private IComponent QueueItem(string title, TimeSpan duration)
        {
            var card = HStack().AlignItemsCenter().Gap(6.px()).Class("tss-scheduler-sample-queue").Children(
                Icon(UIcons.GripDotsVertical).Small(),
                VStack().Children(
                    TextBlock(title).SemiBold().Small(),
                    TextBlock(duration.TotalMinutes >= 60 ? $"{duration.TotalHours:0.#} h" : $"{duration.TotalMinutes} min").Small().Secondary()));

            return Scheduler.MakeDraggable(card, title, title, duration);
        }

        private void ShowMenu(SchedulerAppointment a, MouseEvent e)
        {
            ContextMenu().Items(
                ContextMenuItem("Edit…").Disabled(a.IsLocked).OnClick(() => OpenEditor(a, null)),
                ContextMenuItem("Duplicate").OnClick(() =>
                {
                    var copy = a.Clone();
                    copy.Id = _nextId++;
                    copy.IsLocked = false;
                    copy.Start = a.Start.AddDays(1);
                    copy.End = a.End.AddDays(1);
                    _appointments.Add(copy);
                    Refresh();
                }),
                ContextMenuItem(a.IsLocked ? "Unlock" : "Lock").OnClick(() =>
                {
                    a.IsLocked = !a.IsLocked;
                    Refresh();
                }),
                ContextMenuItem().Divider(),
                ContextMenuItem("Delete").Disabled(a.IsLocked).OnClick(() =>
                {
                    _appointments.Remove(a);
                    Log($"Deleted “{a.Title}”");
                    Refresh();
                })
            ).ShowAt((int)e.clientX, (int)e.clientY, 160);
        }

        private void OpenEditor(SchedulerAppointment existing, SchedulerSlot slot)
        {
            var title = TextBox(existing?.Title ?? "New appointment").WS();
            var notes = TextBox(existing?.Description ?? "").WS();
            var when  = existing is object ? $"{Describe(existing.Start)} – {Describe(existing.End)}"
                      : slot.IsAllDay ? $"{slot.Start.Day}/{slot.Start.Month}, all day"
                      : $"{Describe(slot.Start)} – {slot.End.Hour:00}:{slot.End.Minute:00}" + (slot.ResourceId is object ? $", {NameOf(slot.ResourceId)}" : "");

            var dialog = Dialog(VStack().W(320).Children(
                TextBlock(when).Small().Secondary().MB(8),
                Label("Title").SetContent(title),
                Label("Notes").SetContent(notes)), TextBlock(existing is object ? "Edit appointment" : "New appointment").SemiBold());

            dialog.OkCancel(() =>
            {
                if (existing is object)
                {
                    existing.Title = title.Text;
                    existing.Description = notes.Text;
                    Log($"Edited “{existing.Title}”");
                }
                else
                {
                    _appointments.Add(new SchedulerAppointment
                    {
                        Id = _nextId++, Title = title.Text, Description = notes.Text, Start = slot.Start, End = slot.End, IsAllDay = slot.IsAllDay,
                        ResourceIds = slot.ResourceId is object ? new List<object> { slot.ResourceId } : new List<object>(),
                    });
                    Log($"Created “{title.Text}” at {Describe(slot.Start)}");
                }
                Refresh();
            });
        }

        public HTMLElement Render() => _content.Render();
    }
}
