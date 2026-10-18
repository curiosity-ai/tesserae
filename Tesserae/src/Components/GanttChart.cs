using System;
using System.Collections.Generic;
using static Transpose.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// The preset colors a <see cref="GanttBar"/> can be painted with. Each one is a pastel fill under a
    /// stronger outline of the same hue, which reads the same in light and dark mode.
    /// </summary>
    [Transpose.Name("tss.GanttBarColor")]
    public enum GanttBarColor
    {
        Blue,
        Green,
        Purple,
        Orange,
        Red,
        Teal,
        Magenta,
        Yellow,
        Neutral
    }

    /// <summary>
    /// One bar on a <see cref="GanttChart"/> row: a span from <see cref="Start"/> to <see cref="End"/>, in
    /// whatever unit the chart's axis is in (days, seconds, milliseconds since an origin, …).
    /// </summary>
    [Transpose.Name("tss.GanttBar")]
    public sealed class GanttBar
    {
        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        public GanttBar(double start, double end)
        {
            Start = start;
            End   = end < start ? start : end;
        }

        /// <summary>Where the bar starts, on the chart's axis.</summary>
        public double        Start         { get; }
        /// <summary>Where the bar ends, on the chart's axis. Never before <see cref="Start"/>.</summary>
        public double        End           { get; }
        /// <summary>The preset color, when no custom color was set.</summary>
        public GanttBarColor PresetColor   { get; private set; } = GanttBarColor.Blue;
        /// <summary>The custom CSS color, or null for <see cref="PresetColor"/>.</summary>
        public string        CustomColor   { get; private set; }
        /// <summary>The text drawn beside the bar, or null for none.</summary>
        public string        Label         { get; private set; }
        /// <summary>What the bar shows on hover, or null for nothing.</summary>
        public IComponent    TooltipContent { get; private set; }
        /// <summary>Whether the bar is drawn hatched, for a span that is pending or still running.</summary>
        public bool          IsStriped     { get; private set; }
        /// <summary>Any value the caller wants back from <see cref="GanttChart.OnBarClick"/>.</summary>
        public object        Tag           { get; private set; }

        /// <summary>Paints the bar with one of the preset colors.</summary>
        public GanttBar Color(GanttBarColor color)
        {
            PresetColor = color;
            CustomColor = null;
            return this;
        }

        /// <summary>Paints the bar with a CSS color; the fill and outline are derived from it.</summary>
        public GanttBar Color(string cssColor)
        {
            CustomColor = cssColor;
            return this;
        }

        /// <summary>Draws <paramref name="text"/> beside the bar, after its end (or before its start, near the right edge).</summary>
        public GanttBar Text(string text)
        {
            Label = text;
            return this;
        }

        /// <summary>Shows <paramref name="text"/> when the bar is hovered.</summary>
        public GanttBar Tooltip(string text)
        {
            TooltipContent = string.IsNullOrWhiteSpace(text) ? null : TextBlock(text).Style(s => s.whiteSpace = "pre-wrap");
            return this;
        }

        /// <summary>Shows <paramref name="content"/> when the bar is hovered.</summary>
        public GanttBar Tooltip(IComponent content)
        {
            TooltipContent = content;
            return this;
        }

        /// <summary>Draws the bar hatched, for a span that is pending, queued or still running.</summary>
        public GanttBar Striped(bool striped = true)
        {
            IsStriped = striped;
            return this;
        }

        /// <summary>Attaches a value that <see cref="GanttChart.OnBarClick"/> hands back.</summary>
        public GanttBar SetTag(object tag)
        {
            Tag = tag;
            return this;
        }
    }

    /// <summary>
    /// One row of a <see cref="GanttChart"/>: a label in the first column and any number of bars on the timeline.
    /// </summary>
    [Transpose.Name("tss.GanttRow")]
    public sealed class GanttRow
    {
        private readonly List<GanttBar> _bars = new List<GanttBar>();

        /// <summary>
        /// Initializes a new instance of this class with a text label.
        /// </summary>
        public GanttRow(string label, params GanttBar[] bars) : this(TextBlock(label ?? "").Ellipsis().Tooltip(TextBlock(label ?? "")), bars) { }

        /// <summary>
        /// Initializes a new instance of this class with a component as its label.
        /// </summary>
        public GanttRow(IComponent label, params GanttBar[] bars)
        {
            LabelComponent = label;
            if (bars is object) _bars.AddRange(bars);
        }

        /// <summary>What the first column shows for this row.</summary>
        public IComponent              LabelComponent { get; }
        /// <summary>The row's bars.</summary>
        public IReadOnlyList<GanttBar> Bars           => _bars;
        /// <summary>How many levels the label is indented by, to show a row belongs under the one above.</summary>
        public int                     IndentLevel    { get; private set; }

        /// <summary>Adds a bar to the row.</summary>
        public GanttRow Add(GanttBar bar)
        {
            if (bar is object) _bars.Add(bar);
            return this;
        }

        /// <summary>Indents the label by <paramref name="level"/> steps, to nest the row under the one above it.</summary>
        public GanttRow Indent(int level = 1)
        {
            IndentLevel = level < 0 ? 0 : level;
            return this;
        }
    }

    /// <summary>
    /// A timeline of labelled rows, each holding one or more bars laid out on a shared horizontal axis -
    /// a schedule of tasks over days, or the steps of a process over seconds. The axis is either a set of
    /// named, equal-width columns (<see cref="Columns(string[])"/>) or a numeric range with evenly spaced
    /// ticks (<see cref="Range(double, double)"/>, <see cref="TickFormat(Func{double, string})"/>).
    /// </summary>
    /// <remarks>
    /// The label column stays put while the timeline scrolls sideways once it is narrower than
    /// <see cref="TrackMinWidth(UnitSize)"/>. Without an explicit range the axis spans the bars.
    /// </remarks>
    [Transpose.Name("tss.GanttChart")]
    public sealed class GanttChart : ComponentBase<GanttChart, HTMLElement>
    {
        private readonly List<GanttRow> _rows = new List<GanttRow>();
        private readonly HTMLElement    _grid;

        private string               _title          = "Task";
        private string[]             _columns;
        private double               _rangeStart     = double.NaN;
        private double               _rangeEnd       = double.NaN;
        private int                  _tickCount      = 6;
        private Func<double, string> _tickFormat;
        private string               _emptyMessage   = "Nothing to show";
        private Action<GanttBar>     _onBarClick;

        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        public GanttChart()
        {
            _grid        = Div(Att("tss-gantt-grid"));
            InnerElement = Div(Att("tss-gantt"), _grid);
            Refresh();
        }

        /// <summary>Sets the heading of the label column. Defaults to "Task".</summary>
        public GanttChart Title(string title)
        {
            _title = title ?? "";
            Refresh();
            return this;
        }

        /// <summary>
        /// Lays the axis out as named, equal-width columns - days of a week, sprints, phases. Column
        /// <c>i</c> spans <c>i</c> to <c>i + 1</c> on the axis, so a bar from 0 to 2 covers the first two.
        /// </summary>
        public GanttChart Columns(params string[] columns)
        {
            _columns = columns is object && columns.Length > 0 ? columns : null;
            Refresh();
            return this;
        }

        /// <summary>Pins the numeric axis to <paramref name="start"/>..<paramref name="end"/>. Defaults to the span of the bars.</summary>
        public GanttChart Range(double start, double end)
        {
            _rangeStart = start;
            _rangeEnd   = end;
            Refresh();
            return this;
        }

        /// <summary>Sets roughly how many ticks the numeric axis shows; the step is rounded to 1, 2 or 5 times a power of ten. Defaults to 6.</summary>
        public GanttChart Ticks(int approximateCount)
        {
            _tickCount = approximateCount < 2 ? 2 : approximateCount;
            Refresh();
            return this;
        }

        /// <summary>Sets how a tick of the numeric axis is written - for example as a duration.</summary>
        public GanttChart TickFormat(Func<double, string> format)
        {
            _tickFormat = format;
            Refresh();
            return this;
        }

        /// <summary>Sets the width of the label column. Defaults to 240px.</summary>
        public GanttChart LabelWidth(UnitSize width)
        {
            InnerElement.style.setProperty("--tss-gantt-label-width", width.ToString());
            return this;
        }

        /// <summary>Sets the height of each row. Defaults to 44px.</summary>
        public GanttChart RowHeight(UnitSize height)
        {
            InnerElement.style.setProperty("--tss-gantt-row-height", height.ToString());
            return this;
        }

        /// <summary>Sets the narrowest the timeline gets before the chart scrolls sideways. Defaults to 480px.</summary>
        public GanttChart TrackMinWidth(UnitSize width)
        {
            InnerElement.style.setProperty("--tss-gantt-track-min-width", width.ToString());
            return this;
        }

        /// <summary>Sets what the chart says when it has no rows.</summary>
        public GanttChart EmptyMessage(string message)
        {
            _emptyMessage = message ?? "";
            Refresh();
            return this;
        }

        /// <summary>Calls <paramref name="onClick"/> with the bar that was clicked.</summary>
        public GanttChart OnBarClick(Action<GanttBar> onClick)
        {
            _onBarClick = onClick;
            Refresh();
            return this;
        }

        /// <summary>Adds a row.</summary>
        public GanttChart Add(GanttRow row)
        {
            if (row is object) _rows.Add(row);
            Refresh();
            return this;
        }

        /// <summary>Replaces every row with <paramref name="rows"/>.</summary>
        public GanttChart Rows(params GanttRow[] rows)
        {
            _rows.Clear();

            if (rows is object)
            {
                foreach (var row in rows)
                {
                    if (row is object) _rows.Add(row);
                }
            }

            Refresh();
            return this;
        }

        /// <summary>Removes every row.</summary>
        public GanttChart Clear()
        {
            _rows.Clear();
            Refresh();
            return this;
        }

        private (double start, double end) Extent()
        {
            if (_columns is object) return (0, _columns.Length);

            double start = double.NaN;
            double end   = double.NaN;

            foreach (var row in _rows)
            {
                foreach (var bar in row.Bars)
                {
                    if (double.IsNaN(start) || bar.Start < start) start = bar.Start;
                    if (double.IsNaN(end)   || bar.End   > end)   end   = bar.End;
                }
            }

            if (!double.IsNaN(_rangeStart)) start = _rangeStart;
            if (!double.IsNaN(_rangeEnd))   end   = _rangeEnd;

            if (double.IsNaN(start)) start = 0;
            if (double.IsNaN(end))   end   = start + 1;
            if (end <= start)        end   = start + 1;

            return (start, end);
        }

        private static double NiceStep(double span, int count)
        {
            var raw       = span / count;
            var magnitude = Math.Pow(10, Math.Floor(Math.Log10(raw)));
            var residual  = raw / magnitude;

            if (residual > 5) return 10 * magnitude;
            if (residual > 2) return 5 * magnitude;
            if (residual > 1) return 2 * magnitude;

            return magnitude;
        }

        private string FormatTick(double value)
        {
            if (_tickFormat is object) return _tickFormat(value);

            return Transpose.Script.Write<string>("String(+{0}.toFixed(6))", value);
        }

        private static string Percent(double value) => Transpose.Script.Write<string>("{0}.toFixed(4)", value) + "%";

        private void Refresh()
        {
            ClearChildren(_grid);

            var (start, end) = Extent();
            var span         = end - start;

            double Position(double value) => (value - start) / span * 100.0;

            _grid.style.gridTemplateRows = _rows.Count > 0 ? $"auto repeat({_rows.Count}, var(--tss-gantt-row-height))" : "auto auto";

            var headerTrack = Div(Att("tss-gantt-header-track"));
            _grid.appendChild(Div(Att("tss-gantt-header-label", text: _title)));
            _grid.appendChild(headerTrack);

            var lines = Div(Att("tss-gantt-lines"));

            if (_columns is object)
            {
                for (int i = 0; i < _columns.Length; i++)
                {
                    var column = Div(Att("tss-gantt-column-label", text: _columns[i], title: _columns[i]));
                    column.style.left  = Percent(Position(i));
                    column.style.width = Percent(100.0 / _columns.Length);
                    headerTrack.appendChild(column);

                    if (i > 0)
                    {
                        var line = Div(Att("tss-gantt-line"));
                        line.style.left = Percent(Position(i));
                        lines.appendChild(line);

                        var separator = Div(Att("tss-gantt-header-separator"));
                        separator.style.left = Percent(Position(i));
                        headerTrack.appendChild(separator);
                    }
                }
            }
            else
            {
                var step  = NiceStep(span, _tickCount);
                var first = Math.Ceiling(start / step) * step;

                for (int i = 0; first + i * step <= end + step * 1e-9; i++)
                {
                    var tick     = first + i * step;
                    var position = Position(tick);
                    var label    = Div(Att("tss-gantt-tick-label", text: FormatTick(tick)));
                    label.style.left = Percent(position);

                    if (position < 4)       label.classList.add("tss-gantt-tick-label-start");
                    else if (position > 96) label.classList.add("tss-gantt-tick-label-end");

                    headerTrack.appendChild(label);

                    if (position > 0.01 && position < 99.99)
                    {
                        var line = Div(Att("tss-gantt-line"));
                        line.style.left = Percent(position);
                        lines.appendChild(line);
                    }
                }
            }

            if (_rows.Count == 0)
            {
                _grid.appendChild(Div(Att("tss-gantt-empty", text: _emptyMessage)));
                return;
            }

            _grid.appendChild(lines);

            for (int i = 0; i < _rows.Count; i++)
            {
                var row   = _rows[i];
                var label = Div(Att("tss-gantt-row-label"), row.LabelComponent.Render());
                if (row.IndentLevel > 0) label.style.paddingLeft = $"calc(var(--tss-gantt-cell-padding) + {row.IndentLevel * 16}px)";

                var track = Div(Att("tss-gantt-row-track"));

                // Placed explicitly: the grid lines span every row of the second column, so auto-placement would skip past them
                label.style.gridRow = (i + 2).ToString();
                track.style.gridRow = (i + 2).ToString();

                foreach (var bar in row.Bars)
                {
                    track.appendChild(RenderBar(bar, Position(bar.Start), Position(bar.End)));
                }

                _grid.appendChild(label);
                _grid.appendChild(track);
            }
        }

        private HTMLElement RenderBar(GanttBar gantt, double left, double right)
        {
            var bar = Div(Att("tss-gantt-bar tss-gantt-bar-" + gantt.PresetColor.ToString().ToLower()));
            bar.style.left  = Percent(left);
            bar.style.width = $"max({Percent(right - left)}, var(--tss-gantt-bar-min-width))";

            if (!string.IsNullOrEmpty(gantt.CustomColor)) bar.style.setProperty("--tss-gantt-bar-color", gantt.CustomColor);
            if (gantt.IsStriped)                          bar.classList.add("tss-gantt-bar-striped");

            if (_onBarClick is object)
            {
                bar.classList.add("tss-gantt-bar-clickable");
                bar.addEventListener("click", _ => _onBarClick(gantt));
            }

            if (gantt.TooltipContent is object) Raw(bar).Tooltip(gantt.TooltipContent, followCursor: true);

            if (string.IsNullOrEmpty(gantt.Label)) return bar;

            // Near the right edge there is no room after the bar, so the text goes before it - or inside it, when the bar spans the axis.
            var text = Span(Att("tss-gantt-bar-text", text: gantt.Label));

            if (right > 80 && left < 20)
            {
                text.classList.add("tss-gantt-bar-text-inside");
                text.style.right = Percent(100 - right);
            }
            else if (right > 80)
            {
                text.style.right = Percent(100 - left);
            }
            else
            {
                text.style.left = $"calc(max({Percent(right)}, {Percent(left)} + var(--tss-gantt-bar-min-width)))";
            }

            return Div(Att("tss-gantt-bar-group"), bar, text);
        }

        /// <summary>
        /// Renders the component's root HTML element.
        /// </summary>
        public override HTMLElement Render() => InnerElement;
    }
}
