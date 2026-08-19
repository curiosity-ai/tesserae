using System;
using System.Collections.Generic;
using System.Linq;
using Transpose;
using static Transpose.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// Where a chart draws its legend.
    /// </summary>
    [Transpose.Name("tss.ChartLegendPosition")]
    public enum ChartLegendPosition
    {
        /// <summary>A horizontal row of swatches above the plot.</summary>
        Top,
        /// <summary>A horizontal row of swatches below the plot.</summary>
        Bottom,
        /// <summary>A vertical column of swatches to the left of the plot.</summary>
        Left,
        /// <summary>A vertical column of swatches to the right of the plot.</summary>
        Right
    }

    /// <summary>
    /// The visible X range of a cartesian chart, reported when the user zooms or pans.
    /// </summary>
    [Transpose.Name("tss.ChartRange")]
    public sealed class ChartRange
    {
        /// <summary>The lowest visible X value.</summary>
        public double Min { get; set; }

        /// <summary>The highest visible X value.</summary>
        public double Max { get; set; }

        /// <summary>True when the chart is back to fitting its data instead of showing an explicit range.</summary>
        public bool IsAutoRange { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChartRange"/> class.
        /// </summary>
        public ChartRange(double min, double max, bool isAutoRange)
        {
            Min         = min;
            Max         = max;
            IsAutoRange = isAutoRange;
        }
    }

    /// <summary>
    /// A single named data series for a chart, with an optional explicit color (falling back to the chart palette).
    /// </summary>
    [Transpose.Name("tss.ChartSeries")]
    public sealed class ChartSeries
    {
        /// <summary>The series display name, used in the legend, tooltips and accessibility summary.</summary>
        public string Name { get; set; }

        /// <summary>The series values, one per category/point. <see cref="double.NaN"/> marks a missing sample.</summary>
        public double[] Values { get; set; }

        /// <summary>An optional explicit CSS color; when null the chart assigns a palette color by index.</summary>
        public string Color { get; set; }

        /// <summary>
        /// Optional X positions for this series' values, one per entry in <see cref="Values"/>. Setting these on
        /// any series switches the chart to a continuous X scale, which lets each series carry its own X positions
        /// (irregular sampling, differing time windows, differing point counts) instead of sharing one category list.
        /// </summary>
        public double[] XValues { get; set; }

        /// <summary>The stroke width of this series' line, in pixels (line and area charts).</summary>
        public double LineWidth { get; set; } = 2;

        /// <summary>The opacity of this series' area fill at its peak, fading to near-transparent at the baseline (area charts).</summary>
        public double FillOpacity { get; set; } = 0.45;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChartSeries"/> class.
        /// </summary>
        public ChartSeries(string name, double[] values, string color = null)
        {
            Name   = name;
            Values = values ?? new double[0];
            Color  = color;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChartSeries"/> class with explicit X positions,
        /// putting the chart on a continuous X scale.
        /// </summary>
        public ChartSeries(string name, double[] xValues, double[] values, string color = null)
        {
            Name    = name;
            Values  = values ?? new double[0];
            Color   = color;
            XValues = xValues;
        }
    }

    /// <summary>
    /// Shared base for Tesserae's lightweight, dependency-free SVG charts. Handles the responsive SVG surface
    /// (sized 1:1 to its container via a <see cref="ResizeObserver"/>), the series/palette model, observable-driven
    /// re-rendering, theme colors, tooltips (reusing tippy), PNG export and the role="img" accessibility summary.
    /// Mirrors <see cref="Sparkline"/>'s SVG rendering style.
    /// </summary>
    [Transpose.Name("tss.ChartBase")]
    public abstract class ChartBase<T> : IComponent where T : ChartBase<T>
    {
        /// <summary>The SVG namespace used for every chart element.</summary>
        protected const string SvgNs = "http://www.w3.org/2000/svg";

        /// <summary>
        /// Above this many points in a series, per-point markers (and their tooltips) are suppressed: one DOM
        /// node plus one tooltip per sample stops being readable long before it stops being expensive. Use the
        /// spikeline readout instead of markers for dense series.
        /// </summary>
        protected const int MaxMarkersPerSeries = 300;

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

        /// <summary>Where the legend is drawn.</summary>
        protected ChartLegendPosition _legendPosition = ChartLegendPosition.Top;

        /// <summary>Whether a line/area series draws straight across a <see cref="double.NaN"/> gap instead of breaking.</summary>
        protected bool _connectGaps = true;

        /// <summary>An optional caption used as the accessibility summary; falls back to a generated description.</summary>
        protected string _title;

        /// <summary>Optional formatter for values shown in tooltips / axis labels.</summary>
        protected Func<double, string> _valueFormatter = FormatValueCompact;

        /// <summary>
        /// The default value format: large magnitudes are abbreviated with an SI prefix, because an axis of byte
        /// counts or token totals is unreadable at full length. Small values are left alone. Override with
        /// <see cref="FormatValues"/>.
        /// </summary>
        protected static string FormatValueCompact(double value)
        {
            var abs = Math.Abs(value);

            if (abs >= 1e12) return (value / 1e12).ToString("0.##") + "T";
            if (abs >= 1e9) return (value / 1e9).ToString("0.##") + "G";
            if (abs >= 1e6) return (value / 1e6).ToString("0.##") + "M";
            if (abs >= 1e4) return (value / 1e3).ToString("0.##") + "k";

            return value.ToString("0.##");
        }

        /// <summary>The element subclasses draw their series into; clipped to the plot rectangle on cartesian charts.</summary>
        protected Element _plotSurface;

        /// <summary>Space the legend claimed at the top of the surface, in pixels.</summary>
        protected double _legendInsetTop;

        /// <summary>Space the legend claimed at the bottom of the surface, in pixels.</summary>
        protected double _legendInsetBottom;

        /// <summary>Space the legend claimed on the left of the surface, in pixels.</summary>
        protected double _legendInsetLeft;

        /// <summary>Space the legend claimed on the right of the surface, in pixels.</summary>
        protected double _legendInsetRight;

        private bool _renderQueued;
        private Button _exportButton;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChartBase{T}"/> class.
        /// </summary>
        protected ChartBase(double minWidth = 120, double minHeight = 80)
        {
            _container = Div(Att("tss-chart"));
            _container.style.width     = "100%";
            _container.style.height    = "100%";
            _container.style.minWidth  = minWidth + "px";
            _container.style.minHeight = minHeight + "px";
            _container.style.position  = "relative";
            _container.setAttribute("role", "img");

            _svg = document.createElementNS(SvgNs, "svg");
            _svg.setAttribute("width", "100%");
            _svg.setAttribute("height", "100%");
            _svg.As<HTMLElement>().style.display = "block";
            _container.appendChild(_svg);

            _plotSurface = _svg;

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

        /// <summary>Sets which edge of the chart the legend is drawn on (and enables it).</summary>
        public T Legend(ChartLegendPosition position)
        {
            _legendPosition = position;
            _showLegend     = true;
            QueueRender();
            return Self;
        }

        /// <summary>
        /// When true (the default) a line or area series draws straight across a <see cref="double.NaN"/> value;
        /// when false the line breaks, leaving the gap visible.
        /// </summary>
        public T ConnectGaps(bool connect = true) { _connectGaps = connect; QueueRender(); return Self; }

        /// <summary>Sets an accessibility caption / summary for the chart.</summary>
        public T Title(string title) { _title = title; QueueRender(); return Self; }

        /// <summary>Sets the formatter used for values in tooltips and labels.</summary>
        public T FormatValues(Func<double, string> formatter) { if (formatter != null) _valueFormatter = formatter; QueueRender(); return Self; }

        /// <summary>
        /// Shows a small download button in the chart's top-right corner (revealed on hover) that saves the
        /// chart as a PNG.
        /// </summary>
        public T ExportButton(bool show = true, string fileName = null)
        {
            if (!show)
            {
                if (_exportButton is object)
                {
                    _container.removeChild(_exportButton.Render());
                    _exportButton = null;
                }
                return Self;
            }

            if (_exportButton is object) return Self;

            //Bound through a local so the click handler keeps this chart as its receiver.
            var chart = this;

            _exportButton = Button().Compact().SetIcon(UIcons.Download).Tooltip("Save as PNG").OnClick(() => chart.ExportPng(fileName));

            var el = _exportButton.Render();
            el.style.position   = "absolute";
            el.style.top        = "2px";
            el.style.right      = "2px";
            el.style.opacity    = "0";
            el.style.transition = "opacity 0.15s";

            _container.addEventListener("mouseenter", (Action<Event>)(_ => el.style.opacity = "1"));
            _container.addEventListener("mouseleave", (Action<Event>)(_ => el.style.opacity = "0"));

            _container.appendChild(el);
            return Self;
        }

        /// <summary>
        /// Saves the chart as a PNG. CSS-variable colors are resolved to concrete values first, so the exported
        /// image matches the theme the chart is currently rendered in.
        /// </summary>
        public void ExportPng(string fileName = null)
        {
            var name = string.IsNullOrEmpty(fileName)
                ? (string.IsNullOrEmpty(_title) ? "chart" : _title)
                : fileName;

            if (!name.EndsWith(".png")) name = name + ".png";

            var rect       = _container.getBoundingClientRect().As<DOMRect>();
            var width      = Math.Max(1, rect.width);
            var height     = Math.Max(1, rect.height);
            var background = Color.EvalVar(Theme.Default.Background);

            //Through a local: the template below runs inside an IIFE, so a substituted `this._svg` would
            //resolve `this` to undefined.
            var surface = _svg;

            //Serialising a live SVG needs the var(--tss-*) colors flattened and the result rasterised through an
            //Image, neither of which has a typed binding here.
            Script.Write(@"(function () {
                var svg = {0}.cloneNode(true);
                var computed = window.getComputedStyle(document.body);
                function flatten(el) {
                    ['fill', 'stroke', 'stop-color'].forEach(function (attribute) {
                        var value = el.getAttribute(attribute);
                        if (value && value.indexOf('var(') === 0) {
                            el.setAttribute(attribute, computed.getPropertyValue(value.substring(4, value.length - 1)).trim());
                        }
                    });
                }
                flatten(svg);
                var all = svg.querySelectorAll('*');
                for (var i = 0; i < all.length; i++) flatten(all[i]);
                svg.setAttribute('xmlns', 'http://www.w3.org/2000/svg');
                svg.setAttribute('width', {1});
                svg.setAttribute('height', {2});
                var serialized = new XMLSerializer().serializeToString(svg);
                var image = new Image();
                image.onload = function () {
                    var scale = window.devicePixelRatio > 1 ? 2 : 1;
                    var canvas = document.createElement('canvas');
                    canvas.width = {1} * scale;
                    canvas.height = {2} * scale;
                    var ctx = canvas.getContext('2d');
                    ctx.scale(scale, scale);
                    ctx.fillStyle = {3};
                    ctx.fillRect(0, 0, {1}, {2});
                    ctx.drawImage(image, 0, 0);
                    canvas.toBlob(function (blob) {
                        var url = URL.createObjectURL(blob);
                        var link = document.createElement('a');
                        link.href = url;
                        link.download = {4};
                        document.body.appendChild(link);
                        link.click();
                        document.body.removeChild(link);
                        window.setTimeout(function () { URL.revokeObjectURL(url); }, 1000);
                    });
                };
                image.src = 'data:image/svg+xml;charset=utf-8,' + encodeURIComponent(serialized);
            })();", surface, width, height, background, name);
        }

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
        /// <remarks>
        /// The tippy instance is created the first time the point is hovered rather than at render time.
        /// A chart is drawn point by point, so building one instance per point up front cost a tippy
        /// object, its listeners and a full-document z-index scan for every value in the series — all of
        /// it before the user has pointed at anything. One delegated listener on the SVG surface replaces
        /// them; the created instance keeps tippy's own trigger from then on, so hover behaviour and the
        /// show delay are unchanged.
        /// </remarks>
        protected void AttachPointTooltip(Element el, string content)
        {
            if (string.IsNullOrEmpty(content)) return;

            var title = El("title");
            title.textContent = content;
            el.appendChild(title);

            if (_showTooltips)
            {
                el[TooltipContentProperty] = content;
                EnsureTooltipDelegate();
            }
        }

        private const string TooltipContentProperty = "_tssChartTooltip";

        private bool _tooltipDelegateAttached;

        private bool _hasCreatedTooltip;

        private void EnsureTooltipDelegate()
        {
            if (_tooltipDelegateAttached) return;
            _tooltipDelegateAttached = true;

            _svg.addEventListener("mouseover", (Action<Event>)(e =>
            {
                var target = e.As<MouseEvent>().target.As<Element>();

                while (target is object && target != _svg)
                {
                    if (target.HasOwnProperty(TooltipContentProperty)) break;
                    target = target.parentElement;
                }

                if (target is null || target == _svg || !target.HasOwnProperty(TooltipContentProperty)) return;
                if (target.HasOwnProperty("_tippy")) return;

                _hasCreatedTooltip = true;

                var content = target[TooltipContentProperty].As<string>();

                //Into the application z-index lane, like every other tippy here: a chart in a Modal would
                //otherwise hand its tooltips tippy's fixed 9999 and draw them behind the Layer above it.
                if (!int.TryParse(Layers.AboveCurrent(), out var zIndex)) zIndex = 9999;

                Script.Write("tippy({0}, { content: {1}, allowHTML: true, delay: [100, 0], appendTo: document.body, zIndex: {2} });", target, content, zIndex);

                //The mouseenter that would normally open it has already gone by, so replay it against the
                //instance we just built — tippy then applies its own show delay, as it would on any hover.
                Script.Write("{0}.dispatchEvent(new MouseEvent('mouseenter'));", target);
            }));
        }

        /// <summary>Removes every child of the SVG surface.</summary>
        protected void ClearSvg()
        {
            //Tippy keeps its instances alive independently of the DOM, so a chart that re-renders (on
            //resize, or whenever its data changes) would otherwise strand one popover per hovered point.
            //Charts re-render on every resize, so only pay for the scan once something was hovered.
            if (_hasCreatedTooltip)
            {
                Script.Write(@"{0}.querySelectorAll('*').forEach(function (e) { if (e._tippy) e._tippy.destroy(); });", _svg);
                _hasCreatedTooltip = false;
            }

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
            _plotSurface = _svg;
            ResetLegendInsets();
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
                var name   = string.IsNullOrEmpty(s.Name) ? "series" : s.Name;
                var actual = s.Values.Where(v => !double.IsNaN(v)).ToArray();
                if (actual.Length == 0) return name + " (empty)";
                return $"{name}: {actual.Length} points, from {_valueFormatter(actual.Min())} to {_valueFormatter(actual.Max())}";
            });

            return $"{kind}. " + string.Join("; ", parts);
        }

        /// <summary>Clears the space reserved for the legend, before a fresh render measures it again.</summary>
        protected void ResetLegendInsets()
        {
            _legendInsetTop    = 0;
            _legendInsetBottom = 0;
            _legendInsetLeft   = 0;
            _legendInsetRight  = 0;
        }

        /// <summary>Renders the legend for the chart's series, recording the space it consumed in the legend insets.</summary>
        protected void RenderLegend(double width, double height)
        {
            if (_series.Count == 0) return;

            var labels = new string[_series.Count];
            var colors = new string[_series.Count];

            for (int i = 0; i < _series.Count; i++)
            {
                labels[i] = string.IsNullOrEmpty(_series[i].Name) ? $"Series {i + 1}" : _series[i].Name;
                colors[i] = ColorFor(i, _series[i]);
            }

            RenderLegend(labels, colors, width, height);
        }

        /// <summary>
        /// Renders a legend of the given labels and colors on the configured edge, recording the space it
        /// consumed in <see cref="_legendInsetTop"/> and friends so the caller can lay the plot out around it.
        /// </summary>
        protected void RenderLegend(string[] labels, string[] colors, double width, double height)
        {
            if (!_showLegend || labels == null || labels.Length == 0) return;

            const double swatch  = 10;
            const double gap     = 6;
            const double itemGap = 16;
            const double rowStep = 18;

            if (_legendPosition == ChartLegendPosition.Left || _legendPosition == ChartLegendPosition.Right)
            {
                var columnWidth = labels.Max(l => l.Length) * 6.5 + swatch + gap + 16;
                columnWidth     = Math.Min(columnWidth, Math.Max(40, width * 0.4));

                var x = _legendPosition == ChartLegendPosition.Left ? 8 : width - columnWidth + 8;
                var y = Math.Max(8, height / 2 - labels.Length * rowStep / 2);

                for (int i = 0; i < labels.Length; i++)
                {
                    DrawLegendEntry(labels[i], colors[i], x, y, swatch, gap);
                    y += rowStep;
                }

                if (_legendPosition == ChartLegendPosition.Left) _legendInsetLeft = columnWidth;
                else _legendInsetRight = columnWidth;

                return;
            }

            var rowY = _legendPosition == ChartLegendPosition.Top ? 6 : height - 16;
            double cursor = 8;

            for (int i = 0; i < labels.Length; i++)
            {
                DrawLegendEntry(labels[i], colors[i], cursor, rowY, swatch, gap);
                cursor += swatch + gap + labels[i].Length * 6.5 + itemGap;
            }

            if (_legendPosition == ChartLegendPosition.Top) _legendInsetTop = 22;
            else _legendInsetBottom = 22;
        }

        private void DrawLegendEntry(string label, string color, double x, double y, double swatch, double gap)
        {
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
            text.textContent = label;
            _svg.appendChild(text);
        }

        /// <summary>
        /// Renders the component's root HTML element.
        /// </summary>
        public HTMLElement Render() => _container;
    }

    /// <summary>
    /// Base for cartesian (x/y) charts — line, bar and area. Draws the value (Y) gridlines and labels, the
    /// category (X) axis labels, the axis lines, and computes the plot rectangle and value-to-pixel scale that
    /// subclasses use to plot their series. Also owns the continuous X scale, tick thinning, zoom/pan and the
    /// spikeline readout.
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

        /// <summary>True when the chart plots against a continuous X scale rather than evenly spaced categories.</summary>
        protected bool _continuousX;

        /// <summary>The lowest visible X value on a continuous scale.</summary>
        protected double _viewXMin;

        /// <summary>The highest visible X value on a continuous scale.</summary>
        protected double _viewXMax;

        private readonly List<double> _valueTicks = new List<double>();
        private double[] _sharedX;
        private Func<double, string> _xFormatter;
        private bool _xIsTime;
        private string _xTimeFormat;
        private double _lastTimeStep;
        private int _maxXTicks;
        private bool _hasExplicitRange;
        private double _rangeMin;
        private double _rangeMax;
        private bool? _zeroBaseline;
        private bool _zoomable;
        private bool _showSpikes;
        private bool _interactionsAttached;
        private Action<ChartRange> _onRangeChanged;
        private Element _overlay;
        private double _dataXMin;
        private double _dataXMax;
        private bool _hasDataExtent;
        private double _minXSpan;
        private double _maxXSpan;
        private Action _endPan;
        private readonly string _clipId = "tss-chart-clip-" + Guid.NewGuid().ToString("N").Substring(0, 8);

        /// <summary>
        /// Initializes a new instance of the <see cref="CartesianChartBase{T}"/> class.
        /// </summary>
        protected CartesianChartBase(double minWidth = 200, double minHeight = 120) : base(minWidth, minHeight) { }

        private T Self => (T)this;

        /// <summary>Sets the category labels along the X axis.</summary>
        public T XAxis(params string[] categories) { _categories = categories ?? new string[0]; QueueRender(); return Self; }

        /// <summary>
        /// Sets shared X positions for every series, putting the chart on a continuous X scale so points sit at
        /// their real distance apart instead of being evenly spaced. Per-series positions
        /// (<see cref="ChartSeries.XValues"/>) win over these.
        /// </summary>
        public T XValues(double[] values) { _sharedX = values; QueueRender(); return Self; }

        /// <summary>Sets the formatter used for continuous X axis tick labels (and the spikeline readout).</summary>
        public T FormatXAxis(Func<double, string> formatter) { _xFormatter = formatter; QueueRender(); return Self; }

        /// <summary>
        /// Formats continuous X values as local times, treating them as Unix timestamps in seconds. With no
        /// format the labels adapt to the visible span — seconds when zoomed into a minute, dates when showing
        /// months — and the ticks land on whole seconds, minutes, hours or days rather than on powers of ten.
        /// </summary>
        public T XAxisTime(string format = null)
        {
            _xIsTime      = true;
            _xTimeFormat  = format;
            QueueRender();
            return Self;
        }

        /// <summary>
        /// Caps how many X axis tick labels are drawn; categories beyond the cap are skipped evenly. Pass 0
        /// (the default) to derive the cap from the available width, which keeps dense axes readable.
        /// </summary>
        public T MaxXTicks(int count) { _maxXTicks = Math.Max(0, count); QueueRender(); return Self; }

        /// <summary>Sets the X axis title.</summary>
        public T XAxisTitle(string title) { _xAxisTitle = title; QueueRender(); return Self; }

        /// <summary>Sets the Y axis title.</summary>
        public T YAxisTitle(string title) { _yAxisTitle = title; QueueRender(); return Self; }

        /// <summary>Shows or hides the horizontal gridlines.</summary>
        public T Grid(bool show = true) { _showGrid = show; QueueRender(); return Self; }

        /// <summary>Shows or hides the axes and their labels.</summary>
        public T Axes(bool show = true) { _showAxes = show; QueueRender(); return Self; }

        /// <summary>
        /// Allows the user to zoom the X axis with the wheel, pan it by dragging, and reset to the full data
        /// range by double-clicking. Only meaningful on a continuous X scale.
        /// </summary>
        public T Zoomable(bool enable = true)
        {
            _zoomable = enable;
            if (enable) EnsureInteractions();
            return Self;
        }

        /// <summary>
        /// Bounds how far the wheel may zoom the X axis, as the smallest and largest visible span. Pass 0 for
        /// either to keep the default, which is a fraction (and a multiple) of the data's own X extent. A chart
        /// that loads its data to match the visible range wants an explicit maximum: the widest span the user
        /// can reach is what decides how much has to be fetched.
        /// </summary>
        public T ZoomLimits(double minSpan, double maxSpan)
        {
            _minXSpan = minSpan > 0 ? minSpan : 0;
            _maxXSpan = maxSpan > 0 ? maxSpan : 0;
            return Self;
        }

        /// <summary>
        /// Draws a vertical spikeline that follows the cursor across the plot, with a readout of the X position
        /// and each series' nearest value. The readable alternative to a marker per sample on a dense series.
        /// </summary>
        public T Spikelines(bool enable = true)
        {
            _showSpikes = enable;
            if (enable) EnsureInteractions();
            return Self;
        }

        /// <summary>
        /// Pins the visible X range on a continuous scale. Setting the range does not raise
        /// <see cref="OnRangeChanged"/>, so charts can be kept in sync with each other without a re-entrancy guard.
        /// </summary>
        public T XRange(double min, double max)
        {
            if (max <= min) return Self;

            _hasExplicitRange = true;
            _rangeMin         = min;
            _rangeMax         = max;

            //Apply to the visible range now rather than waiting for the queued render: TryGetXRange and the
            //range reported to OnRangeChanged both read these, and a zoom that published the pre-zoom range
            //would hand a sibling chart the range it already had.
            _viewXMin = min;
            _viewXMax = max;

            QueueRender();
            return Self;
        }

        /// <summary>Returns the chart to fitting the full X extent of its data, undoing any zoom or pan.</summary>
        public T AutoRangeX()
        {
            _hasExplicitRange = false;
            QueueRender();
            return Self;
        }

        /// <summary>Gets the X range the chart is currently showing. False when the chart has no continuous X data yet.</summary>
        public bool TryGetXRange(out double min, out double max)
        {
            min = _viewXMin;
            max = _viewXMax;
            return _continuousX && _viewXMax > _viewXMin;
        }

        /// <summary>True when the visible X range came from a zoom / pan rather than fitting the data.</summary>
        public bool IsXRangePinned => _hasExplicitRange;

        /// <summary>
        /// Raised when the user zooms, pans or resets the X axis — not when <see cref="XRange"/> is called, so a
        /// handler that pushes the range onto sibling charts cannot loop.
        /// </summary>
        public T OnRangeChanged(Action<ChartRange> handler) { _onRangeChanged = handler; return Self; }

        /// <summary>
        /// Overrides whether the value axis includes zero, regardless of the chart type's default. An area chart
        /// of a metric that hovers far from zero reads better fitted to its data, with the fill still running to
        /// the bottom of the plot.
        /// </summary>
        public T ZeroBaseline(bool include = true) { _zeroBaseline = include; QueueRender(); return Self; }

        /// <summary>The chart type's default: true when the value axis should always include zero as a baseline.</summary>
        protected virtual bool DefaultIncludeZeroBaseline => true;

        /// <summary>Whether the value axis includes zero, honouring <see cref="ZeroBaseline"/> over the type default.</summary>
        protected bool IncludeZeroBaseline => _zeroBaseline ?? DefaultIncludeZeroBaseline;

        /// <summary>Maps a value to its pixel Y coordinate within the plot area.</summary>
        protected double PixelY(double value)
        {
            var range = _maxValue - _minValue;
            if (range <= 0) range = 1;
            return _plotTop + _plotHeight - ((value - _minValue) / range) * _plotHeight;
        }

        /// <summary>Maps a continuous X value to its pixel X coordinate within the plot area.</summary>
        protected double PixelXForValue(double x)
        {
            var range = _viewXMax - _viewXMin;
            if (range <= 0) range = 1;
            return _plotLeft + ((x - _viewXMin) / range) * _plotWidth;
        }

        /// <summary>Returns the X position of point <paramref name="index"/> of a series on a continuous scale.</summary>
        protected double XOf(ChartSeries series, int index)
        {
            if (series.XValues != null) return index < series.XValues.Length ? series.XValues[index] : index;
            if (_sharedX != null) return index < _sharedX.Length ? _sharedX[index] : index;
            return index;
        }

        /// <summary>
        /// Returns the pixel X coordinate for point <paramref name="index"/> of <paramref name="series"/>, using
        /// the continuous scale when the chart has X values and evenly spaced slots otherwise.
        /// </summary>
        protected double SeriesPointX(ChartSeries series, int index) => _continuousX ? PixelXForValue(XOf(series, index)) : PointX(index);

        /// <summary>
        /// Splits a series into runs of consecutive plottable points, breaking at <see cref="double.NaN"/> gaps.
        /// Returns a single run covering every non-gap point when gaps are connected.
        /// </summary>
        protected List<List<int>> SeriesRuns(ChartSeries series)
        {
            var runs    = new List<List<int>>();
            var current = new List<int>();

            for (int i = 0; i < series.Values.Length; i++)
            {
                if (double.IsNaN(series.Values[i]))
                {
                    if (!_connectGaps && current.Count > 0)
                    {
                        runs.Add(current);
                        current = new List<int>();
                    }
                    continue;
                }

                current.Add(i);
            }

            if (current.Count > 0) runs.Add(current);
            return runs;
        }

        /// <inheritdoc />
        protected override void RenderChart(double width, double height)
        {
            _pointCount  = _series.Count == 0 ? 0 : _series.Max(s => s.Values.Length);
            _continuousX = _sharedX != null || _series.Any(s => s.XValues != null);

            ResolveXRange();
            RenderLegend(width, height);

            //Before the plot rectangle, because the value labels decide how much room the axis needs: a byte
            //count formatted to ten characters would otherwise be clipped by a fixed margin.
            ComputeValueRange();

            double marginTop    = 8 + _legendInsetTop;
            double marginRight  = 12 + _legendInsetRight;
            double marginBottom = (_showAxes ? (_categories.Length > 0 || _continuousX || !string.IsNullOrEmpty(_xAxisTitle) ? 34 : 18) : 6) + _legendInsetBottom;
            double marginLeft   = (_showAxes ? MeasureValueAxisWidth(width) : 6) + _legendInsetLeft;

            _plotLeft   = marginLeft;
            _plotTop    = marginTop;
            _plotWidth  = Math.Max(1, width - marginLeft - marginRight);
            _plotHeight = Math.Max(1, height - marginTop - marginBottom);

            if (_showGrid || _showAxes) DrawGridAndAxes();

            if (_pointCount > 0)
            {
                _plotSurface = CreateClippedPlotSurface();
                RenderSeries();
            }

            _overlay = El("g");
            _svg.appendChild(_overlay);

            if (_zoomable || _showSpikes) EnsureInteractions();
        }

        // Zoom clips a series to the plot rectangle; without it a panned line draws over the axis labels.
        private Element CreateClippedPlotSurface()
        {
            var defs = El("defs");
            var clip = El("clipPath");
            Attr(clip, "id", _clipId);

            var rect = El("rect");
            Attr(rect, "x", _plotLeft);
            Attr(rect, "y", _plotTop);
            Attr(rect, "width", _plotWidth);
            Attr(rect, "height", _plotHeight);
            clip.appendChild(rect);
            defs.appendChild(clip);
            _svg.appendChild(defs);

            var group = El("g");
            Attr(group, "clip-path", "url(#" + _clipId + ")");
            _svg.appendChild(group);
            return group;
        }

        private void ResolveXRange()
        {
            if (!_continuousX)
            {
                _hasDataExtent = false;
                _viewXMin      = 0;
                _viewXMax      = Math.Max(1, _pointCount - 1);
                return;
            }

            ComputeDataExtent();

            if (_hasExplicitRange)
            {
                _viewXMin = _rangeMin;
                _viewXMax = _rangeMax;
                return;
            }

            if (!_hasDataExtent)
            {
                _viewXMin = 0;
                _viewXMax = 1;
                return;
            }

            _viewXMin = _dataXMin;
            _viewXMax = _dataXMax;
        }

        // Measured on every render, not only when the view is fitting the data: the zoom limits are expressed
        // relative to the data's own extent, so a wheel gesture over a pinned range needs it too.
        private void ComputeDataExtent()
        {
            var min = double.MaxValue;
            var max = double.MinValue;

            for (int s = 0; s < _series.Count; s++)
            {
                var series = _series[s];
                for (int i = 0; i < series.Values.Length; i++)
                {
                    if (double.IsNaN(series.Values[i])) continue;
                    var x = XOf(series, i);
                    if (x < min) min = x;
                    if (x > max) max = x;
                }
            }

            if (min > max)
            {
                _hasDataExtent = false;
                return;
            }

            if (min == max)
            {
                min -= 1;
                max += 1;
            }

            _hasDataExtent = true;
            _dataXMin      = min;
            _dataXMax      = max;
        }

        /// <summary>
        /// True when the chart draws a continuous line between consecutive points, so a segment crossing the
        /// visible window sizes the value axis even with neither of its ends inside it.
        /// </summary>
        protected virtual bool ConnectsPointsAcrossX => false;

        /// <summary>
        /// Collects the values the Y axis must accommodate. Only what is inside the visible X window counts on a
        /// continuous scale, so zooming rescales the value axis to what is on screen.
        /// </summary>
        protected virtual void CollectRangeValues(List<double> into)
        {
            for (int s = 0; s < _series.Count; s++)
            {
                var series = _series[s];

                if (!_continuousX)
                {
                    for (int i = 0; i < series.Values.Length; i++)
                    {
                        if (!double.IsNaN(series.Values[i])) into.Add(series.Values[i]);
                    }
                    continue;
                }

                CollectVisibleValues(series, into);
            }
        }

        // A zoom can land between two samples, with the line crossing the whole window and not one point inside
        // it. Fitting the axis to the points alone leaves it nothing to fit and blanks the chart, so a segment
        // that straddles an edge contributes the value it has at that edge.
        private void CollectVisibleValues(ChartSeries series, List<double> into)
        {
            var previous = -1;

            for (int i = 0; i < series.Values.Length; i++)
            {
                var value = series.Values[i];

                if (double.IsNaN(value))
                {
                    if (!_connectGaps) previous = -1;
                    continue;
                }

                var x = XOf(series, i);

                if (x >= _viewXMin && x <= _viewXMax) into.Add(value);

                if (previous >= 0 && ConnectsPointsAcrossX) AddSegmentInsideWindow(XOf(series, previous), series.Values[previous], x, value, into);

                previous = i;
            }
        }

        private void AddSegmentInsideWindow(double x0, double v0, double x1, double v1, List<double> into)
        {
            var lo = Math.Min(x0, x1);
            var hi = Math.Max(x0, x1);

            if (hi <= lo || hi < _viewXMin || lo > _viewXMax) return;

            into.Add(ValueAt(x0, v0, x1, v1, Math.Max(lo, _viewXMin)));
            into.Add(ValueAt(x0, v0, x1, v1, Math.Min(hi, _viewXMax)));
        }

        private static double ValueAt(double x0, double v0, double x1, double v1, double x) => v0 + (v1 - v0) * ((x - x0) / (x1 - x0));

        private void ComputeValueRange()
        {
            var all = new List<double>();
            CollectRangeValues(all);

            _valueTicks.Clear();

            if (all.Count == 0)
            {
                _minValue = 0;
                _maxValue = 1;
                _valueTicks.Add(0);
                _valueTicks.Add(1);
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

            var step = NiceStep((dataMax - dataMin) / ValueTicks);

            // A series of whole numbers should not be labelled in halves and quarters.
            if (step > 0 && step < 1 && all.All(v => v == Math.Floor(v))) step = 1;

            if (step <= 0)
            {
                // Degenerate range (infinities, or a step too small to represent) - fall back to the raw bounds.
                _minValue = dataMin;
                _maxValue = dataMax;
                _valueTicks.Add(dataMin);
                _valueTicks.Add(dataMax);
                return;
            }

            // Snap the axis outward onto the step so every gridline lands on a round number.
            _minValue = Math.Floor(dataMin / step) * step;
            _maxValue = Math.Ceiling(dataMax / step) * step;

            for (var tick = _minValue; tick <= _maxValue + step * 0.001; tick += step)
            {
                _valueTicks.Add(tick);
                if (_valueTicks.Count > 100) break; // guard against a pathological step
            }
        }

        // The value-axis equivalent of Plotly's automargin: wide enough for the widest tick label it will draw.
        private double MeasureValueAxisWidth(double width)
        {
            var widest = 0;

            foreach (var tick in _valueTicks)
            {
                var text = _valueFormatter(tick);
                if (text != null && text.Length > widest) widest = text.Length;
            }

            //~6px per character at the 10px label size, plus the 6px gap to the axis and a little slack.
            var needed = widest * 6 + 12 + (string.IsNullOrEmpty(_yAxisTitle) ? 0 : 14);

            return Math.Max(28, Math.Min(needed, width * 0.4));
        }

        private const int ValueTicks = 4;

        private int EffectiveMaxXTicks()
        {
            if (_maxXTicks > 0) return _maxXTicks;
            return Math.Max(2, (int)(_plotWidth / 70));
        }

        /// <summary>Formats a continuous X value using the configured X formatter, falling back to a plain number.</summary>
        protected string FormatXValue(double value)
        {
            if (_xIsTime)
            {
                var format = _xTimeFormat ?? AdaptiveTimeFormat(_viewXMax - _viewXMin);
                return DateTimeOffset.FromUnixTimeSeconds(ClampToUnixSeconds(value)).ToLocalTime().ToString(format);
            }

            return _xFormatter is object ? _xFormatter(value) : value.ToString("0.##");
        }

        // Year 1 and year 9999 as Unix seconds: FromUnixTimeSeconds throws outside them, and an axis label is
        // not worth an exception. A range that ran past either end still draws, pinned to the end it ran past.
        private const double MinUnixSeconds = -62135596800.0;
        private const double MaxUnixSeconds = 253402300799.0;

        private static long ClampToUnixSeconds(double value)
        {
            if (double.IsNaN(value)) return 0;
            return (long)Math.Max(MinUnixSeconds, Math.Min(MaxUnixSeconds, value));
        }

        // A fixed "HH:mm" prints the same label ten times across a one-minute window, and no time at all across
        // a year, so the precision follows the tick step: sub-minute steps need seconds, day-scale steps need
        // the date. Keyed off the step rather than the span so neighbouring ticks can never print the same label.
        private string AdaptiveTimeFormat(double spanSeconds)
        {
            var step = _lastTimeStep > 0 ? _lastTimeStep : spanSeconds / 8;

            if (step < 60) return "HH:mm:ss";
            if (step < 86400) return "HH:mm";
            if (step < 2592000) return "MM-dd";
            return "yyyy-MM";
        }

        private void DrawGridAndAxes()
        {
            var gridColor = Theme.Colors.Neutral500Alpha;
            var textColor = Theme.Default.Foreground;

            foreach (var value in _valueTicks)
            {
                var y = PixelY(value);

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

            if (_continuousX) DrawContinuousXLabels(textColor);
            else DrawCategoryXLabels(textColor);

            DrawAxisTitles(textColor);

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

        private void DrawCategoryXLabels(string textColor)
        {
            if (_categories.Length == 0 || _pointCount == 0) return;

            var count = Math.Min(_categories.Length, _pointCount);
            var step  = Math.Max(1, (int)Math.Ceiling(count / (double)EffectiveMaxXTicks()));

            for (int i = 0; i < count; i += step)
            {
                var label = El("text");
                Attr(label, "x", CategoryCenterX(i));
                Attr(label, "y", _plotTop + _plotHeight + 14);
                Attr(label, "text-anchor", "middle");
                Attr(label, "font-size", "10");
                Attr(label, "fill", textColor);
                label.textContent = _categories[i];
                _svg.appendChild(label);
            }
        }

        private void DrawContinuousXLabels(string textColor)
        {
            var tickValues = _xIsTime
                ? TimeTicks(_viewXMin, _viewXMax, EffectiveMaxXTicks())
                : NiceTicks(_viewXMin, _viewXMax, EffectiveMaxXTicks());

            foreach (var tick in tickValues)
            {
                if (tick < _viewXMin || tick > _viewXMax) continue;

                var x = PixelXForValue(tick);

                if (_showGrid)
                {
                    var line = El("line");
                    Attr(line, "x1", x);
                    Attr(line, "y1", _plotTop);
                    Attr(line, "x2", x);
                    Attr(line, "y2", _plotTop + _plotHeight);
                    Attr(line, "stroke", Theme.Colors.Neutral500Alpha);
                    Attr(line, "stroke-width", 1);
                    _svg.appendChild(line);
                }

                var label = El("text");
                Attr(label, "x", x);
                Attr(label, "y", _plotTop + _plotHeight + 14);
                Attr(label, "text-anchor", "middle");
                Attr(label, "font-size", "10");
                Attr(label, "fill", textColor);
                label.textContent = FormatXValue(tick);
                _svg.appendChild(label);
            }
        }

        // Wall-clock steps, so a time axis reads 30s / 5min / 6h rather than the 20s and 50s a decimal
        // 1/2/5 x 10^n progression would land on.
        private static readonly double[] TimeSteps =
        {
            1, 2, 5, 10, 15, 30,                            // seconds
            60, 120, 300, 600, 900, 1800,                   // minutes
            3600, 7200, 10800, 21600, 43200,                // hours
            86400, 172800, 604800, 1209600,                 // days and weeks
            2592000, 7776000, 15552000, 31536000            // months and a year
        };

        private List<double> TimeTicks(double min, double max, int maxCount)
        {
            var span = max - min;

            if (span <= 0 || maxCount < 1) return new List<double>();

            var target = span / maxCount;
            var step   = TimeSteps[TimeSteps.Length - 1];

            for (int i = 0; i < TimeSteps.Length; i++)
            {
                if (TimeSteps[i] >= target)
                {
                    step = TimeSteps[i];
                    break;
                }
            }

            _lastTimeStep = step;

            var result = new List<double>();
            var start  = Math.Ceiling(min / step) * step;

            for (var tick = start; tick <= max; tick += step)
            {
                result.Add(tick);
                if (result.Count > 200) break;
            }

            return result;
        }

        /// <summary>
        /// Rounds a rough interval up to the nearest 1, 2 or 5 times a power of ten, which is what makes an axis
        /// read as 0 / 200 / 400 rather than 0 / 168.4 / 336.8. Returns 0 for an interval it cannot represent.
        /// </summary>
        private static double NiceStep(double rough)
        {
            if (rough <= 0 || double.IsNaN(rough) || double.IsInfinity(rough)) return 0;

            var magnitude  = Math.Pow(10, Math.Floor(Math.Log10(rough)));
            var normalized = rough / magnitude;

            if (normalized <= 1) return magnitude;
            if (normalized <= 2) return 2 * magnitude;
            if (normalized <= 5) return 5 * magnitude;
            return 10 * magnitude;
        }

        // Ticks land on 1/2/5 x 10^n so labels read as round numbers at any zoom level.
        private static List<double> NiceTicks(double min, double max, int maxCount)
        {
            var result = new List<double>();
            var span   = max - min;

            if (span <= 0 || maxCount < 1) return result;

            var step = NiceStep(span / maxCount);

            if (step <= 0) return result;

            var start = Math.Ceiling(min / step) * step;

            for (var tick = start; tick <= max + step * 0.001; tick += step)
            {
                result.Add(tick);
                if (result.Count > 200) break; // guard against a pathological step
            }

            return result;
        }

        private void DrawAxisTitles(string textColor)
        {
            if (!string.IsNullOrEmpty(_xAxisTitle))
            {
                var label = El("text");
                Attr(label, "x", _plotLeft + _plotWidth / 2);
                Attr(label, "y", _plotTop + _plotHeight + 30);
                Attr(label, "text-anchor", "middle");
                Attr(label, "font-size", "10");
                Attr(label, "fill", textColor);
                label.textContent = _xAxisTitle;
                _svg.appendChild(label);
            }

            if (!string.IsNullOrEmpty(_yAxisTitle))
            {
                var label = El("text");
                Attr(label, "x", 10);
                Attr(label, "y", _plotTop + _plotHeight / 2);
                Attr(label, "text-anchor", "middle");
                Attr(label, "font-size", "10");
                Attr(label, "fill", textColor);
                Attr(label, "transform", $"rotate(-90 10 {(_plotTop + _plotHeight / 2).ToString("0.###")})");
                label.textContent = _yAxisTitle;
                _svg.appendChild(label);
            }
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

        // ===================================================================
        //                     ZOOM / PAN / SPIKELINES
        // ===================================================================

        private void EnsureInteractions()
        {
            if (_interactionsAttached) return;
            _interactionsAttached = true;

            _svg.addEventListener("wheel", (Action<Event>)(e =>
            {
                if (!_zoomable || !_continuousX) return;

                var we = e.As<WheelEvent>();
                we.preventDefault();

                var factor = we.deltaY < 0 ? 0.8 : 1.25;
                ZoomAround(PlotXValueAt(we.clientX), factor);
            }));

            _svg.addEventListener("mousedown", (Action<Event>)(e =>
            {
                if (!_zoomable || !_continuousX) return;

                var me = e.As<MouseEvent>();
                me.preventDefault();
                BeginPan(me.clientX);
            }));

            _svg.addEventListener("dblclick", (Action<Event>)(e =>
            {
                if (!_zoomable || !_continuousX || !_hasExplicitRange) return;

                AutoRangeX();
                ResolveXRange(); // so the reported range is the data extent, not the range we just dropped
                RaiseRangeChanged(isAutoRange: true);
            }));

            _svg.addEventListener("mousemove", (Action<Event>)(e =>
            {
                if (!_showSpikes) return;
                UpdateSpike(e.As<MouseEvent>().clientX);
            }));

            _svg.addEventListener("mouseleave", (Action<Event>)(_ => ClearOverlay()));
        }

        private double PlotXValueAt(double clientX)
        {
            var rect  = _container.getBoundingClientRect().As<DOMRect>();
            var local = clientX - rect.left;
            var ratio = (local - _plotLeft) / Math.Max(1, _plotWidth);

            ratio = Math.Max(0, Math.Min(1, ratio));
            return _viewXMin + (_viewXMax - _viewXMin) * ratio;
        }

        private void ZoomAround(double anchor, double factor)
        {
            var span    = _viewXMax - _viewXMin;
            var newSpan = ClampXSpan(span * factor);

            if (newSpan <= 0 || newSpan == span) return; // already against a zoom limit

            var leftShare = (anchor - _viewXMin) / Math.Max(1e-9, span);

            XRange(anchor - newSpan * leftShare, anchor + newSpan * (1 - leftShare));
            RaiseRangeChanged(isAutoRange: false);
        }

        // A wheel gesture is unbounded and nothing downstream copes with where that ends up: a span of a few
        // microseconds leaves the axis without a tick to print, and one of a hundred thousand years runs the
        // time formatter off the end of the calendar. Limits are relative to the data unless the caller pinned them.
        private double ClampXSpan(double span)
        {
            if (double.IsNaN(span) || double.IsInfinity(span) || span <= 0) return 0;

            var reference = _hasDataExtent ? _dataXMax - _dataXMin : span;
            var min       = _minXSpan > 0 ? _minXSpan : reference * DefaultMinSpanFraction;
            var max       = _maxXSpan > 0 ? _maxXSpan : reference * DefaultMaxSpanFactor;

            if (max > 0 && span > max) return max;
            if (min > 0 && span < min) return min;

            return span;
        }

        private const double DefaultMinSpanFraction = 0.001;
        private const double DefaultMaxSpanFactor   = 100;

        private void BeginPan(double startClientX)
        {
            //A mouseup released outside the document never reaches us, so the previous pan can still be
            //attached; leaving it there would have two of them fight over the range on the next drag.
            if (_endPan is object) _endPan();

            var startMin = _viewXMin;
            var startMax = _viewXMax;
            var perPixel = (startMax - startMin) / Math.Max(1, _plotWidth);
            var moved    = false;

            Action<Event> onMove = null;
            Action<Event> onUp   = null;

            onMove = e =>
            {
                var delta = (e.As<MouseEvent>().clientX - startClientX) * perPixel;

                if (delta == 0) return;

                moved = true;
                XRange(startMin - delta, startMax - delta);
            };

            onUp = e =>
            {
                if (_endPan is object) _endPan();

                //A plain click is a mousedown and a mouseup with nothing in between; republishing the range
                //there would make every click on the chart look like a pan to whatever OnRangeChanged drives.
                if (moved) RaiseRangeChanged(isAutoRange: false);
            };

            _endPan = () =>
            {
                _endPan = null;
                document.body.removeEventListener("mousemove", onMove);
                document.body.removeEventListener("mouseup", onUp);
            };

            document.body.addEventListener("mousemove", onMove);
            document.body.addEventListener("mouseup", onUp);
        }

        private void RaiseRangeChanged(bool isAutoRange)
        {
            if (_onRangeChanged is null) return;
            _onRangeChanged(new ChartRange(_viewXMin, _viewXMax, isAutoRange));
        }

        private void ClearOverlay()
        {
            if (_overlay is null) return;
            while (_overlay.firstChild != null) _overlay.removeChild(_overlay.firstChild);
        }

        private void UpdateSpike(double clientX)
        {
            if (_overlay is null || _pointCount == 0) return;

            ClearOverlay();

            var rect  = _container.getBoundingClientRect().As<DOMRect>();
            var pixel = clientX - rect.left;

            if (pixel < _plotLeft || pixel > _plotLeft + _plotWidth) return;

            var line = El("line");
            Attr(line, "x1", pixel);
            Attr(line, "y1", _plotTop);
            Attr(line, "x2", pixel);
            Attr(line, "y2", _plotTop + _plotHeight);
            Attr(line, "stroke", Theme.Default.Foreground);
            Attr(line, "stroke-width", 1);
            Attr(line, "stroke-dasharray", "3 3");
            Attr(line, "opacity", "0.6");
            _overlay.appendChild(line);

            var readout = new List<string>();
            var xValue  = _continuousX ? PlotXValueAt(clientX) : 0.0;

            if (_continuousX) readout.Add(FormatXValue(xValue));

            for (int s = 0; s < _series.Count; s++)
            {
                var series = _series[s];
                var index  = NearestIndex(series, pixel);

                if (index < 0) continue;

                var name = string.IsNullOrEmpty(series.Name) ? "" : series.Name + ": ";

                if (!_continuousX && readout.Count == 0)
                {
                    readout.Add(index < _categories.Length ? _categories[index] : "#" + (index + 1));
                }

                readout.Add(name + _valueFormatter(series.Values[index]));

                var marker = El("circle");
                Attr(marker, "cx", SeriesPointX(series, index));
                Attr(marker, "cy", PixelY(series.Values[index]));
                Attr(marker, "r", 3.5);
                Attr(marker, "fill", ColorFor(s, series));
                Attr(marker, "stroke", Theme.Default.Background);
                Attr(marker, "stroke-width", 1);
                _overlay.appendChild(marker);
            }

            if (readout.Count > 0) DrawSpikeReadout(readout, pixel);
        }

        private int NearestIndex(ChartSeries series, double pixel)
        {
            var best         = -1;
            var bestDistance = double.MaxValue;

            for (int i = 0; i < series.Values.Length; i++)
            {
                if (double.IsNaN(series.Values[i])) continue;

                var distance = Math.Abs(SeriesPointX(series, i) - pixel);

                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best         = i;
                }
            }

            return best;
        }

        private void DrawSpikeReadout(List<string> lines, double pixel)
        {
            const double lineHeight = 13;
            const double padding    = 5;

            var boxWidth  = lines.Max(l => l.Length) * 6.2 + padding * 2;
            var boxHeight = lines.Count * lineHeight + padding * 2;

            // Flip to the other side of the spike when the box would overflow the plot.
            var boxLeft = pixel + 10;
            if (boxLeft + boxWidth > _plotLeft + _plotWidth) boxLeft = pixel - 10 - boxWidth;

            var boxTop = _plotTop + 4;

            var box = El("rect");
            Attr(box, "x", boxLeft);
            Attr(box, "y", boxTop);
            Attr(box, "width", boxWidth);
            Attr(box, "height", boxHeight);
            Attr(box, "rx", 3);
            Attr(box, "fill", Theme.Default.Background);
            Attr(box, "stroke", Theme.Default.Border);
            Attr(box, "stroke-width", 1);
            Attr(box, "opacity", "0.95");
            _overlay.appendChild(box);

            for (int i = 0; i < lines.Count; i++)
            {
                var text = El("text");
                Attr(text, "x", boxLeft + padding);
                Attr(text, "y", boxTop + padding + lineHeight * (i + 1) - 3);
                Attr(text, "font-size", "10");
                Attr(text, "fill", Theme.Default.Foreground);
                text.textContent = lines[i];
                _overlay.appendChild(text);
            }
        }
    }
}
