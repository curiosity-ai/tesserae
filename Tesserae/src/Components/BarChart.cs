using System;
using System.Collections.Generic;
using static Transpose.Core.dom;

namespace Tesserae
{
    /// <summary>
    /// A lightweight, dependency-free SVG bar chart. Renders grouped bars per category (one bar per series) or,
    /// with <see cref="Stacked"/>, one stacked column per category, with a zero baseline, value gridlines,
    /// category labels, hover tooltips and a theme-aware palette.
    /// </summary>
    [Transpose.Name("tss.BarChart")]
    public sealed class BarChart : CartesianChartBase<BarChart>
    {
        private double _cornerRadius = 2;
        private bool   _stacked;

        /// <summary>
        /// Initializes a new instance of the <see cref="BarChart"/> class.
        /// </summary>
        public BarChart() : base() { }

        /// <summary>Sets the corner radius of the bars.</summary>
        public BarChart Rounded(double radius = 2) { _cornerRadius = radius; QueueRender(); return this; }

        /// <summary>
        /// Stacks the series into one column per category instead of drawing them side by side. Positive and
        /// negative values stack away from the baseline independently.
        /// </summary>
        public BarChart Stacked(bool stacked = true) { _stacked = stacked; QueueRender(); return this; }

        // Bars are read against a zero baseline.
        protected override bool IncludeZeroBaseline => true;

        /// <inheritdoc />
        protected override void CollectRangeValues(List<double> into)
        {
            if (!_stacked)
            {
                base.CollectRangeValues(into);
                return;
            }

            // A stacked column reaches the sum of its parts, so the axis has to fit the totals, not the values.
            for (int i = 0; i < _pointCount; i++)
            {
                double positive = 0;
                double negative = 0;

                for (int s = 0; s < _series.Count; s++)
                {
                    var values = _series[s].Values;
                    if (i >= values.Length) continue;

                    var v = values[i];
                    if (double.IsNaN(v)) continue;

                    if (v >= 0) positive += v;
                    else negative += v;
                }

                into.Add(positive);
                into.Add(negative);
            }
        }

        protected override void RenderSeries()
        {
            if (_stacked) RenderStacked();
            else RenderGrouped();
        }

        private void RenderGrouped()
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
                    if (double.IsNaN(value)) continue;

                    var yValue = PixelY(value);
                    var x      = slotLeft + barWidth * s;
                    var top    = Math.Min(yValue, baselineY);
                    var height = Math.Abs(baselineY - yValue);

                    DrawBar(x, top, Math.Max(0, barWidth - 1), height, ColorFor(s, series), TooltipFor(series, i));
                }
            }
        }

        private void RenderStacked()
        {
            var baselineY  = PixelY(0);
            var slotWidth  = _plotWidth / _pointCount;
            var groupInset = slotWidth * 0.15;
            var barWidth   = slotWidth - groupInset * 2;

            for (int i = 0; i < _pointCount; i++)
            {
                var x = _plotLeft + slotWidth * i + groupInset;

                double positiveTop = 0;
                double negativeTop = 0;

                for (int s = 0; s < _series.Count; s++)
                {
                    var series = _series[s];
                    if (i >= series.Values.Length) continue;

                    var value = series.Values[i];
                    if (double.IsNaN(value) || value == 0) continue;

                    double top;
                    double height;

                    if (value >= 0)
                    {
                        var from = positiveTop;
                        positiveTop += value;
                        top    = PixelY(positiveTop);
                        height = Math.Abs(PixelY(from) - PixelY(positiveTop));
                    }
                    else
                    {
                        var from = negativeTop;
                        negativeTop += value;
                        top    = PixelY(from);
                        height = Math.Abs(PixelY(from) - PixelY(negativeTop));
                    }

                    DrawBar(x, top, Math.Max(0, barWidth - 1), height, ColorFor(s, series), TooltipFor(series, i));
                }
            }
        }

        private void DrawBar(double x, double y, double width, double height, string color, string tooltip)
        {
            var rect = El("rect");
            Attr(rect, "x", x);
            Attr(rect, "y", y);
            Attr(rect, "width", width);
            Attr(rect, "height", height);
            Attr(rect, "rx", _cornerRadius);
            Attr(rect, "fill", color);
            AttachPointTooltip(rect, tooltip);
            _plotSurface.appendChild(rect);
        }

        private string TooltipFor(ChartSeries series, int i)
        {
            var label = i < _categories.Length ? _categories[i] : "#" + (i + 1);
            var name  = string.IsNullOrEmpty(series.Name) ? "" : series.Name + " — ";
            return $"{name}{label}: {_valueFormatter(series.Values[i])}";
        }
    }
}
