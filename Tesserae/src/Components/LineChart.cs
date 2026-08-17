using System.Collections.Generic;
using static Transpose.Core.dom;

namespace Tesserae
{
    /// <summary>
    /// A lightweight, dependency-free SVG line chart. Plots one or more series as connected lines with
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

                foreach (var run in SeriesRuns(series))
                {
                    if (run.Count == 0) continue;

                    var path = El("path");
                    Attr(path, "fill", "none");
                    Attr(path, "stroke", color);
                    Attr(path, "stroke-width", series.LineWidth);
                    Attr(path, "stroke-linejoin", "round");
                    Attr(path, "stroke-linecap", "round");
                    Attr(path, "d", BuildPath(series, run));
                    _plotSurface.appendChild(path);
                }

                if (_showPoints && series.Values.Length <= MaxMarkersPerSeries)
                {
                    for (int i = 0; i < series.Values.Length; i++)
                    {
                        if (double.IsNaN(series.Values[i])) continue;

                        var circle = El("circle");
                        Attr(circle, "cx", SeriesPointX(series, i));
                        Attr(circle, "cy", PixelY(series.Values[i]));
                        Attr(circle, "r", 3);
                        Attr(circle, "fill", color);
                        AttachPointTooltip(circle, TooltipFor(series, i));
                        _plotSurface.appendChild(circle);
                    }
                }
            }
        }

        private string BuildPath(ChartSeries series, List<int> run)
        {
            var d = "";

            for (int p = 0; p < run.Count; p++)
            {
                var i = run[p];
                var x = SeriesPointX(series, i).ToString("0.###");
                var y = PixelY(series.Values[i]).ToString("0.###");
                d += (p == 0 ? "M " : " L ") + x + " " + y;
            }

            return d;
        }

        private string TooltipFor(ChartSeries series, int i)
        {
            var label = _continuousX
                ? FormatXValue(XOf(series, i))
                : (i < _categories.Length ? _categories[i] : "#" + (i + 1));

            var name = string.IsNullOrEmpty(series.Name) ? "" : series.Name + " — ";
            return $"{name}{label}: {_valueFormatter(series.Values[i])}";
        }
    }
}
