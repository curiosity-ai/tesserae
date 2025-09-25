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
            var textArea = TextArea().WS().H(10).Grow();

            nodeView.OnChange(v => textArea.Text = v.GetState(true));

            textArea.OnBlur((ta, ev) => nodeView.SetState(ta.Text));

            content = SectionStack()
               .Title(SampleHeader(nameof(NodeViewSample)))
               .Section(Stack().Children(
                    SampleTitle("Overview"),
                    TextBlock("Use NodeView for rendering flows.")))
               .Section(SplitView().SplitInMiddle().Resizable().H(600).WS().Left(nodeView).Right(textArea));
        }

        public HTMLElement Render()
        {
            return content.Render();
        }
    }
}