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

            nodeView.DefineNode("Hello World", ib => ib.AddInput("inp",  () => NodeView.Interfaces.TextInputInterface("Input", "Hi Input"))
                                                       .AddOutput("out", () => NodeView.Interfaces.TextInputInterface("Output", "Hi Output")));

            nodeView.DefineNode("Complex", ib => ib.AddInput("text", () => NodeView.Interfaces.TextInterface("Input", "Hi Input"))
                                                   .AddInput("int",  () => NodeView.Interfaces.IntegerInterface("Input", 123))
                                                   .AddInput("num",  () => NodeView.Interfaces.NumberInterface("Input", 3.14))
                                                   .AddInput("btn",  () => NodeView.Interfaces.ButtonInterface("Input", () => Toast().Information("Hi!")))
                                                   .AddInput("chk",  () => NodeView.Interfaces.CheckboxInterface("Input", false))
                                                   .AddInput("sel",  () => NodeView.Interfaces.SelectInterface("Input", "A", new ReadOnlyArray<string>(new[] { "A", "B", "C" })))
                                                   .AddInput("sld",  () => NodeView.Interfaces.SliderInterface("Input", 0.5, 0, 1))
                                                   .AddOutput("out", () => NodeView.Interfaces.TextInterface("Output", "Hi Output")));

            nodeView.DefineDynamicNode("Dynamic", ib => ib.AddInput("inp", () => NodeView.Interfaces.IntegerInterface("Output Count", 1)),
                                                  (inputState, outputState, ib) =>
                                                  {
                                                      var inputCount = inputState["inp"].As<int>();
                                                      for(int i  = 0; i < inputCount; i++)
                                                      {
                                                          ib.AddOutput($"out-{i}", () => NodeView.Interfaces.TextInterface($"out-{i}", i.ToString()));
                                                      }
                                                  });


            var textArea = TextArea().WS().H(10).Grow();

            nodeView.OnChange(v => textArea.Text = v.GetJsonState(true));

            textArea.OnBlur((ta, ev) => nodeView.SetState(ta.Text));

            content = SectionStack()
               .Title(SampleHeader(nameof(NodeViewSample)))
               .Section(Stack().Children(
                    SampleTitle("Overview"),
                    TextBlock("NodeView is a powerful utility for creating node-based visual editors and data flows. It allows you to define custom node types with various input and output interfaces, enabling users to build complex logic or data pipelines graphically.")))
               .Section(Stack().Children(
                    SampleTitle("Best Practices"),
                    TextBlock("Use NodeView for scenarios where users need to define relationships or workflows. Keep node definitions logical and consistent. Provide descriptive names for inputs and outputs. Utilize dynamic nodes when the node structure needs to adapt based on its internal state or external data.")))
               .Section(Stack().Children(
                    SampleTitle("Usage"),
                    SplitView().SplitInMiddle().Resizable().H(600).WS().Left(nodeView).Right(VStack().S().Children(Label("JSON State").SetContent(textArea)))));
        }

        public HTMLElement Render()
        {
            return content.Render();
        }
    }
}