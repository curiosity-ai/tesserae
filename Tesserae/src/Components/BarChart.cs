using System;
using static Transpose.Core.dom;

namespace Tesserae
{
    /// <summary>
    /// A lightweight, dependency-free SVG bar chart. Renders grouped bars per category (one bar per series),
    /// with a zero baseline, value gridlines, category labels, hover tooltips and a theme-aware palette.
    /// </summary>
    [Transpose.Name("tss.BarChart")]
    public sealed class BarChart : CartesianChartBase<BarChart>
    {
        private double _cornerRadius = 2;

        /// <summary>
        /// Initializes a new instance of the <see cref="BarChart"/> class.
        /// </summary>
        public BarChart() : base() { }

        /// <summary>Sets the corner radius of the bars.</summary>
        public BarChart Rounded(double radius = 2) { _cornerRadius = radius; QueueRender(); return this; }

        // Bars are read against a zero baseline.
        protected override bool IncludeZeroBaseline => true;

        protected override void RenderSeries()
        {
            var baselineY  = PixelY(0);
            var slotWidth  = _plotWidth / _pointCount;
            var groupInset = slotWidth * 0.15;
            var groupWidth = slotWidth - groupInset * 2;
            var barWidth   = _series.Count > 0 ? groupWidth / _series.Count : groupWidth;

            for (int i = 0; i < _pointCount; i++)
            {
                var slotLeft = _plotLeft + slotWidth * i + groupInset;

                for (int s = 0; s < _series.Count; s++)
                {
                    var series = _series[s];
                    if (i >= series.Values.Length) continue;

                    var value = series.Values[i];
                    var color = ColorFor(s, series);

                    var yValue = PixelY(value);
                    var x      = slotLeft + barWidth * s;
                    var top    = Math.Min(yValue, baselineY);
                    var height = Math.Abs(baselineY - yValue);

                    var rect = El("rect");
                    Attr(rect, "x", x);
                    Attr(rect, "y", top);
                    Attr(rect, "width", Math.Max(0, barWidth - 1));
                    Attr(rect, "height", height);
                    Attr(rect, "rx", _cornerRadius);
                    Attr(rect, "fill", color);
                    AttachPointTooltip(rect, TooltipFor(series, i));
                    _svg.appendChild(rect);
                }
            }
        }

        private string TooltipFor(ChartSeries series, int i)
        {
            var label = i < _categories.Length ? _categories[i] : "#" + (i + 1);
            var name  = string.IsNullOrEmpty(series.Name) ? "" : series.Name + " — ";
            return $"{name}{label}: {_valueFormatter(series.Values[i])}";
        }
    }
}
