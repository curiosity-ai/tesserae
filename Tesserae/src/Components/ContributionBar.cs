using System;
using System.Collections.Generic;
using System.Linq;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// Controls how a collapsable <see cref="ContributionBar"/> reveals its detailed, multi-colored breakdown.
    /// </summary>
    [H5.Name("tss.ContributionBarReveal")]
    public enum ContributionBarReveal
    {
        /// <summary>
        /// Shows an <see cref="UIcons.AngleUp"/> / <see cref="UIcons.AngleDown"/> toggle that expands the
        /// multi-colored segments and legend inline, in place.
        /// </summary>
        Expand,

        /// <summary>
        /// Shows an <see cref="UIcons.Info"/> icon; hovering it reveals the multi-colored breakdown and legend
        /// in a popover (Tippy) instead of expanding inline.
        /// </summary>
        Tooltip
    }

    /// <summary>
    /// A single stacked horizontal bar that shows how a set of weighted components add up to a total
    /// (for example, how each signal contributes to a similarity score). Each segment is sized
    /// proportionally to its value and gets a distinct color, and an optional legend lists every
    /// segment with its label and numeric value.
    /// </summary>
    /// <remarks>
    /// Add segments with <see cref="Add(string, double, string)"/> (or many at once with
    /// <see cref="AddRange{T}"/>). Segments render in the order they were added unless
    /// <see cref="SortByValue(bool)"/> is set, which orders them by magnitude. By default the bar fills
    /// entirely (the largest extent equals the sum of all segments); call <see cref="Max(double)"/> to pin
    /// the full-width value, or <see cref="FillTo(double)"/> to fill only a fraction of the track, so a
    /// remainder track is shown when the segments do not reach it.
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
        private readonly HTMLElement   _barRow;
        private readonly HTMLElement   _toggle;
        private readonly HTMLElement   _legend;

        private double  _max             = double.NaN;
        private double  _fillFraction    = double.NaN;
        private bool?   _sortDescending  = null;
        private bool    _showLegend      = true;
        private bool    _showValues      = true;
        private int     _decimals        = 2;
        private string  _barHeight       = "10px";
        private bool                  _collapsable             = false;
        private bool                  _collapsed               = true;
        private string                _collapsedColor          = Theme.Primary.Background;
        private ContributionBarReveal _reveal                  = ContributionBarReveal.Expand;
        private bool                  _tooltipCleanupRegistered = false;

        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        public ContributionBar()
        {
            _bar         = Div(_("tss-contribution-bar"));
            _toggle      = Button(_("tss-contribution-toggle", role: "button", ariaLabel: "Toggle contribution breakdown"));
            _barRow      = Div(_("tss-contribution-row"), _bar);
            _legend      = Div(_("tss-contribution-legend"));
            InnerElement = Div(_("tss-contribution"), _barRow, _legend);

            _toggle.addEventListener("click", _ =>
            {
                if (_reveal != ContributionBarReveal.Expand) return;
                _collapsed = !_collapsed;
                Refresh();
            });
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
        /// Adds many segments in one call by projecting each item to a label and a value (and optionally a
        /// color). This lets the bar be built directly from a list — for example the per-candidate score
        /// decomposition a ranking / similarity engine returns — without a manual
        /// <see cref="Add(string, double, string)"/> loop. The bar is laid out once, after every segment is added.
        /// </summary>
        /// <param name="items">The source items. Null is treated as empty.</param>
        /// <param name="label">Projects an item to its segment label.</param>
        /// <param name="value">Projects an item to its segment value (its share of the total).</param>
        /// <param name="color">Optional projection of an item to an explicit color; when null the default palette is used.</param>
        public ContributionBar AddRange<T>(IEnumerable<T> items, Func<T, string> label, Func<T, double> value, Func<T, string> color = null)
        {
            if (items != null)
            {
                foreach (var item in items)
                {
                    _segments.Add(new Segment { Label = label(item), Value = value(item), Color = color != null ? color(item) : null });
                }
            }
            Refresh();
            return this;
        }

        /// <summary>
        /// Pins the value that corresponds to a full-width bar. When the segments sum to less than this
        /// value the remaining space is rendered as an empty track. Defaults to the sum of all segments.
        /// </summary>
        public ContributionBar Max(double max)
        {
            _max          = max;
            _fillFraction = double.NaN;
            Refresh();
            return this;
        }

        /// <summary>
        /// Fills the bar to a fraction of its width (0 = empty, 1 = full) while keeping the segments'
        /// relative proportions. The filled portion is split across the segments by their share of the
        /// total and the rest of the track is left empty — so you can dial how "full" the bar reads without
        /// changing any segment value. For example, two equal segments with <c>FillTo(0.5)</c> each take a
        /// quarter of the bar and the remaining half stays empty.
        /// </summary>
        /// <param name="fraction">The fraction of the track to fill, clamped to the range 0..1.</param>
        public ContributionBar FillTo(double fraction)
        {
            _fillFraction = fraction;
            _max          = double.NaN;
            Refresh();
            return this;
        }

        /// <summary>
        /// Orders the segments (and the legend) by value rather than the order they were added. Defaults to
        /// largest-first; pass <c>false</c> to order smallest-first.
        /// </summary>
        public ContributionBar SortByValue(bool descending = true)
        {
            _sortDescending = descending;
            Refresh();
            return this;
        }

        /// <summary>
        /// Makes the bar collapsable: a tiny toggle button (showing an
        /// <see cref="UIcons.AngleUp"/> / <see cref="UIcons.AngleDown"/> icon) is shown next to the bar.
        /// When collapsed, all of the individually colored segments merge into a single bar painted with
        /// <paramref name="color"/> (defaulting to <see cref="Theme.Primary.Background"/>) and the legend is
        /// hidden. Expanding restores the multi-colored segments and the legend.
        /// </summary>
        /// <param name="color">The color used for the single, collapsed bar. Defaults to <see cref="Theme.Primary.Background"/>.</param>
        /// <param name="initiallyCollapsed">Whether the bar starts collapsed. Defaults to <c>true</c>. Ignored when <paramref name="reveal"/> is <see cref="ContributionBarReveal.Tooltip"/>.</param>
        /// <param name="reveal">How the detailed breakdown is revealed. <see cref="ContributionBarReveal.Expand"/> (the default) expands it inline; <see cref="ContributionBarReveal.Tooltip"/> shows an info icon that reveals it in a popover.</param>
        public ContributionBar Collapsable(string color = Theme.Primary.Background, bool initiallyCollapsed = true, ContributionBarReveal reveal = ContributionBarReveal.Expand)
        {
            _collapsable    = true;
            _collapsedColor = string.IsNullOrEmpty(color) ? Theme.Primary.Background : color;
            _collapsed      = initiallyCollapsed;
            _reveal         = reveal;
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

        private string ColorFor(Segment segment, int position)
        {
            if (!string.IsNullOrEmpty(segment.Color)) return segment.Color;
            return DefaultPalette[position % DefaultPalette.Length];
        }

        private string FormatValue(double value) => H5.Script.Write<string>("{0}.toFixed({1})", value, _decimals);

        private double Total()
        {
            double sum = 0;
            foreach (var s in _segments) sum += s.Value > 0 ? s.Value : 0;
            return sum;
        }

        private List<Segment> OrderedSegments()
        {
            if (_sortDescending == null) return _segments;

            return _sortDescending.Value
                ? _segments.OrderByDescending(s => s.Value).ToList()
                : _segments.OrderBy(s => s.Value).ToList();
        }

        private void UpdateToggle()
        {
            if (_collapsable)
            {
                if (_toggle.parentElement == null) _barRow.appendChild(_toggle);

                if (_reveal == ContributionBarReveal.Tooltip)
                {
                    _toggle.appendChild(I(UIcons.Info));
                    _toggle.setAttribute("title", "Show contribution breakdown");
                    RefreshTooltip();
                }
                else
                {
                    var icon = _collapsed ? UIcons.AngleDown : UIcons.AngleUp;
                    _toggle.appendChild(I(icon));
                    _toggle.setAttribute("title", _collapsed ? "Expand contribution breakdown" : "Collapse contribution breakdown");
                }
            }
            else if (_toggle.parentElement != null)
            {
                _toggle.parentElement.removeChild(_toggle);
            }
        }

        private double EffectiveMax()
        {
            if (!double.IsNaN(_fillFraction))
            {
                var fraction = _fillFraction <= 0 ? 1 : (_fillFraction > 1 ? 1 : _fillFraction);
                var total    = Total();
                return total <= 0 ? 1 : total / fraction;
            }

            var max = double.IsNaN(_max) ? Total() : _max;
            return max <= 0 ? 1 : max;
        }

        private void RenderSegmentsInto(HTMLElement bar, List<Segment> segments, double max)
        {
            for (int i = 0; i < segments.Count; i++)
            {
                var segment = segments[i];
                if (segment.Value <= 0) continue;

                var width    = (segment.Value / max) * 100.0;
                var color    = ColorFor(segment, i);
                var fragment = Div(_("tss-contribution-segment", title: $"{segment.Label}: {FormatValue(segment.Value)}"));
                fragment.style.width           = FormatValue(width) + "%";
                fragment.style.backgroundColor = color;
                bar.appendChild(fragment);
            }
        }

        private void RenderLegendInto(HTMLElement legend, List<Segment> segments)
        {
            for (int i = 0; i < segments.Count; i++)
            {
                var segment = segments[i];
                var dot     = Span(_("tss-contribution-legend-dot"));
                dot.style.backgroundColor = ColorFor(segment, i);

                var item = Div(_("tss-contribution-legend-item"),
                    dot,
                    Span(_("tss-contribution-legend-label", text: segment.Label)));

                if (_showValues)
                {
                    item.appendChild(Span(_("tss-contribution-legend-value", text: FormatValue(segment.Value))));
                }

                legend.appendChild(item);
            }
        }

        private HTMLElement BuildBreakdownPopover()
        {
            var ordered = OrderedSegments();

            var bar = Div(_("tss-contribution-bar"));
            bar.style.height = _barHeight;
            RenderSegmentsInto(bar, ordered, EffectiveMax());

            var legend = Div(_("tss-contribution-legend"));
            RenderLegendInto(legend, ordered);

            return Div(_("tss-contribution tss-contribution-popover"), bar, legend);
        }

        private void RefreshTooltip()
        {
            // Destroy any previously-attached popover so re-renders (further .Add calls) don't stack instances.
            if (_toggle.HasOwnProperty("_tippy"))
            {
                H5.Script.Write("{0}._tippy.destroy();", _toggle);
            }

            var content = BuildBreakdownPopover();
            document.body.appendChild(content);

            // Stack the popover into the application z-index lane so it always sits above any visible Layer.
            if (!int.TryParse(Layers.AboveCurrent(), out var zIndex)) zIndex = 9999;

            H5.Script.Write("tippy({0}, { content: {1}, interactive: {2}, placement: {3}, delay: [{4},{5}], appendTo: {6}, maxWidth: {7}, hideOnClick: {8}, arrow: {9}, zIndex: {10} });",
                _toggle, content, true, "bottom", 100, 0, document.body, 360, true, true, zIndex);

            if (!_tooltipCleanupRegistered)
            {
                _tooltipCleanupRegistered = true;
                this.WhenRemoved(() =>
                {
                    if (_toggle.HasOwnProperty("_tippy"))
                    {
                        H5.Script.Write("{0}._tippy.destroy();", _toggle);
                    }
                });
            }
        }

        private void Refresh()
        {
            ClearChildren(_bar);
            ClearChildren(_legend);
            ClearChildren(_toggle);

            _bar.style.height = _barHeight;

            var max     = EffectiveMax();
            var ordered = OrderedSegments();

            UpdateToggle();

            // In tooltip mode the bar always shows the single, collapsed color; the breakdown lives in the popover.
            var isCollapsed = _collapsable && (_collapsed || _reveal == ContributionBarReveal.Tooltip);

            if (isCollapsed)
            {
                var width    = (Total() / max) * 100.0;
                var fragment = Div(_("tss-contribution-segment", title: FormatValue(Total())));
                fragment.style.width           = FormatValue(width) + "%";
                fragment.style.backgroundColor = _collapsedColor;
                _bar.appendChild(fragment);
            }
            else
            {
                RenderSegmentsInto(_bar, ordered, max);
            }

            if (!_showLegend || isCollapsed) return;

            RenderLegendInto(_legend, ordered);
        }

        /// <summary>
        /// Renders the component's root HTML element.
        /// </summary>
        public override HTMLElement Render() => InnerElement;
    }
}
