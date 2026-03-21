using System;
using System.Collections.Generic;
using System.Linq;
using static H5.Core.dom;

namespace Tesserae
{
    public class ForceDirectedNode
    {
        public string Id { get; set; }
        public IComponent Component { get; set; }
    }

    public class ForceDirectedEdge
    {
        public string SourceId { get; set; }
        public string TargetId { get; set; }
    }

    [H5.Name("tss.ForceDirectedGraph")]
    public class ForceDirectedGraph : IComponent
    {
        private HTMLElement _container;
        private SVGElement _svg;
        private HTMLElement _nodesContainer;

        private List<ForceDirectedNode> _nodes = new List<ForceDirectedNode>();
        private List<ForceDirectedEdge> _edges = new List<ForceDirectedEdge>();

        private dynamic _simulation;
        private string _arrowId;
        private int _arrowOffset = 20;

        public ForceDirectedGraph()
        {
            _arrowId = $"arrow-{Guid.NewGuid().ToString("N")}";

            _svg = (SVGElement)document.createElementNS("http://www.w3.org/2000/svg", "svg");
            _svg.style.position = "absolute";
            _svg.style.top = "0";
            _svg.style.left = "0";
            _svg.style.width = "100%";
            _svg.style.height = "100%";
            _svg.style.zIndex = "1";
            _svg.style.pointerEvents = "none";

            _nodesContainer = UI.DIV();
            _nodesContainer.style.position = "absolute";
            _nodesContainer.style.top = "0";
            _nodesContainer.style.left = "0";
            _nodesContainer.style.width = "100%";
            _nodesContainer.style.height = "100%";
            _nodesContainer.style.zIndex = "2";

            _container = UI.DIV();
            _container.appendChild(_svg);
            _container.appendChild(_nodesContainer);
            _container.style.position = "relative";
            _container.style.width = "100%";
            _container.style.height = "100%";
            _container.style.overflow = "hidden";
        }

        public ForceDirectedGraph Nodes(IEnumerable<ForceDirectedNode> nodes)
        {
            _nodes = nodes.ToList();
            return this;
        }

        public ForceDirectedGraph Edges(IEnumerable<ForceDirectedEdge> edges)
        {
            _edges = edges.ToList();
            return this;
        }

        public ForceDirectedGraph ArrowOffset(int offset)
        {
            _arrowOffset = offset;

            var marker = _svg.querySelector($"#{_arrowId}");
            if (marker != null)
            {
                var el = marker.As<dynamic>();
                el.setAttribute("refX", _arrowOffset.ToString());
            }

            return this;
        }

        public HTMLElement Render()
        {
            DomObserver.WhenMounted(_container, () => InitializeD3());
            return _container;
        }

        private void InitializeD3()
        {
            var d3 = window.As<dynamic>().d3;
            if (d3 == null)
            {
                console.error("D3 is not loaded.");
                return;
            }

            var width = _container.clientWidth;
            var height = _container.clientHeight;

            // map data
            var nodesData = _nodes.Select(n => new { id = n.Id, component = n.Component, dom = n.Component.Render() }).ToArray();
            var linksData = _edges.Select(e => new { source = e.SourceId, target = e.TargetId }).ToArray();

            // create nodes
            foreach (var n in nodesData)
            {
                n.dom.style.position = "absolute";
                n.dom.style.transform = "translate(-50%, -50%)"; // center
                _nodesContainer.appendChild(n.dom);
            }

            // create SVG lines
            while (_svg.firstChild != null) _svg.removeChild(_svg.firstChild);

            // arrow marker
            var defs = document.createElementNS("http://www.w3.org/2000/svg", "defs");
            var marker = document.createElementNS("http://www.w3.org/2000/svg", "marker");
            marker.setAttribute("id", _arrowId);
            marker.setAttribute("viewBox", "0 -5 10 10");
            marker.setAttribute("refX", _arrowOffset.ToString());
            marker.setAttribute("refY", "0");
            marker.setAttribute("markerWidth", "6");
            marker.setAttribute("markerHeight", "6");
            marker.setAttribute("orient", "auto");
            var path = document.createElementNS("http://www.w3.org/2000/svg", "path");
            path.setAttribute("d", "M0,-5L10,0L0,5");
            path.setAttribute("fill", "#999");
            marker.appendChild(path);
            defs.appendChild(marker);
            _svg.appendChild(defs);

            var lines = new List<dynamic>();
            foreach (var l in linksData)
            {
                var line = document.createElementNS("http://www.w3.org/2000/svg", "line");
                line.setAttribute("stroke", "#999");
                line.setAttribute("stroke-width", "2");
                line.setAttribute("marker-end", $"url(#{_arrowId})");
                _svg.appendChild(line);
                var dynLink = new { line = line, source = l.source, target = l.target };
                lines.Add(dynLink);
            }

            var d3Nodes = d3.map(nodesData, new Func<dynamic, dynamic>(d => {
                var o = H5.Script.Write<dynamic>("Object.assign({}, d)");
                return o;
            }));

            var d3Links = d3.map(linksData, new Func<dynamic, dynamic>(d => {
                var o = H5.Script.Write<dynamic>("Object.assign({}, d)");
                return o;
            }));

            Action ticked = () => {
                for (int i = 0; i < d3Nodes.length; i++) {
                    var n = d3Nodes[i];
                    var dom = nodesData[i].dom;
                    dom.style.left = n.x + "px";
                    dom.style.top = n.y + "px";
                }
                for (int i = 0; i < d3Links.length; i++) {
                    var l = d3Links[i];
                    var line = lines[i].line;
                    line.setAttribute("x1", l.source.x);
                    line.setAttribute("y1", l.source.y);
                    line.setAttribute("x2", l.target.x);
                    line.setAttribute("y2", l.target.y);
                }
            };

            _simulation = d3.forceSimulation(d3Nodes)
                .force("link", d3.forceLink(d3Links).id(new Func<dynamic, string>(d => d.id)).distance(150))
                .force("charge", d3.forceManyBody().strength(-300))
                .force("center", d3.forceCenter(width / 2, height / 2))
                .on("tick", ticked);
        }
    }
}
