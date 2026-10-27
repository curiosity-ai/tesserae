using System;
using System.Collections.Generic;
using Transpose;
using static Transpose.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// A chart drawn by Microsoft's <see href="https://github.com/microsoft/flint-chart">Flint</see> from a Flint chart
    /// spec: the data, what its fields mean, and which field goes on which channel of which chart type. Flint decides
    /// the rest (scales, axes, formats, layout) from that, and the theme (<see cref="FlintThemes"/>) decides the look;
    /// the default theme reads the page's Tesserae tokens, so the chart follows light/dark mode and custom themes.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The spec is the same JSON a <c>```flint</c> fence in <see cref="MarkdownBlock"/> holds, and can be given as that
    /// JSON (<see cref="FlintChart(string)"/>) or built with the fluent methods, which write into it
    /// (<see cref="Columns"/>/<see cref="AddRow"/>, <see cref="SemanticType"/>, <see cref="Encode(FlintChannel, string)"/>, …).
    /// A spec given as JSON can be adjusted with the same methods afterwards.
    /// </para>
    /// <para>
    /// Flint is fetched the first time a chart is drawn. The chart is laid out for the width it has, again when that
    /// width changes, and drawn again (out of sight, swapped in once drawn) after a change to its spec or a theme
    /// change that changes its theme. A spec that cannot be drawn is reported through <see cref="Error"/> and
    /// <see cref="OnError"/>, and shown in the chart's place until one can.
    /// </para>
    /// <para>
    /// <see cref="OnElementClick"/> reports a click on a mark, a series, a key entry or a category's axis label, with
    /// the rows of the data it stands for.
    /// </para>
    /// </remarks>
    [Transpose.Name("tss.FlintChart")]
    public class FlintChart : ComponentBase<FlintChart, HTMLElement>
    {
        private readonly FlintChartDrawing       _drawing;
        private readonly HTMLElement             _message;
        private          object                  _spec;
        private          string[]                _columns;
        private          string                  _error;
        private          Action<string>          _onError;
        private          Action<FlintChartClick> _onClick;
        private          bool                    _redrawQueued;
        private          bool                    _mounted;

        /// <summary>A chart of <paramref name="type"/>, with no data yet.</summary>
        public FlintChart(FlintChartType type)
        {
            _spec = Script.Write<object>("{ data: { values: [] }, semantic_types: {}, chart_spec: { chartType: {0}, encodings: {} }, options: { addTooltips: true } }", FlintNames.Of(type));
            InnerElement = Div(Att("tss-flintchart tss-flintchart-empty"));
            _message     = Div(Att("tss-flintchart-message"));
            _drawing     = CreateDrawing();
            WatchMounting();
        }

        /// <summary>
        /// A chart drawn from a Flint chart spec (a <c>ChartAssemblyInput</c>): <c>data</c>, <c>semantic_types</c> and
        /// <c>chart_spec</c>, with optional <c>theme_spec</c>, <c>interaction_spec</c> and <c>options</c>. A spec that is
        /// not valid JSON is reported like any other that cannot be drawn.
        /// </summary>
        public FlintChart(string specJson)
        {
            InnerElement = Div(Att("tss-flintchart tss-flintchart-empty"));
            _message     = Div(Att("tss-flintchart-message"));
            _drawing     = CreateDrawing();
            Spec         = specJson;
            WatchMounting();
        }

        /// <summary>Gets or sets the chart's spec as Flint chart JSON. Setting it replaces everything the fluent methods set, and redraws.</summary>
        public string Spec
        {
            get => _spec is null ? null : Script.Write<string>("JSON.stringify({0}, null, 2)", _spec);
            set
            {
                _columns = null;

                try
                {
                    var parsed = Script.Write<object>("JSON.parse({0})", value ?? "");

                    if (!Script.Write<bool>("!!{0} && typeof {0} === 'object' && !Array.isArray({0})", parsed))
                    {
                        throw new Exception("The chart spec must be a JSON object with data, semantic_types and chart_spec.");
                    }

                    _spec = parsed;
                }
                catch (Exception e)
                {
                    _spec = null;
                    ShowError(e.Message.StartsWith("The chart spec") ? e.Message : "The chart spec is not valid JSON: " + e.Message);
                    return;
                }

                Redraw();
            }
        }

        /// <summary>The name Flint gives <paramref name="type"/> in a spec's <c>chartType</c>, e.g. <c>"Scatter Plot"</c>.</summary>
        public static string ChartTypeName(FlintChartType type) => FlintNames.Of(type);

        /// <summary>Gets why the chart could not be drawn the last time it was tried, or <c>null</c> when it was (or has not been tried yet).</summary>
        public string Error => _error;

        /// <summary>Gets whether a chart is on screen.</summary>
        public bool IsDrawn => _drawing.IsDrawn;

        /// <summary>Sets the chart type, keeping the data and the encodings, and redraws.</summary>
        public FlintChart ChartType(FlintChartType type)
        {
            Script.Write("{0}.chartType = {1};", ChartSpec(), FlintNames.Of(type));
            return Redraw();
        }

        /// <summary>
        /// Names the fields of the rows <see cref="AddRow"/> and <see cref="Data{T}"/> add, in order, and drops the rows
        /// there were.
        /// </summary>
        public FlintChart Columns(params string[] names)
        {
            _columns = names ?? new string[0];
            return ClearData();
        }

        /// <summary>
        /// Adds one row, a value per field named by <see cref="Columns"/>. A value can be a string, a number, a
        /// <see cref="bool"/>, a <see cref="DateTime"/> (written as an ISO date) or <c>null</c>.
        /// </summary>
        public FlintChart AddRow(params object[] values)
        {
            Script.Write("{0}.push({1});", Rows(), RowOf(values));
            return Redraw();
        }

        /// <summary>Adds a row per item, with a value per field named by <see cref="Columns"/>.</summary>
        public FlintChart Data<T>(IEnumerable<T> items, Func<T, object[]> values)
        {
            var rows = Rows();
            foreach (var item in items) Script.Write("{0}.push({1});", rows, RowOf(values(item)));
            return Redraw();
        }

        /// <summary>Replaces the data with rows given as a JSON array of objects, one property per field.</summary>
        public FlintChart DataJson(string rowsJson)
        {
            var rows = Script.Write<object>("JSON.parse({0})", rowsJson);
            if (!Script.Write<bool>("Array.isArray({0})", rows)) throw new ArgumentException("The rows must be a JSON array of objects.", nameof(rowsJson));

            Script.Write("{0}.data = { values: {1} };", Input(), rows);
            return Redraw();
        }

        /// <summary>
        /// Draws the data from a JSON or CSV file at <paramref name="url"/>, in place of rows given here. The file is a
        /// request to whatever server hosts it.
        /// </summary>
        public FlintChart DataUrl(string url)
        {
            Script.Write("{0}.data = { url: {1} };", Input(), url);
            return Redraw();
        }

        /// <summary>Drops every row.</summary>
        public FlintChart ClearData()
        {
            Script.Write("{0}.data = { values: [] };", Input());
            return Redraw();
        }

        /// <summary>Says what <paramref name="field"/> means, which decides how Flint encodes, scales and formats it.</summary>
        public FlintChart SemanticType(string field, FlintSemanticType type)
        {
            Script.Write("var s = {0}; if (!s.semantic_types || typeof s.semantic_types !== 'object') { s.semantic_types = {}; } s.semantic_types[{1}] = {2};", Input(), field, FlintNames.Of(type));
            return Redraw();
        }

        /// <summary>Draws <paramref name="field"/> on <paramref name="channel"/>.</summary>
        public FlintChart Encode(FlintChannel channel, string field)
        {
            Script.Write("{0}[{1}] = {2};", Encodings(), FlintNames.Of(channel), field);
            return Redraw();
        }

        /// <summary>Draws <paramref name="encoding"/>'s field on <paramref name="channel"/>, the way it says.</summary>
        public FlintChart Encode(FlintChannel channel, FlintEncoding encoding)
        {
            Script.Write("{0}[{1}] = {2};", Encodings(), FlintNames.Of(channel), encoding.ToSpec());
            return Redraw();
        }

        /// <summary>
        /// Draws several measures on one measure channel (<c>y</c>, or <c>x</c> when it is the measure), as a series
        /// each: Flint folds them into one series field, coloured and keyed by name.
        /// </summary>
        public FlintChart Encode(FlintChannel channel, params string[] fields)
        {
            Script.Write("{0}[{1}] = {2}.slice();", Encodings(), FlintNames.Of(channel), fields);
            return Redraw();
        }

        /// <summary>Takes <paramref name="channel"/> off the chart.</summary>
        public FlintChart RemoveEncoding(FlintChannel channel)
        {
            Script.Write("delete {0}[{1}];", Encodings(), FlintNames.Of(channel));
            return Redraw();
        }

        /// <summary>Draws <paramref name="field"/> along x.</summary>
        public FlintChart X(string field) => Encode(FlintChannel.X, field);

        /// <summary>Draws <paramref name="field"/> along y.</summary>
        public FlintChart Y(string field) => Encode(FlintChannel.Y, field);

        /// <summary>Colours the marks by <paramref name="field"/>.</summary>
        public FlintChart ColorBy(string field) => Encode(FlintChannel.Color, field);

        /// <summary>The headline: what the chart says, in words.</summary>
        public FlintChart Title(string title) => SetChartSpec("title", title);

        /// <summary>The reading of the headline: what is measured, of whom, when, in what units.</summary>
        public FlintChart Subtitle(string subtitle) => SetChartSpec("subtitle", subtitle);

        /// <summary>A short line above the headline, such as a figure label.</summary>
        public FlintChart Label(string label) => SetChartSpec("label", label);

        /// <summary>Where the data comes from, drawn as the chart's source line; a link when <paramref name="href"/> is given.</summary>
        public FlintChart Source(string text, string href = null)
        {
            var spec = ChartSpec();
            if (text is null) Script.Write("delete {0}.source;", spec);
            else if (href is null) Script.Write("{0}.source = {1};", spec, text);
            else Script.Write("{0}.source = { text: {1}, href: {2} };", spec, text, href);
            return Redraw();
        }

        /// <summary>
        /// Sets one of the chart type's own properties (Flint's <c>chartProperties</c>), such as <c>innerRadius</c>,
        /// <c>showPoints</c>, <c>stackMode</c>, <c>binCount</c>, <c>cornerRadius</c> or <c>region</c>. <c>null</c> unsets it.
        /// </summary>
        public FlintChart Property(string name, object value)
        {
            Script.Write("var c = {0}; if (!c.chartProperties) { c.chartProperties = {}; } if ({2} === null || {2} === undefined) { delete c.chartProperties[{1}]; } else { c.chartProperties[{1}] = {2}; }", ChartSpec(), name, ValueOf(value));
            return Redraw();
        }

        /// <summary>The size the chart is laid out for when its data fits comfortably (Flint's default is 400 by 320); it still fits the width it has.</summary>
        public FlintChart BaseSize(int width, int height)
        {
            Script.Write("{0}.baseSize = { width: {1}, height: {2} };", ChartSpec(), width, height);
            return Redraw();
        }

        /// <summary>Whether hovering a mark shows its values (on by default).</summary>
        public FlintChart Tooltips(bool show = true)
        {
            Script.Write("var s = {0}; if (!s.options) { s.options = {}; } s.options.addTooltips = {1};", Input(), show);
            return Redraw();
        }

        /// <summary>Replaces the chart's interactions (Flint's <c>interaction_spec</c>) with these presets; none removes them.</summary>
        public FlintChart Interactions(params FlintInteraction[] interactions)
        {
            var names = new string[interactions.Length];
            for (int i = 0; i < interactions.Length; i++) names[i] = FlintNames.Of(interactions[i]);

            Script.Write("var s = {0}; if ({1}.length === 0) { delete s.interaction_spec; } else { s.interaction_spec = { interactions: {1}.map(function (t) { return { type: t }; }) }; }", Input(), names);
            return Redraw();
        }

        /// <summary>
        /// The theme the chart is drawn in when its spec names none (<see cref="FlintThemes.Tesserae"/> by default), or
        /// in any case when <paramref name="overrideSpecTheme"/> is set.
        /// </summary>
        public FlintChart ChartTheme(IFlintTheme theme, bool overrideSpecTheme = false)
        {
            _drawing.Theme             = theme ?? FlintThemes.Tesserae;
            _drawing.OverrideSpecTheme = overrideSpecTheme;
            return Redraw();
        }

        /// <summary>
        /// Lays the chart out for the height it is given (with <c>.Height(...)</c>, <c>.HS()</c> or <c>.Grow()</c>) as well
        /// as its width. By default the chart chooses its own height for the width it has.
        /// </summary>
        public FlintChart FillHeight(bool fill = true)
        {
            _drawing.FillHeight = fill;
            InnerElement.UpdateClassIf(fill, "tss-flintchart-fill");
            return Redraw();
        }

        /// <summary>
        /// Called when a mark, a series (a line or an area), a key entry or a category's axis label is clicked, with
        /// what was clicked and the rows of the data it stands for. A click reported here changes nothing on the chart;
        /// add <see cref="FlintInteraction.ClickHighlight"/> to also emphasise what was clicked.
        /// </summary>
        public FlintChart OnElementClick(Action<FlintChartClick> onClick)
        {
            var hadListener = _onClick != null;
            _onClick = onClick;
            _drawing.OnClick = onClick is null ? null : (Action<FlintChartClick>)(click => _onClick?.Invoke(click));

            //Clicks are reported by an interaction mounted with the chart, so a chart drawn without one is drawn again.
            if (hadListener != (onClick != null)) Redraw();
            return this;
        }

        /// <summary>Called with the reason the chart cannot be drawn (a field that is not in the data, a channel the chart type does not have, the library failing to load), once per failed attempt.</summary>
        public FlintChart OnError(Action<string> onError)
        {
            _onError = onError;
            return this;
        }

        /// <summary>Draws the chart again now, rather than once the current change settles.</summary>
        public FlintChart Refresh()
        {
            _redrawQueued = false;
            if (_mounted) _drawing.DrawAsync().FireAndForget();
            return this;
        }

        public override HTMLElement Render() => InnerElement;

        private FlintChartDrawing CreateDrawing()
        {
            var drawing = new FlintChartDrawing(InnerElement, "tss-flintchart-frame")
            {
                Input = MakeInput,
            };

            drawing.OnDrawn = () =>
            {
                _error = null;
                InnerElement.classList.remove("tss-flintchart-empty");
                InnerElement.classList.remove("tss-flintchart-failed");
                _message.remove();
            };

            drawing.OnFailed = (reason, stillDrawn) => ShowError(reason, stillDrawn);
            return drawing;
        }

        private void WatchMounting()
        {
            DomObserver.WhenMounted(InnerElement, () =>
            {
                var first = !_mounted;
                _mounted = true;

                if (first) Refresh();
                else _drawing.Resume();

                DomObserver.WhenRemoved(InnerElement, () =>
                {
                    _drawing.Suspend();
                    WatchMounting();
                });
            });
        }

        //A fresh copy for every draw: Flint and the theme write into the input they are given.
        private object MakeInput()
        {
            if (_spec is null) throw new Exception(_error ?? "The chart has no spec.");

            return Script.Write<object>("JSON.parse(JSON.stringify({0}))", _spec);
        }

        //Several changes in a row (a fluent chain) draw once, after the last.
        private FlintChart Redraw()
        {
            if (_redrawQueued || !_mounted) return this;

            _redrawQueued = true;
            window.setTimeout(_ =>
            {
                if (_redrawQueued) Refresh();
            }, 0);

            return this;
        }

        private void ShowError(string reason, bool stillDrawn = false)
        {
            _error = reason;
            _onError?.Invoke(reason);

            //A redraw that failed leaves the chart that was there; the reason is in Error.
            if (stillDrawn) return;

            InnerElement.classList.remove("tss-flintchart-empty");
            InnerElement.classList.add("tss-flintchart-failed");
            _message.textContent = reason;
            if (_message.parentElement != InnerElement) InnerElement.appendChild(_message);
        }

        private object Input()
        {
            if (_spec is null) _spec = Script.Write<object>("{ data: { values: [] }, semantic_types: {}, chart_spec: { encodings: {} } }");
            return _spec;
        }

        private object ChartSpec()
            => Script.Write<object>("(function (s) { if (!s.chart_spec || typeof s.chart_spec !== 'object') { s.chart_spec = { encodings: {} }; } return s.chart_spec; })({0})", Input());

        private object Encodings()
            => Script.Write<object>("(function (c) { if (!c.encodings || typeof c.encodings !== 'object') { c.encodings = {}; } return c.encodings; })({0})", ChartSpec());

        private object Rows()
            => Script.Write<object>("(function (s) { if (!s.data || !Array.isArray(s.data.values)) { s.data = { values: [] }; } return s.data.values; })({0})", Input());

        private FlintChart SetChartSpec(string key, string value)
        {
            if (value is null) Script.Write("delete {0}[{1}];", ChartSpec(), key);
            else Script.Write("{0}[{1}] = {2};", ChartSpec(), key, value);
            return Redraw();
        }

        private object RowOf(object[] values)
        {
            if (_columns is null) throw new InvalidOperationException("Name the fields with Columns(...) before adding rows.");
            if (values.Length != _columns.Length) throw new ArgumentException($"A row has {values.Length} values for {_columns.Length} columns.");

            var row = Script.Write<object>("{}");
            for (int i = 0; i < values.Length; i++) Script.Write("{0}[{1}] = {2};", row, _columns[i], ValueOf(values[i]));
            return row;
        }

        //The values a chart's rows hold are JSON values, which is what the spec is copied through on every draw.
        private static object ValueOf(object value)
        {
            switch (value)
            {
                case null:           return null;
                case string s:       return s;
                case bool b:         return b;
                case DateTime d:     return d.TimeOfDay == TimeSpan.Zero ? d.ToString("yyyy-MM-dd") : d.ToString("yyyy-MM-ddTHH:mm:ss");
                case DateTimeOffset o: return o.ToString("yyyy-MM-ddTHH:mm:sszzz");
                case long l:         return (double)l;
                case decimal m:      return (double)m;
                case float f:        return (double)f;
                case Enum e:         return e.ToString();
                case int _:
                case double _:       return value;
                default:             return value.ToString();
            }
        }
    }
}
