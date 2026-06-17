using System;
using System.Linq;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// A lightweight, dependency-free SVG pie / donut chart. Renders the first series' values as slices, labelled
    /// by the configured category labels, with hover tooltips, an optional legend and a theme-aware palette.
    /// </summary>
    [H5.Name("tss.PieChart")]
    public sealed class PieChart : ChartBase<PieChart>
    {
        private string[] _labels = new string[0];
        private double   _donutRatio;   // 0 = full pie; (0,1) = donut hole as a fraction of the radius

        /// <summary>
        /// Initializes a new instance of the <see cref="PieChart"/> class.
        /// </summary>
        public PieChart() : base() { }

        /// <summary>Sets the per-slice labels.</summary>
        public PieChart Labels(params string[] labels) { _labels = labels ?? new string[0]; QueueRender(); return this; }

        /// <summary>Renders the chart as a donut with a hole sized as a fraction (0..1) of the radius.</summary>
        public PieChart Donut(double holeRatio = 0.6) { _donutRatio = Math.Max(0, Math.Min(0.95, holeRatio)); QueueRender(); return this; }

        protected override void RenderChart(double width, double height)
        {
            var values = _series.Count > 0 ? _series[0].Values : new double[0];
            var total  = values.Sum();

            var legendWidth = 0.0;
            if (_showLegend && values.Length > 0)
            {
                legendWidth = Math.Min(width * 0.4, 140);
            }

            var chartWidth = width - legendWidth;
            var cx         = chartWidth / 2;
            var cy         = height / 2;
            var radius     = Math.Max(4, Math.Min(chartWidth, height) / 2 - 8);
            var innerR     = radius * _donutRatio;

            if (total <= 0 || values.Length == 0)
            {
                return;
            }

            double startAngle = -Math.PI / 2; // start at 12 o'clock

            for (int i = 0; i < values.Length; i++)
            {
                if (values[i] <= 0) continue;

                var sweep    = values[i] / total * Math.PI * 2;
                var endAngle = startAngle + sweep;
                var color    = _series[0].Color != null && i == 0 ? _series[0].Color : _palette[i % _palette.Length];

                var path = El("path");
                Attr(path, "d", SlicePath(cx, cy, radius, innerR, startAngle, endAngle, sweep));
                Attr(path, "fill", color);
                Attr(path, "stroke", Theme.Default.Background);
                Attr(path, "stroke-width", 1);
                AttachPointTooltip(path, TooltipFor(i, values[i], total));
                _svg.appendChild(path);

                startAngle = endAngle;
            }

            DrawLegend(values, chartWidth, height);
        }

        private string SlicePath(double cx, double cy, double ro, double ri, double a0, double a1, double sweep)
        {
            var largeArc = sweep > Math.PI ? 1 : 0;

            var ox0 = cx + ro * Math.Cos(a0);
            var oy0 = cy + ro * Math.Sin(a0);
            var ox1 = cx + ro * Math.Cos(a1);
            var oy1 = cy + ro * Math.Sin(a1);

            if (ri <= 0)
            {
                // Full wedge from the center.
                return $"M {F(cx)} {F(cy)} L {F(ox0)} {F(oy0)} A {F(ro)} {F(ro)} 0 {largeArc} 1 {F(ox1)} {F(oy1)} Z";
            }

            // Donut annulus segment.
            var ix1 = cx + ri * Math.Cos(a1);
            var iy1 = cy + ri * Math.Sin(a1);
            var ix0 = cx + ri * Math.Cos(a0);
            var iy0 = cy + ri * Math.Sin(a0);

            return $"M {F(ox0)} {F(oy0)} A {F(ro)} {F(ro)} 0 {largeArc} 1 {F(ox1)} {F(oy1)} " +
                   $"L {F(ix1)} {F(iy1)} A {F(ri)} {F(ri)} 0 {largeArc} 0 {F(ix0)} {F(iy0)} Z";
        }

        private static string F(double v) => v.ToString("0.###");

        private void DrawLegend(double[] values, double chartWidth, double height)
        {
            if (!_showLegend) return;

            double x = chartWidth + 8;
            double y = Math.Max(12, height / 2 - values.Length * 9);

            for (int i = 0; i < values.Length; i++)
            {
                var color = _palette[i % _palette.Length];
                var label = i < _labels.Length ? _labels[i] : "#" + (i + 1);

                var rect = El("rect");
                Attr(rect, "x", x);
                Attr(rect, "y", y);
                Attr(rect, "width", 10);
                Attr(rect, "height", 10);
                Attr(rect, "rx", 2);
                Attr(rect, "fill", color);
                _svg.appendChild(rect);

                var text = El("text");
                Attr(text, "x", x + 16);
                Attr(text, "y", y + 9);
                Attr(text, "font-size", "11");
                Attr(text, "fill", Theme.Default.Foreground);
                text.textContent = label;
                _svg.appendChild(text);

                y += 18;
            }
        }

        private string TooltipFor(int i, double value, double total)
        {
            var label   = i < _labels.Length ? _labels[i] : "#" + (i + 1);
            var percent = (value / total * 100).ToString("0.#");
            return $"{label}: {_valueFormatter(value)} ({percent}%)";
        }

        protected override string BuildAriaLabel()
        {
            if (!string.IsNullOrEmpty(_title)) return _title;

            var values = _series.Count > 0 ? _series[0].Values : new double[0];
            if (values.Length == 0) return (_donutRatio > 0 ? "Donut" : "Pie") + " chart with no data";

            var total = values.Sum();
            var parts = values.Select((v, i) =>
            {
                var label = i < _labels.Length ? _labels[i] : "#" + (i + 1);
                var pct   = total > 0 ? (v / total * 100).ToString("0.#") : "0";
                return $"{label} {pct}%";
            });

            return (_donutRatio > 0 ? "Donut" : "Pie") + " chart. " + string.Join(", ", parts);
        }
    }
}
