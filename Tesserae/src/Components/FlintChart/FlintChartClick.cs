using System.Collections.Generic;
using Transpose;

namespace Tesserae
{
    /// <summary>What was clicked on a <see cref="FlintChart"/>.</summary>
    public enum FlintChartClickTarget
    {
        /// <summary>One mark: a bar, a point, a slice, a cell. Its row (or the rows it sums) is in <see cref="FlintChartClick.Rows"/>.</summary>
        Mark,

        /// <summary>A line or an area, which stands for a whole series: every row of the series.</summary>
        Series,

        /// <summary>An entry in the chart's key: every row of that series or category.</summary>
        LegendEntry,

        /// <summary>A category's label on an axis: every row of that category.</summary>
        AxisLabel,
    }

    /// <summary>
    /// A click on a <see cref="FlintChart"/> (<see cref="FlintChart.OnClick"/>): what was clicked, and the rows of the
    /// chart's data it stands for, as they were given to the chart.
    /// </summary>
    [Transpose.Name("tss.FlintChartClick")]
    public sealed class FlintChartClick
    {
        private FlintChartClick(FlintChartClickTarget target, string role, FlintChartRow[] rows, string field, object value)
        {
            Target = target;
            Role   = role;
            Rows   = rows;
            Field  = field;
            Value  = value;
        }

        /// <summary>What kind of thing was clicked.</summary>
        public FlintChartClickTarget Target { get; }

        /// <summary>The role Flint gives what was clicked, e.g. <c>bar</c>, <c>symbol</c>, <c>arc</c>, <c>line</c>, <c>legend-item</c>, <c>axis-label</c>.</summary>
        public string Role { get; }

        /// <summary>The rows of the chart's data the clicked thing stands for: one for a mark drawn from one row, more for a series, a key entry, a category or a mark that sums several.</summary>
        public IReadOnlyList<FlintChartRow> Rows { get; }

        /// <summary>The first of <see cref="Rows"/>, or <c>null</c>.</summary>
        public FlintChartRow Row => Rows.Count > 0 ? Rows[0] : null;

        /// <summary>For a key entry or an axis label, the field it names; otherwise <c>null</c>.</summary>
        public string Field { get; }

        /// <summary>For a key entry or an axis label, the value it names (a category, a series); otherwise <c>null</c>.</summary>
        public object Value { get; }

        /// <summary>Reads Flint's interaction event; <c>null</c> for anything that is not a click on something.</summary>
        internal static FlintChartClick From(object detail)
        {
            //The event is Flint's own JavaScript object (FlintInteractionEventDetail); read once, here, into typed values.
            var action = Script.Write<string>("{0} && {0}.event ? {0}.event.action : null", detail);
            var phase  = Script.Write<string>("{0}.event.phase", detail);

            if (action is null || !action.StartsWith("click-") || (phase != null && phase != "commit")) return null;
            if (Script.Write<bool>("!{0}.event.target", detail)) return null;

            var kind = Script.Write<string>("{0}.event.target.visual.kind", detail);
            var role = Script.Write<string>("{0}.event.target.visual.role", detail);

            //A mark click reaches every interaction that listens, so it is read from the click report's own event. A key
            //entry or an axis label has one owner, which is an interaction of the spec when it clicks them (Flint.Mount).
            var fromReport = Script.Write<string>("{0}.interactionId", detail) == Flint.ClickReportId;
            if (!fromReport && kind != "legend" && kind != "axis" && role != "legend-item") return null;

            FlintChartClickTarget target;
            switch (kind)
            {
                case "mark":   target = FlintChartClickTarget.Mark; break;
                case "path":   target = FlintChartClickTarget.Series; break;
                case "legend": target = FlintChartClickTarget.LegendEntry; break;
                case "axis":   target = FlintChartClickTarget.AxisLabel; break;
                default:       target = role == "legend-item" ? FlintChartClickTarget.LegendEntry : FlintChartClickTarget.Mark; break;
            }

            var records = Script.Write<object[]>("(function (els) { var out = []; for (var i = 0; i < els.length; i++) { var r = els[i].records || []; for (var j = 0; j < r.length; j++) out.push(r[j]); } return out; })({0}.event.target.elements || [])", detail);
            var rows    = new FlintChartRow[records.Length];
            for (int i = 0; i < records.Length; i++) rows[i] = new FlintChartRow(records[i]);

            string field = null;
            object value = null;

            if (target == FlintChartClickTarget.LegendEntry || target == FlintChartClickTarget.AxisLabel)
            {
                //A key entry carries { field, domain: { value } }; an axis label carries { field, value }.
                field = Script.Write<string>("(function (v) { return v ? (v.field || null) : null; })(({0}.event.target.elements[0] || {}).value)", detail);
                value = Script.Write<object>("(function (v) { if (!v) return null; if (v.domain && v.domain.kind === 'value') return v.domain.value; return v.value === undefined ? null : v.value; })(({0}.event.target.elements[0] || {}).value)", detail);
            }

            return new FlintChartClick(target, role, rows, field, value);
        }
    }

    /// <summary>One row of a <see cref="FlintChart"/>'s data, as it was given to the chart.</summary>
    [Transpose.Name("tss.FlintChartRow")]
    public sealed class FlintChartRow
    {
        //The row is the JavaScript object the chart was given; the readers below are its typed surface.
        private readonly object _row;

        internal FlintChartRow(object row) => _row = row;

        /// <summary>The fields the row has, in its own order.</summary>
        public string[] Fields => Script.Write<string[]>("Object.keys({0})", _row);

        /// <summary>Whether the row has <paramref name="field"/>.</summary>
        public bool Has(string field) => Script.Write<bool>("Object.prototype.hasOwnProperty.call({0}, {1})", _row, field);

        /// <summary>The value of <paramref name="field"/> as it was given: a string, a number, a boolean or <c>null</c>.</summary>
        public object this[string field] => Script.Write<object>("{0}[{1}] === undefined ? null : {0}[{1}]", _row, field);

        /// <summary>The value of <paramref name="field"/> as text, or <c>null</c> when the row has none.</summary>
        public string GetString(string field) => Script.Write<string>("{0}[{1}] === undefined || {0}[{1}] === null ? null : String({0}[{1}])", _row, field);

        /// <summary>The value of <paramref name="field"/> as a number, or <c>null</c> when it is not one.</summary>
        public double? GetNumber(string field)
        {
            if (!Script.Write<bool>("typeof {0}[{1}] === 'number' && isFinite({0}[{1}])", _row, field)) return null;
            return Script.Write<double>("{0}[{1}]", _row, field);
        }

        /// <summary>The row as JSON.</summary>
        public string ToJson() => Script.Write<string>("JSON.stringify({0})", _row);

        /// <inheritdoc />
        public override string ToString() => ToJson();
    }
}
