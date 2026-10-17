using System;
using System.Collections.Generic;
using static Transpose.Core.dom;

namespace Tesserae
{
    /// <summary>The layouts a <see cref="Scheduler"/> can draw its appointments in.</summary>
    [Transpose.Name("tss.SchedulerView")]
    public enum SchedulerView
    {
        /// <summary>A time grid of one or more consecutive days (see <see cref="Scheduler.VisibleDays"/>).</summary>
        Day,

        /// <summary>A time grid of the seven days of the week the date falls in.</summary>
        Week,

        /// <summary>The month the date falls in, as a grid of day cells with one chip per appointment.</summary>
        Month,

        /// <summary>A list of the appointments of the coming days, grouped by date.</summary>
        Agenda,

        /// <summary>One horizontal lane per resource over a week, time running left to right.</summary>
        Timeline
    }

    /// <summary>How a <see cref="Scheduler"/>'s Day and Week views split their columns by resource.</summary>
    [Transpose.Name("tss.SchedulerGrouping")]
    public enum SchedulerGrouping
    {
        /// <summary>One column per day, every resource's appointments sharing it.</summary>
        None,

        /// <summary>One group per resource, each with a column per day.</summary>
        ByResource,

        /// <summary>One group per day, each with a column per resource.</summary>
        ByDate
    }

    /// <summary>
    /// One appointment shown by a <see cref="Scheduler"/>. A plain data object: map your own records onto it and keep
    /// the original in <see cref="Tag"/>. The scheduler never changes one; it reports what the user asked for and
    /// waits for the application to hand in the updated list.
    /// </summary>
    [Transpose.Name("tss.SchedulerAppointment")]
    public sealed class SchedulerAppointment
    {
        /// <summary>The appointment's identity, compared with <c>Equals</c>. Selection is kept by it across updates.</summary>
        public object       Id          { get; set; }

        /// <summary>The text on the appointment's block.</summary>
        public string       Title       { get; set; } = "";

        /// <summary>A longer text, shown in the default tooltip and the agenda.</summary>
        public string       Description { get; set; }

        /// <summary>Where the appointment begins, as wall-clock time.</summary>
        public DateTime     Start       { get; set; }

        /// <summary>Where the appointment ends, as wall-clock time. Must not be before <see cref="Start"/>.</summary>
        public DateTime     End         { get; set; }

        /// <summary>
        /// Draws the appointment in the all-day row instead of the time grid. An all-day appointment covers every day
        /// from <see cref="Start"/>'s date up to, and not including, <see cref="End"/> — or <see cref="Start"/>'s day
        /// alone when <see cref="End"/> is on the same date.
        /// </summary>
        public bool         IsAllDay    { get; set; }

        /// <summary>The resources the appointment belongs to. One block is drawn per resource column or lane.</summary>
        public List<object> ResourceIds { get; set; } = new List<object>();

        /// <summary>A CSS colour for the block. Defaults to the first resource's colour, then the theme's primary colour.</summary>
        public string       Color       { get; set; }

        /// <summary>Shows a lock mark and refuses every edit: no move, resize, delete or edit request.</summary>
        public bool         IsLocked    { get; set; }

        /// <summary>Shows a recurrence mark. The scheduler does not expand occurrences: hand each one in.</summary>
        public bool         IsRecurring { get; set; }

        /// <summary>Any value the application wants to keep with the appointment. Never read by the scheduler.</summary>
        public object       Tag         { get; set; }

        /// <summary>The time between <see cref="Start"/> and <see cref="End"/>.</summary>
        public TimeSpan     Duration    => End - Start;

        /// <summary>Returns a copy of the appointment, with its own resource list.</summary>
        public SchedulerAppointment Clone() => new SchedulerAppointment
        {
            Id          = Id,
            Title       = Title,
            Description = Description,
            Start       = Start,
            End         = End,
            IsAllDay    = IsAllDay,
            ResourceIds = new List<object>(ResourceIds ?? new List<object>()),
            Color       = Color,
            IsLocked    = IsLocked,
            IsRecurring = IsRecurring,
            Tag         = Tag
        };
    }

    /// <summary>A person, room or machine a <see cref="Scheduler"/> splits its columns and lanes by.</summary>
    [Transpose.Name("tss.SchedulerResource")]
    public sealed class SchedulerResource
    {
        /// <summary>Matched against <see cref="SchedulerAppointment.ResourceIds"/> with <c>Equals</c>.</summary>
        public object Id    { get; set; }

        /// <summary>The column or lane header.</summary>
        public string Name  { get; set; } = "";

        /// <summary>A CSS colour for the header chip and the resource's appointments.</summary>
        public string Color { get; set; }

        /// <summary>Initializes a new instance of the <see cref="SchedulerResource"/> class.</summary>
        public SchedulerResource() { }

        /// <summary>Initializes a new instance of the <see cref="SchedulerResource"/> class.</summary>
        public SchedulerResource(object id, string name, string color = null)
        {
            Id    = id;
            Name  = name;
            Color = color;
        }
    }

    /// <summary>An appointment dragged to a new time and/or resource. Nothing has changed yet.</summary>
    [Transpose.Name("tss.SchedulerMoveRequest")]
    public sealed class SchedulerMoveRequest
    {
        /// <summary>The application's own appointment instance.</summary>
        public SchedulerAppointment Appointment    { get; internal set; }

        /// <summary>The proposed start.</summary>
        public DateTime             NewStart       { get; internal set; }

        /// <summary>The proposed end; the duration is kept.</summary>
        public DateTime             NewEnd         { get; internal set; }

        /// <summary>The resource of the column or lane the drag started in, or null without resources.</summary>
        public object               FromResourceId { get; internal set; }

        /// <summary>The resource of the column or lane it was dropped in, or null without resources.</summary>
        public object               ToResourceId   { get; internal set; }

        /// <summary>Ctrl (or Cmd) was held on release: the user asked for a copy rather than a move.</summary>
        public bool                 IsCopy         { get; internal set; }

        /// <summary>Whether the drop lands on another resource.</summary>
        public bool                 ChangesResource => !Equals(FromResourceId, ToResourceId);
    }

    /// <summary>An appointment's start or end edge dragged. Nothing has changed yet.</summary>
    [Transpose.Name("tss.SchedulerResizeRequest")]
    public sealed class SchedulerResizeRequest
    {
        /// <summary>The application's own appointment instance.</summary>
        public SchedulerAppointment Appointment { get; internal set; }

        /// <summary>The proposed start.</summary>
        public DateTime             NewStart    { get; internal set; }

        /// <summary>The proposed end.</summary>
        public DateTime             NewEnd      { get; internal set; }
    }

    /// <summary>
    /// A stretch of time on the scheduler: what the user dragged across or double-clicked to create an appointment, or
    /// the empty slot they clicked.
    /// </summary>
    [Transpose.Name("tss.SchedulerSlot")]
    public sealed class SchedulerSlot
    {
        /// <summary>Where the slot begins.</summary>
        public DateTime   Start      { get; internal set; }

        /// <summary>Where the slot ends.</summary>
        public DateTime   End        { get; internal set; }

        /// <summary>The resource of the column or lane, or null.</summary>
        public object     ResourceId { get; internal set; }

        /// <summary>Whether the slot is a whole day (the all-day row or a month cell): <see cref="End"/> is the next midnight.</summary>
        public bool       IsAllDay   { get; internal set; }

        /// <summary>The pointer event behind it, for anchoring a popup.</summary>
        public MouseEvent Event      { get; internal set; }
    }

    /// <summary>Something dragged in from outside the scheduler and dropped on a slot (see <see cref="Scheduler.MakeDraggable"/>).</summary>
    [Transpose.Name("tss.SchedulerDropRequest")]
    public sealed class SchedulerDropRequest
    {
        /// <summary>The payload handed to <see cref="Scheduler.MakeDraggable"/>.</summary>
        public object   Payload    { get; internal set; }

        /// <summary>The title handed to <see cref="Scheduler.MakeDraggable"/>.</summary>
        public string   Title      { get; internal set; }

        /// <summary>Where it was dropped.</summary>
        public DateTime Start      { get; internal set; }

        /// <summary><see cref="Start"/> plus the duration handed to <see cref="Scheduler.MakeDraggable"/>.</summary>
        public DateTime End        { get; internal set; }

        /// <summary>The resource of the column or lane, or null.</summary>
        public object   ResourceId { get; internal set; }

        /// <summary>Whether it landed on a whole day (a month cell).</summary>
        public bool     IsAllDay   { get; internal set; }
    }

    /// <summary>
    /// The text a <see cref="Scheduler"/> draws on its own, so an application can hand in its translations. Every
    /// property has an English default; set only the ones that need to change.
    /// </summary>
    [Transpose.Name("tss.SchedulerLabels")]
    public sealed class SchedulerLabels
    {
        /// <summary>The toolbar's button back to today.</summary>
        public string   Today          { get; set; } = "Today";

        /// <summary>The toolbar's previous-range button.</summary>
        public string   Previous       { get; set; } = "Previous";

        /// <summary>The toolbar's next-range button.</summary>
        public string   Next           { get; set; } = "Next";

        /// <summary>The Day view's name.</summary>
        public string   Day            { get; set; } = "Day";

        /// <summary>The Week view's name.</summary>
        public string   Week           { get; set; } = "Week";

        /// <summary>The Month view's name.</summary>
        public string   Month          { get; set; } = "Month";

        /// <summary>The Agenda view's name.</summary>
        public string   Agenda         { get; set; } = "Agenda";

        /// <summary>The Timeline view's name.</summary>
        public string   Timeline       { get; set; } = "Timeline";

        /// <summary>The caption of the all-day row, and the agenda's time for an all-day appointment.</summary>
        public string   AllDay         { get; set; } = "All day";

        /// <summary>A month cell's overflow link; <c>{0}</c> is the number of appointments not shown.</summary>
        public string   More           { get; set; } = "+{0} more";

        /// <summary>The agenda's text when there is nothing in its range.</summary>
        public string   NoAppointments { get; set; } = "No appointments";

        /// <summary>The column or lane of appointments that name no resource.</summary>
        public string   Unassigned     { get; set; } = "Unassigned";

        /// <summary>The caption above the timeline's resource names.</summary>
        public string   Resources      { get; set; } = "Resources";

        /// <summary>The month names, January first.</summary>
        public string[] MonthNames     { get; set; } = { "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" };

        /// <summary>The short month names, January first.</summary>
        public string[] ShortMonthNames { get; set; } = { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };

        /// <summary>The weekday names, Sunday first.</summary>
        public string[] DayNames       { get; set; } = { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };

        /// <summary>The short weekday names, Sunday first.</summary>
        public string[] ShortDayNames  { get; set; } = { "Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat" };
    }
}
