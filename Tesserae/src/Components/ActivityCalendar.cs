using System;
using System.Collections.Generic;
using System.Linq;
using Transpose;
using static Transpose.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// How an <see cref="ActivityCalendar"/> turns a day's value into one of its colour levels.
    /// </summary>
    [Transpose.Name("tss.ActivityCalendarScale")]
    public enum ActivityCalendarScale
    {
        /// <summary>The levels split the range from zero to the largest value into equal steps.</summary>
        Linear,

        /// <summary>Equal steps of the square root: small values are spread out more than on <see cref="Linear"/>.</summary>
        Sqrt,

        /// <summary>
        /// Each level covers whole doublings (powers of two), counted down from the largest value. Reads well when
        /// the values span a few orders of magnitude and a doubling is the change that matters.
        /// </summary>
        Log2,

        /// <summary>
        /// Each level covers whole orders of magnitude (powers of ten), counted down from the largest value, so the
        /// legend reads as round numbers: 1, 10, 100, 1,000.
        /// </summary>
        Log10,

        /// <summary>Each level holds the same number of active days, whatever their values.</summary>
        Quantile
    }

    /// <summary>
    /// Which way an <see cref="ActivityCalendar"/> lays its weeks out.
    /// </summary>
    [Transpose.Name("tss.ActivityCalendarOrientation")]
    public enum ActivityCalendarOrientation
    {
        /// <summary>One column per week and one row per weekday, months labelled along the top. The year view.</summary>
        Horizontal,

        /// <summary>One row per week and one column per weekday, like a wall calendar. The month view.</summary>
        Vertical
    }

    /// <summary>
    /// One day of an <see cref="ActivityCalendar"/>, as handed to the click handler and the tooltip formatter.
    /// </summary>
    [Transpose.Name("tss.ActivityCalendarDay")]
    public sealed class ActivityCalendarDay
    {
        /// <summary>The day, at midnight.</summary>
        public DateTime Date     { get; }

        /// <summary>The day's value, or zero when the data has none.</summary>
        public double   Value    { get; }

        /// <summary>Whether the data had a value for this day at all, which tells "nothing recorded" from "zero".</summary>
        public bool     HasValue { get; }

        /// <summary>The colour level the day is drawn in: 0 for no activity, up to the calendar's level count.</summary>
        public int      Level    { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ActivityCalendarDay"/> class.
        /// </summary>
        public ActivityCalendarDay(DateTime date, double value, bool hasValue, int level)
        {
            Date     = date;
            Value    = value;
            HasValue = hasValue;
            Level    = level;
        }
    }

    /// <summary>
    /// The text an <see cref="ActivityCalendar"/> draws on its own, so an application can hand in its translations.
    /// Every property has an English default; set only the ones that need to change.
    /// </summary>
    [Transpose.Name("tss.ActivityCalendarLabels")]
    public sealed class ActivityCalendarLabels
    {
        /// <summary>The legend's low end.</summary>
        public string   Less          { get; set; } = "Less";

        /// <summary>The legend's high end.</summary>
        public string   More          { get; set; } = "More";

        /// <summary>The tooltip of a day with nothing recorded, and of the legend's first swatch.</summary>
        public string   NoActivity    { get; set; } = "No activity";

        /// <summary>The joining word of the default tooltip: "12 on May 3, 2026".</summary>
        public string   On            { get; set; } = "on";

        /// <summary>The summary's longest-streak caption.</summary>
        public string   LongestStreak { get; set; } = "Longest streak";

        /// <summary>The summary's daily-average caption.</summary>
        public string   AveragePerDay { get; set; } = "Avg / day";

        /// <summary>The summary's weekly-average caption.</summary>
        public string   AveragePerWeek { get; set; } = "Avg / week";

        /// <summary>The summary's total caption.</summary>
        public string   Total         { get; set; } = "Total";

        /// <summary>The unit of a one-day streak.</summary>
        public string   Day           { get; set; } = "day";

        /// <summary>The unit of any other streak.</summary>
        public string   Days          { get; set; } = "days";

        /// <summary>The month names, January first, used for the month labels.</summary>
        public string[] MonthNames    { get; set; } = { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };

        /// <summary>The weekday labels, Sunday first, whatever day the calendar starts its weeks on.</summary>
        public string[] WeekdayNames  { get; set; } = { "S", "M", "T", "W", "T", "F", "S" };
    }

    /// <summary>
    /// A calendar heat map of daily activity, from a single month up to a year and more: one square per day, coloured
    /// by how much happened on it, with month and weekday labels, a "Less … More" legend, an optional summary
    /// (longest streak, averages, total) and a tooltip on every day.
    /// <para>
    /// The tooltip is the calendar's own, not a Tippy instance per day: one element is shared by every calendar on
    /// the page and moved to whichever day is under the pointer, through a single listener on the grid. A year is
    /// 365 squares, and a popover object per square would cost more than the squares do.
    /// </para>
    /// <para>
    /// Changing the data re-colours the existing squares in place; only a change of range or layout rebuilds them.
    /// </para>
    /// </summary>
    [Transpose.Name("tss.ActivityCalendar")]
    public sealed class ActivityCalendar : ComponentBase<ActivityCalendar, HTMLElement>
    {
        private const string DayClass      = "tss-activitycalendar-day";
        private const string CellProperty  = "_tssActivityDay";
        private const string LevelProperty = "_tssActivityLevel";
        private const int    MaxDays       = 3700; // ten years and change: past this a range is a mistake, not a calendar

        private sealed class Cell
        {
            public DateTime    Date;
            public int         Key;
            public HTMLElement Element;
            public double      Value;
            public bool        HasValue;
            public int         Level;
            public bool        Future;
        }

        private enum RangeMode { FromData, LastDays, Fixed }

        private readonly HTMLElement _header;
        private readonly HTMLElement _titleHost;
        private readonly HTMLElement _commandsHost;
        private readonly HTMLElement _summary;
        private readonly HTMLElement _scroll;
        private readonly HTMLElement _grid;
        private readonly HTMLElement _legend;
        private readonly HTMLElement _live;

        private readonly Dictionary<int, double> _values = new Dictionary<int, double>();
        private readonly List<Cell>              _cells  = new List<Cell>();

        private RangeMode _rangeMode = RangeMode.FromData;
        private DateTime  _from;
        private DateTime  _to;
        private int       _lastDays  = 365;
        private DateTime? _today;

        private ActivityCalendarOrientation _orientation  = ActivityCalendarOrientation.Horizontal;
        private DayOfWeek                   _firstDay     = DayOfWeek.Sunday;
        private ActivityCalendarScale       _scale        = ActivityCalendarScale.Linear;
        private ActivityCalendarLabels      _labels       = new ActivityCalendarLabels();
        private int                         _levels       = 4; // what Levels() asked for; LevelCount is what is drawn
        private double[]                    _thresholds;
        private double                      _max          = double.NaN;
        private string[]                    _palette;
        private bool                        _showSummary;
        private bool                        _showLegend   = true;
        private bool?                       _showMonths;
        private bool                        _showWeekdays = true;
        private bool                        _showDayNumbers;
        private DateTime?                   _selected;

        private Func<double, string>                   _formatValue;
        private Func<DateTime, string>                 _formatDate;
        private Func<ActivityCalendarDay, string>      _tooltipText;
        private Func<ActivityCalendarDay, IComponent>  _tooltip;
        private Action<ActivityCalendarDay>            _onDayClick;

        // The lower bound of each level 1..N, computed per render.
        private double[] _bounds = new double[0];

        private bool _structureDirty = true;
        private bool _levelsDirty    = true;
        private bool _flushQueued;
        private bool _rendered;

        private int  _activeIndex    = -1;
        private Cell _hovered;
        private bool _removalWatched;

        /// <summary>
        /// Initializes a new instance of the <see cref="ActivityCalendar"/> class.
        /// </summary>
        public ActivityCalendar()
        {
            _titleHost    = Div(Att("tss-activitycalendar-title"));
            _commandsHost = Div(Att("tss-activitycalendar-commands"));
            _header       = Div(Att("tss-activitycalendar-header"), _titleHost, _commandsHost);
            _summary      = Div(Att("tss-activitycalendar-summary"));
            _grid         = Div(Att("tss-activitycalendar-grid", role: "application"));
            _scroll       = Div(Att("tss-activitycalendar-scroll"), _grid);
            _legend       = Div(Att("tss-activitycalendar-legend"));
            _live         = Div(Att("tss-activitycalendar-live"));

            _header.style.display  = "none";
            _summary.style.display = "none";

            _grid.tabIndex = 0;
            _grid.setAttribute("aria-roledescription", "activity calendar");
            _live.setAttribute("aria-live", "polite");

            InnerElement = Div(Att("tss-activitycalendar"), _header, _summary, _scroll, _legend, _live);

            // mousemove rather than mouseover: a tooltip hidden by a scroll comes back as soon as the pointer moves,
            // even within the day it was on.
            _grid.addEventListener("mousemove",  e => OnGridHover(e));
            _grid.addEventListener("mouseleave", _ => { _hovered = null; HideTooltip(); });
            _grid.addEventListener("click",      e => OnGridClick(e));
            _grid.addEventListener("keydown",    e => OnGridKeyDown(e.As<KeyboardEvent>()));
            _grid.addEventListener("focus",      _ => OnGridFocus());
            _grid.addEventListener("blur",       _ => SetActive(-1));

            _legend.addEventListener("mouseover",  e => OnLegendHover(e));
            _legend.addEventListener("mouseleave", _ => HideTooltip());
        }

        // ------------------------------------------------------------------ data

        /// <summary>
        /// Replaces the data with one value per day. The time of day is ignored, and two entries for the same day
        /// are added together, so raw events can be passed in as they are.
        /// </summary>
        public ActivityCalendar Data(IEnumerable<(DateTime date, double value)> values)
        {
            _values.Clear();

            if (values is object)
            {
                foreach (var (date, value) in values) AddValue(date, value);
            }

            return InvalidateData();
        }

        /// <summary>Replaces the data with <paramref name="values"/>[i] on <paramref name="dates"/>[i].</summary>
        public ActivityCalendar Data(DateTime[] dates, double[] values)
        {
            _values.Clear();

            if (dates is object && values is object)
            {
                var n = Math.Min(dates.Length, values.Length);
                for (int i = 0; i < n; i++) AddValue(dates[i], values[i]);
            }

            return InvalidateData();
        }

        /// <summary>Sets the value of one day, replacing whatever it had.</summary>
        public ActivityCalendar SetValue(DateTime date, double value)
        {
            _values[KeyOf(date)] = value;
            return InvalidateData();
        }

        private void AddValue(DateTime date, double value)
        {
            if (double.IsNaN(value) || double.IsInfinity(value)) return;

            var key = KeyOf(date);
            _values[key] = _values.TryGetValue(key, out var existing) ? existing + value : value;
        }

        private ActivityCalendar InvalidateData()
        {
            // A calendar sized from its data has to rebuild when the data moves its edges.
            if (_rangeMode == RangeMode.FromData) _structureDirty = true;
            _levelsDirty = true;
            QueueFlush();
            return this;
        }

        // ----------------------------------------------------------------- range

        /// <summary>Shows every day from <paramref name="from"/> to <paramref name="to"/>, both included.</summary>
        public ActivityCalendar Range(DateTime from, DateTime to)
        {
            if (to < from) (from, to) = (to, from);

            if ((to.Date - from.Date).TotalDays > MaxDays) throw new ArgumentOutOfRangeException(nameof(to), $"An activity calendar shows at most {MaxDays} days.");

            _rangeMode = RangeMode.Fixed;
            _from      = from.Date;
            _to        = to.Date;
            return InvalidateStructure();
        }

        /// <summary>Shows one calendar year, January 1st to December 31st.</summary>
        public ActivityCalendar Year(int year) => Range(new DateTime(year, 1, 1), new DateTime(year, 12, 31));

        /// <summary>Shows one month. Pair it with <see cref="Vertical"/> for a wall-calendar layout.</summary>
        public ActivityCalendar Month(int year, int month)
        {
            var first = new DateTime(year, month, 1);
            return Range(first, first.AddMonths(1).AddDays(-1));
        }

        /// <summary>Shows the last <paramref name="days"/> days, ending today (see <see cref="Today"/>).</summary>
        public ActivityCalendar LastDays(int days = 365)
        {
            if (days < 1 || days > MaxDays) throw new ArgumentOutOfRangeException(nameof(days), $"An activity calendar shows between 1 and {MaxDays} days.");

            _rangeMode = RangeMode.LastDays;
            _lastDays  = days;
            return InvalidateStructure();
        }

        /// <summary>
        /// Overrides what the calendar treats as today: where <see cref="LastDays"/> ends, and after which days
        /// are drawn as still to come and left out of the summary. Defaults to the clock.
        /// </summary>
        public ActivityCalendar Today(DateTime today)
        {
            _today = today.Date;
            return InvalidateStructure();
        }

        // ---------------------------------------------------------------- layout

        /// <summary>Lays the weeks out as columns, months along the top. This is the default.</summary>
        public ActivityCalendar Horizontal() => Orientation(ActivityCalendarOrientation.Horizontal);

        /// <summary>Lays the weeks out as rows, weekdays along the top, like a wall calendar.</summary>
        public ActivityCalendar Vertical() => Orientation(ActivityCalendarOrientation.Vertical);

        /// <summary>Sets which way the weeks are laid out.</summary>
        public ActivityCalendar Orientation(ActivityCalendarOrientation orientation)
        {
            _orientation = orientation;
            return InvalidateStructure();
        }

        /// <summary>Sets the day a week starts on. Defaults to Sunday.</summary>
        public ActivityCalendar FirstDayOfWeek(DayOfWeek day)
        {
            _firstDay = day;
            return InvalidateStructure();
        }

        /// <summary>
        /// Sets the largest a day's square grows to, in pixels. The squares shrink below it to fit the width they
        /// are given, down to a floor past which the calendar scrolls sideways instead. Defaults to 18.
        /// </summary>
        public ActivityCalendar CellSize(double pixels)
        {
            InnerElement.style.setProperty("--tss-activitycalendar-cell-size", Math.Max(4, pixels) + "px");
            return this;
        }

        /// <summary>Sets the gap between squares, in pixels. Defaults to 3.</summary>
        public ActivityCalendar CellGap(double pixels)
        {
            InnerElement.style.setProperty("--tss-activitycalendar-gap", Math.Max(0, pixels) + "px");
            return this;
        }

        /// <summary>Shows or hides the month labels. By default they are shown unless the range is a single month.</summary>
        public ActivityCalendar MonthLabels(bool show = true)
        {
            _showMonths = show;
            return InvalidateStructure();
        }

        /// <summary>Shows or hides the weekday labels. Shown by default.</summary>
        public ActivityCalendar WeekdayLabels(bool show = true)
        {
            _showWeekdays = show;
            return InvalidateStructure();
        }

        /// <summary>Prints the day of the month inside each square, which wants a larger <see cref="CellSize"/>.</summary>
        public ActivityCalendar ShowDayNumbers(bool show = true)
        {
            _showDayNumbers = show;
            return InvalidateStructure();
        }

        /// <summary>Replaces the text the calendar draws on its own, for translation.</summary>
        public ActivityCalendar Labels(ActivityCalendarLabels labels)
        {
            _labels = labels ?? new ActivityCalendarLabels();
            return InvalidateStructure();
        }

        /// <summary>Marks one day as selected, or clears the mark with <c>null</c>.</summary>
        public ActivityCalendar Selected(DateTime? date)
        {
            _selected = date?.Date;
            return InvalidateData();
        }

        // ------------------------------------------------------------- colouring

        /// <summary>
        /// Sets how a value maps to a colour level. Defaults to <see cref="ActivityCalendarScale.Linear"/>. Hand-set
        /// <see cref="Thresholds"/> take precedence; clear them with <c>Thresholds()</c> to go back to a scale.
        /// </summary>
        public ActivityCalendar Scale(ActivityCalendarScale scale)
        {
            _scale = scale;
            return InvalidateData();
        }

        /// <summary>
        /// Sets how many colour levels an active day can take, besides "no activity". Defaults to 4. Hand-set
        /// <see cref="Thresholds"/> and a <see cref="Palette"/> both bring their own count, which takes precedence.
        /// </summary>
        public ActivityCalendar Levels(int levels)
        {
            _levels = Math.Max(1, Math.Min(10, levels));
            return InvalidateData();
        }

        /// <summary>
        /// Sets the levels by hand: a day takes the last level whose lower bound it reaches, so
        /// <c>Thresholds(1, 10, 100)</c> makes three levels and ignores <see cref="Scale"/> and <see cref="Levels"/>,
        /// whichever order they are called in. Any value above zero takes at least the first level. Call it with
        /// no bounds to go back to the scale.
        /// </summary>
        public ActivityCalendar Thresholds(params double[] lowerBounds)
        {
            if (lowerBounds is null || lowerBounds.Length == 0)
            {
                _thresholds = null;
            }
            else
            {
                _thresholds = lowerBounds.OrderBy(b => b).ToArray();
            }

            return InvalidateData();
        }

        /// <summary>
        /// Pins the value the top level is reached at, for <see cref="ActivityCalendarScale.Linear"/>,
        /// <see cref="ActivityCalendarScale.Sqrt"/> and the log scales — so two calendars side by side colour the
        /// same value the same way. Defaults to the largest value in the range.
        /// </summary>
        public ActivityCalendar Max(double max)
        {
            _max = max > 0 ? max : double.NaN;
            return InvalidateData();
        }

        /// <summary>
        /// Sets the colour the levels are drawn in: the top level is this colour, the lower ones fade it into the
        /// empty square's tone. Defaults to the theme's primary colour.
        /// </summary>
        public ActivityCalendar Color(string color)
        {
            if (string.IsNullOrEmpty(color)) InnerElement.style.removeProperty("--tss-activitycalendar-color");
            else InnerElement.style.setProperty("--tss-activitycalendar-color", color);
            return this;
        }

        /// <summary>
        /// Sets an explicit colour per level, lowest first, instead of fading one colour. Without
        /// <see cref="Thresholds"/> the number of colours is the number of levels; with them, the thresholds decide
        /// and the colours are taken in order, the last one repeated when there are fewer colours than levels.
        /// Either can be set first.
        /// </summary>
        public ActivityCalendar Palette(params string[] colors)
        {
            if (colors is null || colors.Length == 0)
            {
                _palette = null;
            }
            else
            {
                _palette = colors;
            }

            return InvalidateData();
        }

        // ------------------------------------------------------------ decoration

        /// <summary>Shows a title above the calendar, with an optional line of explanation behind an info icon.</summary>
        public ActivityCalendar Title(string title, string info = null)
        {
            ClearChildren(_titleHost);

            if (!string.IsNullOrEmpty(title))
            {
                _titleHost.appendChild(Span(Att("tss-activitycalendar-title-text", text: title)));
                _grid.setAttribute("aria-label", title);
            }

            if (!string.IsNullOrEmpty(info))
            {
                var icon = Icon(UIcons.Info).Render();
                icon.classList.add("tss-activitycalendar-info");
                icon.setAttribute("aria-label", info);
                icon.addEventListener("mouseenter", _ => ShowTooltip(icon, TextNode(info)));
                icon.addEventListener("mouseleave", _ => HideTooltip());
                _titleHost.appendChild(icon);
            }

            UpdateHeaderVisibility();
            return this;
        }

        /// <summary>Puts components at the right end of the title row, such as a picker of what the calendar shows.</summary>
        public ActivityCalendar Commands(params IComponent[] commands)
        {
            ClearChildren(_commandsHost);

            if (commands is object)
            {
                foreach (var c in commands)
                {
                    if (c is object) _commandsHost.appendChild(c.Render());
                }
            }

            UpdateHeaderVisibility();
            return this;
        }

        private void UpdateHeaderVisibility()
        {
            _header.style.display = _titleHost.hasChildNodes() || _commandsHost.hasChildNodes() ? "" : "none";
        }

        /// <summary>Shows or hides the summary row: longest streak, average per day and per week, and the total.</summary>
        public ActivityCalendar Summary(bool show = true)
        {
            _showSummary = show;
            return InvalidateData();
        }

        /// <summary>Shows or hides the "Less … More" legend. Shown by default.</summary>
        public ActivityCalendar Legend(bool show = true)
        {
            _showLegend = show;
            return InvalidateData();
        }

        /// <summary>Sets how values are written in the tooltips, the legend and the summary.</summary>
        public ActivityCalendar FormatValue(Func<double, string> format)
        {
            _formatValue = format;
            return InvalidateData();
        }

        /// <summary>Sets how a day is written in the default tooltip. Defaults to "May 3, 2026".</summary>
        public ActivityCalendar FormatDate(Func<DateTime, string> format)
        {
            _formatDate = format;
            return this;
        }

        /// <summary>Replaces the tooltip's text. Return null or an empty string to show no tooltip for that day.</summary>
        public ActivityCalendar TooltipText(Func<ActivityCalendarDay, string> text)
        {
            _tooltipText = text;
            _tooltip     = null;
            return this;
        }

        /// <summary>
        /// Replaces the tooltip with a component, built when its day is hovered. Return null to show no tooltip for
        /// that day.
        /// </summary>
        public ActivityCalendar Tooltip(Func<ActivityCalendarDay, IComponent> tooltip)
        {
            _tooltip     = tooltip;
            _tooltipText = null;
            return this;
        }

        /// <summary>
        /// Raised when a day is clicked, or chosen with Enter or Space while the calendar has the keyboard focus.
        /// Setting a handler also gives the squares a pointer cursor. Days still to come are not clickable.
        /// </summary>
        public ActivityCalendar OnDayClick(Action<ActivityCalendarDay> onClick)
        {
            _onDayClick = onClick;
            InnerElement.classList.toggle("tss-activitycalendar-clickable", onClick is object);
            return this;
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

        private ActivityCalendar InvalidateStructure()
        {
            _structureDirty = true;
            _levelsDirty    = true;
            QueueFlush();
            return this;
        }

        // A burst of fluent calls on a calendar already rendered costs one rebuild, on the next frame; one that has
        // not been rendered yet builds when it is.
        private void QueueFlush()
        {
            if (_flushQueued || !_rendered) return;

            _flushQueued = true;

            window.requestAnimationFrame(_ =>
            {
                _flushQueued = false;
                Flush();
            });
        }

        private void Flush()
        {
            if (_structureDirty)
            {
                _structureDirty = false;
                BuildCells();

                // A range wider than the room it has scrolls sideways; open it on the most recent days.
                window.requestAnimationFrame(_ => _scroll.scrollLeft = _scroll.scrollWidth);
            }

            if (_levelsDirty)
            {
                _levelsDirty = false;
                ApplyLevels();
                RenderSummary();
                RenderLegend();
            }
        }

        private DateTime TodayValue => _today ?? DateTime.Today;

        private void ResolveRange(out DateTime from, out DateTime to)
        {
            switch (_rangeMode)
            {
                case RangeMode.Fixed:
                {
                    from = _from;
                    to   = _to;
                    return;
                }
                case RangeMode.LastDays:
                {
                    to   = TodayValue;
                    from = to.AddDays(1 - _lastDays);
                    return;
                }
                default:
                {
                    if (_values.Count == 0)
                    {
                        to   = TodayValue;
                        from = to.AddDays(-364);
                        return;
                    }

                    var min = int.MaxValue;
                    var max = int.MinValue;

                    foreach (var key in _values.Keys)
                    {
                        if (key < min) min = key;
                        if (key > max) max = key;
                    }

                    from = DateOfKey(min);
                    to   = DateOfKey(max);

                    if ((to - from).TotalDays > MaxDays) from = to.AddDays(-MaxDays);
                    return;
                }
            }
        }

        private void BuildCells()
        {
            HideTooltip();
            ClearChildren(_grid);
            _cells.Clear();
            _activeIndex = -1;
            _hovered     = null;

            ResolveRange(out var from, out var to);

            var horizontal  = _orientation == ActivityCalendarOrientation.Horizontal;
            var startOffset = ((int)from.DayOfWeek - (int)_firstDay + 7) % 7;
            var today       = TodayValue;

            // Iterating with AddDays on the date keeps every day at midnight across a DST change, which adding
            // 24 hours would not.
            for (var date = from; date <= to; date = date.AddDays(1))
            {
                var index = _cells.Count;
                var pos   = startOffset + index;
                var week  = pos / 7;
                var dow   = pos % 7;

                var el = Div(Att(DayClass));
                el.style.gridColumn = ((horizontal ? week : dow) + 2).ToString();
                el.style.gridRow    = ((horizontal ? dow : week) + 2).ToString();

                if (_showDayNumbers) el.textContent = date.Day.ToString();

                var cell = new Cell { Date = date, Key = KeyOf(date), Element = el, Future = date > today };
                if (cell.Future) el.classList.add("tss-activitycalendar-future");

                el[CellProperty] = index;
                _grid.appendChild(el);
                _cells.Add(cell);
            }

            var weeks = (startOffset + _cells.Count + 6) / 7;

            InnerElement.classList.toggle("tss-activitycalendar-vertical",    !horizontal);
            InnerElement.classList.toggle("tss-activitycalendar-daynumbers",  _showDayNumbers);
            InnerElement.style.setProperty("--tss-activitycalendar-columns", (horizontal ? weeks : 7).ToString());

            var singleMonth = from.Year == to.Year && from.Month == to.Month;
            var showMonths  = _showMonths ?? !singleMonth;

            if (showMonths)   AddMonthLabels(from, to, startOffset, weeks, horizontal);
            if (_showWeekdays) AddWeekdayLabels(horizontal);

            InnerElement.classList.toggle("tss-activitycalendar-no-months",   !showMonths);
            InnerElement.classList.toggle("tss-activitycalendar-no-weekdays", !_showWeekdays);
        }

        private void AddMonthLabels(DateTime from, DateTime to, int startOffset, int weeks, bool horizontal)
        {
            // Each month is labelled at the first week that starts inside it (the range's first month at the first
            // week), and a label too close to the next one is dropped rather than drawn under it.
            var starts = new List<(int week, DateTime month)>();
            var month  = new DateTime(from.Year, from.Month, 1);

            while (month <= to)
            {
                int week;

                if (month <= from)
                {
                    week = 0;
                }
                else
                {
                    var pos = startOffset + (int)Math.Round((month - from).TotalDays);
                    week = horizontal ? (pos + 6) / 7 : pos / 7;
                }

                if (week < weeks)
                {
                    if (starts.Count > 0 && week - starts[starts.Count - 1].week < (horizontal ? 2 : 1)) starts.RemoveAt(starts.Count - 1);
                    starts.Add((week, month));
                }

                month = month.AddMonths(1);
            }

            for (int i = 0; i < starts.Count; i++)
            {
                var (week, m) = starts[i];
                var end       = i + 1 < starts.Count ? starts[i + 1].week : weeks;
                var label     = Div(Att("tss-activitycalendar-month", text: MonthName(m)));

                if (horizontal)
                {
                    label.style.gridRow    = "1";
                    label.style.gridColumn = $"{week + 2} / {end + 2}";
                }
                else
                {
                    label.style.gridColumn = "1";
                    label.style.gridRow    = (week + 2).ToString();
                }

                _grid.appendChild(label);
            }
        }

        private void AddWeekdayLabels(bool horizontal)
        {
            var names = _labels.WeekdayNames ?? new ActivityCalendarLabels().WeekdayNames;

            for (int dow = 0; dow < 7; dow++)
            {
                var dayOfWeek = ((int)_firstDay + dow) % 7;

                // The year view labels Monday, Wednesday and Friday, as a row is only a cell's height and the labels
                // need the gaps between them; the month view has a column's width for every day.
                if (horizontal && dayOfWeek != 1 && dayOfWeek != 3 && dayOfWeek != 5) continue;

                var label     = Div(Att("tss-activitycalendar-weekday", text: dayOfWeek < names.Length ? names[dayOfWeek] : ""));

                if (horizontal)
                {
                    label.style.gridColumn = "1";
                    label.style.gridRow    = (dow + 2).ToString();
                }
                else
                {
                    label.style.gridRow    = "1";
                    label.style.gridColumn = (dow + 2).ToString();
                }

                _grid.appendChild(label);
            }
        }

        private void ApplyLevels()
        {
            foreach (var cell in _cells)
            {
                cell.HasValue = _values.TryGetValue(cell.Key, out var v);
                cell.Value    = cell.HasValue ? v : 0;
            }

            _bounds = ComputeBounds();

            var selectedKey = _selected.HasValue ? KeyOf(_selected.Value) : -1;

            foreach (var cell in _cells)
            {
                cell.Level = LevelOf(cell.Value);
                PaintLevel(cell.Element, cell.Level);
                cell.Element.classList.toggle("tss-activitycalendar-selected", cell.Key == selectedKey);
            }
        }

        private void PaintLevel(HTMLElement el, int level)
        {
            el.setAttribute("data-level", level.ToString());
            el.classList.toggle("tss-activitycalendar-strong", level > 0 && level * 2 > LevelCount);

            if (level == 0)
            {
                el.style.removeProperty("--tss-activitycalendar-mix");
                el.style.background = "";
            }
            else if (_palette is object)
            {
                el.style.background = _palette[Math.Min(level, _palette.Length) - 1];
            }
            else
            {
                // The top level is the colour itself; the lower ones mix it into the empty tone in even steps from a
                // quarter, which keeps the first level visible on both the light and the dark canvas.
                var levels = LevelCount;
                var mix    = levels == 1 ? 100 : 25 + 75 * (level - 1) / (levels - 1);
                el.style.background = "";
                el.style.setProperty("--tss-activitycalendar-mix", mix + "%");
            }
        }

        // Hand-set thresholds decide how many levels there are, then a palette's length, then Levels(). None of the
        // three resets another, so the order they are called in does not matter.
        private int LevelCount => _thresholds?.Length ?? _palette?.Length ?? _levels;

        private double[] ComputeBounds()
        {
            if (_thresholds is object) return _thresholds;

            var n         = LevelCount;
            var positives = new List<double>();

            foreach (var cell in _cells)
            {
                if (cell.Value > 0) positives.Add(cell.Value);
            }

            var bounds = new double[n];
            if (positives.Count == 0 && double.IsNaN(_max)) return bounds;

            var max = double.IsNaN(_max) ? positives.Max() : _max;

            switch (_scale)
            {
                case ActivityCalendarScale.Sqrt:
                {
                    for (int i = 1; i < n; i++) bounds[i] = max * ((double)i / n) * ((double)i / n);
                    break;
                }
                case ActivityCalendarScale.Log2:
                case ActivityCalendarScale.Log10:
                {
                    // Every level covers a whole number of powers of the base, counted down from the one the largest
                    // value is in. That is what makes the base matter: a log ratio alone is the same in any base.
                    var b    = _scale == ActivityCalendarScale.Log2 ? 2.0 : 10.0;
                    var min  = positives.Count > 0 ? positives.Min() : max;
                    var eMax = FloorLog(max, b);
                    var eMin = FloorLog(Math.Min(min, max), b);
                    var step = Math.Max(1, (int)Math.Ceiling((eMax - eMin + 1) / (double)n));

                    for (int i = 1; i < n; i++) bounds[i] = Math.Pow(b, eMax - (n - i) * step + 1);
                    break;
                }
                case ActivityCalendarScale.Quantile:
                {
                    positives.Sort();
                    for (int i = 1; i < n; i++) bounds[i] = positives.Count == 0 ? max : positives[Math.Min(positives.Count - 1, i * positives.Count / n)];
                    break;
                }
                default:
                {
                    for (int i = 1; i < n; i++) bounds[i] = max * i / n;
                    break;
                }
            }

            return bounds;
        }

        private static int FloorLog(double value, double b)
        {
            // The epsilon keeps an exact power (1000 in base 10) from landing a level low on a rounding error.
            return (int)Math.Floor(Math.Log(value) / Math.Log(b) + 1e-9);
        }

        private int LevelOf(double value)
        {
            if (!(value > 0)) return 0;

            for (int i = _bounds.Length - 1; i >= 1; i--)
            {
                if (value >= _bounds[i]) return i + 1;
            }

            return 1;
        }

        private void RenderSummary()
        {
            ClearChildren(_summary);
            _summary.style.display = _showSummary ? "" : "none";
            if (!_showSummary) return;

            double total   = 0;
            int    days    = 0;
            int    streak  = 0;
            int    longest = 0;

            foreach (var cell in _cells)
            {
                if (cell.Future) continue;

                days++;
                total += cell.Value;

                if (cell.Value > 0)
                {
                    streak++;
                    if (streak > longest) longest = streak;
                }
                else
                {
                    streak = 0;
                }
            }

            var perDay = days > 0 ? total / days : 0;

            _summary.appendChild(SummaryItem(_labels.LongestStreak, longest.ToString("n0"), longest == 1 ? _labels.Day : _labels.Days));
            _summary.appendChild(SummaryItem(_labels.AveragePerDay,  Format(perDay),     null));
            _summary.appendChild(SummaryItem(_labels.AveragePerWeek, Format(perDay * 7), null));
            _summary.appendChild(SummaryItem(_labels.Total,          Format(total),      null));
        }

        private static HTMLElement SummaryItem(string caption, string value, string unit)
        {
            var valueEl = Div(Att("tss-activitycalendar-summary-value", text: value));
            if (!string.IsNullOrEmpty(unit)) valueEl.appendChild(Span(Att("tss-activitycalendar-summary-unit", text: " " + unit)));

            return Div(Att("tss-activitycalendar-summary-item"),
                Div(Att("tss-activitycalendar-summary-caption", text: caption)),
                valueEl);
        }

        private void RenderLegend()
        {
            ClearChildren(_legend);
            _legend.style.display = _showLegend ? "" : "none";
            if (!_showLegend) return;

            _legend.appendChild(Span(Att("tss-activitycalendar-legend-label", text: _labels.Less)));

            for (int level = 0; level <= LevelCount; level++)
            {
                var swatch = Div(Att(DayClass + " tss-activitycalendar-swatch"));
                PaintLevel(swatch, level);
                swatch[LevelProperty] = level;
                _legend.appendChild(swatch);
            }

            _legend.appendChild(Span(Att("tss-activitycalendar-legend-label", text: _labels.More)));
        }

        private string LevelRange(int level)
        {
            if (level == 0) return _labels.NoActivity;

            var n = _bounds.Length;
            if (n <= 1) return "> 0";
            if (level == 1) return "< " + Format(_bounds[1]);
            if (level == n) return "≥ " + Format(_bounds[n - 1]);

            return Format(_bounds[level - 1]) + " – " + Format(_bounds[level]);
        }

        private string Format(double value)
        {
            if (_formatValue is object) return _formatValue(value);

            if (value == Math.Floor(value) || Math.Abs(value) >= 1000) return value.ToString("n0");
            if (Math.Abs(value) >= 1) return value.ToString("n2");

            // Three significant digits for a small amount, as a cost per day tends to be: 0.0000396, not 0.00.
            return Script.Write<string>("String(Number({0}.toPrecision(3)))", value);
        }

        private string FormatDateText(DateTime date) => _formatDate is object ? _formatDate(date) : MonthName(date) + " " + date.Day + ", " + date.Year;

        private string MonthName(DateTime date)
        {
            var names = _labels.MonthNames;
            return names is object && names.Length == 12 ? names[date.Month - 1] : date.ToString("MMM");
        }

        private ActivityCalendarDay DayOf(Cell cell) => new ActivityCalendarDay(cell.Date, cell.Value, cell.HasValue, cell.Level);

        // -------------------------------------------------------------- tooltips

        private string DefaultTooltipText(Cell cell)
        {
            if (cell.Value == 0 && !cell.HasValue) return _labels.NoActivity + " " + _labels.On + " " + FormatDateText(cell.Date);
            return Format(cell.Value) + " " + _labels.On + " " + FormatDateText(cell.Date);
        }

        private Node TooltipFor(Cell cell)
        {
            var day = DayOf(cell);

            if (_tooltip is object)
            {
                return _tooltip(day)?.Render();
            }

            if (_tooltipText is object)
            {
                var text = _tooltipText(day);
                return string.IsNullOrEmpty(text) ? null : TextNode(text);
            }

            var value = cell.Value == 0 && !cell.HasValue ? _labels.NoActivity : Format(cell.Value);

            return Span(Att(),
                Span(Att("tss-activitycalendar-tooltip-value", text: value)),
                Span(Att(text: " " + _labels.On + " " + FormatDateText(cell.Date))));
        }

        private Cell CellFromEvent(Event e)
        {
            var target = e.target.As<HTMLElement>();

            while (target is object && target != _grid)
            {
                if (target.HasOwnProperty(CellProperty)) return _cells[target[CellProperty].As<int>()];
                target = target.parentElement;
            }

            return null;
        }

        private void OnGridHover(Event e)
        {
            var cell = CellFromEvent(e);
            if (cell == _hovered && (cell is null || _tooltipOwner == this)) return;

            _hovered = cell;

            if (cell is null || cell.Future)
            {
                HideTooltip();
                return;
            }

            ShowTooltip(cell.Element, TooltipFor(cell));
        }

        private void OnLegendHover(Event e)
        {
            var target = e.target.As<HTMLElement>();

            if (target is null || !target.HasOwnProperty(LevelProperty))
            {
                HideTooltip();
                return;
            }

            ShowTooltip(target, TextNode(LevelRange(target[LevelProperty].As<int>())));
        }

        private void OnGridClick(Event e)
        {
            var cell = CellFromEvent(e);
            if (cell is null || cell.Future || _onDayClick is null) return;

            _onDayClick(DayOf(cell));
        }

        // ------------------------------------------------------------- keyboard

        private void OnGridFocus()
        {
            // A click focuses the grid too; only the keyboard should get a day picked for it, or the tooltip would
            // jump away from the day under the pointer.
            if (_activeIndex >= 0 || _cells.Count == 0 || !_grid.matches(":focus-visible")) return;

            // Start where the eye already is: the selected day, else today, else the last day shown.
            var start = _cells.Count - 1;
            var key   = KeyOf(_selected ?? TodayValue);

            for (int i = 0; i < _cells.Count; i++)
            {
                if (_cells[i].Key == key) { start = i; break; }
                if (!_cells[i].Future) start = i;
            }

            SetActive(start);
        }

        private void OnGridKeyDown(KeyboardEvent e)
        {
            if (_cells.Count == 0) return;

            var horizontal = _orientation == ActivityCalendarOrientation.Horizontal;
            var index      = _activeIndex < 0 ? 0 : _activeIndex;
            int next;

            switch (e.key)
            {
                case "ArrowLeft":  next = index - (horizontal ? 7 : 1); break;
                case "ArrowRight": next = index + (horizontal ? 7 : 1); break;
                case "ArrowUp":    next = index - (horizontal ? 1 : 7); break;
                case "ArrowDown":  next = index + (horizontal ? 1 : 7); break;
                case "Home":       next = 0;                            break;
                case "End":        next = _cells.Count - 1;             break;
                case "Enter":
                case " ":
                {
                    if (_activeIndex >= 0 && !_cells[_activeIndex].Future && _onDayClick is object) _onDayClick(DayOf(_cells[_activeIndex]));
                    e.preventDefault();
                    return;
                }
                case "Escape":
                {
                    HideTooltip();
                    return;
                }
                default: return;
            }

            e.preventDefault();
            SetActive(Math.Max(0, Math.Min(_cells.Count - 1, next)));
        }

        private void SetActive(int index)
        {
            if (_activeIndex >= 0 && _activeIndex < _cells.Count) _cells[_activeIndex].Element.classList.remove("tss-activitycalendar-active");

            _activeIndex = index;

            if (index < 0)
            {
                HideTooltip();
                return;
            }

            var cell = _cells[index];
            cell.Element.classList.add("tss-activitycalendar-active");
            cell.Element.scrollIntoView(new ScrollIntoViewOptions { block = ScrollLogicalPosition.nearest, inline = ScrollLogicalPosition.nearest });

            _live.textContent = cell.Future ? FormatDateText(cell.Date) : DefaultTooltipText(cell);

            if (cell.Future) HideTooltip();
            else ShowTooltip(cell.Element, TooltipFor(cell));
        }

        // ------------------------------------------------------- shared tooltip

        // One tooltip element for every calendar on the page, attached to the body only while it shows, so a
        // calendar inside a scrolling or clipped container is never cut off by it.
        private static HTMLElement     _tooltipElement;
        private static ActivityCalendar _tooltipOwner;
        private static Action<Event>   _onScroll;

        private void ShowTooltip(HTMLElement anchor, Node content)
        {
            if (content is null)
            {
                HideTooltip();
                return;
            }

            if (_tooltipElement is null)
            {
                _tooltipElement = Div(Att("tss-activitycalendar-tooltip", role: "tooltip"));
                _onScroll       = _ => _tooltipOwner?.HideTooltip();
            }

            var tip        = _tooltipElement;
            var wasShowing = tip.parentElement is object;

            ClearChildren(tip);
            tip.appendChild(content);

            if (!wasShowing)
            {
                tip.classList.remove("tss-activitycalendar-tooltip-moving");
                document.body.appendChild(tip);
                window.addEventListener("scroll", _onScroll, true);
            }
            else
            {
                tip.classList.add("tss-activitycalendar-tooltip-moving");
            }

            _tooltipOwner = this;

            // Into the application z-index lane, so a calendar inside a modal does not draw its tooltip behind it.
            tip.style.zIndex = Layers.AboveCurrent();

            var rect    = anchor.getBoundingClientRect().As<DOMRect>();
            var tipRect = tip.getBoundingClientRect().As<DOMRect>();
            var margin  = 4.0;
            var centre  = rect.left + rect.width / 2;
            var left    = Math.Max(margin, Math.Min(window.innerWidth - tipRect.width - margin, centre - tipRect.width / 2));
            var top     = rect.top - tipRect.height - 8;
            var below   = top < margin;

            if (below) top = rect.bottom + 8;

            tip.classList.toggle("tss-activitycalendar-tooltip-below", below);
            tip.style.left = left + "px";
            tip.style.top  = top + "px";
            tip.style.setProperty("--tss-activitycalendar-arrow", (centre - left) + "px");

            if (!_removalWatched)
            {
                // A calendar taken off the page while hovered gets no mouseleave; this is what clears its tooltip.
                _removalWatched = true;
                DomObserver.WhenRemoved(InnerElement, () =>
                {
                    _removalWatched = false;
                    HideTooltip();
                });
            }
        }

        private void HideTooltip()
        {
            if (_tooltipOwner != this || _tooltipElement is null) return;

            _tooltipOwner = null;
            window.removeEventListener("scroll", _onScroll, true);
            _tooltipElement.remove();
        }

        // ------------------------------------------------------------------ keys

        private static int KeyOf(DateTime date) => date.Year * 10000 + date.Month * 100 + date.Day;

        private static DateTime DateOfKey(int key) => new DateTime(key / 10000, key / 100 % 100, key % 100);

        private static Text TextNode(string text) => document.createTextNode(text);
    }
}
