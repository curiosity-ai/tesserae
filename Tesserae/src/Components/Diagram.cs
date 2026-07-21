using System;
using System.Collections.Generic;
using System.Linq;
using static Transpose.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// A flow-chart style diagram surface with draggable nodes, auto-computed arrows drawn on a background canvas,
    /// a pannable dotted background and automatic node positioning based on node sizes and connectivity.
    /// </summary>
    [Transpose.Name("tss.Diagram")]
    public partial class Diagram : IComponent, ISpecialCaseStyling
    {
        private readonly HTMLElement       _container;
        private readonly HTMLCanvasElement _canvas;
        private readonly HTMLElement       _nodesHost;
        private readonly List<Node>        _nodes = new List<Node>();
        private readonly List<Edge>        _edges = new List<Edge>();

        private ResizeObserver _resizeObserver;
        private double         _offsetX;
        private double         _offsetY;
        private int            _dotSpacing = 24;
        private bool           _userPanned;
        private bool           _panEnabled = true;
        private double         _layoutTimeout;

        /// <summary>
        /// Gets the HTMLElement that should receive styling.
        /// </summary>
        public HTMLElement StylingContainer => _container;

        /// <summary>
        /// Gets whether styling should propagate to the stack item parent.
        /// </summary>
        public bool PropagateToStackItemParent => false;

        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        public Diagram()
        {
            _canvas    = Canvas(_("tss-diagram-canvas"));
            _nodesHost = Div(_("tss-diagram-nodes"));
            _container = Div(_("tss-diagram"), _canvas, _nodesHost);

            ApplyDotSpacing();
            HookPanEvents();

            DomObserver.WhenMounted(_container, () =>
            {
                _resizeObserver = new ResizeObserver((entries, obs) => ResizeCanvas());
                _resizeObserver.observe(_container);
                ResizeCanvas();
                RequestAutoArrange();

                DomObserver.WhenRemoved(_container, () =>
                {
                    _resizeObserver.unobserve(_container);
                    _resizeObserver = null;
                });
            });
        }

        /// <summary>
        /// Adds the given nodes to the diagram.
        /// </summary>
        public Diagram Nodes(params Node[] nodes)
        {
            foreach (var node in nodes)
            {
                Add(node);
            }
            return this;
        }

        /// <summary>
        /// Adds a single node to the diagram.
        /// </summary>
        public Diagram Add(Node node)
        {
            if (_nodes.Contains(node)) return this;
            _nodes.Add(node);
            node.AttachTo(this);
            _nodesHost.appendChild(node.Render());
            RequestAutoArrange();
            return this;
        }

        /// <summary>
        /// Removes a node (and any arrows connected to it) from the diagram.
        /// </summary>
        public Diagram Remove(Node node)
        {
            if (_nodes.Remove(node))
            {
                _edges.RemoveAll(e => e.From == node || e.To == node);
                node.Render().remove();
                RequestAutoArrange();
            }
            return this;
        }

        /// <summary>
        /// Removes all nodes and arrows from the diagram.
        /// </summary>
        public Diagram Clear()
        {
            _nodes.Clear();
            _edges.Clear();
            ClearChildren(_nodesHost);
            Redraw();
            return this;
        }

        /// <summary>
        /// Connects two nodes with an auto-computed arrow, adding them to the diagram if needed.
        /// </summary>
        public Diagram Connect(Node from, Node to)
        {
            Add(from);
            Add(to);
            _edges.Add(new Edge() { From = from, To = to });
            RequestAutoArrange();
            return this;
        }

        /// <summary>
        /// Sets the spacing of the dotted background grid (in pixels).
        /// </summary>
        public Diagram DotSpacing(int pixels)
        {
            _dotSpacing = Math.Max(4, pixels);
            ApplyDotSpacing();
            return this;
        }

        /// <summary>
        /// Hides the dotted background grid.
        /// </summary>
        public Diagram NoDots()
        {
            _container.classList.add("tss-diagram-no-dots");
            return this;
        }

        /// <summary>
        /// Disables panning of the diagram background.
        /// </summary>
        public Diagram NotDraggable()
        {
            _panEnabled = false;
            _container.classList.add("tss-diagram-no-pan");
            return this;
        }

        /// <summary>
        /// Re-runs the automatic layout for all nodes (including ones previously dragged or explicitly positioned)
        /// and re-centers the view.
        /// </summary>
        public Diagram AutoArrange()
        {
            foreach (var node in _nodes)
            {
                node.IsPinned = false;
            }
            _userPanned = false;
            RequestAutoArrange();
            return this;
        }

        /// <summary>
        /// Renders the component's root HTML element.
        /// </summary>
        public HTMLElement Render() => _container;

        internal void RequestAutoArrange()
        {
            window.clearTimeout(_layoutTimeout);

            _layoutTimeout = window.setTimeout((_) =>
            {
                if (_container.IsMounted())
                {
                    RunAutoArrange();
                    Redraw();
                }
            }, 16);
        }

        internal void Redraw()
        {
            var ctx = _canvas.getContext("2d").As<CanvasRenderingContext2D>();
            if (ctx is null) return;

            var dpr = GetDevicePixelRatio();

            ctx.setTransform(1, 0, 0, 1, 0, 0);
            ctx.clearRect(0, 0, _canvas.width, _canvas.height);
            ctx.setTransform(dpr, 0, 0, dpr, _offsetX * dpr, _offsetY * dpr);

            var lineColor = GetThemeColor("--tss-diagram-line-color", "#b8bcc2");

            foreach (var edge in _edges)
            {
                DrawEdge(ctx, edge, lineColor);
            }
        }

        internal void OnNodeDragged()
        {
            Redraw();
        }

        private void HookPanEvents()
        {
            double startX = 0, startY = 0, origX = 0, origY = 0;

            _container.onmousedown += (me) =>
            {
                if (!_panEnabled || me.button != 0) return;
                if (me.target != _container && me.target != _canvas) return;

                startX = me.clientX;
                startY = me.clientY;
                origX  = _offsetX;
                origY  = _offsetY;

                _container.classList.add("tss-diagram-panning");
                window.onmousemove += Pan;
                window.onmouseup   += StopPan;
                StopEvent(me);
            };

            void Pan(MouseEvent me)
            {
                _offsetX    = origX + (me.clientX - startX);
                _offsetY    = origY + (me.clientY - startY);
                _userPanned = true;
                ApplyOffset();
                StopEvent(me);
            }

            void StopPan(MouseEvent me)
            {
                window.onmousemove -= Pan;
                window.onmouseup   -= StopPan;
                _container.classList.remove("tss-diagram-panning");
                StopEvent(me);
            }
        }

        private void ApplyOffset()
        {
            _nodesHost.style.transform           = $"translate({_offsetX}px, {_offsetY}px)";
            _container.style.backgroundPosition = $"{_offsetX}px {_offsetY}px";
            Redraw();
        }

        private void ApplyDotSpacing()
        {
            _container.style.backgroundSize = $"{_dotSpacing}px {_dotSpacing}px";
        }

        private void ResizeCanvas()
        {
            var dpr = GetDevicePixelRatio();
            _canvas.width  = (uint)Math.Max(1, _container.clientWidth  * dpr);
            _canvas.height = (uint)Math.Max(1, _container.clientHeight * dpr);
            Redraw();
        }

        private static double GetDevicePixelRatio()
        {
            var dpr = window.devicePixelRatio;
            return dpr > 0 ? dpr : 1;
        }

        private string GetThemeColor(string variable, string fallback)
        {
            var value = window.getComputedStyle(_container).getPropertyValue(variable);
            return string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
        }

        /// <summary>
        /// Assigns layers to nodes based on connectivity (longest path from the roots), orders nodes within each
        /// layer by the position of their parents, and spaces everything using the measured node sizes.
        /// </summary>
        private void RunAutoArrange()
        {
            if (_nodes.Count == 0) return;

            foreach (var node in _nodes)
            {
                node.Measure();
            }

            var layerOf  = new Dictionary<Node, int>();
            var incoming = new Dictionary<Node, List<Node>>();

            foreach (var node in _nodes)
            {
                incoming[node] = new List<Node>();
            }

            foreach (var edge in _edges)
            {
                incoming[edge.To].Add(edge.From);
            }

            int LayerFor(Node node, HashSet<Node> visiting)
            {
                if (layerOf.TryGetValue(node, out var known)) return known;
                if (!visiting.Add(node)) return 0; //Cycle - break it by treating this node as a root

                var layer = 0;

                foreach (var parent in incoming[node])
                {
                    layer = Math.Max(layer, LayerFor(parent, visiting) + 1);
                }

                visiting.Remove(node);
                layerOf[node] = layer;
                return layer;
            }

            var visitingSet = new HashSet<Node>();

            foreach (var node in _nodes)
            {
                LayerFor(node, visitingSet);
            }

            var layers = _nodes.GroupBy(n => layerOf[n]).OrderBy(g => g.Key).Select(g => g.ToList()).ToList();

            //Order nodes within each layer by the average vertical order of their parents to reduce arrow crossings
            var orderIndex = new Dictionary<Node, double>();

            for (int l = 0; l < layers.Count; l++)
            {
                var layer = layers[l];

                if (l > 0)
                {
                    layer.Sort((a, b) => Barycenter(a).CompareTo(Barycenter(b)));
                }

                for (int i = 0; i < layer.Count; i++)
                {
                    orderIndex[layer[i]] = i;
                }
            }

            double Barycenter(Node node)
            {
                var parents = incoming[node];
                if (parents.Count == 0) return orderIndex.TryGetValue(node, out var self) ? self : 0;
                return parents.Average(p => orderIndex.TryGetValue(p, out var i) ? i : 0);
            }

            const double gapX = 80;
            const double gapY = 32;

            double x = 0;

            foreach (var layer in layers)
            {
                var maxWidth    = layer.Max(n => n.MeasuredWidth);
                var totalHeight = layer.Sum(n => n.MeasuredHeight) + gapY * (layer.Count - 1);

                double y = -totalHeight / 2;

                foreach (var node in layer)
                {
                    if (!node.IsPinned)
                    {
                        node.MoveTo(x + (maxWidth - node.MeasuredWidth) / 2, y);
                    }

                    y += node.MeasuredHeight + gapY;
                }

                x += maxWidth + gapX;
            }

            if (!_userPanned)
            {
                CenterContent();
            }
        }

        private void CenterContent()
        {
            if (_nodes.Count == 0) return;

            var minX = _nodes.Min(n => n.X);
            var minY = _nodes.Min(n => n.Y);
            var maxX = _nodes.Max(n => n.X + n.MeasuredWidth);
            var maxY = _nodes.Max(n => n.Y + n.MeasuredHeight);

            _offsetX = (_container.clientWidth  - (maxX - minX)) / 2 - minX;
            _offsetY = (_container.clientHeight - (maxY - minY)) / 2 - minY;
            ApplyOffset();
        }

        private static void DrawEdge(CanvasRenderingContext2D ctx, Edge edge, string lineColor)
        {
            const double arrowLength = 7;
            const double anchorGap   = 4;

            var from = edge.From;
            var to   = edge.To;

            var fromCenterX = from.X + from.MeasuredWidth  / 2;
            var fromCenterY = from.Y + from.MeasuredHeight / 2;
            var toCenterX   = to.X   + to.MeasuredWidth    / 2;
            var toCenterY   = to.Y   + to.MeasuredHeight   / 2;

            var dx = toCenterX - fromCenterX;
            var dy = toCenterY - fromCenterY;

            double sx, sy, ex, ey, c1x, c1y, c2x, c2y;

            if (Math.Abs(dx) >= Math.Abs(dy))
            {
                //Mostly horizontal - anchor on the left/right sides facing each other
                var dir  = dx >= 0 ? 1 : -1;
                sx = dir > 0 ? from.X + from.MeasuredWidth + anchorGap : from.X - anchorGap;
                sy = fromCenterY;
                ex = dir > 0 ? to.X - anchorGap - arrowLength : to.X + to.MeasuredWidth + anchorGap + arrowLength;
                ey = toCenterY;

                var bend = Math.Min(60, Math.Max(20, Math.Abs(ex - sx) / 2)) * dir;
                c1x = sx + bend;
                c1y = sy;
                c2x = ex - bend;
                c2y = ey;
            }
            else
            {
                //Mostly vertical - anchor on the top/bottom sides facing each other
                var dir  = dy >= 0 ? 1 : -1;
                sx = fromCenterX;
                sy = dir > 0 ? from.Y + from.MeasuredHeight + anchorGap : from.Y - anchorGap;
                ex = toCenterX;
                ey = dir > 0 ? to.Y - anchorGap - arrowLength : to.Y + to.MeasuredHeight + anchorGap + arrowLength;

                var bend = Math.Min(60, Math.Max(20, Math.Abs(ey - sy) / 2)) * dir;
                c1x = sx;
                c1y = sy + bend;
                c2x = ex;
                c2y = ey - bend;
            }

            ctx.strokeStyle = lineColor;
            ctx.fillStyle   = lineColor;
            ctx.lineWidth   = 1.5;

            ctx.beginPath();
            ctx.moveTo(sx, sy);
            ctx.bezierCurveTo(c1x, c1y, c2x, c2y, ex, ey);
            ctx.stroke();

            //Arrowhead pointing along the end tangent of the curve
            var angle = Math.Atan2(ey - c2y, ex - c2x);

            var tipX = ex + Math.Cos(angle) * arrowLength;
            var tipY = ey + Math.Sin(angle) * arrowLength;

            ctx.beginPath();
            ctx.moveTo(tipX, tipY);
            ctx.lineTo(tipX - Math.Cos(angle - Math.PI / 6) * arrowLength, tipY - Math.Sin(angle - Math.PI / 6) * arrowLength);
            ctx.lineTo(tipX - Math.Cos(angle + Math.PI / 6) * arrowLength, tipY - Math.Sin(angle + Math.PI / 6) * arrowLength);
            ctx.closePath();
            ctx.fill();
        }

        private sealed class Edge
        {
            public Node From;
            public Node To;
        }
    }
}
