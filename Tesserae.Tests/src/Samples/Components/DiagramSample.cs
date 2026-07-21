using static Transpose.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Components", Order = 207, Icon = UIcons.Workflow)]
    public class DiagramSample : IComponent, ISample
    {
        private readonly IComponent _content;

        public DiagramSample()
        {
            // Basic example - mirrors a "connected bindings" style view
            var basicSource  = DiagramNode("llm-prices").SetIcon(UIcons.Database);
            var basicBinding = DiagramNode("Binding").SetIcon(UIcons.Plus).Primary();

            var basicDiagram = Diagram().Connect(basicSource, basicBinding).WS().H(300.px());

            // Pipeline example - auto-layout from connectivity, mixed styles, icon-only circle nodes
            var source    = DiagramNode("Source").SetIcon(UIcons.CloudDownloadAlt);
            var parse     = DiagramNode("Parse").SetIcon(UIcons.FileCode).Secondary();
            var enrich    = DiagramNode("Enrich").SetIcon(UIcons.Sparkles).Primary();
            var index     = DiagramNode("Search Index").SetIcon(UIcons.Search).Success();
            var graph     = DiagramNode("Graph").SetIcon(UIcons.ChartNetwork).Danger();
            var iconOnly  = DiagramNode().SetIcon(UIcons.Bolt, color: "white").Color("linear-gradient(135deg, #6a11cb, #2575fc)", "white", "transparent");
            var custom    = DiagramNode("Custom Colors").SetIcon(UIcons.Palette, color: "#7c4700").Color("#ffd9a1", "#7c4700", "#e0a96d");

            var pipeline = Diagram()
               .Connect(source, parse)
               .Connect(parse, enrich)
               .Connect(enrich, index)
               .Connect(enrich, graph)
               .Connect(iconOnly, parse)
               .Connect(graph, custom)
               .WS().H(420.px());

            // Events example
            var clickable = DiagramNode("Click Me").SetIcon(UIcons.CursorFinger).Primary()
               .OnClick(() => Toast().Success("Node clicked!"))
               .OnContextMenu(() => Toast().Information("Node context menu!"));

            var pinned = DiagramNode("Pinned at (260, 40)").SetIcon(UIcons.Thumbtack).At(260, 40);

            var eventsDiagram = Diagram().Connect(clickable, pinned).WS().H(280.px());

            _content = SectionStack().Secondary()
               .SampleTitle(typeof(DiagramSample), UIcons.Workflow, "A flow-chart surface with draggable nodes and auto-computed arrows")
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                        TextBlock("Diagram renders a flow chart on top of a pannable, dotted-grid background. Nodes are pill-shaped components with an optional icon (or image) and text, supporting the usual button tones (default, primary, secondary, success, danger) as well as arbitrary foreground and background colors. Arrows between connected nodes are computed automatically and drawn on a background canvas that follows panning, node dragging and container resizing (via a resize observer)."),
                        TextBlock("Node positions are computed automatically from the node sizes and connectivity (nodes are arranged in layers following the arrows). Dragging a node, or positioning it explicitly with At(x, y), pins it so the automatic layout no longer moves it. Call AutoArrange() to reset all nodes back to the computed layout."))).SetTitle("Overview")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                        SampleDo("Use Connect(from, to) to declare the flow - nodes are added automatically and arrows follow the connection direction."),
                        SampleDo("Use icon-only nodes (no text) for compact junction points - they render as circles."),
                        SampleDont("Don't use a Diagram for static two-axis layouts - use Grid instead."))).SetTitle("Best Practices")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                        SampleSubTitle("Basic"),
                        TextBlock("Two connected nodes. Drag the background to pan, drag the nodes to reposition them."),
                        basicDiagram,
                        SampleSubTitle("Auto-layout, styles and colors"),
                        TextBlock("Nodes are positioned automatically based on their sizes and connectivity. Icon-only nodes render as circles."),
                        pipeline,
                        SampleSubTitle("Events and pinned positions"),
                        TextBlock("Click the left node, or right-click it for the context menu event. The right node was pinned with At(x, y). Clicks are not raised after dragging a node."),
                        eventsDiagram
                    )).SetTitle("Usage")));
        }

        public HTMLElement Render() => _content.Render();
    }
}
