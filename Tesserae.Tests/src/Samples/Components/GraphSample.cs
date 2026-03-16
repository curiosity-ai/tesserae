using System;
using System.Collections.Generic;
using static H5.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Components", Order = 100, Icon = UIcons.GraphCurve)]
    public class GraphSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public GraphSample()
        {
            var nodes = new List<GraphNode>();
            var edges = new List<GraphEdge>();
            var clusters = new List<GraphCluster>();

            var random = new Random();
            var clusterColors = new[] { "#FF6B6B", "#4ECDC4", "#45B7D1", "#FDCB6E", "#6C5CE7" };

            // Generate some random clusters
            for (int c = 0; c < 5; c++)
            {
                var clusterId = "cluster" + c;
                clusters.Add(new GraphCluster { id = clusterId, label = "Group " + c });
                var color = clusterColors[c % clusterColors.Length];

                // Generate nodes for this cluster around a random center
                var centerX = random.NextDouble() * 800 - 400;
                var centerY = random.NextDouble() * 600 - 300;

                int nodeCount = random.Next(5, 15);
                var clusterNodes = new List<string>();

                for (int n = 0; n < nodeCount; n++)
                {
                    var nodeId = "node_" + c + "_" + n;
                    clusterNodes.Add(nodeId);

                    nodes.Add(new GraphNode
                    {
                        id = nodeId,
                        label = "Node " + c + "-" + n,
                        groupId = clusterId,
                        color = color,
                        radius = random.NextDouble() * 8 + 4,
                        x = centerX + (random.NextDouble() - 0.5) * 150,
                        y = centerY + (random.NextDouble() - 0.5) * 150
                    });
                }

                // Internal edges for this cluster
                for (int e = 0; e < nodeCount * 1.5; e++)
                {
                    var src = clusterNodes[random.Next(clusterNodes.Count)];
                    var tgt = clusterNodes[random.Next(clusterNodes.Count)];
                    if (src != tgt)
                    {
                        edges.Add(new GraphEdge { sourceId = src, targetId = tgt, weight = 1 });
                    }
                }
            }

            // Cross-cluster edges
            for (int e = 0; e < 15; e++)
            {
                var src = nodes[random.Next(nodes.Count)].id;
                var tgt = nodes[random.Next(nodes.Count)].id;
                if (src != tgt)
                {
                    edges.Add(new GraphEdge { sourceId = src, targetId = tgt, weight = 0.5 });
                }
            }

            var graph = new Graph().Nodes(nodes.ToArray()).Edges(edges.ToArray()).Clusters(clusters.ToArray());

            _content = SectionStack()
               .Title(SampleHeader(nameof(GraphSample)))
               .Section(Stack().Children(
                    SampleTitle("Overview"),
                    TextBlock("The Graph component allows visualizing network connections, clusters and nodes with zooming and panning support, powered by d3.js.")
                ))
               .Section(Stack().Children(
                    SampleTitle("Usage"),
                    TextBlock("A sample generated network graph with 5 clusters.").PB(16),
                    Card(graph.W(100.percent()).H(600.px())).W(100.percent()).H(100.percent())
                ));
        }

        public HTMLElement Render()
        {
            return _content.Render();
        }
    }
}
