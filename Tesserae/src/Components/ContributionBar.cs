using System;
using System.Collections.Generic;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// A single stacked horizontal bar that shows how a set of weighted components add up to a total
    /// (for example, how each signal contributes to a similarity score). Each segment is sized
    /// proportionally to its value and gets a distinct color, and an optional legend lists every
    /// segment with its label and numeric value.
    /// </summary>
    /// <remarks>
    /// Add segments with <see cref="Add(string, double, string)"/>. By default the bar fills entirely
    /// (the largest extent equals the sum of all segments); call <see cref="Max(double)"/> to pin the
    /// full-width value so a remainder track is shown when the segments do not reach it.
    /// </remarks>
    [H5.Name("tss.ContributionBar")]
    public sealed class ContributionBar : ComponentBase<ContributionBar, HTMLElement>
    {
        private sealed class Segment
        {
            public string Label;
            public double Value;
            public string Color;
        }

        /// <summary>
        /// The default color palette used for segments that do not specify an explicit color, cycled in order.
        /// </summary>
        public static readonly string[] DefaultPalette =
        {
            Theme.Colors.Blue500,
            Theme.Colors.Teal500,
            Theme.Colors.Green500,
            Theme.Colors.Orange500,
            Theme.Colors.Purple500,
            Theme.Colors.Magenta500,
            Theme.Colors.Red500,
            Theme.Colors.Blue400,
        };

        private readonly List<Segment> _segments = new List<Segment>();
        private readonly HTMLElement   _bar;
        private readonly HTMLElement   _legend;

        private double  _max         = double.NaN;
        private bool    _showLegend  = true;
        private bool    _showValues  = true;
        private int     _decimals    = 2;
        private string  _barHeight   = "10px";

        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        public ContributionBar()
        {
            _bar         = Div(_("tss-contribution-bar"));
            _legend      = Div(_("tss-contribution-legend"));
            InnerElement = Div(_("tss-contribution"), _bar, _legend);
        }

        /// <summary>
        /// Adds a contribution segment. When <paramref name="color"/> is null a color is picked from
        /// <see cref="DefaultPalette"/> based on the segment's position.
        /// </summary>
        public ContributionBar Add(string label, double value, string color = null)
        {
            _segments.Add(new Segment { Label = label, Value = value, Color = color });
            Refresh();
            return this;
        }

        /// <summary>
        /// Pins the value that corresponds to a full-width bar. When the segments sum to less than this
        /// value the remaining space is rendered as an empty track. Defaults to the sum of all segments.
        /// </summary>
        public ContributionBar Max(double max)
        {
            _max = max;
            Refresh();
            return this;
        }

        /// <summary>
        /// Controls whether the legend below the bar is shown. Defaults to <c>true</c>.
        /// </summary>
        public ContributionBar ShowLegend(bool show = true)
        {
            _showLegend = show;
            Refresh();
            return this;
        }

        /// <summary>
        /// Hides the legend below the bar.
        /// </summary>
        public ContributionBar HideLegend() => ShowLegend(false);

        /// <summary>
        /// Controls whether the numeric value of each segment is shown in the legend. Defaults to <c>true</c>.
        /// </summary>
        public ContributionBar ShowValues(bool show = true)
        {
            _showValues = show;
            Refresh();
            return this;
        }

        /// <summary>
        /// Sets the number of decimal places used when formatting segment values. Defaults to 2.
        /// </summary>
        public ContributionBar Decimals(int decimals)
        {
            _decimals = decimals < 0 ? 0 : decimals;
            Refresh();
            return this;
        }

        /// <summary>
        /// Sets the thickness (height) of the bar. Defaults to <c>10px</c>.
        /// </summary>
        public ContributionBar Thickness(UnitSize size)
        {
            _barHeight = size.ToString();
            _bar.style.height = _barHeight;
            return this;
        }

        private string ColorFor(int index)
        {
            var segment = _segments[index];
            if (!string.IsNullOrEmpty(segment.Color)) return segment.Color;
            return DefaultPalette[index % DefaultPalette.Length];
        }

        private string FormatValue(double value) => H5.Script.Write<string>("{0}.toFixed({1})", value, _decimals);

        private double Total()
        {
            double sum = 0;
            foreach (var s in _segments) sum += s.Value > 0 ? s.Value : 0;
            return sum;
        }

        private void Refresh()
        {
            ClearChildren(_bar);
            ClearChildren(_legend);

            _bar.style.height = _barHeight;

            var max = double.IsNaN(_max) ? Total() : _max;
            if (max <= 0) max = 1;

            for (int i = 0; i < _segments.Count; i++)
            {
                var segment = _segments[i];
                if (segment.Value <= 0) continue;

                var width    = (segment.Value / max) * 100.0;
                var color    = ColorFor(i);
                var fragment = Div(_("tss-contribution-segment", title: $"{segment.Label}: {FormatValue(segment.Value)}"));
                fragment.style.width           = FormatValue(width) + "%";
                fragment.style.backgroundColor = color;
                _bar.appendChild(fragment);
            }

            if (!_showLegend) return;

            for (int i = 0; i < _segments.Count; i++)
            {
                var segment = _segments[i];
                var dot     = Span(_("tss-contribution-legend-dot"));
                dot.style.backgroundColor = ColorFor(i);

                var item = Div(_("tss-contribution-legend-item"),
                    dot,
                    Span(_("tss-contribution-legend-label", text: segment.Label)));

                if (_showValues)
                {
                    item.appendChild(Span(_("tss-contribution-legend-value", text: FormatValue(segment.Value))));
                }

                _legend.appendChild(item);
            }
        }

        /// <summary>
        /// Renders the component's root HTML element.
        /// </summary>
        public override HTMLElement Render() => InnerElement;
    }
}
