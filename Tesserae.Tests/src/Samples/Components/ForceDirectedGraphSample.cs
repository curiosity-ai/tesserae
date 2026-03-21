using System.Collections.Generic;
using Tesserae;
using static H5.Core.dom;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Components", Order = 100, Icon = UIcons.Network)]
    public class ForceDirectedGraphSample : IComponent, ISample
    {
        public HTMLElement Render()
        {
            var nodes1 = new List<ForceDirectedNode>
            {
                new ForceDirectedNode { Id = "1", Component = UI.Card(UI.TextBlock("Node 1").Medium()) },
                new ForceDirectedNode { Id = "2", Component = UI.Card(UI.TextBlock("Node 2").Medium()) },
                new ForceDirectedNode { Id = "3", Component = UI.Card(UI.TextBlock("Node 3").Medium()) }
            };

            var edges1 = new List<ForceDirectedEdge>
            {
                new ForceDirectedEdge { SourceId = "1", TargetId = "2" },
                new ForceDirectedEdge { SourceId = "2", TargetId = "3" },
                new ForceDirectedEdge { SourceId = "3", TargetId = "1" }
            };

            var graph1 = new ForceDirectedGraph().Nodes(nodes1).Edges(edges1);
            graph1.Render().style.height = "400px";

            var nodes2 = new List<ForceDirectedNode>();
            var edges2 = new List<ForceDirectedEdge>();

            for (int i = 0; i < 50; i++)
            {
                nodes2.Add(new ForceDirectedNode { Id = i.ToString(), Component = UI.Badge(i.ToString()) });
            }

            for (int i = 0; i < 100; i++)
            {
                edges2.Add(new ForceDirectedEdge { SourceId = (i % 50).ToString(), TargetId = ((i + 1) % 50).ToString() });
            }

            var graph2 = new ForceDirectedGraph().Nodes(nodes2).Edges(edges2);
            graph2.Render().style.height = "600px";

            var stack = UI.VStack();
            stack.Add(UI.TextBlock("Few Nodes").Large());
            stack.Add(graph1);
            stack.Add(UI.TextBlock("Lots of Nodes").Large());
            stack.Add(graph2);
            return stack.Render();
        }
    }
}
