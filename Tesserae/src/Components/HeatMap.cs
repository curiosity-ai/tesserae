using System;
using System.Linq;
using Transpose;
using static Transpose.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// One cell of a <see cref="HeatMap"/>, as handed to the cell click handler.
    /// </summary>
    [Transpose.Name("tss.HeatMapCell")]
    public sealed class HeatMapCell
    {
        /// <summary>The zero-based column (horizontal / X) index of the cell.</summary>
        public int Column { get; }

        /// <summary>The zero-based row (vertical / Y) index of the cell.</summary>
        public int Row { get; }

        /// <summary>The cell's value, or <see cref="double.NaN"/> when the matrix has no sample there.</summary>
        public double Value { get; }

        /// <summary>The text of the column label, when the chart knows one.</summary>
        public string ColumnLabel { get; }

        /// <summary>The text of the row label, when the chart knows one.</summary>
        public string RowLabel { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="HeatMapCell"/> class.
        /// </summary>
        public HeatMapCell(int column, int row, double value, string columnLabel, string rowLabel)
        {
            Column      = column;
            Row         = row;
            Value       = value;
            ColumnLabel = columnLabel;
            RowLabel    = rowLabel;
        }
    }

    /// <summary>
    /// A lightweight, dependency-free SVG heat map: a matrix of cells coloured by value, with axis labels that can
    /// be plain strings or full components, and click handlers on both the cells and the labels.
    /// <para>
    /// One row per <see cref="ChartSeries"/> (the series name is the row label), one column per value, so the
    /// observable-driven <c>Series</c> overloads of <see cref="ChartBase{T}"/> drive it like any other chart.
    /// </para>
    /// <para>
    /// The vertical (row) labels are drawn rotated 90°, reading bottom-to-top, so a long label costs the chart the
    /// height of one line of text instead of its full length. That holds for a component label too: it is laid out
    /// normally and then rotated as a whole, which is what keeps it legible in the narrow gutter beside the matrix.
    /// </para>
    /// </summary>
    [Transpose.Name("tss.HeatMap")]
    public sealed class HeatMap : ChartBase<HeatMap>
    {
        //~6px per character at the 10px label size the other charts use, which is what they size their axes with too.
        private const double LabelCharWidth = 6.2;
        private const double LabelFontSize  = 10;

        private string[]     _columnLabels     = new string[0];
        private string[]     _rowLabels        = new string[0];
        private IComponent[] _columnComponents;
        private IComponent[] _rowComponents;

        private HTMLElement   _labelLayer;
        private HTMLElement[] _columnWrappers;
        private HTMLElement[] _rowWrappers;
        private bool          _labelsDirty = true;

        private Action<HeatMapCell>  _onCellClick;
        private Action<int, string>  _onColumnLabelClick;
        private Action<int, string>  _onRowLabelClick;

        private double   _cellGap      = 2;
        private double   _cornerRadius = 2;
        private bool     _showValues;
        private string   _valueColor;
        private string[] _colorScale;
        private string   _scaleColor   = Theme.Colors.Blue600;
        private double   _minIntensity = 0.06;

        private bool   _hasExplicitRange;
        private double _rangeMin;
        private double _rangeMax;

        // The value range the current render is coloured against, also used by the scale legend.
        private double _cellMin;
        private double _cellMax;

        // Plot rectangle, computed per render.
        private double _plotLeft;
        private double _plotTop;
        private double _plotWidth;
        private double _plotHeight;

        private int _rowCount;
        private int _columnCount;

        /// <summary>
        /// Initializes a new instance of the <see cref="HeatMap"/> class.
        /// </summary>
        public HeatMap() : base(minWidth: 160, minHeight: 100)
        {
            // A heat map's legend is the colour scale, and it reads better as a bar beside the matrix.
            _legendPosition = ChartLegendPosition.Right;
        }

        /// <summary>
        /// Replaces the matrix with one row per array. <c>rows[y][x]</c> is the value in row <c>y</c>, column
        /// <c>x</c>; <see cref="double.NaN"/> leaves a cell empty.
        /// </summary>
        public HeatMap Data(double[][] rows)
        {
            _series.Clear();

            if (rows != null)
            {
                for (int i = 0; i < rows.Length; i++) _series.Add(new ChartSeries(null, rows[i]));
            }

            QueueRender();
            return this;
        }

        /// <summary>Sets the horizontal (column) labels as plain text.</summary>
        public HeatMap XAxis(params string[] labels)
        {
            _columnLabels     = labels ?? new string[0];
            _columnComponents = null;
            _labelsDirty      = true;
            QueueRender();
            return this;
        }

        /// <summary>
        /// Sets the horizontal (column) labels as components — a button, a link, an icon and a caption, anything
        /// that renders. <paramref name="names"/> is optional text for the tooltips, the click handler and the
        /// accessibility summary, which a component cannot supply on its own.
        /// </summary>
        public HeatMap XAxis(IComponent[] labels, params string[] names)
        {
            _columnComponents = labels;
            _columnLabels     = names ?? new string[0];
            _labelsDirty      = true;
            QueueRender();
            return this;
        }

        /// <summary>
        /// Sets the vertical (row) labels as plain text, drawn rotated 90°. With none set the row label is the
        /// name of the series that row came from.
        /// </summary>
        public HeatMap YAxis(params string[] labels)
        {
            _rowLabels     = labels ?? new string[0];
            _rowComponents = null;
            _labelsDirty   = true;
            QueueRender();
            return this;
        }

        /// <summary>
        /// Sets the vertical (row) labels as components, each rendered normally and then rotated 90° as a whole.
        /// <paramref name="names"/> is optional text for the tooltips, the click handler and the accessibility
        /// summary.
        /// </summary>
        public HeatMap YAxis(IComponent[] labels, params string[] names)
        {
            _rowComponents = labels;
            _rowLabels     = names ?? new string[0];
            _labelsDirty   = true;
            QueueRender();
            return this;
        }

        /// <summary>Raised when a cell is clicked. Setting a handler also makes the cells show a pointer cursor.</summary>
        public HeatMap OnCellClick(Action<HeatMapCell> handler) { _onCellClick = handler; QueueRender(); return this; }

        /// <summary>
        /// Raised when a horizontal (column) label is clicked, with its index and its text (null when the label is
        /// a component with no name given). A component label keeps its own handlers too, so a label that is a
        /// <see cref="Button"/> can answer on its own instead.
        /// </summary>
        public HeatMap OnXLabelClick(Action<int, string> handler) { _onColumnLabelClick = handler; _labelsDirty = true; QueueRender(); return this; }

        /// <summary>Raised when a vertical (row) label is clicked, with its index and its text.</summary>
        public HeatMap OnYLabelClick(Action<int, string> handler) { _onRowLabelClick = handler; _labelsDirty = true; QueueRender(); return this; }

        /// <summary>Sets the gap between cells, in pixels.</summary>
        public HeatMap CellGap(double gap) { _cellGap = Math.Max(0, gap); QueueRender(); return this; }

        /// <summary>Sets the corner radius of the cells.</summary>
        public HeatMap Rounded(double radius = 2) { _cornerRadius = Math.Max(0, radius); QueueRender(); return this; }

        /// <summary>
        /// Prints each cell's value inside it, in cells with the room for it. Pass a color to take over the
        /// automatic choice, which is the theme foreground on pale cells and white on saturated ones.
        /// </summary>
        public HeatMap ShowValues(bool show = true, string color = null) { _showValues = show; _valueColor = color; QueueRender(); return this; }

        /// <summary>
        /// Sets the base colour of the default scale: every cell is this colour, faded towards the background as
        /// its value approaches the minimum. Ignored once <see cref="ColorScale"/> sets explicit stops.
        /// </summary>
        public HeatMap ScaleColor(string color, double minIntensity = 0.06)
        {
            if (!string.IsNullOrEmpty(color)) _scaleColor = color;
            _minIntensity = Math.Max(0, Math.Min(1, minIntensity));
            QueueRender();
            return this;
        }

        /// <summary>
        /// Colours the cells from an explicit ramp instead of the default single-colour fade: the stops run from
        /// the lowest value to the highest, and a cell takes the nearest one.
        /// </summary>
        public HeatMap ColorScale(params string[] stops)
        {
            _colorScale = stops != null && stops.Length > 0 ? stops : null;
            QueueRender();
            return this;
        }

        /// <summary>
        /// Pins the value range the colours are scaled against, so two heat maps can be read against each other.
        /// Values outside it are clamped to the ends of the scale.
        /// </summary>
        public HeatMap ValueRange(double min, double max)
        {
            if (max <= min) return this;

            _hasExplicitRange = true;
            _rangeMin         = min;
            _rangeMax         = max;
            QueueRender();
            return this;
        }

        /// <summary>Returns the chart to colouring against the range of its own data.</summary>
        public HeatMap AutoValueRange() { _hasExplicitRange = false; QueueRender(); return this; }

        /// <inheritdoc />
        protected override void RenderChart(double width, double height)
        {
            _rowCount    = _series.Count;
            _columnCount = _rowCount == 0 ? 0 : _series.Max(s => s.Values.Length);

            EnsureLabelWrappers();

            if (_rowCount == 0 || _columnCount == 0)
            {
                HideLabelWrappers();
                return;
            }

            ComputeValueRange();

            //Measured before the plot rectangle, the way the cartesian charts size their value axis: the labels
            //are what decides how much room each gutter needs.
            var bottomGutter = MeasureColumnLabelHeight();
            var leftGutter   = MeasureRowLabelWidth();

            var legend = MeasureScaleLegend();

            _plotLeft   = leftGutter + (_legendPosition == ChartLegendPosition.Left ? legend : 0) + 2;
            _plotTop    = 4 + (_legendPosition == ChartLegendPosition.Top ? legend : 0);
            _plotWidth  = Math.Max(1, width - _plotLeft - 6 - (_legendPosition == ChartLegendPosition.Right ? legend : 0));
            _plotHeight = Math.Max(1, height - _plotTop - bottomGutter - 4 - (_legendPosition == ChartLegendPosition.Bottom ? legend : 0));

            DrawCells();
            DrawScaleLegend(width, height, legend);
            DrawColumnLabels();
            DrawRowLabels();
        }

        // ===================================================================
        //                              CELLS
        // ===================================================================

        private void ComputeValueRange()
        {
            if (_hasExplicitRange)
            {
                _cellMin = _rangeMin;
                _cellMax = _rangeMax;
                return;
            }

            var min = double.MaxValue;
            var max = double.MinValue;

            for (int r = 0; r < _rowCount; r++)
            {
                var values = _series[r].Values;

                for (int c = 0; c < values.Length; c++)
                {
                    var v = values[c];
                    if (double.IsNaN(v)) continue;
                    if (v < min) min = v;
                    if (v > max) max = v;
                }
            }

            if (min > max)
            {
                _cellMin = 0;
                _cellMax = 1;
                return;
            }

            // A matrix where every cell holds the same value is one flat colour, not a division by zero.
            if (min == max) max = min + 1;

            _cellMin = min;
            _cellMax = max;
        }

        private double Intensity(double value)
        {
            var range = _cellMax - _cellMin;
            if (range <= 0) return 1;
            return Math.Max(0, Math.Min(1, (value - _cellMin) / range));
        }

        private void DrawCells()
        {
            var cells = El("g");
            _svg.appendChild(cells);

            var cellWidth  = _plotWidth / _columnCount;
            var cellHeight = _plotHeight / _rowCount;
            var gap        = Math.Min(_cellGap, Math.Min(cellWidth, cellHeight) / 3);
            var clickable  = _onCellClick is object;

            for (int r = 0; r < _rowCount; r++)
            {
                var values = _series[r].Values;

                for (int c = 0; c < _columnCount; c++)
                {
                    var value = c < values.Length ? values[c] : double.NaN;

                    var x = _plotLeft + cellWidth * c + gap / 2;
                    var y = _plotTop + cellHeight * r + gap / 2;
                    var w = Math.Max(1, cellWidth - gap);
                    var h = Math.Max(1, cellHeight - gap);

                    var rect = El("rect");
                    Attr(rect, "x", x);
                    Attr(rect, "y", y);
                    Attr(rect, "width", w);
                    Attr(rect, "height", h);
                    Attr(rect, "rx", Math.Min(_cornerRadius, Math.Min(w, h) / 2));

                    if (double.IsNaN(value))
                    {
                        // An absent sample is not a zero, so it is drawn as a hole rather than as the palest cell.
                        Attr(rect, "fill", "none");
                        Attr(rect, "stroke", Theme.Colors.Neutral500Alpha);
                        Attr(rect, "stroke-width", 1);
                        Attr(rect, "stroke-dasharray", "2 2");
                        cells.appendChild(rect);
                        continue;
                    }

                    var intensity = Intensity(value);
                    ApplyFill(rect, intensity);
                    AttachPointTooltip(rect, TooltipFor(r, c, value));

                    if (clickable)
                    {
                        rect.As<HTMLElement>().style.cursor = "pointer";

                        //Locals, not the loop variables: the handler outlives the iteration that built it.
                        var row    = r;
                        var column = c;
                        var cell   = value;

                        rect.addEventListener("click", (Action<Event>)(_ => _onCellClick(new HeatMapCell(column, row, cell, ColumnLabelText(column), RowLabelText(row)))));
                    }

                    cells.appendChild(rect);

                    if (_showValues && w >= 26 && h >= 14) DrawCellValue(value, intensity, x + w / 2, y + h / 2, cells);
                }
            }

            AttachHoverOutline(cells);
        }

        private void ApplyFill(Element rect, double intensity)
        {
            if (_colorScale is object)
            {
                var index = (int)Math.Round(intensity * (_colorScale.Length - 1));
                Attr(rect, "fill", _colorScale[Math.Max(0, Math.Min(_colorScale.Length - 1, index))]);
                return;
            }

            //One colour faded towards the background rather than a ramp of literal colours: the low end then sits
            //on whatever the theme's background is, instead of staying a pale blue on a dark page.
            Attr(rect, "fill", _scaleColor);
            Attr(rect, "fill-opacity", (_minIntensity + intensity * (1 - _minIntensity)).ToString("0.###"));
        }

        private void DrawCellValue(double value, double intensity, double cx, double cy, Element into)
        {
            var text = El("text");
            Attr(text, "x", cx);
            Attr(text, "y", cy + 3);
            Attr(text, "text-anchor", "middle");
            Attr(text, "font-size", LabelFontSize.ToString("0"));
            Attr(text, "fill", _valueColor ?? (intensity >= 0.55 ? "#ffffff" : Theme.Default.Foreground));
            Attr(text, "pointer-events", "none");
            text.textContent = _valueFormatter(value);
            into.appendChild(text);
        }

        // One delegated listener for the whole matrix: a cell per sample means a hover outline per sample is the
        // difference between two listeners and two thousand.
        private void AttachHoverOutline(Element cells)
        {
            Element hovered = null;

            cells.addEventListener("mouseover", (Action<Event>)(e =>
            {
                var target = e.As<MouseEvent>().target.As<Element>();

                //An empty cell is drawn as a dashed hole and already owns its stroke - outlining it would only
                //repaint the hole in the highlight colour.
                if (target is null || target.tagName != "rect" || target.hasAttribute("stroke-dasharray")) return;

                if (hovered is object && hovered != target) hovered.removeAttribute("stroke");

                hovered = target;
                Attr(hovered, "stroke", Theme.Default.Foreground);
                Attr(hovered, "stroke-width", 1.5);
            }));

            cells.addEventListener("mouseout", (Action<Event>)(e =>
            {
                if (hovered is null) return;
                if (e.As<MouseEvent>().target.As<Element>() != hovered) return;

                hovered.removeAttribute("stroke");
                hovered = null;
            }));
        }

        private string TooltipFor(int row, int column, double value)
        {
            var rowLabel    = RowLabelText(row);
            var columnLabel = ColumnLabelText(column);
            return $"{rowLabel} / {columnLabel}: {_valueFormatter(value)}";
        }

        private string ColumnLabelText(int index)
        {
            if (index < _columnLabels.Length && !string.IsNullOrEmpty(_columnLabels[index])) return _columnLabels[index];
            return "Column " + (index + 1);
        }

        private string RowLabelText(int index)
        {
            if (index < _rowLabels.Length && !string.IsNullOrEmpty(_rowLabels[index])) return _rowLabels[index];
            if (index < _series.Count && !string.IsNullOrEmpty(_series[index].Name)) return _series[index].Name;
            return "Row " + (index + 1);
        }

        // ===================================================================
        //                          SCALE LEGEND
        // ===================================================================

        private double MeasureScaleLegend()
        {
            if (!_showLegend) return 0;

            if (_legendPosition == ChartLegendPosition.Left || _legendPosition == ChartLegendPosition.Right)
            {
                var widest = Math.Max(_valueFormatter(_cellMin).Length, _valueFormatter(_cellMax).Length);
                return 12 + 6 + widest * LabelCharWidth + 6;
            }

            return 26;
        }

        private void DrawScaleLegend(double width, double height, double inset)
        {
            if (!_showLegend || inset <= 0) return;

            var gradientId = "tss-heatmap-scale-" + Guid.NewGuid().ToString("N").Substring(0, 8);

            var defs     = El("defs");
            var gradient = El("linearGradient");
            Attr(gradient, "id", gradientId);

            var vertical = _legendPosition == ChartLegendPosition.Left || _legendPosition == ChartLegendPosition.Right;

            // A vertical bar runs from the maximum at the top to the minimum at the bottom, matching the axis it sits beside.
            Attr(gradient, "x1", vertical ? "0" : "0");
            Attr(gradient, "y1", vertical ? "1" : "0");
            Attr(gradient, "x2", vertical ? "0" : "1");
            Attr(gradient, "y2", "0");

            const int steps = 12;

            for (int i = 0; i <= steps; i++)
            {
                var t    = i / (double)steps;
                var stop = El("stop");
                Attr(stop, "offset", (t * 100).ToString("0.##") + "%");

                if (_colorScale is object)
                {
                    var index = (int)Math.Round(t * (_colorScale.Length - 1));
                    Attr(stop, "stop-color", _colorScale[Math.Max(0, Math.Min(_colorScale.Length - 1, index))]);
                }
                else
                {
                    Attr(stop, "stop-color", _scaleColor);
                    Attr(stop, "stop-opacity", (_minIntensity + t * (1 - _minIntensity)).ToString("0.###"));
                }

                gradient.appendChild(stop);
            }

            defs.appendChild(gradient);
            _svg.appendChild(defs);

            double barX, barY, barWidth, barHeight;

            if (vertical)
            {
                barWidth  = 12;
                barHeight = Math.Max(20, _plotHeight);
                barX      = _legendPosition == ChartLegendPosition.Left ? 4 : width - inset + 4;
                barY      = _plotTop;
            }
            else
            {
                barWidth  = Math.Max(20, _plotWidth);
                barHeight = 8;
                barX      = _plotLeft;
                barY      = _legendPosition == ChartLegendPosition.Top ? 6 : height - 18;
            }

            var bar = El("rect");
            Attr(bar, "x", barX);
            Attr(bar, "y", barY);
            Attr(bar, "width", barWidth);
            Attr(bar, "height", barHeight);
            Attr(bar, "rx", 2);
            Attr(bar, "fill", "url(#" + gradientId + ")");
            _svg.appendChild(bar);

            if (vertical)
            {
                DrawScaleLabel(_valueFormatter(_cellMax), barX + barWidth + 4, barY + 8, "start");
                DrawScaleLabel(_valueFormatter(_cellMin), barX + barWidth + 4, barY + barHeight - 2, "start");
            }
            else
            {
                DrawScaleLabel(_valueFormatter(_cellMin), barX, barY + barHeight + 10, "start");
                DrawScaleLabel(_valueFormatter(_cellMax), barX + barWidth, barY + barHeight + 10, "end");
            }
        }

        private void DrawScaleLabel(string text, double x, double y, string anchor)
        {
            var label = El("text");
            Attr(label, "x", x);
            Attr(label, "y", y);
            Attr(label, "text-anchor", anchor);
            Attr(label, "font-size", LabelFontSize.ToString("0"));
            Attr(label, "fill", Theme.Default.Foreground);
            label.textContent = text;
            _svg.appendChild(label);
        }

        // ===================================================================
        //                             LABELS
        // ===================================================================

        private void EnsureLabelLayer()
        {
            if (_labelLayer is object) return;

            //Above the SVG, but transparent to the pointer except where a label actually is, so hovering a cell
            //through the gutter-free part of the layer still reaches the cell.
            _labelLayer                      = Div(Att("tss-heatmap-labels"));
            _labelLayer.style.position       = "absolute";
            _labelLayer.style.left           = "0";
            _labelLayer.style.top            = "0";
            _labelLayer.style.width          = "100%";
            _labelLayer.style.height         = "100%";
            _labelLayer.style.pointerEvents  = "none";
            _labelLayer.style.overflow       = "hidden";

            _container.appendChild(_labelLayer);
        }

        private void EnsureLabelWrappers()
        {
            if (!_labelsDirty) return;
            _labelsDirty = false;

            DisposeWrappers(_columnWrappers);
            DisposeWrappers(_rowWrappers);

            _columnWrappers = null;
            _rowWrappers    = null;

            if (_columnComponents is object) _columnWrappers = BuildWrappers(_columnComponents, rotated: false, onClick: _onColumnLabelClick, names: _columnLabels);
            if (_rowComponents is object) _rowWrappers       = BuildWrappers(_rowComponents, rotated: true, onClick: _onRowLabelClick, names: _rowLabels);
        }

        private void DisposeWrappers(HTMLElement[] wrappers)
        {
            if (wrappers is null || _labelLayer is null) return;

            foreach (var wrapper in wrappers)
            {
                if (wrapper.parentElement == _labelLayer) _labelLayer.removeChild(wrapper);
            }
        }

        private HTMLElement[] BuildWrappers(IComponent[] components, bool rotated, Action<int, string> onClick, string[] names)
        {
            EnsureLabelLayer();

            var wrappers = new HTMLElement[components.Length];

            for (int i = 0; i < components.Length; i++)
            {
                var wrapper = Div(Att("tss-heatmap-label"));
                wrapper.style.position        = "absolute";
                wrapper.style.whiteSpace      = "nowrap";
                wrapper.style.overflow         = "hidden";
                wrapper.style.textOverflow     = "ellipsis";
                wrapper.style.pointerEvents   = "auto";
                wrapper.style.transformOrigin = "center center";

                if (components[i] is object) wrapper.appendChild(components[i].Render());

                if (onClick is object)
                {
                    var index = i;
                    var name  = index < names.Length ? names[index] : null;

                    wrapper.style.cursor = "pointer";
                    wrapper.addEventListener("click", (Action<Event>)(_ => onClick(index, name)));
                }

                _labelLayer.appendChild(wrapper);
                wrappers[i] = wrapper;
            }

            return wrappers;
        }

        private void HideLabelWrappers()
        {
            HideWrappers(_columnWrappers);
            HideWrappers(_rowWrappers);
        }

        private static void HideWrappers(HTMLElement[] wrappers)
        {
            if (wrappers is null) return;
            foreach (var wrapper in wrappers) wrapper.style.display = "none";
        }

        // The wrappers are laid out unrotated, so offsetWidth/offsetHeight are the label's natural size whatever
        // transform is on it — which is what makes the rotated gutter cost one line of text rather than its length.
        private double MeasureColumnLabelHeight()
        {
            if (_columnWrappers is object)
            {
                double tallest = 0;

                for (int i = 0; i < _columnWrappers.Length && i < _columnCount; i++)
                {
                    var h = _columnWrappers[i].offsetHeight;
                    if (h > tallest) tallest = h;
                }

                return tallest > 0 ? tallest + 8 : 8;
            }

            return _columnLabels.Length > 0 ? 18 : 6;
        }

        private double MeasureRowLabelWidth()
        {
            if (_rowWrappers is object)
            {
                double thickest = 0;

                for (int i = 0; i < _rowWrappers.Length && i < _rowCount; i++)
                {
                    var h = _rowWrappers[i].offsetHeight;
                    if (h > thickest) thickest = h;
                }

                return thickest > 0 ? thickest + 8 : 8;
            }

            return HasRowText() ? LabelFontSize + 8 : 6;
        }

        private bool HasRowText()
        {
            if (_rowLabels.Length > 0) return true;
            return _series.Any(s => !string.IsNullOrEmpty(s.Name));
        }

        private void DrawColumnLabels()
        {
            var cellWidth = _plotWidth / _columnCount;

            if (_columnWrappers is object)
            {
                for (int i = 0; i < _columnWrappers.Length; i++)
                {
                    var wrapper = _columnWrappers[i];

                    if (i >= _columnCount)
                    {
                        wrapper.style.display = "none";
                        continue;
                    }

                    wrapper.style.display   = "block";
                    wrapper.style.maxWidth  = Math.Max(0, cellWidth - 4).ToString("0.##") + "px";
                    wrapper.style.left      = (_plotLeft + cellWidth * i + cellWidth / 2).ToString("0.##") + "px";
                    wrapper.style.top       = (_plotTop + _plotHeight + 4).ToString("0.##") + "px";
                    wrapper.style.transform = "translateX(-50%)";
                }

                return;
            }

            if (_columnLabels.Length == 0) return;

            // Thinned the way the cartesian axis thins its ticks: one label per cell only while they fit.
            var step = Math.Max(1, (int)Math.Ceiling(_columnCount / Math.Max(1.0, _plotWidth / 40)));

            for (int i = 0; i < _columnCount; i += step)
            {
                if (i >= _columnLabels.Length) break;

                var label = El("text");
                Attr(label, "x", _plotLeft + cellWidth * i + cellWidth / 2);
                Attr(label, "y", _plotTop + _plotHeight + 14);
                Attr(label, "text-anchor", "middle");
                Attr(label, "font-size", LabelFontSize.ToString("0"));
                Attr(label, "fill", Theme.Default.Foreground);
                label.textContent = Ellipsize(_columnLabels[i], cellWidth * step - 4);
                AttachLabelClick(label, i, _columnLabels[i], _onColumnLabelClick);
                _svg.appendChild(label);
            }
        }

        private void DrawRowLabels()
        {
            var cellHeight = _plotHeight / _rowCount;
            var gutterEnd  = _plotLeft;

            if (_rowWrappers is object)
            {
                for (int i = 0; i < _rowWrappers.Length; i++)
                {
                    var wrapper = _rowWrappers[i];

                    if (i >= _rowCount)
                    {
                        wrapper.style.display = "none";
                        continue;
                    }

                    var centerX = Math.Max(0, gutterEnd - MeasureRowLabelWidth() / 2 - 2);
                    var centerY = _plotTop + cellHeight * i + cellHeight / 2;

                    wrapper.style.display  = "block";
                    wrapper.style.maxWidth = Math.Max(0, cellHeight - 4).ToString("0.##") + "px";
                    wrapper.style.left     = centerX.ToString("0.##") + "px";
                    wrapper.style.top      = centerY.ToString("0.##") + "px";

                    //Rotated about its own centre after being centred on the row, so the label reads bottom-to-top
                    //and stays registered with the row whatever its length.
                    wrapper.style.transform = "translate(-50%, -50%) rotate(-90deg)";
                }

                return;
            }

            if (!HasRowText()) return;

            var x = Math.Max(LabelFontSize, gutterEnd - 6);

            for (int i = 0; i < _rowCount; i++)
            {
                if (cellHeight < LabelFontSize) break; // no room to print a row per line

                var y = _plotTop + cellHeight * i + cellHeight / 2;

                var label = El("text");
                Attr(label, "x", x);
                Attr(label, "y", y);
                Attr(label, "text-anchor", "middle");
                Attr(label, "font-size", LabelFontSize.ToString("0"));
                Attr(label, "fill", Theme.Default.Foreground);
                Attr(label, "transform", $"rotate(-90 {x.ToString("0.###")} {y.ToString("0.###")})");
                label.textContent = Ellipsize(RowLabelText(i), cellHeight - 4);
                AttachLabelClick(label, i, RowLabelText(i), _onRowLabelClick);
                _svg.appendChild(label);
            }
        }

        private void AttachLabelClick(Element label, int index, string text, Action<int, string> handler)
        {
            if (handler is null) return;

            label.As<HTMLElement>().style.cursor = "pointer";
            Attr(label, "text-decoration", "underline");
            label.addEventListener("click", (Action<Event>)(_ => handler(index, text)));
        }

        private static string Ellipsize(string text, double maxPixels)
        {
            if (string.IsNullOrEmpty(text)) return text;

            var maxChars = (int)(maxPixels / LabelCharWidth);

            if (maxChars >= text.Length) return text;
            if (maxChars <= 1) return "";

            return text.Substring(0, maxChars - 1) + "…";
        }

        /// <inheritdoc />
        protected override string BuildAriaLabel()
        {
            if (!string.IsNullOrEmpty(_title)) return _title;

            if (_rowCount == 0 || _columnCount == 0) return "Heat map with no data";

            return $"Heat map. {_rowCount} rows by {_columnCount} columns, values from {_valueFormatter(_cellMin)} to {_valueFormatter(_cellMax)}.";
        }
    }
}
