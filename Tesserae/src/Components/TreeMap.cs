using System;
using System.Collections.Generic;
using System.Linq;
using Transpose;
using static Transpose.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// One rectangle of a <see cref="TreeMap"/>: a leaf with a <see cref="Value"/>, or a group whose size is the sum
    /// of its children. Build a tree with <see cref="Add(TreeMapNode[])"/> and hand its root to
    /// <see cref="TreeMap.Data(TreeMapNode)"/>.
    /// </summary>
    [Transpose.Name("tss.TreeMapNode")]
    public sealed class TreeMapNode
    {
        private readonly List<TreeMapNode> _children = new List<TreeMapNode>();

        /// <summary>
        /// Initializes a new instance of the <see cref="TreeMapNode"/> class.
        /// </summary>
        /// <param name="label">The name drawn on the rectangle (and on a group's name plate).</param>
        /// <param name="value">The leaf's size. Ignored on a node that has children, which is sized by their sum.</param>
        /// <param name="category">The category a leaf is coloured by and counted under in the legend.</param>
        public TreeMapNode(string label, double value = 0, string category = null)
        {
            Label    = label ?? "";
            Value    = value;
            Category = category;
        }

        /// <summary>The name drawn on the rectangle.</summary>
        public string Label { get; }

        /// <summary>The leaf's size. A node with children ignores it and is sized by their sum.</summary>
        public double Value { get; set; }

        /// <summary>The category the leaf is coloured by and counted under in the legend.</summary>
        public string Category { get; set; }

        /// <summary>A colour for this leaf alone, which beats the colour of its category. Any CSS colour, tokens included.</summary>
        public string Color { get; set; }

        /// <summary>An extra line for the hover tooltip.</summary>
        public string Tooltip { get; set; }

        /// <summary>Anything the application wants to carry along, such as the record the node stands for.</summary>
        public object Data { get; set; }

        /// <summary>The group this node belongs to, or null for the root.</summary>
        public TreeMapNode Parent { get; private set; }

        /// <summary>The node's children, in the order they were added.</summary>
        public IReadOnlyList<TreeMapNode> Children => _children;

        /// <summary>Whether the node has no children.</summary>
        public bool IsLeaf => _children.Count == 0;

        /// <summary>The size the node was laid out with: its <see cref="Value"/>, or the sum of its leaves. Set by the last layout.</summary>
        public double Total { get; internal set; }

        /// <summary>How many groups sit above the node; the root is 0.</summary>
        public int Depth { get; internal set; }

        /// <summary>The labels from the first level below the root down to this node, joined with <c>/</c>.</summary>
        public string Path
        {
            get
            {
                var parts = new List<string>();
                for (var n = this; n is object && n.Parent is object; n = n.Parent) parts.Insert(0, n.Label);
                return string.Join("/", parts);
            }
        }

        /// <summary>Adds children and returns this node, so a tree can be written as one expression.</summary>
        public TreeMapNode Add(params TreeMapNode[] children)
        {
            if (children is null) return this;

            foreach (var child in children)
            {
                if (child is null) continue;
                child.Parent = this;
                _children.Add(child);
            }

            return this;
        }

        /// <summary>Adds a leaf and returns this node.</summary>
        public TreeMapNode Add(string label, double value, string category = null) => Add(new TreeMapNode(label, value, category));

        // Layout, in map coordinates (the map at zoom 1 is 0..W by 0..H).
        internal double X0, Y0, X1, Y1;
        internal int    LeafIndex = -1;
    }

    /// <summary>
    /// A zoomable, canvas-drawn tree map: nested rectangles sized by value (squarified layout), leaves coloured by
    /// category, groups separated by outlines and named on plates in their top-left corner, and a legend that gives
    /// each category's share and highlights it on click.
    /// <para>
    /// It is drawn on a canvas rather than built from elements, so it stays smooth with tens of thousands of leaves.
    /// The wheel zooms around the pointer (once the map is zoomed in, or always when zooming in), dragging pans,
    /// two fingers pinch, double click zooms in, and a click reports the leaf, or the group whose plate was clicked.
    /// With the keyboard: <c>+</c> and <c>-</c> zoom, the arrows pan and <c>0</c> shows the whole map again.
    /// </para>
    /// <para>
    /// A highlight (<see cref="Highlight"/>, or a legend entry) keeps the matching leaves at full colour and fades
    /// the rest, which is how a search or a filter shows where its results live.
    /// </para>
    /// </summary>
    [Transpose.Name("tss.TreeMap")]
    public sealed class TreeMap : IComponent
    {
        // The squarified layout aims for rectangles near the golden ratio, as d3's treemapSquarify does.
        private static readonly double Ratio = (1 + Math.Sqrt(5)) / 2;

        // Outline width on screen, by depth: the outermost groups are separated by the thickest lines. The width is
        // fixed on screen, so a gap does not grow into a thick band when you zoom into a small group.
        private static readonly double[] OutlineWidths = { 0, 3, 2, 1 };

        private static readonly string[] DefaultPalette =
        {
            SeriesColor(1, Theme.Colors.Blue600),
            SeriesColor(2, Theme.Colors.Green600),
            SeriesColor(3, Theme.Colors.Orange600),
            SeriesColor(4, Theme.Colors.Purple600),
            SeriesColor(5, Theme.Colors.Red600),
            SeriesColor(6, Theme.Colors.Teal600),
            SeriesColor(7, Theme.Colors.Yellow600),
            SeriesColor(8, Theme.Colors.Neutral600)
        };

        private static string SeriesColor(int index, string fallback) => "var(--tss-chart-series-" + index + ", " + fallback + ")";

        private const string UncategorizedColor = Theme.Colors.Neutral400;
        private const double DimAlpha           = 0.16;
        private const double ClickSlop          = 5;

        private readonly HTMLElement       _container;
        private readonly HTMLElement       _legend;
        private readonly HTMLElement       _mapWrap;
        private readonly HTMLCanvasElement _canvas;
        private readonly HTMLElement       _tip;
        private readonly Button            _resetButton;
        private readonly ResizeObserver    _resizeObserver;

        private TreeMapNode _root;

        // Leaves and groups of the current tree, flattened at layout time. Groups are in pre-order, parents first.
        private readonly List<TreeMapNode> _leaves = new List<TreeMapNode>();
        private readonly List<TreeMapNode> _groups = new List<TreeMapNode>();
        private int[]                      _leafColor = new int[0];
        private int[][]                    _leavesByColor = new int[0][];

        // Colours: the CSS strings (category palette first, then per-leaf overrides), and what they resolve to here.
        private readonly List<string>               _categoryOrder = new List<string>();
        private readonly Dictionary<string, string> _categoryColors = new Dictionary<string, string>();
        private string[]                            _palette = DefaultPalette;
        private readonly List<string>               _colorCss = new List<string>();
        private string[]                            _colorResolved = new string[0];
        private string[]                            _colorInk = new string[0];
        private bool                                _colorsDirty = true;
        private string                              _surface = "#ffffff";
        private string                              _foreground = "#1a1a1a";
        private string                              _secondary = "#666666";
        private string                              _fontFamily = "sans-serif";

        // Highlight.
        private bool[]                  _mask;
        private Func<TreeMapNode, bool> _highlightPredicate;
        private string                  _highlightLabel;
        private string                  _highlightCategory;
        private Action<TreeMap>         _onHighlightChanged;

        // View: map coordinates to screen is x * k + tx.
        private double _w, _h;
        private double _k = 1, _tx, _ty;
        private double _maxZoom = 1000;
        private bool   _zoomable = true;
        private int    _animation;

        private bool                    _showLegend = true;
        private bool                    _showTooltips = true;
        private bool                    _showLeafLabels = true;
        private Func<double, string>    _valueFormatter = v => v.ToString("N0");
        private string                  _unit;
        private Action<TreeMapNode>     _onNodeClick;

        private int         _hover = -1;
        private bool        _drawQueued;
        private bool        _layoutDirty = true;
        private bool        _legendDirty = true;
        private readonly List<Plate> _plates = new List<Plate>();

        // Pointers: one pans, two pinch.
        private readonly Dictionary<int, double[]> _pointers = new Dictionary<int, double[]>();
        private double   _downX, _downY;
        private bool     _moved;
        private double   _panTx, _panTy, _panX, _panY;
        private double   _pinchDistance, _pinchK, _pinchWorldX, _pinchWorldY;

        private static bool        _wheelWatched;
        private static double      _lastWheel;
        private static EventTarget _wheelOwner;

        private sealed class Plate
        {
            public double      X, Y, W, H;
            public TreeMapNode Node;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TreeMap"/> class.
        /// </summary>
        public TreeMap(TreeMapNode root = null)
        {
            _canvas = Canvas(Att("tss-treemap-canvas"));
            _canvas.tabIndex = 0;
            _canvas.setAttribute("role", "img");

            _tip = Div(Att("tss-treemap-tip"));
            _tip.setAttribute("aria-hidden", "true");

            _resetButton = Button("Show all").SetIcon(UIcons.ZoomOut).Class("tss-treemap-reset").OnClick(() => ResetZoom());
            var resetElement = _resetButton.Render();
            resetElement.style.display = "none";

            _mapWrap = Div(Att("tss-treemap-map"), _canvas, _tip, resetElement);
            _legend  = Div(Att("tss-treemap-legend"));
            _container = Div(Att("tss-treemap"), _legend, _mapWrap);

            HookInteractions();

            _resizeObserver = new ResizeObserver((entries, obs) => OnResize());
            _resizeObserver.observe(_mapWrap);

            Action onTheme = () => { _colorsDirty = true; RequestDraw(); };

            DomObserver.WhenMounted(_container, () =>
            {
                Theme.OnThemeChanged += onTheme;
                _colorsDirty = true;
                OnResize();
            });

            DomObserver.WhenRemoved(_container, () =>
            {
                Theme.OnThemeChanged -= onTheme;
                _resizeObserver.unobserve(_mapWrap);
            });

            if (root is object) Data(root);
        }

        // ------------------------------------------------------------------------------------------------ public API

        /// <summary>The root of the tree being drawn.</summary>
        public TreeMapNode Root => _root;

        /// <summary>How many leaves the current highlight keeps at full colour, or 0 with no highlight.</summary>
        public int HighlightedCount => _mask is null ? 0 : _mask.Count(m => m);

        /// <summary>What the current highlight shows, as given to <see cref="Highlight"/> (the category name for a legend entry).</summary>
        public string HighlightLabel => _highlightLabel;

        /// <summary>Whether something is highlighted.</summary>
        public bool IsHighlighted => _mask is object;

        /// <summary>The current zoom factor, 1 for the whole map.</summary>
        public double Zoom => _k;

        /// <summary>Replaces the tree. The root itself is not drawn: its children are the top-level groups.</summary>
        public TreeMap Data(TreeMapNode root)
        {
            _root        = root;
            _hover       = -1;
            _layoutDirty = true;
            _legendDirty = true;
            _colorsDirty = true;
            RequestDraw();
            return this;
        }

        /// <summary>
        /// Gives a category a colour, and its place in the colour order. Categories never named here take the chart
        /// palette in the order they first appear among the leaves.
        /// </summary>
        public TreeMap Category(string name, string color)
        {
            if (name is null) return this;
            if (!_categoryOrder.Contains(name)) _categoryOrder.Add(name);
            if (color is object) _categoryColors[name] = color;
            _colorsDirty = true;
            _legendDirty = true;
            _layoutDirty = true;
            RequestDraw();
            return this;
        }

        /// <summary>Sets the palette categories take their colours from, in order. Defaults to <c>--tss-chart-series-1</c> … <c>8</c>.</summary>
        public TreeMap Colors(params string[] palette)
        {
            if (palette is object && palette.Length > 0) _palette = palette;
            _colorsDirty = true;
            _legendDirty = true;
            _layoutDirty = true;
            RequestDraw();
            return this;
        }

        /// <summary>Shows or hides the legend: one entry per category, with its share of the total, that highlights it on click.</summary>
        public TreeMap Legend(bool show = true)
        {
            _showLegend  = show;
            _legendDirty = true;
            RequestDraw();
            return this;
        }

        /// <summary>Shows or hides the hover tooltip.</summary>
        public TreeMap Tooltips(bool show = true)
        {
            _showTooltips = show;
            if (!show) HideTip();
            return this;
        }

        /// <summary>Whether leaves large enough on screen show their label. On by default.</summary>
        public TreeMap LeafLabels(bool show = true)
        {
            _showLeafLabels = show;
            RequestDraw();
            return this;
        }

        /// <summary>Formats values in the tooltip. <paramref name="unit"/> is appended (<c>"1,204 lines"</c>).</summary>
        public TreeMap FormatValues(Func<double, string> formatter, string unit = null)
        {
            if (formatter is object) _valueFormatter = formatter;
            _unit = unit;
            return this;
        }

        /// <summary>Names what the values measure, appended to them in the tooltip (<c>"1,204 lines"</c>).</summary>
        public TreeMap Unit(string unit)
        {
            _unit = unit;
            return this;
        }

        /// <summary>Enables or disables zooming and panning.</summary>
        public TreeMap Zoomable(bool zoomable = true)
        {
            _zoomable = zoomable;
            if (!zoomable) SetView(1, 0, 0);
            return this;
        }

        /// <summary>The furthest the map can be zoomed in. Defaults to 1000.</summary>
        public TreeMap MaxZoom(double maxZoom)
        {
            _maxZoom = Math.Max(1, maxZoom);
            return this;
        }

        /// <summary>
        /// Raised when a leaf is clicked, or a group when its name plate is. A click is a press that did not move;
        /// a drag pans instead. Setting a handler gives the map a pointer cursor.
        /// </summary>
        public TreeMap OnNodeClick(Action<TreeMapNode> handler)
        {
            _onNodeClick = handler;
            _canvas.classList.toggle("tss-treemap-clickable", handler is object);
            return this;
        }

        /// <summary>Raised whenever the highlight changes, from code or from the legend.</summary>
        public TreeMap OnHighlightChanged(Action<TreeMap> handler)
        {
            _onHighlightChanged = handler;
            return this;
        }

        /// <summary>
        /// Keeps the leaves that match at full colour and fades the rest. Call it again with another predicate to
        /// change what is shown, or <see cref="ClearHighlight"/> to end it. The predicate is kept and applied again
        /// when the data changes.
        /// </summary>
        /// <param name="predicate">Which leaves to keep.</param>
        /// <param name="label">What the highlight shows, for <see cref="HighlightLabel"/>.</param>
        public TreeMap Highlight(Func<TreeMapNode, bool> predicate, string label = null)
        {
            SetHighlight(predicate, label, null);
            return this;
        }

        /// <summary>Highlights the leaves of one category, as clicking its legend entry does.</summary>
        public TreeMap HighlightCategory(string category)
        {
            SetHighlight(n => n.Category == category, category, category);
            return this;
        }

        /// <summary>Ends the highlight.</summary>
        public TreeMap ClearHighlight()
        {
            SetHighlight(null, null, null);
            return this;
        }

        /// <summary>Shows the whole map again.</summary>
        public TreeMap ResetZoom()
        {
            AnimateTo(1, 0, 0);
            return this;
        }

        /// <summary>Zooms so that the node fills the view.</summary>
        public TreeMap ZoomTo(TreeMapNode node)
        {
            if (node is null) return this;
            EnsureLayout();
            if (_w <= 0 || _h <= 0) return this;

            var nw = Math.Max(node.X1 - node.X0, 1e-6);
            var nh = Math.Max(node.Y1 - node.Y0, 1e-6);
            // A little margin around the node, unless that is all the zoom there is: a group as tall as the map fits
            // at zoom 1 exactly, and the margin would leave it there instead of filling what it can.
            var fit = Math.Min(_w / nw, _h / nh);
            var k   = Clamp(fit * 0.96 >= 1.05 ? fit * 0.96 : fit, 1, _maxZoom);
            var cx = (node.X0 + node.X1) / 2;
            var cy = (node.Y0 + node.Y1) / 2;

            AnimateTo(k, _w / 2 - cx * k, _h / 2 - cy * k);
            return this;
        }

        /// <summary>Renders the component.</summary>
        public HTMLElement Render() => _container;

        // ------------------------------------------------------------------------------------------------ layout

        private void EnsureLayout()
        {
            if (!_layoutDirty || _root is null || _w <= 0 || _h <= 0) return;
            _layoutDirty = false;

            _leaves.Clear();
            _groups.Clear();

            Sum(_root, 0);
            _root.X0 = 0;
            _root.Y0 = 0;
            _root.X1 = _w;
            _root.Y1 = _h;
            Squarify(_root);

            AssignColors();
            ApplyHighlight();
            UpdateSummary();
        }

        private double Sum(TreeMapNode node, int depth)
        {
            node.Depth = depth;

            if (node.IsLeaf)
            {
                node.LeafIndex = _leaves.Count;
                _leaves.Add(node);
                var v = node.Value;
                node.Total = double.IsNaN(v) || double.IsInfinity(v) || v < 0 ? 0 : v;
                return node.Total;
            }

            node.LeafIndex = -1;
            if (depth > 0) _groups.Add(node);

            double total = 0;
            foreach (var child in node.Children) total += Sum(child, depth + 1);
            node.Total = total;
            return total;
        }

        // Bruls, Huizing and van Wijk's squarified layout, row by row, as d3.treemapSquarify lays it out.
        private static void Squarify(TreeMapNode parent)
        {
            if (parent.IsLeaf) return;

            var nodes = parent.Children.OrderByDescending(c => c.Total).ToArray();
            var x0    = parent.X0;
            var y0    = parent.Y0;
            var x1    = parent.X1;
            var y1    = parent.Y1;
            var value = parent.Total;
            var n     = nodes.Length;
            var i0    = 0;
            var i1    = 0;

            while (i0 < n)
            {
                var dx = x1 - x0;
                var dy = y1 - y0;

                double sumValue;
                do
                {
                    sumValue = nodes[i1++].Total;
                } while (sumValue <= 0 && i1 < n);

                var minValue = sumValue;
                var maxValue = sumValue;
                var alpha    = Math.Max(dy / Math.Max(dx, 1e-12), dx / Math.Max(dy, 1e-12)) / (Math.Max(value, 1e-12) * Ratio);
                var beta     = sumValue * sumValue * alpha;
                var minRatio = Math.Max(maxValue / beta, beta / minValue);

                for (; i1 < n; ++i1)
                {
                    var nodeValue = nodes[i1].Total;
                    sumValue += nodeValue;
                    if (nodeValue < minValue) minValue = nodeValue;
                    if (nodeValue > maxValue) maxValue = nodeValue;
                    beta = sumValue * sumValue * alpha;
                    var newRatio = Math.Max(maxValue / beta, beta / minValue);
                    if (newRatio > minRatio)
                    {
                        sumValue -= nodeValue;
                        break;
                    }
                    minRatio = newRatio;
                }

                if (dx < dy)
                {
                    var rowY1 = value > 0 ? y0 + dy * sumValue / value : y1;
                    Dice(nodes, i0, i1, x0, y0, x1, rowY1);
                    y0 = rowY1;
                }
                else
                {
                    var rowX1 = value > 0 ? x0 + dx * sumValue / value : x1;
                    Slice(nodes, i0, i1, x0, y0, rowX1, y1);
                    x0 = rowX1;
                }

                value -= sumValue;
                i0 = i1;
            }

            foreach (var child in nodes) Squarify(child);
        }

        // A row laid out left to right.
        private static void Dice(TreeMapNode[] nodes, int i0, int i1, double x0, double y0, double x1, double y1)
        {
            double sum = 0;
            for (var i = i0; i < i1; i++) sum += nodes[i].Total;
            var scale = sum > 0 ? (x1 - x0) / sum : 0;

            for (var i = i0; i < i1; i++)
            {
                var node = nodes[i];
                node.Y0 = y0;
                node.Y1 = y1;
                node.X0 = x0;
                node.X1 = x0 += node.Total * scale;
            }
        }

        // A column laid out top to bottom.
        private static void Slice(TreeMapNode[] nodes, int i0, int i1, double x0, double y0, double x1, double y1)
        {
            double sum = 0;
            for (var i = i0; i < i1; i++) sum += nodes[i].Total;
            var scale = sum > 0 ? (y1 - y0) / sum : 0;

            for (var i = i0; i < i1; i++)
            {
                var node = nodes[i];
                node.X0 = x0;
                node.X1 = x1;
                node.Y0 = y0;
                node.Y1 = y0 += node.Total * scale;
            }
        }

        // ------------------------------------------------------------------------------------------------ colours

        // Categories in colour order: the ones given a colour or a place with Category(...) first, then the rest in
        // the order they first appear among the leaves.
        private List<string> CategoriesInOrder()
        {
            var order = new List<string>(_categoryOrder);
            var seen  = new HashSet<string>(order);

            foreach (var leaf in _leaves)
            {
                if (leaf.Category is object && seen.Add(leaf.Category)) order.Add(leaf.Category);
            }

            return order;
        }

        private string CategoryCss(string category, List<string> order)
        {
            if (category is null) return UncategorizedColor;
            if (_categoryColors.TryGetValue(category, out var explicitColor)) return explicitColor;
            var index = order.IndexOf(category);
            return _palette[(index < 0 ? 0 : index) % _palette.Length];
        }

        private void AssignColors()
        {
            var order = CategoriesInOrder();
            _colorCss.Clear();
            var indexOf = new Dictionary<string, int>();

            int IndexFor(string css)
            {
                if (indexOf.TryGetValue(css, out var i)) return i;
                indexOf[css] = _colorCss.Count;
                _colorCss.Add(css);
                return _colorCss.Count - 1;
            }

            _leafColor = new int[_leaves.Count];

            for (var i = 0; i < _leaves.Count; i++)
            {
                var leaf = _leaves[i];
                _leafColor[i] = IndexFor(leaf.Color ?? CategoryCss(leaf.Category, order));
            }

            var buckets = new List<int>[_colorCss.Count];
            for (var c = 0; c < buckets.Length; c++) buckets[c] = new List<int>();
            for (var i = 0; i < _leafColor.Length; i++) buckets[_leafColor[i]].Add(i);
            _leavesByColor = buckets.Select(b => b.ToArray()).ToArray();

            _colorsDirty = true;
        }

        // A canvas takes literal colours only, so every token is resolved where the map is (a theme can restate it
        // on any ancestor) and read back as rgb through a 1px canvas, which also gives its luminance for label ink.
        private void ResolveColors()
        {
            if (!_colorsDirty) return;
            _colorsDirty = false;

            _colorResolved = new string[_colorCss.Count];
            _colorInk      = new string[_colorCss.Count];

            for (var i = 0; i < _colorCss.Count; i++)
            {
                var rgb = ResolveRgb(_container, _colorCss[i]);
                _colorResolved[i] = "rgb(" + rgb[0] + "," + rgb[1] + "," + rgb[2] + ")";
                var luminance = (0.2126 * rgb[0] + 0.7152 * rgb[1] + 0.0722 * rgb[2]) / 255;
                _colorInk[i] = luminance > 0.5 ? "#14130f" : "#ffffff";
            }

            _surface    = SurfaceBehind(_mapWrap);
            var fg      = ResolveRgb(_container, "var(--tss-default-foreground-color)");
            var sec     = ResolveRgb(_container, "var(--tss-secondary-foreground-color)");
            _foreground = "rgb(" + fg[0] + "," + fg[1] + "," + fg[2] + ")";
            _secondary  = "rgb(" + sec[0] + "," + sec[1] + "," + sec[2] + ")";
            _fontFamily = window.getComputedStyle(_container).fontFamily;
            if (string.IsNullOrWhiteSpace(_fontFamily)) _fontFamily = "sans-serif";
        }

        private static int[] ResolveRgb(HTMLElement where, string css)
            => Script.Write<int[]>(@"(function (host, c) {
                    var probe = document.createElement('span');
                    probe.style.color = c;
                    host.appendChild(probe);
                    var computed = getComputedStyle(probe).color;
                    host.removeChild(probe);
                    var cv = document.createElement('canvas'); cv.width = cv.height = 1;
                    var g = cv.getContext('2d', { willReadFrequently: true });
                    g.fillStyle = '#ffffff'; g.fillRect(0, 0, 1, 1);
                    g.fillStyle = computed; g.fillRect(0, 0, 1, 1);
                    var d = g.getImageData(0, 0, 1, 1).data;
                    return [d[0], d[1], d[2]];
                })({0}, {1})", where, css);

        // The colour the map sits on: the first opaque background from the host outwards, with the translucent ones in
        // front of it laid over it. A background still transitioning (a theme switch fades panes) counts as its end value.
        private static string SurfaceBehind(HTMLElement host)
            => Script.Write<string>(@"(function (el) {
                    var cv = document.createElement('canvas'); cv.width = cv.height = 1;
                    var ctx = cv.getContext('2d', { willReadFrequently: true });
                    var layers = [];
                    for (; el && el.nodeType === 1; el = el.parentElement) {
                        var bg = getComputedStyle(el).backgroundColor;
                        var running = el.getAnimations ? el.getAnimations() : [];
                        for (var j = 0; j < running.length; j++) {
                            if (running[j].transitionProperty !== 'background-color') continue;
                            var frames = running[j].effect.getKeyframes();
                            bg = frames[frames.length - 1].backgroundColor || bg;
                        }
                        ctx.clearRect(0, 0, 1, 1); ctx.fillStyle = bg; ctx.fillRect(0, 0, 1, 1);
                        var a = ctx.getImageData(0, 0, 1, 1).data[3];
                        if (a === 0) continue;
                        layers.push(bg);
                        if (a === 255) break;
                    }
                    ctx.clearRect(0, 0, 1, 1); ctx.fillStyle = '#ffffff'; ctx.fillRect(0, 0, 1, 1);
                    for (var i = layers.length - 1; i >= 0; i--) { ctx.fillStyle = layers[i]; ctx.fillRect(0, 0, 1, 1); }
                    var d = ctx.getImageData(0, 0, 1, 1).data;
                    return 'rgb(' + d[0] + ',' + d[1] + ',' + d[2] + ')';
                })({0})", host);

        // ------------------------------------------------------------------------------------------------ highlight

        private void SetHighlight(Func<TreeMapNode, bool> predicate, string label, string category)
        {
            _highlightPredicate = predicate;
            _highlightLabel     = predicate is null ? null : label;
            _highlightCategory  = predicate is null ? null : category;
            ApplyHighlight();
            UpdateLegendPressed();
            RequestDraw();
            _onHighlightChanged?.Invoke(this);
        }

        private void ApplyHighlight()
        {
            if (_highlightPredicate is null)
            {
                _mask = null;
                return;
            }

            _mask = new bool[_leaves.Count];
            for (var i = 0; i < _leaves.Count; i++) _mask[i] = _highlightPredicate(_leaves[i]);
        }

        // ------------------------------------------------------------------------------------------------ legend

        private void RenderLegend()
        {
            if (!_legendDirty) return;
            _legendDirty = false;

            ClearChildren(_legend);
            _legend.style.display = _showLegend ? "" : "none";
            if (!_showLegend || _root is null) return;

            var order  = CategoriesInOrder();
            var totals = new Dictionary<string, double>();
            double all = 0;

            foreach (var leaf in _leaves)
            {
                all += leaf.Total;
                if (leaf.Category is null) continue;
                totals.TryGetValue(leaf.Category, out var t);
                totals[leaf.Category] = t + leaf.Total;
            }

            if (all <= 0) return;

            foreach (var category in order.Where(c => totals.ContainsKey(c) && totals[c] > 0).OrderByDescending(c => totals[c]))
            {
                var share  = totals[category] / all;
                var name   = category;
                var swatch = Span(Att("tss-treemap-swatch"));
                swatch.style.background = CategoryCss(category, order);

                var item = Button(Att("tss-treemap-legend-item"), swatch, Span(Att("tss-treemap-legend-name", text: category)), Span(Att("tss-treemap-legend-share", text: FormatShare(share))));
                item.type = "button";
                item.setAttribute("data-category", category);
                item.title = FormatShare(share) + " of the total. Click to highlight.";
                item.onclick += e =>
                {
                    if (_highlightCategory == name) ClearHighlight();
                    else HighlightCategory(name);
                };
                _legend.appendChild(item);
            }

            UpdateLegendPressed();
        }

        private void UpdateLegendPressed()
        {
            var items = _legend.querySelectorAll(".tss-treemap-legend-item");
            for (uint i = 0; i < items.length; i++)
            {
                var item = items[i].As<HTMLElement>();
                item.setAttribute("aria-pressed", (item.getAttribute("data-category") == _highlightCategory) ? "true" : "false");
            }
        }

        private static string FormatShare(double share)
        {
            if (share <= 0) return "0%";
            if (share < 0.01) return "<1%";
            return Math.Round(share * 100) + "%";
        }

        private void UpdateSummary()
        {
            var top = _root?.Children.OrderByDescending(c => c.Total).Take(3).Where(c => _root.Total > 0).Select(c => c.Label + " " + FormatShare(c.Total / _root.Total)).ToArray() ?? new string[0];
            var label = "Tree map of " + _leaves.Count.ToString("N0") + " items in " + _groups.Count.ToString("N0") + " groups"
                      + (top.Length > 0 ? ". Largest: " + string.Join(", ", top) : "") + ".";
            _canvas.setAttribute("aria-label", label);
        }

        // ------------------------------------------------------------------------------------------------ drawing

        private void RequestDraw()
        {
            if (_drawQueued) return;
            _drawQueued = true;
            window.requestAnimationFrame(_ =>
            {
                _drawQueued = false;
                Draw();
            });
        }

        private void OnResize()
        {
            var rect = _mapWrap.getBoundingClientRect().As<DOMRect>();
            var w    = Math.Round(rect.width);
            var h    = Math.Round(rect.height);
            if (w <= 0 || h <= 0) return;
            if (w == _w && h == _h && _canvas.width > 0) return;

            // Keep the same part of the map in view: the layout scales with the box, so the offset does too.
            if (_w > 0 && _h > 0)
            {
                _tx *= w / _w;
                _ty *= h / _h;
            }

            _w = w;
            _h = h;

            var dpr = Math.Min(window.devicePixelRatio > 0 ? window.devicePixelRatio : 1, 2);
            _canvas.width        = (uint)Math.Max(1, Math.Round(w * dpr));
            _canvas.height       = (uint)Math.Max(1, Math.Round(h * dpr));
            _canvas.style.width  = w + "px";
            _canvas.style.height = h + "px";

            _layoutDirty = true;
            ClampView();
            Draw();
        }

        private void Draw()
        {
            if (_w <= 0 || _h <= 0) return;

            EnsureLayout();
            RenderLegend();
            ResolveColors();

            var ctx = _canvas.getContext("2d").As<CanvasRenderingContext2D>();
            if (ctx is null) return;

            var dpr = _canvas.width / _w;
            ctx.setTransform(dpr, 0, 0, dpr, 0, 0);
            ctx.globalAlpha = 1;
            ctx.fillStyle   = _surface;
            ctx.fillRect(0, 0, _w, _h);

            _plates.Clear();
            UpdateResetButton();

            if (_root is null || _root.Total <= 0) return;

            var k  = _k;
            var tx = _tx;
            var ty = _ty;

            // The visible part of the map, in map coordinates.
            var vx0 = -tx / k;
            var vy0 = -ty / k;
            var vx1 = (_w - tx) / k;
            var vy1 = (_h - ty) / k;

            // Leaves, one fill colour at a time. With a highlight, the others are drawn faded first.
            for (var pass = 0; pass < (_mask is object ? 2 : 1); pass++)
            {
                var dimmed = _mask is object && pass == 0;
                ctx.globalAlpha = dimmed ? DimAlpha : 1;

                for (var c = 0; c < _leavesByColor.Length; c++)
                {
                    ctx.fillStyle = _colorResolved[c];

                    foreach (var i in _leavesByColor[c])
                    {
                        if (_mask is object && _mask[i] == dimmed) continue;

                        var leaf = _leaves[i];
                        if (leaf.X1 < vx0 || leaf.X0 > vx1 || leaf.Y1 < vy0 || leaf.Y0 > vy1) continue;

                        var w  = (leaf.X1 - leaf.X0) * k;
                        var h  = (leaf.Y1 - leaf.Y0) * k;
                        if (w <= 0 || h <= 0) continue;
                        var gx = w > 4 ? 0.5 : 0;
                        var gy = h > 4 ? 0.5 : 0;
                        ctx.fillRect(leaf.X0 * k + tx, leaf.Y0 * k + ty, Math.Max(w - gx, 0.35), Math.Max(h - gy, 0.35));
                    }
                }
            }

            ctx.globalAlpha = 1;

            // Group outlines, deepest first so the outer ones stay on top.
            ctx.strokeStyle = _surface;
            for (var depth = OutlineWidths.Length - 1; depth >= 1; depth--)
            {
                ctx.lineWidth = OutlineWidths[depth];
                ctx.beginPath();
                var any = false;

                foreach (var g in _groups)
                {
                    var gd = Math.Min(g.Depth, OutlineWidths.Length - 1);
                    if (gd != depth) continue;
                    if (g.X1 < vx0 || g.X0 > vx1 || g.Y1 < vy0 || g.Y0 > vy1) continue;
                    var w = (g.X1 - g.X0) * k;
                    var h = (g.Y1 - g.Y0) * k;
                    if (w < 12 || h < 12) continue; // an outline would swallow a group this small
                    ctx.rect(g.X0 * k + tx, g.Y0 * k + ty, w, h);
                    any = true;
                }

                if (any) ctx.stroke();
            }

            // Group names: a plate in the top-left corner, once the group is large enough on screen, and only where
            // it would not cover a plate already drawn. Top-level names are bold and go first.
            ctx.textBaseline = "middle";

            foreach (var g in _groups)
            {
                if (g.Depth > 8) continue;
                if (g.X1 < vx0 || g.X0 > vx1 || g.Y1 < vy0 || g.Y0 > vy1) continue;

                var w   = (g.X1 - g.X0) * k;
                var h   = (g.Y1 - g.Y0) * k;
                var top = g.Depth == 1;

                if (top ? (w < 60 || h < 24) : g.Depth == 2 ? (w < 90 || h < 40) : (w < 150 || h < 60)) continue;

                var size = top ? 13 : 11;
                ctx.font = (top ? "600 " : "400 ") + size + "px " + _fontFamily;

                var x    = Math.Max(g.X0 * k + tx, 0) + 4;
                var room = Math.Min(g.X1 * k + tx - x - 4, 260);
                var text = Ellipsize(ctx, g.Label, room);
                if (text is null) continue;

                var y  = Math.Max(g.Y0 * k + ty, 0);
                var pw = ctx.measureText(text).width + 8;
                var ph = top ? 20 : 16;

                if (_plates.Any(p => x - 4 < p.X + p.W && p.X < x - 4 + pw && y < p.Y + p.H && p.Y < y + ph)) continue;

                _plates.Add(new Plate { X = x - 4, Y = y, W = pw, H = ph, Node = g });
                ctx.fillStyle = _surface;
                ctx.fillRect(x - 4, y, pw, ph);
                ctx.fillStyle = top ? _foreground : _secondary;
                ctx.fillText(text, x, y + ph / 2 + 0.5);
            }

            // Leaf names, once a rectangle is large enough to hold one.
            if (_showLeafLabels)
            {
                ctx.font         = "400 11px " + _fontFamily;
                ctx.textBaseline = "top";

                for (var i = 0; i < _leaves.Count; i++)
                {
                    var leaf = _leaves[i];
                    if (leaf.X1 < vx0 || leaf.X0 > vx1 || leaf.Y1 < vy0 || leaf.Y0 > vy1) continue;
                    if (_mask is object && !_mask[i]) continue;

                    var w = (leaf.X1 - leaf.X0) * k;
                    var h = (leaf.Y1 - leaf.Y0) * k;
                    if (w < 48 || h < 16) continue;

                    // A group's plate sits in the same corner as its first leaf's name: move the name below it.
                    var nx = leaf.X0 * k + tx + 4;
                    var ny = leaf.Y0 * k + ty + 3;
                    foreach (var p in _plates)
                    {
                        if (nx < p.X + p.W && nx + 48 > p.X && ny < p.Y + p.H && ny + 13 > p.Y) ny = p.Y + p.H + 2;
                    }
                    if (ny + 13 > leaf.Y1 * k + ty) continue;

                    var text = Ellipsize(ctx, leaf.Label, w - 8);
                    if (text is null) continue;

                    ctx.fillStyle = _colorInk[_leafColor[i]];
                    ctx.fillText(text, nx, ny);
                }
            }

            // The hovered leaf, and a dashed outline around its group.
            if (_hover >= 0 && _hover < _leaves.Count)
            {
                var leaf = _leaves[_hover];
                ctx.strokeStyle = _foreground;

                var parent = leaf.Parent;
                if (parent is object && parent != _root)
                {
                    ctx.lineWidth = 1;
                    ctx.setLineDash(new double[] { 3, 3 });
                    ctx.strokeRect(parent.X0 * k + tx, parent.Y0 * k + ty, (parent.X1 - parent.X0) * k, (parent.Y1 - parent.Y0) * k);
                    ctx.setLineDash(new double[0]);
                }

                ctx.lineWidth = 1.5;
                ctx.strokeRect(leaf.X0 * k + tx, leaf.Y0 * k + ty, Math.Max((leaf.X1 - leaf.X0) * k, 3), Math.Max((leaf.Y1 - leaf.Y0) * k, 3));
            }
        }

        // The text cut to fit the room with an ellipsis, or null when not even a few characters would fit.
        private static string Ellipsize(CanvasRenderingContext2D ctx, string text, double room)
        {
            if (string.IsNullOrEmpty(text) || room <= 0) return null;
            if (ctx.measureText(text).width <= room) return text;

            // Binary search for the longest prefix that fits with the ellipsis.
            int lo = 0, hi = text.Length;
            while (lo < hi)
            {
                var mid = (lo + hi + 1) / 2;
                if (ctx.measureText(text.Substring(0, mid) + "…").width <= room) lo = mid;
                else hi = mid - 1;
            }

            if (lo < Math.Min(3, text.Length)) return null;
            return text.Substring(0, lo).TrimEnd() + "…";
        }

        private void UpdateResetButton()
        {
            _resetButton.Render().style.display = _k > 1.02 ? "" : "none";
            _canvas.style.touchAction = _zoomable && _k > 1.0001 ? "none" : "pan-x pan-y";
        }

        // ------------------------------------------------------------------------------------------------ view

        private static double Clamp(double v, double min, double max) => v < min ? min : v > max ? max : v;

        private void ClampView()
        {
            _k  = Clamp(_k, 1, _maxZoom);
            _tx = Clamp(_tx, _w * (1 - _k), 0);
            _ty = Clamp(_ty, _h * (1 - _k), 0);
        }

        private void SetView(double k, double tx, double ty)
        {
            _k  = k;
            _tx = tx;
            _ty = ty;
            ClampView();
            HideTip();
            RequestDraw();
        }

        private void ZoomAround(double px, double py, double factor)
        {
            var k  = Clamp(_k * factor, 1, _maxZoom);
            var wx = (px - _tx) / _k;
            var wy = (py - _ty) / _k;
            CancelAnimation();
            SetView(k, px - wx * k, py - wy * k);
        }

        private void CancelAnimation() => _animation++;

        private void AnimateTo(double k, double tx, double ty)
        {
            var token = ++_animation;
            var reduce = window.matchMedia("(prefers-reduced-motion: reduce)").matches;

            if (reduce || !_container.IsMounted())
            {
                SetView(k, tx, ty);
                return;
            }

            // Interpolated in log space for the zoom, so a large change in scale does not rush its last part.
            var k0 = _k;
            var x0 = _tx;
            var y0 = _ty;
            double start = -1;
            const double duration = 350;

            void Step(double now)
            {
                if (token != _animation) return;
                if (start < 0) start = now;
                var t = Math.Min(1, (now - start) / duration);
                var e = t < 0.5 ? 4 * t * t * t : 1 - Math.Pow(-2 * t + 2, 3) / 2;

                var kk = Math.Exp(Math.Log(k0) + (Math.Log(k) - Math.Log(k0)) * e);
                // Keep the interpolated view consistent: blend the map point at the centre of the view.
                var cx0 = (_w / 2 - x0) / k0;
                var cy0 = (_h / 2 - y0) / k0;
                var cx1 = (_w / 2 - tx) / k;
                var cy1 = (_h / 2 - ty) / k;
                var cx  = cx0 + (cx1 - cx0) * e;
                var cy  = cy0 + (cy1 - cy0) * e;

                _k  = kk;
                _tx = _w / 2 - cx * kk;
                _ty = _h / 2 - cy * kk;
                ClampView();
                HideTip();
                Draw();

                if (t < 1) window.requestAnimationFrame(Step);
            }

            window.requestAnimationFrame(Step);
        }

        // ------------------------------------------------------------------------------------------------ hit testing

        private TreeMapNode NodeAt(double px, double py, out bool onPlate)
        {
            onPlate = false;

            foreach (var p in _plates)
            {
                if (px >= p.X && px < p.X + p.W && py >= p.Y && py < p.Y + p.H)
                {
                    onPlate = true;
                    return p.Node;
                }
            }

            if (_root is null) return null;

            var wx   = (px - _tx) / _k;
            var wy   = (py - _ty) / _k;
            var node = _root;

            while (!node.IsLeaf)
            {
                TreeMapNode next = null;
                foreach (var c in node.Children)
                {
                    if (wx >= c.X0 && wx < c.X1 && wy >= c.Y0 && wy < c.Y1 && c.Total > 0)
                    {
                        next = c;
                        break;
                    }
                }
                if (next is null) return node == _root ? null : node;
                node = next;
            }

            return node;
        }

        // ------------------------------------------------------------------------------------------------ tooltip

        private void HideTip()
        {
            _tip.style.display = "none";
            if (_hover >= 0)
            {
                _hover = -1;
                RequestDraw();
            }
        }

        private string FormatValue(double v) => _valueFormatter(v) + (string.IsNullOrEmpty(_unit) ? "" : " " + _unit);

        private void ShowTip(TreeMapNode node, double px, double py)
        {
            if (!_showTooltips || node is null)
            {
                _tip.style.display = "none";
                return;
            }

            ClearChildren(_tip);
            _tip.appendChild(Div(Att("tss-treemap-tip-title", text: node.Label)));

            var path = node.Parent?.Path;
            if (!string.IsNullOrEmpty(path)) _tip.appendChild(Div(Att("tss-treemap-tip-path", text: path)));

            var share = _root is object && _root.Total > 0 ? node.Total / _root.Total : 0;
            var facts = FormatValue(node.Total) + " · " + FormatShare(share);
            if (!node.IsLeaf) facts = CountLeaves(node).ToString("N0") + " items · " + facts;
            _tip.appendChild(Div(Att("tss-treemap-tip-facts", text: facts)));

            if (node.IsLeaf && node.Category is object)
            {
                var swatch = Span(Att("tss-treemap-swatch"));
                swatch.style.background = _colorCss[_leafColor[node.LeafIndex]];
                _tip.appendChild(Div(Att("tss-treemap-tip-category"), swatch, Span(Att(text: node.Category))));
            }

            if (!string.IsNullOrEmpty(node.Tooltip)) _tip.appendChild(Div(Att("tss-treemap-tip-extra", text: node.Tooltip)));

            _tip.style.display = "block";
            var tw = _tip.offsetWidth;
            var th = _tip.offsetHeight;
            _tip.style.left = Math.Max(0, Math.Min(px + 14, _w - tw)) + "px";
            _tip.style.top  = (py + 18 + th > _h ? Math.Max(0, py - th - 10) : py + 18) + "px";
        }

        private static int CountLeaves(TreeMapNode node)
        {
            if (node.IsLeaf) return 1;
            var n = 0;
            foreach (var c in node.Children) n += CountLeaves(c);
            return n;
        }

        // ------------------------------------------------------------------------------------------------ input

        private double[] LocalPoint(MouseEvent e)
        {
            var r = _canvas.getBoundingClientRect().As<DOMRect>();
            return new[] { e.clientX - r.left, e.clientY - r.top };
        }

        private static int PointerId(Event e)      => Script.Write<int>("{0}.pointerId", e);
        private static string PointerType(Event e) => Script.Write<string>("{0}.pointerType", e);

        private static void EnsureWheelWatch()
        {
            if (_wheelWatched) return;
            _wheelWatched = true;

            // A wheel gesture belongs to whatever was under the pointer when it began, so a page scroll that carries
            // the map under the pointer keeps scrolling the page instead of suddenly zooming the map.
            window.addEventListener("wheel", (Action<Event>)(e =>
            {
                if (e.timeStamp - _lastWheel > 350) _wheelOwner = e.target;
                _lastWheel = e.timeStamp;
            }), new AddEventListenerOptions { capture = true, passive = true });
        }

        private void HookInteractions()
        {
            EnsureWheelWatch();

            _canvas.addEventListener("wheel", (Action<Event>)(e =>
            {
                if (!_zoomable || _wheelOwner != _canvas) return;
                var we = e.As<WheelEvent>();

                // Scrolling out of a map that is already whole is the page's.
                if (we.deltaY > 0 && _k <= 1.0001) return;

                we.preventDefault();
                var scale  = we.deltaMode == 1 ? 0.05 : we.deltaMode == 2 ? 1 : 0.002;
                var factor = Math.Pow(2, -we.deltaY * scale * (we.ctrlKey ? 5 : 1));
                var p      = LocalPoint(we);
                ZoomAround(p[0], p[1], factor);
            }), new AddEventListenerOptions { passive = false });

            _canvas.addEventListener("pointerdown", (Action<Event>)(e =>
            {
                var me = e.As<MouseEvent>();
                if (PointerType(e) == "mouse" && me.button != 0) return;

                var id = PointerId(e);
                var p  = LocalPoint(me);
                _pointers[id] = p;
                Script.Write("try { {0}.setPointerCapture({1}); } catch (x) { }", _canvas, id);

                if (_pointers.Count == 1)
                {
                    _downX = p[0];
                    _downY = p[1];
                    _moved = false;
                    BeginPan(p);
                }
                else if (_pointers.Count == 2)
                {
                    _moved = true;
                    BeginPinch();
                }
            }));

            _canvas.addEventListener("pointermove", (Action<Event>)(e =>
            {
                var me = e.As<MouseEvent>();
                var id = PointerId(e);
                var p  = LocalPoint(me);

                if (!_pointers.ContainsKey(id))
                {
                    if (PointerType(e) == "mouse") Hover(p);
                    return;
                }

                _pointers[id] = p;

                if (_pointers.Count >= 2)
                {
                    if (_zoomable) Pinch();
                    return;
                }

                if (Math.Abs(p[0] - _downX) + Math.Abs(p[1] - _downY) > ClickSlop) _moved = true;

                // A touch at zoom 1 is the page's scroll; only a zoomed-in map pans under one finger.
                if (_moved && _zoomable && (PointerType(e) != "touch" || _k > 1.0001))
                {
                    CancelAnimation();
                    SetView(_k, _panTx + (p[0] - _panX), _panTy + (p[1] - _panY));
                    _canvas.classList.add("tss-treemap-panning");
                }
            }));

            Action<Event> release = e =>
            {
                var id = PointerId(e);
                if (!_pointers.ContainsKey(id)) return;
                var p = LocalPoint(e.As<MouseEvent>());
                _pointers.Remove(id);
                _canvas.classList.remove("tss-treemap-panning");

                if (e.type == "pointerup" && _pointers.Count == 0 && !_moved)
                {
                    var node = NodeAt(p[0], p[1], out _);
                    if (node is object) _onNodeClick?.Invoke(node);
                    if (PointerType(e) == "mouse") Hover(p);
                }

                // The finger left behind after a pinch carries on panning from where it is.
                if (_pointers.Count == 1) BeginPan(_pointers.Values.First());
            };

            _canvas.addEventListener("pointerup", release);
            _canvas.addEventListener("pointercancel", release);
            _canvas.addEventListener("pointerleave", (Action<Event>)(e => { if (_pointers.Count == 0) HideTip(); }));

            _canvas.addEventListener("dblclick", (Action<Event>)(e =>
            {
                if (!_zoomable) return;
                var me = e.As<MouseEvent>();
                var p  = LocalPoint(me);
                ZoomAround(p[0], p[1], me.shiftKey ? 0.5 : 2);
            }));

            _canvas.addEventListener("keydown", (Action<Event>)(e =>
            {
                if (!_zoomable) return;
                var ke   = e.As<KeyboardEvent>();
                var step = Math.Max(_w, _h) * 0.1;

                switch (ke.key)
                {
                    case "+":
                    case "=":        ZoomAround(_w / 2, _h / 2, 1.5);   break;
                    case "-":
                    case "_":        ZoomAround(_w / 2, _h / 2, 1 / 1.5); break;
                    case "0":        ResetZoom();                        break;
                    case "ArrowLeft":  SetView(_k, _tx + step, _ty);     break;
                    case "ArrowRight": SetView(_k, _tx - step, _ty);     break;
                    case "ArrowUp":    if (_k <= 1.0001) return; SetView(_k, _tx, _ty + step); break;
                    case "ArrowDown":  if (_k <= 1.0001) return; SetView(_k, _tx, _ty - step); break;
                    default: return;
                }

                ke.preventDefault();
            }));
        }

        private void BeginPan(double[] p)
        {
            _panX  = p[0];
            _panY  = p[1];
            _panTx = _tx;
            _panTy = _ty;
        }

        private void BeginPinch()
        {
            var pts = _pointers.Values.Take(2).ToArray();
            _pinchDistance = Math.Max(1, Math.Sqrt(Math.Pow(pts[0][0] - pts[1][0], 2) + Math.Pow(pts[0][1] - pts[1][1], 2)));
            _pinchK        = _k;
            var mx = (pts[0][0] + pts[1][0]) / 2;
            var my = (pts[0][1] + pts[1][1]) / 2;
            _pinchWorldX = (mx - _tx) / _k;
            _pinchWorldY = (my - _ty) / _k;
            HideTip();
        }

        private void Pinch()
        {
            var pts = _pointers.Values.Take(2).ToArray();
            var d   = Math.Max(1, Math.Sqrt(Math.Pow(pts[0][0] - pts[1][0], 2) + Math.Pow(pts[0][1] - pts[1][1], 2)));
            var mx  = (pts[0][0] + pts[1][0]) / 2;
            var my  = (pts[0][1] + pts[1][1]) / 2;
            var k   = Clamp(_pinchK * d / _pinchDistance, 1, _maxZoom);
            CancelAnimation();
            SetView(k, mx - _pinchWorldX * k, my - _pinchWorldY * k);
        }

        private void Hover(double[] p)
        {
            var node = NodeAt(p[0], p[1], out var onPlate);
            var leaf = node is object && node.IsLeaf && !onPlate ? node.LeafIndex : -1;

            if (leaf != _hover)
            {
                _hover = leaf;
                RequestDraw();
            }

            ShowTip(node, p[0], p[1]);
        }
    }
}
