using System;
using System.Linq;
using static H5.Core.dom;

namespace Tesserae
{
    /// <summary>
    /// A lightweight, dependency-free SVG area chart. Renders each series as a gradient-filled region under its
    /// line (matching <see cref="Sparkline"/>'s fill style), with hoverable points, gridlines and category labels.
    /// </summary>
    [H5.Name("tss.AreaChart")]
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
        protected override bool IncludeZeroBaseline => true;

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
                Attr(stop1, "stop-opacity", "0.45");
                var stop2 = El("stop");
                Attr(stop2, "offset", "100%");
                Attr(stop2, "stop-color", color);
                Attr(stop2, "stop-opacity", "0.02");
                linearGradient.appendChild(stop1);
                linearGradient.appendChild(stop2);
                defs.appendChild(linearGradient);
                _svg.appendChild(defs);

                var linePoints = string.Join(" ", series.Values.Select((v, i) => $"{PointX(i).ToString("0.###")},{PixelY(v).ToString("0.###")}"));

                var firstX = PointX(0).ToString("0.###");
                var lastX  = PointX(series.Values.Length - 1).ToString("0.###");

                var polygon = El("polygon");
                Attr(polygon, "fill", "url(#" + gradientId + ")");
                Attr(polygon, "points", $"{firstX},{baselineY.ToString("0.###")} {linePoints} {lastX},{baselineY.ToString("0.###")}");
                _svg.appendChild(polygon);

                var polyline = El("polyline");
                Attr(polyline, "fill", "none");
                Attr(polyline, "stroke", color);
                Attr(polyline, "stroke-width", 2);
                Attr(polyline, "stroke-linejoin", "round");
                Attr(polyline, "stroke-linecap", "round");
                Attr(polyline, "points", linePoints);
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
