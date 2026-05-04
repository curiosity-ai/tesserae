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

        public NodeViewSample()
        {
            var nodeView    = NodeView().S();

            nodeView.DefineNode("Hello World", ib => ib.TextInput("inp", "Input", "Hi Input")
                                                       .TextOutput("out", "Output", "Hi Output"));

            nodeView.DefineNode("Complex", ib => ib.TextInput("text", "Input", "Hi Input")
                                                   .IntegerInput("int", "Input", 123)
                                                   .NumberInput("num", "Input", 3.14)
                                                   .ButtonInput("btn", "Input", () => Toast().Information("Hi!"))
                                                   .CheckboxInput("chk", "Input", false)
                                                   .SelectInput("sel", "Input", "A", new[] { "A", "B", "C" })
                                                   .SliderInput("sld", "Input", 0.5, 0, 1)
                                                   .TextOutput("out", "Output", "Hi Output"));

            nodeView.DefineDynamicNode("Dynamic", ib => ib.IntegerInput("inp", "Output Count", 1),
                                                  (inputState, outputState, ib) =>
                                                  {
                                                      var inputCount = inputState["inp"].As<int>();
                                                      for(int i  = 0; i < inputCount; i++)
                                                      {
                                                          ib.TextOutput($"out-{i}", $"out-{i}", i.ToString());
                                                      }
                                                  });

            var workflow = NodeView.Workflow();

            var node1 = workflow.AddNode("Hello World", "My First Node").Position(100, 100).Width(200);
            node1.Input("inp", "Hello");
            var node1Out = node1.Output("out");

            var node2 = workflow.AddNode("Complex", "A Complex Node").Position(400, 100).Width(320);
            var node2Inp = node2.Input("text", "World");
            node2.Input("int", 42);
            node2.Input("num", 9.99);
            node2.Input("btn", null);
            node2.Input("chk", true);
            node2.Input("sel", "B");
            node2.Input("sld", 0.75);
            node2.Output("out");

            workflow.Connect(node1Out, node2Inp);

            nodeView.SetState(workflow.Build());


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