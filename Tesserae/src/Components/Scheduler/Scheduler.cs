using System;
using System.Collections.Generic;
using System.Linq;
using Transpose;
using static Transpose.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// A calendar of appointments: Day, Week, Month, Agenda and a multi-resource Timeline view, with a toolbar to move
    /// between ranges and views. Appointments can be selected, dragged to another time or resource, resized by their
    /// edges, created by dragging across empty time or double-clicking a slot, and dropped in from outside.
    /// <para>
    /// The scheduler never changes the appointments it is given. Every gesture is reported as a request carrying the
    /// proposed change (<see cref="OnMove"/>, <see cref="OnResize"/>, <see cref="OnCreate"/>, <see cref="OnDelete"/>,
    /// …); the application decides, updates its own data and hands the new list back through
    /// <see cref="Appointments"/>. An unhandled request changes nothing on screen.
    /// </para>
    /// <para>
    /// A burst of configuration calls on a scheduler already on the page costs one rebuild, on the next frame. The
    /// scroll position of each view survives a rebuild, so live updates do not jump the grid back to the morning.
    /// </para>
    /// </summary>
    [Transpose.Name("tss.Scheduler")]
    public sealed class Scheduler : ComponentBase<Scheduler, HTMLElement>
    {
        private const string ItemProperty   = "_tssSchedulerItem";
        private const string ColumnProperty = "_tssSchedulerColumn";
        private const string LaneProperty   = "_tssSchedulerLane";
        private const string DayProperty    = "_tssSchedulerDay";
        private const double DragThreshold  = 5;

        private enum Surface { TimeGrid, Timeline, Days, Agenda }

        private enum DragKind { Move, ResizeStart, ResizeEnd, Create, DayMove }

        private sealed class Item
        {
            public SchedulerAppointment Appointment;
            public HTMLElement          Element;
            public object               ResourceId;
            public Surface              Surface;
        }

        private sealed class Column
        {
            public DateTime          Date;
            public SchedulerResource Resource;
            public HTMLElement       Body;
        }

        private sealed class Lane
        {
            public SchedulerResource Resource;
            public HTMLElement       Body;
        }

        private sealed class DayCell
        {
            public DateTime          Date;
            public SchedulerResource Resource;
            public HTMLElement       Element;
        }

        private sealed class Segment
        {
            public SchedulerAppointment Appointment;
            public DateTime             Start;
            public DateTime             End;
            public bool                 IsFirst;
            public bool                 IsLast;
            public int                  Lane;
            public int                  Lanes;
        }

        private sealed class Drag
        {
            public DragKind             Kind;
            public Surface              Surface;
            public Item                 Item;
            public SchedulerAppointment Appointment;
            public object               FromResourceId;
            public object               ToResourceId;
            public double               StartX;
            public double               StartY;
            public bool                 Active;
            public DateTime             Anchor;
            public Column               AnchorColumn;
            public Lane                 AnchorLane;
            public DateTime             NewStart;
            public DateTime             NewEnd;
            public bool                 Valid;
        }

        // A drag from another surface of the page (see MakeDraggable). In-process only, as the payload is the live object.
        private static bool     _externalActive;
        private static object   _externalPayload;
        private static string   _externalTitle;
        private static TimeSpan _externalDuration;

        private readonly HTMLElement _toolbar;
        private readonly HTMLElement _title;
        private readonly HTMLElement _views;
        private readonly HTMLElement _commands;
        private readonly HTMLElement _body;
        private readonly Button      _todayButton;
        private readonly Button      _previousButton;
        private readonly Button      _nextButton;

        private readonly Dictionary<SchedulerView, Button>          _viewButtons  = new Dictionary<SchedulerView, Button>();
        private readonly Dictionary<SchedulerView, (double, double)> _scrollMemory = new Dictionary<SchedulerView, (double, double)>();

        private readonly List<Item>    _items    = new List<Item>();
        private readonly List<Column>  _columns  = new List<Column>();
        private readonly List<Lane>    _lanes    = new List<Lane>();
        private readonly List<DayCell> _dayCells = new List<DayCell>();
        private readonly List<object>  _selected = new List<object>();

        private List<SchedulerAppointment> _appointments = new List<SchedulerAppointment>();
        private List<SchedulerResource>    _resources    = new List<SchedulerResource>();

        private SchedulerView     _view              = SchedulerView.Week;
        private SchedulerView[]   _availableViews    = { SchedulerView.Day, SchedulerView.Week, SchedulerView.Month, SchedulerView.Agenda, SchedulerView.Timeline };
        private SchedulerGrouping _grouping          = SchedulerGrouping.ByResource;
        private SchedulerLabels   _labels            = new SchedulerLabels();
        private DateTime          _date              = DateTime.Today;
        private DateTime?         _now;
        private DayOfWeek         _firstDay          = DayOfWeek.Sunday;
        private int               _visibleDays       = 1;
        private int               _agendaDays        = 7;
        private int               _timelineDays      = 7;
        private int               _startHour;
        private int               _endHour           = 24;
        private TimeSpan          _workStart         = TimeSpan.FromHours(8);
        private TimeSpan          _workEnd           = TimeSpan.FromHours(17);
        private bool              _showWorkHours     = true;
        private int               _slotMinutes       = 30;
        private double            _hourHeight        = 48;
        private double            _timelineHourWidth = 24;
        private double            _columnMinWidth    = 90;
        private int               _monthMaxChips     = 3;
        private bool              _readOnly;
        private bool              _singleClickCreates;
        private bool              _twelveHour;
        private bool              _showToolbar       = true;
        private bool              _tooltips          = true;

        private Func<SchedulerAppointment, IComponent> _template;
        private Func<SchedulerAppointment, IComponent> _tooltip;

        private Action<SchedulerMoveRequest>                       _onMove;
        private Action<SchedulerResizeRequest>                     _onResize;
        private Action<SchedulerSlot>                              _onCreate;
        private Action<SchedulerSlot>                              _onSlotClick;
        private Action<SchedulerAppointment>                       _onEdit;
        private Action<SchedulerAppointment, MouseEvent>           _onContextMenu;
        private Action<IReadOnlyList<SchedulerAppointment>>        _onSelectionChanged;
        private Action<IReadOnlyList<SchedulerAppointment>>        _onDelete;
        private Action<SchedulerDropRequest>                       _onExternalDrop;
        private Action<SchedulerView, DateTime>                    _onNavigate;

        private HTMLElement   _scroller;
        private SchedulerView _builtView;
        private bool          _hasBuilt;
        private bool          _dirty = true;
        private bool          _flushQueued;
        private bool          _rendered;
        private bool          _suppressClick;

        private Drag          _drag;
        private HTMLElement   _ghost;
        private Action<Event> _onWindowMove;
        private Action<Event> _onWindowUp;
        private Action<Event> _onWindowKey;

        /// <summary>
        /// Initializes a new instance of the <see cref="Scheduler"/> class.
        /// </summary>
        public Scheduler()
        {
            _todayButton    = Button(_labels.Today).OnClick(() => GoTo(NowValue.Date, raise: true));
            _previousButton = Button().SetIcon(UIcons.AngleSmallLeft).OnClick(() => Step(-1));
            _nextButton     = Button().SetIcon(UIcons.AngleSmallRight).OnClick(() => Step(1));
            _previousButton.Tooltip(_labels.Previous);
            _nextButton.Tooltip(_labels.Next);

            _title    = Div(Att("tss-scheduler-title"));
            _views    = Div(Att("tss-scheduler-views", role: "group"));
            _commands = Div(Att("tss-scheduler-commands"));

            var navigation = Div(Att("tss-scheduler-navigation"), _todayButton.Render(), _previousButton.Render(), _nextButton.Render());

            _toolbar = Div(Att("tss-scheduler-toolbar"), navigation, _title, Div(Att("tss-scheduler-toolbar-spacer")), _commands, _views);
            _body    = Div(Att("tss-scheduler-body"));

            InnerElement          = Div(Att("tss-scheduler", role: "application"), _toolbar, _body);
            InnerElement.tabIndex = 0;
            InnerElement.setAttribute("aria-roledescription", "scheduler");

            _title.setAttribute("aria-live", "polite");

            _body.addEventListener("pointerdown", e => OnPointerDown(e));
            _body.addEventListener("click",       e => OnClick(e));
            _body.addEventListener("dblclick",    e => OnDoubleClick(e));
            _body.addEventListener("contextmenu", e => OnContextMenuEvent(e));
            _body.addEventListener("dragover",    e => OnExternalDragOver(e));
            _body.addEventListener("dragleave",   e => OnExternalDragLeave(e));
            _body.addEventListener("drop",        e => OnExternalDrop(e));
            InnerElement.addEventListener("keydown", e => OnKeyDown(e.As<KeyboardEvent>()));

            BuildViewButtons();
            ApplyCssVariables();
        }

        // ------------------------------------------------------------------ data

        /// <summary>The appointments currently shown.</summary>
        public IReadOnlyList<SchedulerAppointment> CurrentAppointments => _appointments;

        /// <summary>The view currently shown.</summary>
        public SchedulerView CurrentView => _view;

        /// <summary>The date the current range is built around.</summary>
        public DateTime CurrentDate => _date;

        /// <summary>The selected appointments, in the order they were selected.</summary>
        public IReadOnlyList<SchedulerAppointment> SelectedAppointments => _appointments.Where(a => IsSelected(a)).ToList();

        /// <summary>
        /// Replaces the appointments. Hand in the whole list after every change: the scheduler draws what it is given
        /// and keeps the selection by <see cref="SchedulerAppointment.Id"/>.
        /// </summary>
        public Scheduler Appointments(IEnumerable<SchedulerAppointment> appointments)
        {
            _appointments = appointments?.Where(a => a is object).ToList() ?? new List<SchedulerAppointment>();
            return Invalidate();
        }

        /// <summary>Replaces the appointments.</summary>
        public Scheduler Appointments(params SchedulerAppointment[] appointments) => Appointments((IEnumerable<SchedulerAppointment>)appointments);

        /// <summary>
        /// Sets the resources the Day and Week views split their columns by (see <see cref="Grouping"/>) and the
        /// Timeline draws one lane each for. Appointments that name none of them gather in an "Unassigned" column.
        /// </summary>
        public Scheduler Resources(IEnumerable<SchedulerResource> resources)
        {
            _resources = resources?.Where(r => r is object).ToList() ?? new List<SchedulerResource>();
            return Invalidate();
        }

        /// <summary>Sets the resources.</summary>
        public Scheduler Resources(params SchedulerResource[] resources) => Resources((IEnumerable<SchedulerResource>)resources);

        // ------------------------------------------------------------ navigation

        /// <summary>Switches to <paramref name="view"/>.</summary>
        public Scheduler View(SchedulerView view)
        {
            _view = view;
            UpdateViewButtons();
            return Invalidate();
        }

        /// <summary>Sets which views the toolbar offers, in order. Defaults to all five.</summary>
        public Scheduler Views(params SchedulerView[] views)
        {
            _availableViews = views is object && views.Length > 0 ? views : new[] { _view };
            BuildViewButtons();
            return this;
        }

        /// <summary>Moves the range to the one holding <paramref name="date"/>.</summary>
        public Scheduler GoTo(DateTime date) => GoTo(date, raise: false);

        /// <summary>Moves to the previous range, as the toolbar's left arrow does.</summary>
        public Scheduler Previous() => Step(-1, raise: false);

        /// <summary>Moves to the next range, as the toolbar's right arrow does.</summary>
        public Scheduler Next() => Step(1, raise: false);

        /// <summary>Moves to the range holding today.</summary>
        public Scheduler GoToToday() => GoTo(NowValue.Date);

        /// <summary>
        /// Overrides what the scheduler treats as now: where the Today button goes, which day is highlighted and where
        /// the now line is drawn. Defaults to the clock, read on every rebuild.
        /// </summary>
        public Scheduler Now(DateTime now)
        {
            _now = now;
            return Invalidate();
        }

        /// <summary>Sets the day a week starts on, for the Week, Month and Timeline views. Defaults to Sunday.</summary>
        public Scheduler FirstDayOfWeek(DayOfWeek day)
        {
            _firstDay = day;
            return Invalidate();
        }

        /// <summary>Sets how many consecutive days the Day view shows. Defaults to 1.</summary>
        public Scheduler VisibleDays(int days)
        {
            _visibleDays = Math.Max(1, Math.Min(31, days));
            return Invalidate();
        }

        /// <summary>Sets how many days the Agenda lists, from the current date. Defaults to 7.</summary>
        public Scheduler AgendaDays(int days)
        {
            _agendaDays = Math.Max(1, Math.Min(366, days));
            return Invalidate();
        }

        /// <summary>
        /// Sets how many days the Timeline covers. Seven (the default) starts on the first day of the week; any other
        /// count starts on the current date.
        /// </summary>
        public Scheduler TimelineDays(int days)
        {
            _timelineDays = Math.Max(1, Math.Min(62, days));
            return Invalidate();
        }

        // ------------------------------------------------------------------ scale

        /// <summary>
        /// Limits the hours the time grid and the Timeline draw, from <paramref name="startHour"/> up to
        /// <paramref name="endHour"/> (0 to 24). Appointments outside are clipped to the edge. Defaults to the whole day.
        /// </summary>
        public Scheduler VisibleHours(int startHour, int endHour)
        {
            _startHour = Math.Max(0, Math.Min(23, startHour));
            _endHour   = Math.Max(_startHour + 1, Math.Min(24, endHour));
            return Invalidate();
        }

        /// <summary>Sets the working hours, shaded lighter in the time grid and the Timeline. Defaults to 08:00 to 17:00.</summary>
        public Scheduler WorkHours(TimeSpan start, TimeSpan end)
        {
            _workStart     = start;
            _workEnd       = end < start ? start : end;
            _showWorkHours = true;
            return Invalidate();
        }

        /// <summary>Turns the working-hours shading off.</summary>
        public Scheduler NoWorkHours()
        {
            _showWorkHours = false;
            return Invalidate();
        }

        /// <summary>
        /// Sets the length of a slot, in minutes: the time grid's minor lines and the step every drag snaps to (hold
        /// Alt while dragging to snap to the minute). Defaults to 30.
        /// </summary>
        public Scheduler SlotMinutes(int minutes)
        {
            _slotMinutes = Math.Max(5, Math.Min(60, minutes));
            ApplyCssVariables();
            return Invalidate();
        }

        /// <summary>Sets the height of one hour in the Day and Week views, in pixels. Defaults to 48.</summary>
        public Scheduler HourHeight(double pixels)
        {
            _hourHeight = Math.Max(16, pixels);
            ApplyCssVariables();
            return Invalidate();
        }

        /// <summary>Sets the width of one hour in the Timeline, in pixels. Defaults to 24.</summary>
        public Scheduler TimelineHourWidth(double pixels)
        {
            _timelineHourWidth = Math.Max(4, pixels);
            return Invalidate();
        }

        /// <summary>
        /// Sets the narrowest a Day or Week column gets, in pixels. Below it the grid scrolls sideways. Defaults to 90.
        /// </summary>
        public Scheduler ColumnMinWidth(double pixels)
        {
            _columnMinWidth = Math.Max(24, pixels);
            return Invalidate();
        }

        /// <summary>Sets how many appointments a Month cell lists before it shows "+N more". Defaults to 3.</summary>
        public Scheduler MonthMaxAppointments(int count)
        {
            _monthMaxChips = Math.Max(1, count);
            return Invalidate();
        }

        /// <summary>Sets how the Day and Week views split their columns by resource. Defaults to <see cref="SchedulerGrouping.ByResource"/>.</summary>
        public Scheduler Grouping(SchedulerGrouping grouping)
        {
            _grouping = grouping;
            return Invalidate();
        }

        /// <summary>Writes times as 9:30 AM instead of 09:30.</summary>
        public Scheduler TwelveHourClock(bool value = true)
        {
            _twelveHour = value;
            return Invalidate();
        }

        // ------------------------------------------------------------- behaviour

        /// <summary>
        /// Stops every gesture that would change data: move, resize, create, external drop and Delete. Selection, edit
        /// requests, the context menu and tooltips keep working.
        /// </summary>
        public Scheduler ReadOnly(bool value = true)
        {
            _readOnly = value;
            InnerElement.classList.toggle("tss-scheduler-readonly", value);
            return Invalidate();
        }

        /// <summary>Makes a single click on an empty slot request a new appointment, instead of a double-click.</summary>
        public Scheduler SingleClickCreates(bool value = true)
        {
            _singleClickCreates = value;
            return this;
        }

        /// <summary>Hides the toolbar, for an application that drives <see cref="View"/> and <see cref="GoTo(DateTime)"/> itself.</summary>
        public Scheduler HideToolbar(bool hide = true)
        {
            _showToolbar = !hide;
            _toolbar.style.display = hide ? "none" : "";
            return this;
        }

        /// <summary>Adds components to the right of the toolbar, before the view switcher.</summary>
        public Scheduler Commands(params IComponent[] commands)
        {
            ClearChildren(_commands);
            foreach (var command in commands ?? new IComponent[0]) _commands.appendChild(command.Render());
            return this;
        }

        /// <summary>Replaces the text the scheduler draws on its own, for translation.</summary>
        public Scheduler Labels(SchedulerLabels labels)
        {
            _labels = labels ?? new SchedulerLabels();
            _todayButton.SetText(_labels.Today);
            _previousButton.RemoveTooltip().Tooltip(_labels.Previous);
            _nextButton.RemoveTooltip().Tooltip(_labels.Next);
            BuildViewButtons();
            return Invalidate();
        }

        /// <summary>
        /// Replaces what is drawn inside each appointment's block, chip, bar and agenda row. The block keeps its
        /// colour, selection outline and resize edges.
        /// </summary>
        public Scheduler AppointmentTemplate(Func<SchedulerAppointment, IComponent> template)
        {
            _template = template;
            return Invalidate();
        }

        /// <summary>Replaces the tooltip shown over an appointment. Return null for none.</summary>
        public Scheduler AppointmentTooltip(Func<SchedulerAppointment, IComponent> tooltip)
        {
            _tooltip  = tooltip;
            _tooltips = true;
            return Invalidate();
        }

        /// <summary>Turns the appointment tooltips off.</summary>
        public Scheduler NoTooltips()
        {
            _tooltips = false;
            return Invalidate();
        }

        // ---------------------------------------------------------------- events

        /// <summary>Raised when an appointment is dragged to a new time or resource. Nothing has changed yet.</summary>
        public Scheduler OnMove(Action<SchedulerMoveRequest> onMove)
        {
            _onMove = onMove;
            return this;
        }

        /// <summary>Raised when an appointment's start or end edge is dragged. Nothing has changed yet.</summary>
        public Scheduler OnResize(Action<SchedulerResizeRequest> onResize)
        {
            _onResize = onResize;
            return this;
        }

        /// <summary>
        /// Raised when the user asks for a new appointment: by dragging across empty time, by double-clicking an empty
        /// slot (a one-hour slot in the time grid and Timeline, the whole day in the all-day row and Month cells), or
        /// with a single click when <see cref="SingleClickCreates"/> is on.
        /// </summary>
        public Scheduler OnCreate(Action<SchedulerSlot> onCreate)
        {
            _onCreate = onCreate;
            return this;
        }

        /// <summary>Raised on every click on an empty slot, before any create.</summary>
        public Scheduler OnSlotClick(Action<SchedulerSlot> onSlotClick)
        {
            _onSlotClick = onSlotClick;
            return this;
        }

        /// <summary>Raised when an appointment is double-clicked, or Enter is pressed on it. Locked appointments raise nothing.</summary>
        public Scheduler OnEdit(Action<SchedulerAppointment> onEdit)
        {
            _onEdit = onEdit;
            return this;
        }

        /// <summary>
        /// Raised on a right-click on an appointment, with the pointer event to anchor a menu at (for example
        /// <c>ContextMenu().ShowAt(...)</c>). Setting a handler stops the browser's own menu there.
        /// </summary>
        public Scheduler OnContextMenu(Action<SchedulerAppointment, MouseEvent> onContextMenu)
        {
            _onContextMenu = onContextMenu;
            return this;
        }

        /// <summary>Raised whenever the selection changes.</summary>
        public Scheduler OnSelectionChanged(Action<IReadOnlyList<SchedulerAppointment>> onSelectionChanged)
        {
            _onSelectionChanged = onSelectionChanged;
            return this;
        }

        /// <summary>Raised when Delete or Backspace is pressed with appointments selected (locked ones are left out).</summary>
        public Scheduler OnDelete(Action<IReadOnlyList<SchedulerAppointment>> onDelete)
        {
            _onDelete = onDelete;
            return this;
        }

        /// <summary>Raised when something made draggable with <see cref="MakeDraggable"/> is dropped on a slot.</summary>
        public Scheduler OnExternalDrop(Action<SchedulerDropRequest> onExternalDrop)
        {
            _onExternalDrop = onExternalDrop;
            return this;
        }

        /// <summary>Raised when the toolbar changes the view or the date.</summary>
        public Scheduler OnNavigate(Action<SchedulerView, DateTime> onNavigate)
        {
            _onNavigate = onNavigate;
            return this;
        }

        // ------------------------------------------------------------- selection

        /// <summary>Replaces the selection.</summary>
        public Scheduler Select(params SchedulerAppointment[] appointments)
        {
            _selected.Clear();
            foreach (var a in appointments ?? new SchedulerAppointment[0])
            {
                if (a is object && !a.IsLocked) _selected.Add(KeyOf(a));
            }
            ApplySelection();
            RaiseSelectionChanged();
            return this;
        }

        /// <summary>Clears the selection.</summary>
        public Scheduler ClearSelection() => Select();

        /// <summary>Raises <see cref="OnDelete"/> for the selected appointments, as the Delete key does.</summary>
        public Scheduler DeleteSelected()
        {
            if (_readOnly || _onDelete is null) return this;

            var toDelete = SelectedAppointments.Where(a => !a.IsLocked).ToList();
            if (toDelete.Count == 0) return this;

            _selected.Clear();
            ApplySelection();
            RaiseSelectionChanged();
            _onDelete(toDelete);
            return this;
        }

        // ------------------------------------------------------- external source

        /// <summary>
        /// Makes <paramref name="component"/> a drag source the scheduler accepts: dropping it on a time slot, a
        /// Timeline lane or a Month cell raises <see cref="OnExternalDrop"/> with <paramref name="payload"/> and a
        /// slot of <paramref name="duration"/>. A ghost of that length follows the pointer over the scheduler.
        /// </summary>
        public static T MakeDraggable<T>(T component, object payload, string title, TimeSpan duration) where T : IComponent
        {
            var element = component.Render();
            element.draggable = true;
            element.classList.add("tss-scheduler-external");

            element.addEventListener("dragstart", e =>
            {
                _externalActive   = true;
                _externalPayload  = payload;
                _externalTitle    = title ?? "";
                _externalDuration = duration.TotalMinutes > 0 ? duration : TimeSpan.FromHours(1);

                var dt = e.As<DragEvent>().dataTransfer;
                dt.effectAllowed = "copy";
                dt.setData("text/plain", _externalTitle);
            });

            element.addEventListener("dragend", _ =>
            {
                _externalActive  = false;
                _externalPayload = null;

                foreach (var ghost in document.querySelectorAll(".tss-scheduler-ghost-external")) ghost.As<HTMLElement>().remove();
            });

            return component;
        }

        // ------------------------------------------------------------- rendering

        /// <summary>
        /// Renders the component's root HTML element.
        /// </summary>
        public override HTMLElement Render()
        {
            _rendered = true;
            Flush();
            return InnerElement;
        }

        private Scheduler Invalidate()
        {
            _dirty = true;

            if (_flushQueued || !_rendered) return this;

            _flushQueued = true;

            window.requestAnimationFrame(_ =>
            {
                _flushQueued = false;
                Flush();
            });

            return this;
        }

        private void Flush()
        {
            if (!_dirty) return;

            _dirty = false;
            Build();
        }

        private void ApplyCssVariables()
        {
            InnerElement.style.setProperty("--tss-scheduler-hour-height", _hourHeight + "px");
            InnerElement.style.setProperty("--tss-scheduler-slot-height", (_hourHeight * _slotMinutes / 60.0) + "px");
        }

        private DateTime NowValue => _now ?? DateTime.Now;

        private double PixelsPerMinute => _hourHeight / 60.0;

        private Scheduler GoTo(DateTime date, bool raise)
        {
            _date = date.Date;
            Invalidate();
            if (raise) _onNavigate?.Invoke(_view, _date);
            return this;
        }

        private Scheduler Step(int direction, bool raise = true)
        {
            switch (_view)
            {
                case SchedulerView.Day:      _date = _date.AddDays(direction * _visibleDays);  break;
                case SchedulerView.Week:     _date = _date.AddDays(direction * 7);             break;
                case SchedulerView.Month:    _date = new DateTime(_date.Year, _date.Month, 1).AddMonths(direction); break;
                case SchedulerView.Agenda:   _date = _date.AddDays(direction * _agendaDays);   break;
                case SchedulerView.Timeline: _date = _date.AddDays(direction * _timelineDays); break;
            }

            Invalidate();
            if (raise) _onNavigate?.Invoke(_view, _date);
            return this;
        }

        private void BuildViewButtons()
        {
            ClearChildren(_views);
            _viewButtons.Clear();

            foreach (var view in _availableViews)
            {
                var v      = view;
                var button = Button(LabelOf(v)).OnClick(() =>
                {
                    if (_view == v) return;
                    View(v);
                    _onNavigate?.Invoke(_view, _date);
                });

                button.Render().classList.add("tss-scheduler-view-button");
                _viewButtons[v] = button;
                _views.appendChild(button.Render());
            }

            _views.style.display = _availableViews.Length > 1 ? "" : "none";
            UpdateViewButtons();
        }

        private void UpdateViewButtons()
        {
            foreach (var kv in _viewButtons)
            {
                var active = kv.Key == _view;
                kv.Value.IsPrimary = active;
                kv.Value.Render().setAttribute("aria-pressed", active ? "true" : "false");
            }
        }

        private string LabelOf(SchedulerView view)
        {
            switch (view)
            {
                case SchedulerView.Day:      return _labels.Day;
                case SchedulerView.Week:     return _labels.Week;
                case SchedulerView.Month:    return _labels.Month;
                case SchedulerView.Agenda:   return _labels.Agenda;
                default:                     return _labels.Timeline;
            }
        }

        private void Build()
        {
            CancelDrag();

            if (_scroller is object && _hasBuilt) _scrollMemory[_builtView] = (_scroller.scrollTop, _scroller.scrollLeft);

            ClearChildren(_body);
            _items.Clear();
            _columns.Clear();
            _lanes.Clear();
            _dayCells.Clear();
            _scroller = null;

            InnerElement.setAttribute("data-view", _view.ToString().ToLower());

            GetRange(out var from, out var to);
            _title.textContent = TitleOf(from, to);

            switch (_view)
            {
                case SchedulerView.Day:
                case SchedulerView.Week:     BuildTimeGrid(from, to); break;
                case SchedulerView.Month:    BuildMonth(from, to);    break;
                case SchedulerView.Agenda:   BuildAgenda(from, to);   break;
                case SchedulerView.Timeline: BuildTimeline(from, to); break;
            }

            ApplySelection();

            var firstBuildOfView = !_scrollMemory.ContainsKey(_view);
            _builtView = _view;
            _hasBuilt  = true;

            if (_scroller is object) RestoreScroll(_scroller, firstBuildOfView);
        }

        private void RestoreScroll(HTMLElement scroller, bool firstBuild)
        {
            double top  = 0;
            double left = 0;

            if (!firstBuild)
            {
                (top, left) = _scrollMemory[_view];
            }
            else if (_view == SchedulerView.Day || _view == SchedulerView.Week)
            {
                top = Math.Max(0, (_workStart.TotalHours - _startHour) * _hourHeight - 8);
            }
            else if (_view == SchedulerView.Timeline && _workStart.TotalHours - _startHour >= 3)
            {
                left = (_workStart.TotalHours - _startHour - 1) * _timelineHourWidth;
            }

            void Apply()
            {
                scroller.scrollTop  = top;
                scroller.scrollLeft = left;
            }

            if (scroller.isConnected) Apply();
            else DomObserver.WhenMounted(scroller, Apply);
        }

        private void GetRange(out DateTime from, out DateTime to)
        {
            switch (_view)
            {
                case SchedulerView.Day:
                    from = _date;
                    to   = from.AddDays(_visibleDays);
                    return;
                case SchedulerView.Week:
                    from = WeekStart(_date);
                    to   = from.AddDays(7);
                    return;
                case SchedulerView.Month:
                {
                    var first = new DateTime(_date.Year, _date.Month, 1);
                    from = WeekStart(first);
                    var last = first.AddMonths(1);
                    to = WeekStart(last.AddDays(-1)).AddDays(7);
                    return;
                }
                case SchedulerView.Agenda:
                    from = _date;
                    to   = from.AddDays(_agendaDays);
                    return;
                default:
                    from = _timelineDays == 7 ? WeekStart(_date) : _date;
                    to   = from.AddDays(_timelineDays);
                    return;
            }
        }

        private DateTime WeekStart(DateTime date)
        {
            var diff = ((int)date.DayOfWeek - (int)_firstDay + 7) % 7;
            return date.Date.AddDays(-diff);
        }

        // ---------------------------------------------------------------- shared

        private static object KeyOf(SchedulerAppointment a) => a.Id ?? a;

        private bool IsSelected(SchedulerAppointment a)
        {
            var key = KeyOf(a);
            return _selected.Any(k => Equals(k, key));
        }

        private static bool IsBanner(SchedulerAppointment a) => a.IsAllDay || (a.End - a.Start).TotalHours >= 24;

        private static DateTime BannerLastDay(SchedulerAppointment a)
        {
            var first = a.Start.Date;
            var last  = a.End.Date;

            if (last > first && a.End.TimeOfDay.TotalMinutes == 0) last = last.AddDays(-1);
            if (last < first) last = first;
            return last;
        }

        private static bool OverlapsDay(SchedulerAppointment a, DateTime day)
        {
            if (IsBanner(a)) return a.Start.Date <= day && BannerLastDay(a) >= day;

            var dayEnd = day.AddDays(1);
            return a.Start < dayEnd && (a.End > day || (a.End == a.Start && a.Start >= day));
        }

        private List<SchedulerResource> EffectiveResources()
        {
            var list = new List<SchedulerResource>(_resources);

            if (_resources.Count > 0 && _appointments.Any(a => IsUnassigned(a)))
            {
                list.Add(new SchedulerResource(null, _labels.Unassigned));
            }

            return list;
        }

        private bool IsUnassigned(SchedulerAppointment a)
        {
            var ids = a.ResourceIds;
            if (ids is null || ids.Count == 0) return true;
            return !_resources.Any(r => ids.Any(id => Equals(id, r.Id)));
        }

        private bool BelongsTo(SchedulerAppointment a, SchedulerResource resource)
        {
            if (resource is null) return true;
            if (resource.Id is null && !_resources.Contains(resource)) return IsUnassigned(a);
            return a.ResourceIds is object && a.ResourceIds.Any(id => Equals(id, resource.Id));
        }

        private string ColorOf(SchedulerAppointment a, SchedulerResource resource = null)
        {
            if (!string.IsNullOrEmpty(a.Color)) return a.Color;
            if (resource is object && !string.IsNullOrEmpty(resource.Color)) return resource.Color;

            if (a.ResourceIds is object)
            {
                foreach (var id in a.ResourceIds)
                {
                    var r = _resources.FirstOrDefault(x => Equals(x.Id, id));
                    if (r is object && !string.IsNullOrEmpty(r.Color)) return r.Color;
                }
            }

            return null;
        }

        private static string Pad(int value) => value < 10 ? "0" + value : value.ToString();

        private string FormatTime(DateTime t)
        {
            if (!_twelveHour) return Pad(t.Hour) + ":" + Pad(t.Minute);

            var h      = t.Hour % 12 == 0 ? 12 : t.Hour % 12;
            var suffix = t.Hour < 12 ? " AM" : " PM";
            return t.Minute == 0 ? h + suffix : h + ":" + Pad(t.Minute) + suffix;
        }

        private string FormatHour(int hour)
        {
            if (!_twelveHour) return Pad(hour % 24) + ":00";
            var h = hour % 12 == 0 ? 12 : hour % 12;
            return h + ((hour % 24) < 12 ? " AM" : " PM");
        }

        private string FormatRange(SchedulerAppointment a)
        {
            if (a.IsAllDay) return _labels.AllDay;

            if (a.Start.Date == a.End.Date || (a.End.TimeOfDay.TotalMinutes == 0 && a.End.Date == a.Start.Date.AddDays(1)))
            {
                return FormatTime(a.Start) + " – " + FormatTime(a.End);
            }

            return ShortDate(a.Start) + " " + FormatTime(a.Start) + " – " + ShortDate(a.End) + " " + FormatTime(a.End);
        }

        private string ShortDate(DateTime d) => _labels.ShortMonthNames[d.Month - 1] + " " + d.Day;

        private string LongDate(DateTime d) => _labels.DayNames[(int)d.DayOfWeek] + ", " + _labels.MonthNames[d.Month - 1] + " " + d.Day;

        private string TitleOf(DateTime from, DateTime to)
        {
            var last = to.AddDays(-1);

            if (_view == SchedulerView.Month) return _labels.MonthNames[_date.Month - 1] + " " + _date.Year;
            if (from == last) return LongDate(from) + ", " + from.Year;

            if (from.Year != last.Year) return ShortDate(from) + ", " + from.Year + " – " + ShortDate(last) + ", " + last.Year;
            if (from.Month != last.Month) return ShortDate(from) + " – " + ShortDate(last) + ", " + last.Year;
            return ShortDate(from) + " – " + last.Day + ", " + last.Year;
        }

        private string AccessibleNameOf(SchedulerAppointment a)
        {
            var name = (a.Title ?? "") + ", " + ShortDate(a.Start) + ", " + FormatRange(a);
            if (a.IsLocked) name += ", locked";
            if (a.IsRecurring) name += ", recurring";
            return name;
        }

        private Item AddItem(SchedulerAppointment a, HTMLElement element, object resourceId, Surface surface, SchedulerResource resource = null)
        {
            var item = new Item { Appointment = a, Element = element, ResourceId = resourceId, Surface = surface };
            element[ItemProperty] = _items.Count;
            _items.Add(item);

            element.classList.add("tss-scheduler-item");
            element.setAttribute("role", "button");
            element.setAttribute("aria-label", AccessibleNameOf(a));
            element.tabIndex = 0;

            var color = ColorOf(a, resource);
            if (color is object) element.style.setProperty("--tss-scheduler-item-color", color);

            if (a.IsLocked)    element.classList.add("tss-scheduler-locked");
            if (a.IsAllDay)    element.classList.add("tss-scheduler-allday-item");

            if (_tooltips)
            {
                var appointment = a;
                Raw(element).Tooltip(new LazyComponent(() => _tooltip is object ? _tooltip(appointment) : DefaultTooltip(appointment)), delayShow: 500);
            }

            return item;
        }

        private IComponent DefaultTooltip(SchedulerAppointment a)
        {
            var root = Div(Att("tss-scheduler-tooltip"));
            root.appendChild(Div(Att("tss-scheduler-tooltip-title", text: a.Title)));
            root.appendChild(Div(Att("tss-scheduler-tooltip-time", text: LongDate(a.Start) + " · " + FormatRange(a))));

            var names = (a.ResourceIds ?? new List<object>()).Select(id => _resources.FirstOrDefault(r => Equals(r.Id, id))).Where(r => r is object).Select(r => r.Name).ToArray();
            if (names.Length > 0) root.appendChild(Div(Att("tss-scheduler-tooltip-resources", text: string.Join(", ", names))));
            if (!string.IsNullOrEmpty(a.Description)) root.appendChild(Div(Att("tss-scheduler-tooltip-description", text: a.Description)));

            return Raw(root);
        }

        private HTMLElement ItemContent(SchedulerAppointment a, bool showTime)
        {
            if (_template is object)
            {
                var custom = _template(a);
                if (custom is object) return Div(Att("tss-scheduler-item-content"), custom.Render());
            }

            var content = Div(Att("tss-scheduler-item-content"));
            var title   = Div(Att("tss-scheduler-item-title"));

            if (a.IsLocked)    title.appendChild(I(UIcons.Lock,    cssClass: "tss-scheduler-item-mark"));
            if (a.IsRecurring) title.appendChild(I(UIcons.Refresh, cssClass: "tss-scheduler-item-mark"));

            title.appendChild(Span(Att(text: a.Title)));
            content.appendChild(title);

            if (showTime) content.appendChild(Div(Att("tss-scheduler-item-time", text: FormatRange(a))));

            return content;
        }

        private void AddResizeHandles(HTMLElement element, SchedulerAppointment a, bool start, bool end, bool horizontal)
        {
            if (_readOnly || a.IsLocked) return;

            if (start) element.appendChild(Div(Att("tss-scheduler-resize tss-scheduler-resize-start" + (horizontal ? " tss-scheduler-resize-h" : ""))));
            if (end)   element.appendChild(Div(Att("tss-scheduler-resize tss-scheduler-resize-end"   + (horizontal ? " tss-scheduler-resize-h" : ""))));
        }

        private HTMLElement ResourceChip(SchedulerResource r)
        {
            var chip = Span(Att("tss-scheduler-resource-chip"));
            if (!string.IsNullOrEmpty(r.Color)) chip.style.setProperty("--tss-scheduler-item-color", r.Color);
            return chip;
        }

        private bool IsToday(DateTime day) => day.Date == NowValue.Date;

        // ------------------------------------------------------------- time grid

        private void BuildTimeGrid(DateTime from, DateTime to)
        {
            var days = new List<DateTime>();
            for (var d = from; d < to; d = d.AddDays(1)) days.Add(d);

            var resources     = EffectiveResources();
            var useResources  = resources.Count > 0 && _grouping != SchedulerGrouping.None;
            var byDate        = _grouping == SchedulerGrouping.ByDate;

            if (!useResources)
            {
                foreach (var d in days) _columns.Add(new Column { Date = d });
            }
            else if (byDate)
            {
                foreach (var d in days) foreach (var r in resources) _columns.Add(new Column { Date = d, Resource = r });
            }
            else
            {
                foreach (var r in resources) foreach (var d in days) _columns.Add(new Column { Date = d, Resource = r });
            }

            var n = _columns.Count;

            var scroller = Div(Att("tss-scheduler-timegrid"));
            var grid     = Div(Att("tss-scheduler-tg-grid"));
            grid.style.gridTemplateColumns = $"var(--tss-scheduler-gutter-width) repeat({n}, minmax({_columnMinWidth}px, 1fr))";
            scroller.appendChild(grid);

            // --- sticky header: group row, sub row, all-day row, laid out on the grid's own columns (subgrid)
            var header = Div(Att("tss-scheduler-tg-header"));
            grid.appendChild(header);

            var showSubRow  = useResources && !(!byDate && days.Count == 1);
            var headerRows  = showSubRow ? 2 : 1;
            var corner      = Div(Att("tss-scheduler-tg-corner"));
            corner.style.gridColumn = "1";
            corner.style.gridRow    = $"1 / span {headerRows}";
            header.appendChild(corner);

            if (!useResources)
            {
                for (var i = 0; i < n; i++) header.appendChild(DayHeader(_columns[i].Date, i + 2, 1, 1));
            }
            else if (byDate)
            {
                for (var di = 0; di < days.Count; di++) header.appendChild(DayHeader(days[di], 2 + di * resources.Count, resources.Count, 1));
                for (var i = 0; i < n; i++) header.appendChild(ResourceHeader(_columns[i].Resource, i + 2, 1, 2));
            }
            else
            {
                for (var ri = 0; ri < resources.Count; ri++) header.appendChild(ResourceHeader(resources[ri], 2 + ri * days.Count, days.Count, 1));
                if (showSubRow) for (var i = 0; i < n; i++) header.appendChild(DayHeader(_columns[i].Date, i + 2, 1, 2));
            }

            header.appendChild(BuildAllDayRow(headerRows + 1));

            // --- body: hour gutter and one positioned column per day (and resource)
            var hours  = _endHour - _startHour;
            var height = hours * _hourHeight;

            var gutter = Div(Att("tss-scheduler-tg-gutter"));
            gutter.style.height = height + "px";
            for (var h = _startHour; h < _endHour; h++)
            {
                var label = Div(Att("tss-scheduler-tg-hour", text: h == _startHour ? "" : FormatHour(h)));
                label.style.top = ((h - _startHour) * _hourHeight) + "px";
                gutter.appendChild(label);
            }
            grid.appendChild(gutter);

            var now = NowValue;

            for (var i = 0; i < n; i++)
            {
                var col  = _columns[i];
                var body = Div(Att("tss-scheduler-tg-col"));
                body.style.height = height + "px";
                body[ColumnProperty] = i;
                col.Body = body;

                if (IsToday(col.Date)) body.classList.add("tss-scheduler-today");
                if (i > 0 && useResources && (byDate ? _columns[i - 1].Date != col.Date : _columns[i - 1].Resource != col.Resource)) body.classList.add("tss-scheduler-group-start");

                if (_showWorkHours)
                {
                    var top    = Math.Max(0, (_workStart.TotalHours - _startHour) * _hourHeight);
                    var bottom = Math.Min(height, (_workEnd.TotalHours - _startHour) * _hourHeight);
                    if (bottom > top)
                    {
                        var work = Div(Att("tss-scheduler-tg-work"));
                        work.style.top    = top + "px";
                        work.style.height = (bottom - top) + "px";
                        work.style.backgroundPosition = $"0 {-top}px, 0 {-top}px";
                        body.appendChild(work);
                    }
                }

                foreach (var seg in LayoutColumn(col)) body.appendChild(RenderSegment(seg, col));

                if (now.Date == col.Date && now.Hour >= _startHour && now.Hour < _endHour)
                {
                    // Neighbouring columns of the same day draw one line; only the first of them carries the dot.
                    var continues = i > 0 && _columns[i - 1].Date == col.Date;
                    var line = Div(Att("tss-scheduler-now" + (continues ? " tss-scheduler-now-plain" : "")));
                    line.style.top = ((now - col.Date.AddHours(_startHour)).TotalMinutes * PixelsPerMinute) + "px";
                    body.appendChild(line);
                }

                grid.appendChild(body);
            }

            _scroller = scroller;
            _body.appendChild(scroller);
        }

        private HTMLElement DayHeader(DateTime day, int column, int span, int row)
        {
            var el = Div(Att("tss-scheduler-tg-dayheader"));
            el.style.gridColumn = $"{column} / span {span}";
            el.style.gridRow    = row.ToString();

            var name = Span(Att("tss-scheduler-tg-dayname", text: _labels.ShortDayNames[(int)day.DayOfWeek]));
            var num  = Span(Att("tss-scheduler-tg-daynum",  text: day.Day.ToString()));
            el.appendChild(name);
            el.appendChild(num);
            el.setAttribute("aria-label", LongDate(day));

            if (IsToday(day)) el.classList.add("tss-scheduler-today");

            if (_view == SchedulerView.Week || _visibleDays > 1)
            {
                el.classList.add("tss-scheduler-clickable");
                el.addEventListener("click", _ =>
                {
                    _date = day;
                    View(SchedulerView.Day);
                    _onNavigate?.Invoke(_view, _date);
                });
            }

            return el;
        }

        private HTMLElement ResourceHeader(SchedulerResource resource, int column, int span, int row)
        {
            var el = Div(Att("tss-scheduler-tg-resourceheader"));
            el.style.gridColumn = $"{column} / span {span}";
            el.style.gridRow    = row.ToString();
            el.appendChild(ResourceChip(resource));
            el.appendChild(Span(Att("tss-scheduler-resource-name", text: resource.Name)));
            if (resource.Id is null && !_resources.Contains(resource)) el.classList.add("tss-scheduler-unassigned");
            return el;
        }

        private HTMLElement BuildAllDayRow(int row)
        {
            var allDay = Div(Att("tss-scheduler-tg-allday"));
            allDay.style.gridRow = row.ToString();

            // Which columns each banner appointment covers, merged into runs of neighbouring columns.
            var runs = new List<(SchedulerAppointment a, int start, int end)>();

            foreach (var a in _appointments.Where(IsBanner).OrderBy(x => x.Start).ThenByDescending(x => x.End - x.Start))
            {
                var first = a.Start.Date;
                var last  = BannerLastDay(a);
                var start = -1;

                for (var i = 0; i <= _columns.Count; i++)
                {
                    var covers = i < _columns.Count && _columns[i].Date >= first && _columns[i].Date <= last && BelongsTo(a, _columns[i].Resource);

                    if (covers && start < 0) start = i;
                    if (!covers && start >= 0)
                    {
                        runs.Add((a, start, i - 1));
                        start = -1;
                    }
                }
            }

            var rows = new List<List<(int, int)>>();
            var placed = new List<(SchedulerAppointment a, int start, int end, int row)>();

            foreach (var run in runs)
            {
                var r = 0;
                for (; r < rows.Count; r++)
                {
                    if (!rows[r].Any(x => x.Item1 <= run.end && x.Item2 >= run.start)) break;
                }
                if (r == rows.Count) rows.Add(new List<(int, int)>());
                rows[r].Add((run.start, run.end));
                placed.Add((run.a, run.start, run.end, r));
            }

            var rowCount = Math.Max(1, rows.Count);

            var caption = Div(Att("tss-scheduler-tg-allday-caption", text: _labels.AllDay));
            caption.style.gridColumn = "1";
            caption.style.gridRow    = $"1 / span {rowCount}";
            allDay.appendChild(caption);

            for (var i = 0; i < _columns.Count; i++)
            {
                var cell = Div(Att("tss-scheduler-tg-allday-cell tss-scheduler-daycell"));
                cell.style.gridColumn = (i + 2).ToString();
                cell.style.gridRow    = $"1 / span {rowCount}";
                if (IsToday(_columns[i].Date)) cell.classList.add("tss-scheduler-today");
                cell[DayProperty] = _dayCells.Count;
                _dayCells.Add(new DayCell { Date = _columns[i].Date, Resource = _columns[i].Resource, Element = cell });
                allDay.appendChild(cell);
            }

            foreach (var p in placed)
            {
                var resource = _columns[p.start].Resource;
                var chip     = Div(Att("tss-scheduler-chip tss-scheduler-banner"));
                chip.style.gridColumn = $"{p.start + 2} / {p.end + 3}";
                chip.style.gridRow    = (p.row + 1).ToString();

                if (p.a.Start.Date < _columns[p.start].Date) chip.classList.add("tss-scheduler-continues-before");
                if (BannerLastDay(p.a) > _columns[p.end].Date) chip.classList.add("tss-scheduler-continues-after");

                chip.appendChild(ItemContent(p.a, showTime: false));
                AddItem(p.a, chip, resource?.Id, Surface.Days, resource);
                allDay.appendChild(chip);
            }

            return allDay;
        }

        private List<Segment> LayoutColumn(Column col)
        {
            var windowStart = col.Date.AddHours(_startHour);
            var windowEnd   = col.Date.AddHours(_endHour);
            var minLength   = TimeSpan.FromMinutes(Math.Max(15, 20 / PixelsPerMinute));

            var segments = new List<Segment>();

            foreach (var a in _appointments)
            {
                if (IsBanner(a) || !BelongsTo(a, col.Resource)) continue;
                if (!OverlapsDay(a, col.Date)) continue;

                var s = a.Start < windowStart ? windowStart : a.Start;
                var e = a.End   > windowEnd   ? windowEnd   : a.End;

                if (s >= windowEnd)   s = windowEnd - minLength;
                if (e <= windowStart) e = windowStart + minLength;
                if (e - s < minLength) e = s + minLength;
                if (e > windowEnd) { e = windowEnd; s = e - minLength; }

                segments.Add(new Segment
                {
                    Appointment = a,
                    Start       = s,
                    End         = e,
                    IsFirst     = a.Start >= col.Date,
                    IsLast      = a.End <= col.Date.AddDays(1)
                });
            }

            segments = segments.OrderBy(x => x.Start).ThenByDescending(x => x.End - x.Start).ToList();

            // Overlapping appointments share the column side by side: each cluster of overlaps is split into as many
            // lanes as it needs at its busiest, and an appointment takes the first lane free at its start.
            var cluster    = new List<Segment>();
            var laneEnds   = new List<DateTime>();
            var clusterEnd = DateTime.MinValue;

            void CloseCluster()
            {
                foreach (var c in cluster) c.Lanes = laneEnds.Count;
                cluster.Clear();
                laneEnds.Clear();
            }

            foreach (var seg in segments)
            {
                if (cluster.Count > 0 && seg.Start >= clusterEnd) CloseCluster();

                var lane = laneEnds.FindIndex(end => end <= seg.Start);
                if (lane < 0)
                {
                    lane = laneEnds.Count;
                    laneEnds.Add(seg.End);
                }
                else
                {
                    laneEnds[lane] = seg.End;
                }

                seg.Lane = lane;
                cluster.Add(seg);
                if (seg.End > clusterEnd || cluster.Count == 1) clusterEnd = seg.End > clusterEnd ? seg.End : clusterEnd;
            }

            CloseCluster();
            return segments;
        }

        private HTMLElement RenderSegment(Segment seg, Column col)
        {
            var el     = Div(Att("tss-scheduler-event"));
            var top    = (seg.Start - col.Date.AddHours(_startHour)).TotalMinutes * PixelsPerMinute;
            var height = (seg.End - seg.Start).TotalMinutes * PixelsPerMinute;
            var width  = 100.0 / Math.Max(1, seg.Lanes);

            el.style.top    = top + "px";
            el.style.height = height + "px";
            el.style.left   = $"calc({seg.Lane * width}% + 1px)";
            el.style.width  = $"calc({width}% - 3px)";

            if (!seg.IsFirst) el.classList.add("tss-scheduler-continues-before");
            if (!seg.IsLast)  el.classList.add("tss-scheduler-continues-after");
            if (height < 34)  el.classList.add("tss-scheduler-event-short");

            el.appendChild(ItemContent(seg.Appointment, showTime: true));
            AddResizeHandles(el, seg.Appointment, seg.IsFirst, seg.IsLast, horizontal: false);
            AddItem(seg.Appointment, el, col.Resource?.Id, Surface.TimeGrid, col.Resource);
            return el;
        }

        // ----------------------------------------------------------------- month

        private void BuildMonth(DateTime from, DateTime to)
        {
            var root = Div(Att("tss-scheduler-month"));

            var head = Div(Att("tss-scheduler-month-head"));
            for (var i = 0; i < 7; i++)
            {
                var dow = (DayOfWeek)(((int)_firstDay + i) % 7);
                head.appendChild(Div(Att("tss-scheduler-month-weekday", text: _labels.ShortDayNames[(int)dow])));
            }
            root.appendChild(head);

            var weeks = (int)Math.Round((to - from).TotalDays / 7);
            var grid  = Div(Att("tss-scheduler-month-grid"));
            grid.style.gridTemplateRows = $"repeat({weeks}, minmax(var(--tss-scheduler-month-row), 1fr))";

            for (var d = from; d < to; d = d.AddDays(1))
            {
                var day  = d;
                var cell = Div(Att("tss-scheduler-month-cell tss-scheduler-daycell"));
                cell[DayProperty] = _dayCells.Count;
                _dayCells.Add(new DayCell { Date = day, Element = cell });

                if (day.Month != _date.Month) cell.classList.add("tss-scheduler-other-month");
                if (IsToday(day))             cell.classList.add("tss-scheduler-today");

                var number = Button(Att("tss-scheduler-month-date", text: day.Day.ToString(), type: "button"));
                number.setAttribute("aria-label", LongDate(day));
                number.addEventListener("click", e =>
                {
                    e.stopPropagation();
                    _date = day;
                    View(SchedulerView.Day);
                    _onNavigate?.Invoke(_view, _date);
                });
                cell.appendChild(number);

                var list = Div(Att("tss-scheduler-month-items"));
                var dayItems = _appointments.Where(a => OverlapsDay(a, day))
                                            .OrderByDescending(a => IsBanner(a))
                                            .ThenBy(a => a.Start)
                                            .ThenBy(a => a.Title)
                                            .ToList();

                var shown = dayItems.Count > _monthMaxChips ? _monthMaxChips - 1 : dayItems.Count;
                if (shown < 1) shown = Math.Min(1, dayItems.Count);

                for (var i = 0; i < shown; i++)
                {
                    var a    = dayItems[i];
                    var chip = Div(Att("tss-scheduler-chip" + (IsBanner(a) ? " tss-scheduler-banner" : "")));

                    if (IsBanner(a))
                    {
                        if (a.Start.Date < day)        chip.classList.add("tss-scheduler-continues-before");
                        if (BannerLastDay(a) > day)    chip.classList.add("tss-scheduler-continues-after");
                    }
                    else if (_template is null)
                    {
                        chip.appendChild(Span(Att("tss-scheduler-chip-dot")));
                        chip.appendChild(Span(Att("tss-scheduler-chip-time", text: FormatTime(a.Start))));
                    }

                    chip.appendChild(ItemContent(a, showTime: false));
                    AddItem(a, chip, a.ResourceIds?.FirstOrDefault(), Surface.Days);
                    list.appendChild(chip);
                }

                if (dayItems.Count > shown)
                {
                    var more = Button(Att("tss-scheduler-month-more", text: string.Format(_labels.More, dayItems.Count - shown), type: "button"));
                    more.addEventListener("click", e =>
                    {
                        e.stopPropagation();
                        _date = day;
                        View(SchedulerView.Day);
                        _onNavigate?.Invoke(_view, _date);
                    });
                    list.appendChild(more);
                }

                cell.appendChild(list);
                grid.appendChild(cell);
            }

            root.appendChild(grid);
            _scroller = root;
            _body.appendChild(root);
        }

        // ---------------------------------------------------------------- agenda

        private void BuildAgenda(DateTime from, DateTime to)
        {
            var root  = Div(Att("tss-scheduler-agenda", role: "list"));
            var count = 0;

            for (var d = from; d < to; d = d.AddDays(1))
            {
                var day      = d;
                var dayItems = _appointments.Where(a => OverlapsDay(a, day)).OrderByDescending(a => IsBanner(a)).ThenBy(a => a.Start).ThenBy(a => a.Title).ToList();
                if (dayItems.Count == 0) continue;

                var group = Div(Att("tss-scheduler-agenda-day"));
                var head  = Div(Att("tss-scheduler-agenda-date"));
                head.appendChild(Span(Att("tss-scheduler-agenda-daynum", text: day.Day.ToString())));
                head.appendChild(Span(Att("tss-scheduler-agenda-dayname", text: LongDate(day))));
                if (IsToday(day)) group.classList.add("tss-scheduler-today");
                group.appendChild(head);

                var rows = Div(Att("tss-scheduler-agenda-rows"));

                foreach (var a in dayItems)
                {
                    var row = Div(Att("tss-scheduler-agenda-row"));
                    row.setAttribute("role", "listitem");

                    // A timed appointment that runs over midnight is listed on each of its days, with the part that
                    // falls on that day: from its start, until its end, or the whole day in between.
                    var startsToday = a.Start.Date == day;
                    var endsToday   = a.End <= day.AddDays(1);
                    var time = IsBanner(a) || a.IsAllDay ? _labels.AllDay
                             : startsToday && endsToday ? FormatTime(a.Start) + " – " + FormatTime(a.End)
                             : startsToday ? FormatTime(a.Start) + " →"
                             : endsToday ? "→ " + FormatTime(a.End)
                             : _labels.AllDay;

                    row.appendChild(Div(Att("tss-scheduler-agenda-time", text: time)));
                    row.appendChild(Div(Att("tss-scheduler-agenda-bar")));

                    var main = Div(Att("tss-scheduler-agenda-main"));
                    main.appendChild(ItemContent(a, showTime: false));
                    if (_template is null && !string.IsNullOrEmpty(a.Description)) main.appendChild(Div(Att("tss-scheduler-agenda-description", text: a.Description)));
                    row.appendChild(main);

                    var names = (a.ResourceIds ?? new List<object>()).Select(id => _resources.FirstOrDefault(r => Equals(r.Id, id))).Where(r => r is object).ToList();
                    if (names.Count > 0)
                    {
                        var res = Div(Att("tss-scheduler-agenda-resources"));
                        foreach (var r in names)
                        {
                            var tag = Span(Att("tss-scheduler-agenda-resource"));
                            tag.appendChild(ResourceChip(r));
                            tag.appendChild(Span(Att(text: r.Name)));
                            res.appendChild(tag);
                        }
                        row.appendChild(res);
                    }

                    AddItem(a, row, a.ResourceIds?.FirstOrDefault(), Surface.Agenda);
                    rows.appendChild(row);
                    count++;
                }

                group.appendChild(rows);
                root.appendChild(group);
            }

            if (count == 0) root.appendChild(Div(Att("tss-scheduler-agenda-empty", text: _labels.NoAppointments)));

            _scroller = root;
            _body.appendChild(root);
        }

        // -------------------------------------------------------------- timeline

        private double TimelineDayWidth => (_endHour - _startHour) * _timelineHourWidth;

        private double TimelineX(DateTime from, DateTime t)
        {
            var dayIndex = Math.Floor((t.Date - from).TotalDays);
            var minutes  = (t - t.Date).TotalMinutes - _startHour * 60;
            minutes = Math.Max(0, Math.Min((_endHour - _startHour) * 60, minutes));
            return dayIndex * TimelineDayWidth + minutes * _timelineHourWidth / 60.0;
        }

        private DateTime TimelineTime(DateTime from, double x, int days)
        {
            var dayWidth = TimelineDayWidth;
            var index    = (int)Math.Floor(x / dayWidth);
            index = Math.Max(0, Math.Min(days - 1, index));
            var within = Math.Max(0, Math.Min(dayWidth, x - index * dayWidth));
            return from.AddDays(index).AddHours(_startHour).AddMinutes(within * 60.0 / _timelineHourWidth);
        }

        private void BuildTimeline(DateTime from, DateTime to)
        {
            var days      = (int)Math.Round((to - from).TotalDays);
            var dayWidth  = TimelineDayWidth;
            var width     = days * dayWidth;
            var resources = EffectiveResources();
            if (resources.Count == 0) resources.Add(new SchedulerResource(null, ""));

            var scroller = Div(Att("tss-scheduler-timeline"));
            var grid     = Div(Att("tss-scheduler-tl-grid"));
            grid.style.gridTemplateColumns = $"var(--tss-scheduler-lane-header-width) {width}px";
            grid.style.setProperty("--tss-scheduler-tl-hour-width", _timelineHourWidth + "px");
            grid.style.setProperty("--tss-scheduler-tl-day-width",  dayWidth + "px");
            scroller.appendChild(grid);

            var corner = Div(Att("tss-scheduler-tl-corner", text: _resources.Count > 0 ? _labels.Resources : ""));
            grid.appendChild(corner);

            var header = Div(Att("tss-scheduler-tl-header"));
            header.style.width = width + "px";

            var dayRow  = Div(Att("tss-scheduler-tl-days"));
            var hourRow = Div(Att("tss-scheduler-tl-hours"));
            var step    = Math.Max(1, (int)Math.Ceiling(40 / _timelineHourWidth));
            if (step > 2 && step < 6) step = step == 5 ? 6 : step;
            else if (step >= 6) step = step <= 12 ? 12 : 24;

            for (var i = 0; i < days; i++)
            {
                var day = from.AddDays(i);
                var dh  = Div(Att("tss-scheduler-tl-day"));
                dh.style.left  = (i * dayWidth) + "px";
                dh.style.width = dayWidth + "px";
                var dayLabel = Span(Att("tss-scheduler-tl-daylabel"));
                dayLabel.appendChild(Span(Att("tss-scheduler-tg-dayname", text: _labels.ShortDayNames[(int)day.DayOfWeek])));
                dayLabel.appendChild(Span(Att("tss-scheduler-tg-daynum",  text: day.Day.ToString())));
                dh.appendChild(dayLabel);
                if (IsToday(day)) dh.classList.add("tss-scheduler-today");
                dayRow.appendChild(dh);

                for (var h = _startHour; h < _endHour; h += step)
                {
                    var label = Div(Att("tss-scheduler-tl-hour", text: FormatHour(h)));
                    label.style.left  = (i * dayWidth + (h - _startHour) * _timelineHourWidth) + "px";
                    label.style.width = (step * _timelineHourWidth) + "px";
                    hourRow.appendChild(label);
                }
            }

            header.appendChild(dayRow);
            header.appendChild(hourRow);
            grid.appendChild(header);

            var now = NowValue;

            foreach (var resource in resources)
            {
                var laneHeader = Div(Att("tss-scheduler-tl-laneheader"));
                if (!string.IsNullOrEmpty(resource.Name) || resource.Id is object)
                {
                    laneHeader.appendChild(ResourceChip(resource));
                    laneHeader.appendChild(Span(Att("tss-scheduler-resource-name", text: resource.Name)));
                }
                if (resource.Id is null && !_resources.Contains(resource)) laneHeader.classList.add("tss-scheduler-unassigned");
                grid.appendChild(laneHeader);

                var lane = Div(Att("tss-scheduler-tl-lane"));
                lane.style.width = width + "px";
                lane[LaneProperty] = _lanes.Count;
                var laneModel = new Lane { Resource = _resources.Contains(resource) || resource.Id is object || _resources.Count > 0 ? resource : null, Body = lane };
                _lanes.Add(laneModel);

                if (_showWorkHours)
                {
                    for (var i = 0; i < days; i++)
                    {
                        var s = Math.Max(0, (_workStart.TotalHours - _startHour) * _timelineHourWidth);
                        var e = Math.Min(dayWidth, (_workEnd.TotalHours - _startHour) * _timelineHourWidth);
                        if (e <= s) continue;
                        var work = Div(Att("tss-scheduler-tl-work"));
                        work.style.left  = (i * dayWidth + s) + "px";
                        work.style.width = (e - s) + "px";
                        work.style.backgroundPosition = $"{-s}px 0";
                        lane.appendChild(work);
                    }
                }

                // Pack the lane's bars into as few rows as they need: a bar takes the first row free at its start.
                var bars = _appointments.Where(a => BelongsTo(a, laneModel.Resource) && a.Start < to && (a.End > from || (a.End == a.Start && a.Start >= from)))
                                        .OrderBy(a => a.Start).ThenByDescending(a => a.End - a.Start).ToList();

                var rowEnds = new List<double>();

                foreach (var a in bars)
                {
                    var bs = a.IsAllDay ? a.Start.Date : a.Start;
                    var be = a.IsAllDay ? BannerLastDay(a).AddDays(1) : a.End;

                    var x1 = a.Start < from ? 0 : TimelineX(from, bs);
                    var x2 = be >= to ? width : (be.TimeOfDay.TotalMinutes == 0 && be > bs ? TimelineX(from, be.AddMinutes(-1)) + _timelineHourWidth / 60.0 : TimelineX(from, be));
                    if (a.IsAllDay) x2 = Math.Min(width, ((be - from).TotalDays) * dayWidth);
                    if (x2 - x1 < 6) x2 = x1 + 6;

                    var row = rowEnds.FindIndex(end => end <= x1);
                    if (row < 0) { row = rowEnds.Count; rowEnds.Add(x2); }
                    else rowEnds[row] = x2;

                    var bar = Div(Att("tss-scheduler-bar"));
                    bar.style.left  = x1 + "px";
                    bar.style.width = (x2 - x1 - 2) + "px";
                    bar.style.top   = $"calc(var(--tss-scheduler-tl-pad) + {row} * var(--tss-scheduler-tl-row))";

                    if (a.Start < from) bar.classList.add("tss-scheduler-continues-before");
                    if (be > to)        bar.classList.add("tss-scheduler-continues-after");

                    bar.appendChild(ItemContent(a, showTime: false));
                    AddResizeHandles(bar, a, a.Start >= from && !a.IsAllDay, be <= to && !a.IsAllDay, horizontal: true);
                    AddItem(a, bar, laneModel.Resource?.Id, Surface.Timeline, laneModel.Resource);
                    lane.appendChild(bar);
                }

                lane.style.setProperty("--tss-scheduler-tl-rows", Math.Max(1, rowEnds.Count).ToString());

                if (now >= from && now < to && now.Hour >= _startHour && now.Hour < _endHour)
                {
                    var line = Div(Att("tss-scheduler-now tss-scheduler-now-v" + (_lanes.Count == 1 ? "" : " tss-scheduler-now-plain")));
                    line.style.left = TimelineX(from, now) + "px";
                    lane.appendChild(line);
                }

                grid.appendChild(lane);
            }

            _scroller = scroller;
            _body.appendChild(scroller);
        }

        // ------------------------------------------------------------- selection

        private void ApplySelection()
        {
            foreach (var item in _items)
            {
                var selected = IsSelected(item.Appointment);
                item.Element.classList.toggle("tss-scheduler-selected", selected);
                item.Element.setAttribute("aria-pressed", selected ? "true" : "false");
            }
        }

        private void RaiseSelectionChanged() => _onSelectionChanged?.Invoke(SelectedAppointments);

        private void SelectFromClick(SchedulerAppointment a, bool toggle)
        {
            if (a.IsLocked) return;

            var key = KeyOf(a);

            if (toggle)
            {
                var index = _selected.FindIndex(k => Equals(k, key));
                if (index >= 0) _selected.RemoveAt(index);
                else _selected.Add(key);
            }
            else
            {
                if (_selected.Count == 1 && Equals(_selected[0], key)) return;
                _selected.Clear();
                _selected.Add(key);
            }

            ApplySelection();
            RaiseSelectionChanged();
        }

        // ------------------------------------------------------------ hit tests

        private Item ItemOf(HTMLElement target)
        {
            var el = target?.closest(".tss-scheduler-item")?.As<HTMLElement>();
            if (el is null || !el.HasOwnProperty(ItemProperty)) return null;
            var index = el[ItemProperty].As<int>();
            return index >= 0 && index < _items.Count && _items[index].Element == el ? _items[index] : null;
        }

        private Column ColumnOf(HTMLElement target)
        {
            var el = target?.closest(".tss-scheduler-tg-col")?.As<HTMLElement>();
            if (el is null || !el.HasOwnProperty(ColumnProperty)) return null;
            var index = el[ColumnProperty].As<int>();
            return index < _columns.Count ? _columns[index] : null;
        }

        private Lane LaneOf(HTMLElement target)
        {
            var el = target?.closest(".tss-scheduler-tl-lane")?.As<HTMLElement>();
            if (el is null || !el.HasOwnProperty(LaneProperty)) return null;
            var index = el[LaneProperty].As<int>();
            return index < _lanes.Count ? _lanes[index] : null;
        }

        private DayCell DayCellOf(HTMLElement target)
        {
            var el = target?.closest(".tss-scheduler-daycell")?.As<HTMLElement>();
            if (el is null || !el.HasOwnProperty(DayProperty)) return null;
            var index = el[DayProperty].As<int>();
            return index < _dayCells.Count ? _dayCells[index] : null;
        }

        private Column ColumnAt(double x)
        {
            if (_columns.Count == 0) return null;

            for (var i = 0; i < _columns.Count; i++)
            {
                var rect = _columns[i].Body.getBoundingClientRect().As<DOMRect>();
                if (x < rect.right || i == _columns.Count - 1) return x < rect.left && i == 0 ? _columns[0] : _columns[i];
            }

            return _columns[_columns.Count - 1];
        }

        private DateTime TimeInColumn(Column col, double y)
        {
            var rect    = col.Body.getBoundingClientRect().As<DOMRect>();
            var minutes = (y - rect.top) / PixelsPerMinute;
            minutes = Math.Max(0, Math.Min((_endHour - _startHour) * 60, minutes));
            return col.Date.AddHours(_startHour).AddMinutes(minutes);
        }

        private Lane LaneAt(double y)
        {
            if (_lanes.Count == 0) return null;

            for (var i = 0; i < _lanes.Count; i++)
            {
                var rect = _lanes[i].Body.getBoundingClientRect().As<DOMRect>();
                if (y < rect.bottom) return _lanes[i];
            }

            return _lanes[_lanes.Count - 1];
        }

        private DateTime TimeInLane(Lane lane, double x)
        {
            GetRange(out var from, out var to);
            var rect = lane.Body.getBoundingClientRect().As<DOMRect>();
            return TimelineTime(from, x - rect.left, (int)Math.Round((to - from).TotalDays));
        }

        private DayCell DayCellAt(double x, double y)
        {
            foreach (var cell in _dayCells)
            {
                var rect = cell.Element.getBoundingClientRect().As<DOMRect>();
                if (x >= rect.left && x < rect.right && y >= rect.top && y < rect.bottom) return cell;
            }
            return null;
        }

        private DateTime Snap(DateTime t, bool fine)
        {
            var slot    = fine ? 1 : _slotMinutes;
            var minutes = (t - t.Date).TotalMinutes;
            return t.Date.AddMinutes(Math.Round(minutes / slot) * slot);
        }

        private DateTime SnapDown(DateTime t)
        {
            var minutes = (t - t.Date).TotalMinutes;
            return t.Date.AddMinutes(Math.Floor(minutes / _slotMinutes) * _slotMinutes);
        }

        // ---------------------------------------------------------------- input

        private void OnPointerDown(Event ev)
        {
            var e = ev.As<MouseEvent>();
            if (e.button != 0 || _drag is object) return;

            var target = e.target.As<HTMLElement>();
            if (target.closest("button, .tss-scheduler-tg-dayheader") is object) return;

            var item = ItemOf(target);
            Drag drag = null;

            if (item is object)
            {
                var a = item.Appointment;
                ev.preventDefault();
                FocusRoot();

                if (_readOnly || a.IsLocked || item.Surface == Surface.Agenda) return;

                drag = new Drag { Item = item, Appointment = a, Surface = item.Surface, FromResourceId = item.ResourceId, ToResourceId = item.ResourceId };

                if      (target.closest(".tss-scheduler-resize-start") is object) drag.Kind = DragKind.ResizeStart;
                else if (target.closest(".tss-scheduler-resize-end")   is object) drag.Kind = DragKind.ResizeEnd;
                else    drag.Kind = item.Surface == Surface.Days ? DragKind.DayMove : DragKind.Move;

                if (item.Surface == Surface.TimeGrid) drag.Anchor = TimeInColumn(ColumnOf(target), e.clientY);
                else if (item.Surface == Surface.Timeline) drag.Anchor = TimeInLane(LaneOf(target), e.clientX);
                else
                {
                    var cell = DayCellAt(e.clientX, e.clientY);
                    if (cell is null) return;
                    drag.Anchor = cell.Date;
                }
            }
            else if (!_readOnly)
            {
                var col = ColumnOf(target);
                if (col is object)
                {
                    drag = new Drag { Kind = DragKind.Create, Surface = Surface.TimeGrid, AnchorColumn = col, Anchor = TimeInColumn(col, e.clientY), ToResourceId = col.Resource?.Id };
                }

                var lane = col is null ? LaneOf(target) : null;
                if (lane is object)
                {
                    drag = new Drag { Kind = DragKind.Create, Surface = Surface.Timeline, AnchorLane = lane, Anchor = TimeInLane(lane, e.clientX), ToResourceId = lane.Resource?.Id };
                }

                if (drag is object && e.pointerType() == "mouse") ev.preventDefault();
            }

            if (drag is null) return;

            drag.StartX = e.clientX;
            drag.StartY = e.clientY;
            _drag = drag;

            _onWindowMove = me => OnWindowPointerMove(me.As<MouseEvent>());
            _onWindowUp   = ue => OnWindowPointerUp(ue.As<MouseEvent>(), cancelled: ue.type == "pointercancel");
            _onWindowKey  = ke =>
            {
                if (ke.As<KeyboardEvent>().key == "Escape")
                {
                    ke.preventDefault();
                    if (_drag is object && _drag.Active) _suppressClick = true;
                    CancelDrag();
                }
            };

            window.addEventListener("pointermove",   _onWindowMove);
            window.addEventListener("pointerup",     _onWindowUp);
            window.addEventListener("pointercancel", _onWindowUp);
            window.addEventListener("keydown",       _onWindowKey, true);
        }

        private void OnWindowPointerMove(MouseEvent e)
        {
            var drag = _drag;
            if (drag is null) return;

            if (!drag.Active)
            {
                var dx = e.clientX - drag.StartX;
                var dy = e.clientY - drag.StartY;
                if (Math.Sqrt(dx * dx + dy * dy) < DragThreshold) return;

                drag.Active = true;
                InnerElement.classList.add("tss-scheduler-dragging");
                if (drag.Item is object) foreach (var it in _items.Where(i => i.Appointment == drag.Appointment)) it.Element.classList.add("tss-scheduler-drag-source");
                HideTooltips();
            }

            e.preventDefault();
            AutoScroll(e);

            var fine = e.altKey;

            switch (drag.Surface)
            {
                case Surface.TimeGrid: UpdateTimeGridDrag(drag, e, fine); break;
                case Surface.Timeline: UpdateTimelineDrag(drag, e, fine); break;
                case Surface.Days:     UpdateDayDrag(drag, e);            break;
            }
        }

        private void UpdateTimeGridDrag(Drag drag, MouseEvent e, bool fine)
        {
            var a    = drag.Appointment;
            var slot = TimeSpan.FromMinutes(fine ? 1 : _slotMinutes);
            Column col;

            switch (drag.Kind)
            {
                case DragKind.Move:
                {
                    col = ColumnAt(e.clientX);
                    var t = TimeInColumn(col, e.clientY);
                    drag.NewStart     = Snap(a.Start + (t - drag.Anchor), fine);
                    drag.NewEnd       = drag.NewStart + (a.End - a.Start);
                    drag.ToResourceId = col.Resource?.Id ?? (col.Resource is object ? null : drag.FromResourceId);
                    break;
                }
                case DragKind.ResizeEnd:
                {
                    col = ColumnAt(e.clientX);
                    var t = Snap(TimeInColumn(col, e.clientY), fine);
                    drag.NewStart = a.Start;
                    drag.NewEnd   = t < a.Start + slot ? a.Start + slot : t;
                    col = ColumnFor(drag.NewEnd.AddMinutes(-1), drag.FromResourceId) ?? col;
                    break;
                }
                case DragKind.ResizeStart:
                {
                    col = ColumnAt(e.clientX);
                    var t = Snap(TimeInColumn(col, e.clientY), fine);
                    drag.NewEnd   = a.End;
                    drag.NewStart = t > a.End - slot ? a.End - slot : t;
                    col = ColumnFor(drag.NewStart, drag.FromResourceId) ?? col;
                    break;
                }
                default:
                {
                    col = drag.AnchorColumn;
                    var t     = TimeInColumn(col, e.clientY);
                    var start = drag.Anchor < t ? drag.Anchor : t;
                    var end   = drag.Anchor < t ? t : drag.Anchor;
                    drag.NewStart = SnapDown(start);
                    drag.NewEnd   = SnapDown(end).AddMinutes(_slotMinutes);
                    break;
                }
            }

            drag.Valid = true;

            var label = drag.Kind == DragKind.Create ? "" : (a?.Title ?? "");
            ShowTimeGridGhost(col, drag.NewStart, drag.NewEnd, label);
        }

        private Column ColumnFor(DateTime t, object resourceId)
        {
            return _columns.FirstOrDefault(c => c.Date == t.Date && (c.Resource is null || Equals(c.Resource.Id, resourceId)));
        }

        private void ShowTimeGridGhost(Column col, DateTime start, DateTime end, string label, bool external = false)
        {
            EnsureGhost(external);

            var windowStart = col.Date.AddHours(_startHour);
            var windowEnd   = col.Date.AddHours(_endHour);
            var s = start < windowStart ? windowStart : start;
            var e = end   > windowEnd   ? windowEnd   : end;
            if (e <= s) e = s.AddMinutes(_slotMinutes);

            _ghost.className = "tss-scheduler-ghost" + (external ? " tss-scheduler-ghost-external" : "");
            _ghost.style.left   = "1px";
            _ghost.style.right  = "3px";
            _ghost.style.width  = "";
            _ghost.style.top    = ((s - windowStart).TotalMinutes * PixelsPerMinute) + "px";
            _ghost.style.height = Math.Max(14, (e - s).TotalMinutes * PixelsPerMinute) + "px";
            _ghost.textContent  = FormatTime(start) + " – " + FormatTime(end) + (string.IsNullOrEmpty(label) ? "" : "  " + label);

            if (_ghost.parentElement != col.Body) col.Body.appendChild(_ghost);
        }

        private void UpdateTimelineDrag(Drag drag, MouseEvent e, bool fine)
        {
            var a    = drag.Appointment;
            var slot = TimeSpan.FromMinutes(fine ? 1 : _slotMinutes);
            Lane lane;

            switch (drag.Kind)
            {
                case DragKind.Move:
                {
                    lane = LaneAt(e.clientY);
                    var t = TimeInLane(lane, e.clientX);
                    drag.NewStart     = Snap(a.Start + (t - drag.Anchor), fine);
                    drag.NewEnd       = drag.NewStart + (a.End - a.Start);
                    drag.ToResourceId = lane.Resource is object ? lane.Resource.Id : drag.FromResourceId;
                    break;
                }
                case DragKind.ResizeEnd:
                {
                    lane = _lanes.FirstOrDefault(l => l.Body.contains(drag.Item.Element)) ?? LaneAt(e.clientY);
                    var t = Snap(TimeInLane(lane, e.clientX), fine);
                    drag.NewStart = a.Start;
                    drag.NewEnd   = t < a.Start + slot ? a.Start + slot : t;
                    break;
                }
                case DragKind.ResizeStart:
                {
                    lane = _lanes.FirstOrDefault(l => l.Body.contains(drag.Item.Element)) ?? LaneAt(e.clientY);
                    var t = Snap(TimeInLane(lane, e.clientX), fine);
                    drag.NewEnd   = a.End;
                    drag.NewStart = t > a.End - slot ? a.End - slot : t;
                    break;
                }
                default:
                {
                    lane = drag.AnchorLane;
                    var t     = TimeInLane(lane, e.clientX);
                    var start = drag.Anchor < t ? drag.Anchor : t;
                    var end   = drag.Anchor < t ? t : drag.Anchor;
                    drag.NewStart = SnapDown(start);
                    drag.NewEnd   = SnapDown(end).AddMinutes(_slotMinutes);
                    break;
                }
            }

            drag.Valid = true;
            ShowTimelineGhost(lane, drag.NewStart, drag.NewEnd, drag.Kind == DragKind.Create ? "" : a?.Title);
        }

        private void ShowTimelineGhost(Lane lane, DateTime start, DateTime end, string label, bool external = false)
        {
            EnsureGhost(external);
            GetRange(out var from, out var to);

            var x1 = TimelineX(from, start < from ? from : start);
            var x2 = end >= to ? TimelineDayWidth * (to - from).TotalDays : TimelineX(from, end);
            if (x2 - x1 < 6) x2 = x1 + 6;

            _ghost.className    = "tss-scheduler-ghost tss-scheduler-ghost-h" + (external ? " tss-scheduler-ghost-external" : "");
            _ghost.style.top    = "";
            _ghost.style.height = "";
            _ghost.style.right  = "";
            _ghost.style.left   = x1 + "px";
            _ghost.style.width  = (x2 - x1) + "px";
            _ghost.textContent  = FormatTime(start) + " – " + FormatTime(end) + (string.IsNullOrEmpty(label) ? "" : "  " + label);

            if (_ghost.parentElement != lane.Body) lane.Body.appendChild(_ghost);
        }

        private void UpdateDayDrag(Drag drag, MouseEvent e)
        {
            var cell = DayCellAt(e.clientX, e.clientY);
            ClearDropTargets();

            if (cell is null)
            {
                drag.Valid = false;
                return;
            }

            var a     = drag.Appointment;
            var delta = (cell.Date - drag.Anchor).TotalDays;
            drag.NewStart     = a.Start.AddDays(delta);
            drag.NewEnd       = a.End.AddDays(delta);
            drag.ToResourceId = cell.Resource is object ? cell.Resource.Id : drag.FromResourceId;
            drag.Valid        = true;

            var first = drag.NewStart.Date;
            var last  = IsBanner(a) ? BannerLastDay(new SchedulerAppointment { Start = drag.NewStart, End = drag.NewEnd, IsAllDay = a.IsAllDay }) : first;

            foreach (var c in _dayCells)
            {
                var sameResource = cell.Resource is null || c.Resource == cell.Resource;
                if (sameResource && c.Date >= first && c.Date <= last) c.Element.classList.add("tss-scheduler-drop-target");
            }
        }

        private void ClearDropTargets()
        {
            foreach (var c in _dayCells) c.Element.classList.remove("tss-scheduler-drop-target");
        }

        private void EnsureGhost(bool external)
        {
            if (_ghost is null) _ghost = Div(Att("tss-scheduler-ghost"));
            _ghost.setAttribute("aria-hidden", "true");
        }

        private void RemoveGhost()
        {
            _ghost?.remove();
            ClearDropTargets();
        }

        private void AutoScroll(MouseEvent e)
        {
            if (_scroller is null) return;

            var rect = _scroller.getBoundingClientRect().As<DOMRect>();
            const double edge = 28;

            if (e.clientY < rect.top + edge + 60 && _drag?.Surface == Surface.TimeGrid) _scroller.scrollTop -= 12;
            else if (e.clientY > rect.bottom - edge) _scroller.scrollTop += 12;

            if (e.clientX < rect.left + edge + 120 && _drag?.Surface == Surface.Timeline) _scroller.scrollLeft -= 16;
            else if (e.clientX > rect.right - edge) _scroller.scrollLeft += 16;
        }

        private void OnWindowPointerUp(MouseEvent e, bool cancelled)
        {
            var drag = _drag;
            CancelDrag();

            if (drag is null || !drag.Active || cancelled || !drag.Valid) return;

            _suppressClick = true;
            window.setTimeout(_ => _suppressClick = false, 0);

            var a = drag.Appointment;

            switch (drag.Kind)
            {
                case DragKind.Move:
                case DragKind.DayMove:
                {
                    if (drag.NewStart == a.Start && Equals(drag.FromResourceId, drag.ToResourceId)) return;
                    _onMove?.Invoke(new SchedulerMoveRequest
                    {
                        Appointment    = a,
                        NewStart       = drag.NewStart,
                        NewEnd         = drag.NewEnd,
                        FromResourceId = drag.FromResourceId,
                        ToResourceId   = drag.ToResourceId,
                        IsCopy         = e.ctrlKey || e.metaKey
                    });
                    break;
                }
                case DragKind.ResizeStart:
                case DragKind.ResizeEnd:
                {
                    if (drag.NewStart == a.Start && drag.NewEnd == a.End) return;
                    _onResize?.Invoke(new SchedulerResizeRequest { Appointment = a, NewStart = drag.NewStart, NewEnd = drag.NewEnd });
                    break;
                }
                case DragKind.Create:
                {
                    _onCreate?.Invoke(new SchedulerSlot { Start = drag.NewStart, End = drag.NewEnd, ResourceId = drag.ToResourceId, Event = e });
                    break;
                }
            }
        }

        private void CancelDrag()
        {
            if (_onWindowMove is object)
            {
                window.removeEventListener("pointermove",   _onWindowMove);
                window.removeEventListener("pointerup",     _onWindowUp);
                window.removeEventListener("pointercancel", _onWindowUp);
                window.removeEventListener("keydown",       _onWindowKey, true);
                _onWindowMove = null;
                _onWindowUp   = null;
                _onWindowKey  = null;
            }

            _drag = null;
            RemoveGhost();
            InnerElement.classList.remove("tss-scheduler-dragging");
            foreach (var it in _items) it.Element.classList.remove("tss-scheduler-drag-source");
        }

        private static void HideTooltips() => Script.Write("try { document.querySelectorAll('[data-tippy-root]').forEach(function(t){ if (t._tippy) t._tippy.hide(); }); } catch (e) { }");

        private void FocusRoot() => Script.Write("try { {0}.focus({ preventScroll: true }); } catch (e) { }", InnerElement);

        private SchedulerSlot SlotAt(MouseEvent e, out bool found)
        {
            found = true;
            var target = e.target.As<HTMLElement>();

            var col = ColumnOf(target);
            if (col is object)
            {
                var t = SnapDown(TimeInColumn(col, e.clientY));
                return new SchedulerSlot { Start = t, End = t.AddMinutes(_slotMinutes), ResourceId = col.Resource?.Id, Event = e };
            }

            var lane = LaneOf(target);
            if (lane is object)
            {
                var t = SnapDown(TimeInLane(lane, e.clientX));
                return new SchedulerSlot { Start = t, End = t.AddMinutes(_slotMinutes), ResourceId = lane.Resource?.Id, Event = e };
            }

            var cell = DayCellOf(target);
            if (cell is object)
            {
                return new SchedulerSlot { Start = cell.Date, End = cell.Date.AddDays(1), ResourceId = cell.Resource?.Id, IsAllDay = true, Event = e };
            }

            found = false;
            return null;
        }

        private void OnClick(Event ev)
        {
            if (_suppressClick)
            {
                _suppressClick = false;
                return;
            }

            var e      = ev.As<MouseEvent>();
            var target = e.target.As<HTMLElement>();
            if (target.closest("button") is object) return;

            var item = ItemOf(target);

            if (item is object)
            {
                SelectFromClick(item.Appointment, e.ctrlKey || e.metaKey);
                return;
            }

            var slot = SlotAt(e, out var found);
            if (!found) return;

            if (_selected.Count > 0)
            {
                _selected.Clear();
                ApplySelection();
                RaiseSelectionChanged();
            }

            if (_readOnly) return;

            _onSlotClick?.Invoke(slot);

            if (_singleClickCreates) _onCreate?.Invoke(CreateSlotFor(slot));
        }

        private SchedulerSlot CreateSlotFor(SchedulerSlot clicked)
        {
            if (clicked.IsAllDay) return clicked;
            return new SchedulerSlot { Start = clicked.Start, End = clicked.Start.AddMinutes(Math.Max(60, _slotMinutes)), ResourceId = clicked.ResourceId, Event = clicked.Event };
        }

        private void OnDoubleClick(Event ev)
        {
            var e      = ev.As<MouseEvent>();
            var target = e.target.As<HTMLElement>();
            if (target.closest("button") is object) return;

            var item = ItemOf(target);

            if (item is object)
            {
                if (item.Appointment.IsLocked) return;
                SelectFromClick(item.Appointment, toggle: false);
                _onEdit?.Invoke(item.Appointment);
                return;
            }

            if (_readOnly || _singleClickCreates) return;

            var slot = SlotAt(e, out var found);
            if (found) _onCreate?.Invoke(CreateSlotFor(slot));
        }

        private void OnContextMenuEvent(Event ev)
        {
            var e    = ev.As<MouseEvent>();
            var item = ItemOf(e.target.As<HTMLElement>());
            if (item is null || _onContextMenu is null) return;

            ev.preventDefault();
            if (!item.Appointment.IsLocked && !IsSelected(item.Appointment)) SelectFromClick(item.Appointment, toggle: false);
            _onContextMenu(item.Appointment, e);
        }

        private void OnKeyDown(KeyboardEvent e)
        {
            var target = e.target.As<HTMLElement>();
            var item   = ItemOf(target);

            switch (e.key)
            {
                case "Delete":
                case "Backspace":
                {
                    if (target.tagName == "INPUT" || target.tagName == "TEXTAREA") return;
                    if (_selected.Count == 0 || _onDelete is null || _readOnly) return;
                    e.preventDefault();
                    DeleteSelected();
                    return;
                }
                case "Enter":
                {
                    if (item is null || item.Appointment.IsLocked) return;
                    e.preventDefault();
                    SelectFromClick(item.Appointment, toggle: false);
                    _onEdit?.Invoke(item.Appointment);
                    return;
                }
                case " ":
                {
                    if (item is null) return;
                    e.preventDefault();
                    SelectFromClick(item.Appointment, e.ctrlKey || e.metaKey);
                    return;
                }
                case "Escape":
                {
                    if (_selected.Count == 0) return;
                    ClearSelection();
                    return;
                }
            }
        }

        // ---------------------------------------------------------- external drop

        private void OnExternalDragOver(Event ev)
        {
            if (!_externalActive || _readOnly || _onExternalDrop is null) return;

            var e    = ev.As<DragEvent>();
            var slot = ExternalSlot(e, out var col, out var lane);
            if (slot is null)
            {
                RemoveGhost();
                return;
            }

            ev.preventDefault();
            e.dataTransfer.dropEffect = "copy";

            if (col is object)       ShowTimeGridGhost(col, slot.Start, slot.End, _externalTitle, external: true);
            else if (lane is object) ShowTimelineGhost(lane, slot.Start, slot.End, _externalTitle, external: true);
            else
            {
                _ghost?.remove();
                ClearDropTargets();
                DayCellOf(e.target.As<HTMLElement>())?.Element.classList.add("tss-scheduler-drop-target");
            }
        }

        private void OnExternalDragLeave(Event ev)
        {
            var related = ev.As<MouseEvent>().relatedTarget.As<Node>();
            if (related is object && _body.contains(related)) return;
            RemoveGhost();
        }

        private void OnExternalDrop(Event ev)
        {
            if (!_externalActive || _readOnly || _onExternalDrop is null) return;

            var slot = ExternalSlot(ev.As<DragEvent>(), out _, out _);
            RemoveGhost();
            if (slot is null) return;

            ev.preventDefault();

            var request = new SchedulerDropRequest
            {
                Payload    = _externalPayload,
                Title      = _externalTitle,
                Start      = slot.Start,
                End        = slot.End,
                ResourceId = slot.ResourceId,
                IsAllDay   = slot.IsAllDay
            };

            _externalActive  = false;
            _externalPayload = null;
            _onExternalDrop(request);
        }

        private SchedulerSlot ExternalSlot(DragEvent e, out Column col, out Lane lane)
        {
            var target = e.target.As<HTMLElement>();
            col  = null;
            lane = null;

            var c = ColumnOf(target);
            if (c is object)
            {
                col = c;
                var t = Snap(TimeInColumn(c, e.clientY - 8), false);
                return new SchedulerSlot { Start = t, End = t + _externalDuration, ResourceId = c.Resource?.Id };
            }

            var l = LaneOf(target);
            if (l is object)
            {
                lane = l;
                var t = Snap(TimeInLane(l, e.clientX), false);
                return new SchedulerSlot { Start = t, End = t + _externalDuration, ResourceId = l.Resource?.Id };
            }

            var cell = DayCellOf(target);
            if (cell is object) return new SchedulerSlot { Start = cell.Date, End = cell.Date.AddDays(1), ResourceId = cell.Resource?.Id, IsAllDay = true };

            return null;
        }

        // A component whose content is only built the first time something renders it: a tooltip per appointment
        // costs nothing until it is hovered.
        private sealed class LazyComponent : IComponent
        {
            private readonly Func<IComponent> _factory;
            private          HTMLElement      _element;

            public LazyComponent(Func<IComponent> factory) => _factory = factory;

            public HTMLElement Render() => _element ?? (_element = _factory()?.Render() ?? Div(Att()));
        }
    }

    internal static class SchedulerPointerExtensions
    {
        public static string pointerType(this MouseEvent e) => Script.Write<string>("({0}.pointerType || 'mouse')", e);
    }
}
