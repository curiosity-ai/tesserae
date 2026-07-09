using System;
using System.Threading.Tasks;
using static H5.Core.dom;
using static Tesserae.UI;
using static Tesserae.Tests.Samples.SamplesHelper;

namespace Tesserae.Tests.Samples
{
    [SampleDetails(Group = "Utilities", Order = 0, Icon = UIcons.Network)]
    public class NodeViewSample : IComponent, ISample
    {
        private readonly IComponent content;

        public class HelloWorldNode : INodeView
        {
            public string Name => "Hello World";
            public INodeInput[] Inputs => new INodeInput[] { new NodeInput<string>("inp", "Input", "Hi Input") };
            public INodeOutput[] Outputs => new INodeOutput[] { new NodeOutput<string>("out", "Output", "Hi Output") };
        }

        public class ComplexNode : INodeView
        {
            public string Name => "Complex";
            public INodeInput[] Inputs => new INodeInput[]
            {
                new NodeInput<string>("text", "Input", "Hi Input"),
                new NodeInput<int>("int", "Input", 123),
                new NodeInput<double>("num", "Input", 3.14),
                new NodeInput<Action>("btn", "Input", () => Toast().Information("Hi!")),
                new NodeInput<bool>("chk", "Input", false),
                new NodeSelectInput("sel", "Input", "A", new ReadOnlyArray<string>(new[] { "A", "B", "C" })),
                new NodeSliderInput("sld", "Input", 0.5, 0, 1)
            };
            public INodeOutput[] Outputs => new INodeOutput[] { new NodeOutput<string>("out", "Output", "Hi Output") };
        }

        public class DynamicNode : IDynamicNodeView
        {
            public string Name => "Dynamic";

            public INodeInput[] Inputs => new INodeInput[] { new NodeInput<int>("inp", "Output Count", 1) };
            public INodeOutput[] Outputs => null;

            public void UpdateNode(NodeView.InputsState inputs, NodeView.OutputsState outputs, Action<INodeInput> addInput, Action<INodeOutput> addOutput)
            {
                var inputCount = inputs["inp"].As<int>();
                for(int i  = 0; i < inputCount; i++)
                {
                    addOutput(new NodeOutput<string>($"out-{i}", $"out-{i}", i.ToString()));
                }
            }
        }

        public NodeViewSample()
        {
            var nodeView    = NodeView().S();

            nodeView.Register<HelloWorldNode>();
            nodeView.Register<ComplexNode>();
            nodeView.Register<DynamicNode>();

            var stateBuilder = NodeView.State();

            var node1 = stateBuilder.AddNode("Hello World", "My First Node", 100, 100)
                .AddInput("inp", "Hello");

            var node2 = stateBuilder.AddNode("Complex", "A Complex Node", 400, 100, 320)
                .AddInput("text", "World")
                .AddInput("int", 42)
                .AddInput("num", 9.99)
                .AddInput("btn", null)
                .AddInput("chk", true)
                .AddInput("sel", "B")
                .AddInput("sld", 0.75);

            node1.ConnectTo("out", node2, "text");

            nodeView.SetState(stateBuilder.Build());


            var textArea = TextArea().WS().H(10).Grow();

            nodeView.OnChange(v => textArea.Text = v.GetJsonState(true));

            textArea.OnBlur((ta, ev) => nodeView.SetState(ta.Text));

            content = SectionStack().Secondary()
               .SampleTitle(typeof(NodeViewSample), UIcons.Sitemap, "A utility to display nodes")
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    TextBlock("NodeView is a powerful utility for creating node-based visual editors and data flows. It allows you to define custom node types with various input and output interfaces, enabling users to build complex logic or data pipelines graphically."))).SetTitle("Overview")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    TextBlock("Use NodeView for scenarios where users need to define relationships or workflows. Keep node definitions logical and consistent. Provide descriptive names for inputs and outputs. Utilize dynamic nodes when the node structure needs to adapt based on its internal state or external data."))).SetTitle("Best Practices")))
               .FlatSection(Stack().Children(
                    Card(VStack().WS().Children(
                    SplitView().SplitInMiddle().Resizable().H(600).WS().Left(nodeView).Right(VStack().S().Children(Label("JSON State"), textArea)))).SetTitle("Usage")));
        }

        public HTMLElement Render()
        {
            return content.Render();
        }
    }
}