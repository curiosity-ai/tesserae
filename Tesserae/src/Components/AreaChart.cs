using System;
using System.Collections.Generic;
using static Transpose.Core.dom;

namespace Tesserae
{
    /// <summary>
    /// A lightweight, dependency-free SVG area chart. Renders each series as a gradient-filled region under its
    /// line (matching <see cref="Sparkline"/>'s fill style), with hoverable points, gridlines and category labels.
    /// </summary>
    [Transpose.Name("tss.AreaChart")]
    public sealed class AreaChart : CartesianChartBase<AreaChart>
    {
        private bool _showPoints = true;

        /// <summary>
        /// Initializes a new instance of the <see cref="AreaChart"/> class.
        /// </summary>
        public AreaChart() : base() { }

        /// <summary>Shows or hides the per-point markers.</summary>
        public AreaChart Points(bool show = true) { _showPoints = show; QueueRender(); return this; }

        // Areas read most naturally filled down to a zero baseline.
        protected override bool DefaultIncludeZeroBaseline => true;

        // The line runs between the points, so a segment crossing the window sizes the value axis on its own.
        protected override bool ConnectsPointsAcrossX => true;

        protected override void RenderSeries()
        {
            var baselineY = PixelY(Math.Max(0, _minValue));

            for (int s = 0; s < _series.Count; s++)
            {
                var series = _series[s];
                var color  = ColorFor(s, series);

                if (series.Values.Length == 0) continue;

                var gradientId = "tss-area-grad-" + Guid.NewGuid().ToString("N").Substring(0, 8);

                var defs           = El("defs");
                var linearGradient = El("linearGradient");
                Attr(linearGradient, "id", gradientId);
                Attr(linearGradient, "x1", "0%");
                Attr(linearGradient, "y1", "0%");
                Attr(linearGradient, "x2", "0%");
                Attr(linearGradient, "y2", "100%");

                var stop1 = El("stop");
                Attr(stop1, "offset", "0%");
                Attr(stop1, "stop-color", color);
                Attr(stop1, "stop-opacity", series.FillOpacity.ToString("0.###"));
                var stop2 = El("stop");
                Attr(stop2, "offset", "100%");
                Attr(stop2, "stop-color", color);
                Attr(stop2, "stop-opacity", (series.FillOpacity * 0.045).ToString("0.####"));
                linearGradient.appendChild(stop1);
                linearGradient.appendChild(stop2);
                defs.appendChild(linearGradient);
                _plotSurface.appendChild(defs);

                foreach (var run in SeriesRuns(series))
                {
                    if (run.Count == 0) continue;

                    var segments = BuildSegments(series, run);
                    var linePath = "M " + segments;

                    var firstX = SeriesPointX(series, run[0]).ToString("0.###");
                    var lastX  = SeriesPointX(series, run[run.Count - 1]).ToString("0.###");
                    var baseY  = baselineY.ToString("0.###");

                    var polygon = El("path");
                    Attr(polygon, "fill", "url(#" + gradientId + ")");
                    Attr(polygon, "stroke", "none");
                    Attr(polygon, "d", $"M {firstX} {baseY} L {segments} L {lastX} {baseY} Z");
                    _plotSurface.appendChild(polygon);

                    var line = El("path");
                    Attr(line, "fill", "none");
                    Attr(line, "stroke", color);
                    Attr(line, "stroke-width", series.LineWidth);
                    Attr(line, "stroke-linejoin", "round");
                    Attr(line, "stroke-linecap", "round");
                    Attr(line, "d", linePath);
                    _plotSurface.appendChild(line);
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

        // "x y L x y L …" — usable both as the line's own path and as the top edge of the filled polygon.
        private string BuildSegments(ChartSeries series, List<int> run)
        {
            var parts = new List<string>();

            for (int p = 0; p < run.Count; p++)
            {
                var i = run[p];
                parts.Add(SeriesPointX(series, i).ToString("0.###") + " " + PixelY(series.Values[i]).ToString("0.###"));
            }

            return string.Join(" L ", parts);
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
