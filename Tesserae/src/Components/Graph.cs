using System;
using System.Collections.Generic;
using H5;
using H5.Core;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    [H5.Name("tss.Graph")]
    public class Graph : IComponent, IHasMarginPadding
    {
        private readonly HTMLDivElement _container;
        private readonly Raw _parent;
        private readonly HTMLElement _svgContainer;

        private GraphNode[] _nodes = new GraphNode[0];
        private GraphEdge[] _edges = new GraphEdge[0];
        private GraphCluster[] _clusters = new GraphCluster[0];

        public string Margin { get => _container.style.margin; set => _container.style.margin = value; }
        public string Padding { get => _container.style.padding; set => _container.style.padding = value; }

        public Graph()
        {
            _container = Div(_("tss-graph"));
            _parent = Raw(_container);

            _svgContainer = Div(_("tss-graph-svg-container"));
            _container.appendChild(_svgContainer);

            _parent.WhenMounted(() =>
            {
                EnsureD3Loaded(() =>
                {
                    window.setTimeout((_) => RenderGraph(), 10);
                });
            });
        }

        public HTMLElement Render() => _container;

        public Graph Nodes(params GraphNode[] nodes)
        {
            _nodes = nodes;
            return this;
        }

        public Graph Edges(params GraphEdge[] edges)
        {
            _edges = edges;
            return this;
        }

        public Graph Clusters(params GraphCluster[] clusters)
        {
            _clusters = clusters;
            return this;
        }

        private void EnsureD3Loaded(Action onLoaded)
        {
            if (Script.Write<bool>("typeof d3 !== 'undefined'"))
            {
                onLoaded();
                return;
            }

            var script = new HTMLScriptElement
            {
                type = "text/javascript",
                src = "assets/js/d3.min.js" // Since we added it to tss-dep.js
            };

            script.onload = (e) => onLoaded();
            document.head.appendChild(script);
        }

        private void RenderGraph()
        {
            if (_svgContainer.clientWidth == 0 || _svgContainer.clientHeight == 0)
            {
                window.setTimeout((_) => RenderGraph(), 100);
                return;
            }

            ClearChildren(_svgContainer);

            var width = _svgContainer.clientWidth;
            var height = _svgContainer.clientHeight;

            Script.Write(@"
                var width = {1};
                var height = {2};
                var nodes = {3};
                var edges = {4};
                var clusters = {5};

                var svg = d3.select({0})
                    .append('svg')
                    .attr('width', width)
                    .attr('height', height)
                    .attr('class', 'tss-graph-svg');

                var g = svg.append('g').attr('class', 'tss-graph-world');

                var zoom = d3.zoom()
                    .scaleExtent([0.1, 10])
                    .on('zoom', function(e) {
                        g.attr('transform', e.transform);
                    });

                svg.call(zoom);

                // Group nodes by cluster to draw hulls/boundaries
                var nodesByCluster = {};
                nodes.forEach(function(n) {
                    if (n.groupId) {
                        if (!nodesByCluster[n.groupId]) nodesByCluster[n.groupId] = [];
                        nodesByCluster[n.groupId].push(n);
                    }
                });

                var clusterData = [];
                Object.keys(nodesByCluster).forEach(function(k) {
                    var clusterNodes = nodesByCluster[k];
                    var clusterDef = clusters.find(function(c) { return c.id === k; }) || { id: k, label: k };

                    var minX = d3.min(clusterNodes, function(d) { return d.x - (d.radius || 5); });
                    var minY = d3.min(clusterNodes, function(d) { return d.y - (d.radius || 5); });
                    var maxX = d3.max(clusterNodes, function(d) { return d.x + (d.radius || 5); });
                    var maxY = d3.max(clusterNodes, function(d) { return d.y + (d.radius || 5); });

                    clusterData.push({
                        id: k,
                        label: clusterDef.label,
                        nodes: clusterNodes,
                        cx: (minX + maxX) / 2,
                        cy: (minY + maxY) / 2,
                        r: Math.max((maxX - minX) / 2, (maxY - minY) / 2) + 20
                    });
                });

                // Draw clusters (boundaries)
                var clusterGroup = g.append('g')
                    .attr('class', 'tss-graph-clusters')
                    .selectAll('g')
                    .data(clusterData)
                    .enter().append('g')
                    .attr('class', 'tss-graph-cluster-group');

                clusterGroup.append('circle')
                    .attr('class', 'tss-graph-cluster-boundary')
                    .attr('cx', function(d) { return d.cx; })
                    .attr('cy', function(d) { return d.cy; })
                    .attr('r', function(d) { return d.r; });

                clusterGroup.append('text')
                    .attr('class', 'tss-graph-cluster-label')
                    .attr('x', function(d) { return d.cx; })
                    .attr('y', function(d) { return d.cy - d.r - 5; })
                    .attr('text-anchor', 'middle')
                    .text(function(d) { return d.label; });


                // Draw links
                var link = g.append('g')
                    .attr('class', 'tss-graph-links')
                    .selectAll('line')
                    .data(edges)
                    .enter().append('line')
                    .attr('class', 'tss-graph-link')
                    .attr('x1', function(d) { var n = nodes.find(x => x.id === d.sourceId); return n ? n.x : 0; })
                    .attr('y1', function(d) { var n = nodes.find(x => x.id === d.sourceId); return n ? n.y : 0; })
                    .attr('x2', function(d) { var n = nodes.find(x => x.id === d.targetId); return n ? n.x : 0; })
                    .attr('y2', function(d) { var n = nodes.find(x => x.id === d.targetId); return n ? n.y : 0; })
                    .attr('stroke-width', function(d) { return Math.max(0.5, d.weight || 1); });

                // Draw nodes
                var node = g.append('g')
                    .attr('class', 'tss-graph-nodes')
                    .selectAll('g')
                    .data(nodes)
                    .enter().append('g')
                    .attr('class', 'tss-graph-node-group')
                    .attr('transform', function(d) { return 'translate(' + d.x + ',' + d.y + ')'; });

                node.append('circle')
                    .attr('class', 'tss-graph-node')
                    .attr('r', function(d) { return d.radius || 5; })
                    .attr('fill', function(d) { return d.color || '#444'; });

                node.append('text')
                    .attr('class', 'tss-graph-label')
                    .attr('dy', 12)
                    .attr('text-anchor', 'middle')
                    .text(function(d) { return d.label; });

                // Simple auto-zoom to fit bounding box
                if (nodes.length > 0) {
                    var minX = d3.min(nodes, function(d) { return d.x - (d.radius || 5); });
                    var minY = d3.min(nodes, function(d) { return d.y - (d.radius || 5); });
                    var maxX = d3.max(nodes, function(d) { return d.x + (d.radius || 5); });
                    var maxY = d3.max(nodes, function(d) { return d.y + (d.radius || 5); });

                    var dx = maxX - minX,
                        dy = maxY - minY,
                        x = (minX + maxX) / 2,
                        y = (minY + maxY) / 2,
                        scale = Math.max(0.1, Math.min(8, 0.9 / Math.max(dx / width, dy / height))),
                        translate = [width / 2 - scale * x, height / 2 - scale * y];

                    svg.call(zoom.transform, d3.zoomIdentity.translate(translate[0], translate[1]).scale(scale));
                }
            ", _svgContainer, width, height, _nodes, _edges, _clusters);
        }
    }

    [ObjectLiteral]
    public class GraphNode
    {
        public string id;
        public string label;
        public string groupId;
        public double radius;
        public double x;
        public double y;
        public string color;
    }

    [ObjectLiteral]
    public class GraphEdge
    {
        public string sourceId;
        public string targetId;
        public double weight;
    }

    [ObjectLiteral]
    public class GraphCluster
    {
        public string id;
        public string label;
    }
}
