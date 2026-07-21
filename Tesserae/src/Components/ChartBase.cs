using System;
using System.Collections.Generic;
using System.Linq;
using Transpose;
using static Transpose.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// A single named data series for a chart, with an optional explicit color (falling back to the chart palette).
    /// </summary>
    [Transpose.Name("tss.ChartSeries")]
    public sealed class ChartSeries
    {
        /// <summary>The series display name, used in the legend, tooltips and accessibility summary.</summary>
        public string Name { get; set; }

        /// <summary>The series values, one per category/point.</summary>
        public double[] Values { get; set; }

        /// <summary>An optional explicit CSS color; when null the chart assigns a palette color by index.</summary>
        public string Color { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChartSeries"/> class.
        /// </summary>
        public ChartSeries(string name, double[] values, string color = null)
        {
            Name   = name;
            Values = values ?? new double[0];
            Color  = color;
        }
    }

    /// <summary>
    /// Shared base for Tesserae's lightweight, dependency-free SVG charts. Handles the responsive SVG surface
    /// (sized 1:1 to its container via a <see cref="ResizeObserver"/>), the series/palette model, observable-driven
    /// re-rendering, theme colors, tooltips (reusing tippy) and the role="img" accessibility summary.
    /// Mirrors <see cref="Sparkline"/>'s SVG rendering style.
    /// </summary>
    [Transpose.Name("tss.ChartBase")]
    public abstract class ChartBase<T> : IComponent where T : ChartBase<T>
    {
        /// <summary>The SVG namespace used for every chart element.</summary>
        protected const string SvgNs = "http://www.w3.org/2000/svg";

        /// <summary>The default theme-aware palette (CSS variables that adapt to light/dark mode).</summary>
        protected static readonly string[] DefaultPalette =
        {
            Theme.Colors.Blue600,
            Theme.Colors.Green600,
            Theme.Colors.Orange600,
            Theme.Colors.Purple600,
            Theme.Colors.Red600,
            Theme.Colors.Teal600,
            Theme.Colors.Yellow600,
            Theme.Colors.Neutral600
        };

        /// <summary>The root container element.</summary>
        protected readonly HTMLElement _container;

        /// <summary>The SVG surface that the chart draws into.</summary>
        protected readonly Element _svg;

        /// <summary>The chart's series.</summary>
        protected readonly List<ChartSeries> _series = new List<ChartSeries>();

        private readonly ResizeObserver _resizeObserver;
        private readonly List<IDisposable> _subscriptions = new List<IDisposable>();

        /// <summary>The active color palette.</summary>
        protected string[] _palette = DefaultPalette;

        /// <summary>Whether to render per-element tippy + native &lt;title&gt; tooltips.</summary>
        protected bool _showTooltips = true;

        /// <summary>Whether to render the legend.</summary>
        protected bool _showLegend = false;

        /// <summary>An optional caption used as the accessibility summary; falls back to a generated description.</summary>
        protected string _title;

        /// <summary>Optional formatter for values shown in tooltips / axis labels.</summary>
        protected Func<double, string> _valueFormatter = v => v.ToString("0.##");

        private bool _renderQueued;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChartBase{T}"/> class.
        /// </summary>
        protected ChartBase(double minWidth = 120, double minHeight = 80)
        {
            _container = Div(_("tss-chart"));
            _container.style.width     = "100%";
            _container.style.height    = "100%";
            _container.style.minWidth  = minWidth + "px";
            _container.style.minHeight = minHeight + "px";
            _container.setAttribute("role", "img");

            _svg = document.createElementNS(SvgNs, "svg");
            _svg.setAttribute("width", "100%");
            _svg.setAttribute("height", "100%");
            _svg.As<HTMLElement>().style.display = "block";
            _container.appendChild(_svg);

            _resizeObserver = new ResizeObserver((entries, obs) => QueueRender());
            _resizeObserver.observe(_container);
            DomObserver.WhenMounted(_container, QueueRender);
            DomObserver.WhenRemoved(_container, () =>
            {
                _resizeObserver.unobserve(_container);
                foreach (var s in _subscriptions) s.Dispose();
                _subscriptions.Clear();
            });
        }

        private T Self => (T)this;

        /// <summary>Replaces the chart's series with a single unnamed series of plain values.</summary>
        public T Data(double[] values) => Series(new ChartSeries(null, values));

        /// <summary>Replaces the chart's series.</summary>
        public T Series(params ChartSeries[] series)
        {
            _series.Clear();
            if (series != null) _series.AddRange(series);
            QueueRender();
            return Self;
        }

        /// <summary>Appends a single named series of plain values.</summary>
        public T Series(string name, double[] values, string color = null)
        {
            _series.Add(new ChartSeries(name, values, color));
            QueueRender();
            return Self;
        }

        /// <summary>
        /// Binds a single series to an observable sequence of values: the chart re-renders whenever the
        /// observable changes. The subscription is released when the chart leaves the DOM.
        /// </summary>
        public T Series(IObservable<double[]> values, string name = null, string color = null)
        {
            var series = new ChartSeries(name, values?.Value ?? new double[0], color);
            _series.Add(series);

            if (values != null)
            {
                _subscriptions.Add(values.Subscribe(v =>
                {
                    series.Values = v ?? new double[0];
                    QueueRender();
                }, fireImmediately: false));
            }

            QueueRender();
            return Self;
        }

        /// <summary>Binds the chart to an observable list of series, re-rendering on every change.</summary>
        public T Series(IObservable<ChartSeries[]> series)
        {
            if (series != null)
            {
                _subscriptions.Add(series.Subscribe(s =>
                {
                    _series.Clear();
                    if (s != null) _series.AddRange(s);
                    QueueRender();
                }, fireImmediately: true));
            }
            return Self;
        }

        /// <summary>Sets the series color palette (used for series without an explicit color).</summary>
        public T Colors(params string[] palette)
        {
            if (palette != null && palette.Length > 0) _palette = palette;
            QueueRender();
            return Self;
        }

        /// <summary>Enables or disables per-element tooltips.</summary>
        public T Tooltips(bool show = true) { _showTooltips = show; QueueRender(); return Self; }

        /// <summary>Enables or disables the legend.</summary>
        public T Legend(bool show = true) { _showLegend = show; QueueRender(); return Self; }

        /// <summary>Sets an accessibility caption / summary for the chart.</summary>
        public T Title(string title) { _title = title; QueueRender(); return Self; }

        /// <summary>Sets the formatter used for values in tooltips and labels.</summary>
        public T FormatValues(Func<double, string> formatter) { if (formatter != null) _valueFormatter = formatter; QueueRender(); return Self; }

        /// <summary>Returns the color for the series at the given index (explicit color or palette by index).</summary>
        protected string ColorFor(int index, ChartSeries series) => series.Color ?? _palette[index % _palette.Length];

        /// <summary>Creates an SVG element in the SVG namespace.</summary>
        protected Element El(string name) => document.createElementNS(SvgNs, name);

        /// <summary>Sets an attribute on an SVG element (double values are invariant-formatted).</summary>
        protected static void Attr(Element el, string name, double value) => el.setAttribute(name, value.ToString("0.###"));

        /// <summary>Sets an attribute on an SVG element.</summary>
        protected static void Attr(Element el, string name, string value) => el.setAttribute(name, value);

        /// <summary>
        /// Attaches a hover tooltip to an SVG element: a native &lt;title&gt; child (for accessibility / no-JS fallback)
        /// and, when enabled, a tippy popover reusing the bundled tippy.js.
        /// </summary>
        protected void AttachPointTooltip(Element el, string content)
        {
            if (string.IsNullOrEmpty(content)) return;

            var title = El("title");
            title.textContent = content;
            el.appendChild(title);

            if (_showTooltips)
            {
                Script.Write("tippy({0}, { content: {1}, allowHTML: true, delay: [100, 0], appendTo: document.body });", el, content);
            }
        }

        /// <summary>Removes every child of the SVG surface.</summary>
        protected void ClearSvg()
        {
            while (_svg.firstChild != null) _svg.removeChild(_svg.firstChild);
        }

        /// <summary>Schedules a render on the next animation frame, coalescing bursts of changes.</summary>
        protected void QueueRender()
        {
            if (_renderQueued) return;
            _renderQueued = true;
            window.requestAnimationFrame(_ =>
            {
                _renderQueued = false;
                Render(force: true);
            });
        }

        private void Render(bool force)
        {
            var rect = _container.getBoundingClientRect().As<DOMRect>();
            var w    = rect.width;
            var h    = rect.height;

            if (w < 1 || h < 1) return; // not laid out yet; a later resize/mount will trigger us

            _svg.setAttribute("viewBox", $"0 0 {w.ToString("0.##")} {h.ToString("0.##")}");
            _svg.setAttribute("preserveAspectRatio", "none");

            ClearSvg();
            RenderChart(w, h);
            _container.setAttribute("aria-label", BuildAriaLabel());
        }

        /// <summary>Draws the chart into the SVG surface at the given pixel dimensions.</summary>
        protected abstract void RenderChart(double width, double height);

        /// <summary>Builds the accessibility summary describing the chart's data.</summary>
        protected virtual string BuildAriaLabel()
        {
            if (!string.IsNullOrEmpty(_title)) return _title;

            var kind = GetType().Name.Replace("Chart", " chart");

            if (_series.Count == 0) return kind + " with no data";

            var parts = _series.Select(s =>
            {
                var name = string.IsNullOrEmpty(s.Name) ? "series" : s.Name;
                if (s.Values.Length == 0) return name + " (empty)";
                return $"{name}: {s.Values.Length} points, from {_valueFormatter(s.Values.Min())} to {_valueFormatter(s.Values.Max())}";
            });

            return $"{kind}. " + string.Join("; ", parts);
        }

        /// <summary>Renders the legend swatches into the supplied container element, returning its height.</summary>
        protected double RenderLegend(double width, double y)
        {
            if (!_showLegend || _series.Count == 0) return 0;

            const double swatch = 10;
            const double gap    = 6;
            const double itemGap = 16;
            double x = 8;

            for (int i = 0; i < _series.Count; i++)
            {
                var color = ColorFor(i, _series[i]);
                var name  = string.IsNullOrEmpty(_series[i].Name) ? $"Series {i + 1}" : _series[i].Name;

                var rect = El("rect");
                Attr(rect, "x", x);
                Attr(rect, "y", y);
                Attr(rect, "width", swatch);
                Attr(rect, "height", swatch);
                Attr(rect, "rx", 2);
                Attr(rect, "fill", color);
                _svg.appendChild(rect);

                var text = El("text");
                Attr(text, "x", x + swatch + gap);
                Attr(text, "y", y + swatch);
                Attr(text, "font-size", "11");
                Attr(text, "fill", Theme.Default.Foreground);
                text.textContent = name;
                _svg.appendChild(text);

                x += swatch + gap + name.Length * 6.5 + itemGap;
            }

            return 22;
        }

        /// <summary>
        /// Renders the component's root HTML element.
        /// </summary>
        public HTMLElement Render() => _container;
    }

    /// <summary>
    /// Base for cartesian (x/y) charts — line, bar and area. Draws the value (Y) gridlines and labels, the
    /// category (X) axis labels, the axis lines, and computes the plot rectangle and value-to-pixel scale that
    /// subclasses use to plot their series.
    /// </summary>
    [Transpose.Name("tss.CartesianChartBase")]
    public abstract class CartesianChartBase<T> : ChartBase<T> where T : CartesianChartBase<T>
    {
        /// <summary>Category labels along the X axis.</summary>
        protected string[] _categories = new string[0];

        /// <summary>Whether to draw horizontal gridlines.</summary>
        protected bool _showGrid = true;

        /// <summary>Whether to draw the axes and their labels.</summary>
        protected bool _showAxes = true;

        /// <summary>Optional X axis title.</summary>
        protected string _xAxisTitle;

        /// <summary>Optional Y axis title.</summary>
        protected string _yAxisTitle;

        // Plot rectangle + value scale, populated before RenderSeries is called.
        /// <summary>Left edge of the plot area, in pixels.</summary>
        protected double _plotLeft;
        /// <summary>Top edge of the plot area, in pixels.</summary>
        protected double _plotTop;
        /// <summary>Width of the plot area, in pixels.</summary>
        protected double _plotWidth;
        /// <summary>Height of the plot area, in pixels.</summary>
        protected double _plotHeight;
        /// <summary>The minimum value mapped to the bottom of the plot.</summary>
        protected double _minValue;
        /// <summary>The maximum value mapped to the top of the plot.</summary>
        protected double _maxValue;
        /// <summary>The number of categories/points along the X axis.</summary>
        protected int _pointCount;

        /// <summary>
        /// Initializes a new instance of the <see cref="CartesianChartBase{T}"/> class.
        /// </summary>
        protected CartesianChartBase(double minWidth = 200, double minHeight = 120) : base(minWidth, minHeight) { }

        private T Self => (T)this;

        /// <summary>Sets the category labels along the X axis.</summary>
        public T XAxis(params string[] categories) { _categories = categories ?? new string[0]; QueueRender(); return Self; }

        /// <summary>Sets the X axis title.</summary>
        public T XAxisTitle(string title) { _xAxisTitle = title; QueueRender(); return Self; }

        /// <summary>Sets the Y axis title.</summary>
        public T YAxisTitle(string title) { _yAxisTitle = title; QueueRender(); return Self; }

        /// <summary>Shows or hides the horizontal gridlines.</summary>
        public T Grid(bool show = true) { _showGrid = show; QueueRender(); return Self; }

        /// <summary>Shows or hides the axes and their labels.</summary>
        public T Axes(bool show = true) { _showAxes = show; QueueRender(); return Self; }

        /// <summary>When true the value axis always includes zero as a baseline (bar/area); when false it fits the data.</summary>
        protected virtual bool IncludeZeroBaseline => true;

        /// <summary>Maps a value to its pixel Y coordinate within the plot area.</summary>
        protected double PixelY(double value)
        {
            var range = _maxValue - _minValue;
            if (range <= 0) range = 1;
            return _plotTop + _plotHeight - ((value - _minValue) / range) * _plotHeight;
        }

        /// <inheritdoc />
        protected override void RenderChart(double width, double height)
        {
            _pointCount = _series.Count == 0 ? 0 : _series.Max(s => s.Values.Length);

            var legendHeight = RenderLegend(width, 6);

            double marginTop    = 8 + legendHeight;
            double marginRight  = 12;
            double marginBottom = _showAxes ? (_categories.Length > 0 || !string.IsNullOrEmpty(_xAxisTitle) ? 34 : 18) : 6;
            double marginLeft   = _showAxes ? 44 : 6;

            _plotLeft   = marginLeft;
            _plotTop    = marginTop;
            _plotWidth  = Math.Max(1, width - marginLeft - marginRight);
            _plotHeight = Math.Max(1, height - marginTop - marginBottom);

            ComputeValueRange();

            if (_showGrid || _showAxes) DrawGridAndAxes();

            if (_pointCount > 0) RenderSeries();
        }

        private void ComputeValueRange()
        {
            var all = _series.SelectMany(s => s.Values).ToArray();

            if (all.Length == 0)
            {
                _minValue = 0;
                _maxValue = 1;
                return;
            }

            var dataMin = all.Min();
            var dataMax = all.Max();

            if (IncludeZeroBaseline)
            {
                dataMin = Math.Min(0, dataMin);
                dataMax = Math.Max(0, dataMax);
            }

            if (dataMin == dataMax)
            {
                // Avoid a zero range so a flat series still renders sensibly.
                dataMax = dataMin + 1;
                if (!IncludeZeroBaseline) dataMin -= 1;
            }
            else if (!IncludeZeroBaseline)
            {
                var pad = (dataMax - dataMin) * 0.08;
                dataMin -= pad;
                dataMax += pad;
            }

            _minValue = dataMin;
            _maxValue = dataMax;
        }

        private void DrawGridAndAxes()
        {
            const int ticks = 4;
            var gridColor = Theme.Colors.Neutral500Alpha;
            var textColor = Theme.Default.Foreground;

            for (int i = 0; i <= ticks; i++)
            {
                var value = _minValue + (_maxValue - _minValue) * i / ticks;
                var y     = PixelY(value);

                if (_showGrid)
                {
                    var line = El("line");
                    Attr(line, "x1", _plotLeft);
                    Attr(line, "y1", y);
                    Attr(line, "x2", _plotLeft + _plotWidth);
                    Attr(line, "y2", y);
                    Attr(line, "stroke", gridColor);
                    Attr(line, "stroke-width", 1);
                    _svg.appendChild(line);
                }

                if (_showAxes)
                {
                    var label = El("text");
                    Attr(label, "x", _plotLeft - 6);
                    Attr(label, "y", y + 3);
                    Attr(label, "text-anchor", "end");
                    Attr(label, "font-size", "10");
                    Attr(label, "fill", textColor);
                    label.textContent = _valueFormatter(value);
                    _svg.appendChild(label);
                }
            }

            if (!_showAxes) return;

            // X category labels
            if (_categories.Length > 0 && _pointCount > 0)
            {
                for (int i = 0; i < _categories.Length && i < _pointCount; i++)
                {
                    var x = CategoryCenterX(i);
                    var label = El("text");
                    Attr(label, "x", x);
                    Attr(label, "y", _plotTop + _plotHeight + 14);
                    Attr(label, "text-anchor", "middle");
                    Attr(label, "font-size", "10");
                    Attr(label, "fill", textColor);
                    label.textContent = _categories[i];
                    _svg.appendChild(label);
                }
            }

            // Axis lines
            var axis = El("line");
            Attr(axis, "x1", _plotLeft);
            Attr(axis, "y1", _plotTop + _plotHeight);
            Attr(axis, "x2", _plotLeft + _plotWidth);
            Attr(axis, "y2", _plotTop + _plotHeight);
            Attr(axis, "stroke", textColor);
            Attr(axis, "stroke-width", 1);
            _svg.appendChild(axis);
        }

        /// <summary>Returns the pixel X coordinate of the center of category slot <paramref name="index"/>.</summary>
        protected double CategoryCenterX(int index)
        {
            if (_pointCount <= 1) return _plotLeft + _plotWidth / 2;
            // Bar-style centered slots; line/area override via PointX for edge-to-edge layout.
            var slot = _plotWidth / _pointCount;
            return _plotLeft + slot * index + slot / 2;
        }

        /// <summary>Returns the pixel X coordinate for point <paramref name="index"/> of a line/area series.</summary>
        protected double PointX(int index)
        {
            if (_pointCount <= 1) return _plotLeft + _plotWidth / 2;
            return _plotLeft + _plotWidth * index / (_pointCount - 1);
        }

        /// <summary>Plots the series into the computed plot rectangle.</summary>
        protected abstract void RenderSeries();
    }
}
