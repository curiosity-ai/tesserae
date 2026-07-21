using System.Linq;
using static Transpose.Core.dom;

namespace Tesserae
{
    /// <summary>
    /// A lightweight, dependency-free SVG line chart. Plots one or more series as connected polylines with
    /// hoverable points, value gridlines, category labels and a theme-aware palette. Fluent and responsive.
    /// </summary>
    [Transpose.Name("tss.LineChart")]
    public sealed class LineChart : CartesianChartBase<LineChart>
    {
        private bool _showPoints = true;

        /// <summary>
        /// Initializes a new instance of the <see cref="LineChart"/> class.
        /// </summary>
        public LineChart() : base() { }

        /// <summary>Shows or hides the per-point markers.</summary>
        public LineChart Points(bool show = true) { _showPoints = show; QueueRender(); return this; }

        // Line charts fit the data range rather than forcing a zero baseline.
        protected override bool IncludeZeroBaseline => false;

        protected override void RenderSeries()
        {
            for (int s = 0; s < _series.Count; s++)
            {
                var series = _series[s];
                var color  = ColorFor(s, series);

                if (series.Values.Length == 0) continue;

                var points = string.Join(" ", series.Values.Select((v, i) => $"{PointX(i).ToString("0.###")},{PixelY(v).ToString("0.###")}"));

                var polyline = El("polyline");
                Attr(polyline, "fill", "none");
                Attr(polyline, "stroke", color);
                Attr(polyline, "stroke-width", 2);
                Attr(polyline, "stroke-linejoin", "round");
                Attr(polyline, "stroke-linecap", "round");
                Attr(polyline, "points", points);
                _svg.appendChild(polyline);

                if (_showPoints)
                {
                    for (int i = 0; i < series.Values.Length; i++)
                    {
                        var circle = El("circle");
                        Attr(circle, "cx", PointX(i));
                        Attr(circle, "cy", PixelY(series.Values[i]));
                        Attr(circle, "r", 3);
                        Attr(circle, "fill", color);
                        AttachPointTooltip(circle, TooltipFor(series, i));
                        _svg.appendChild(circle);
                    }
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
