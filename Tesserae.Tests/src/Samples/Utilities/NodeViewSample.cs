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

            content = SectionStack()
               .Title(SampleHeader(nameof(NodeViewSample)))
               .Section(Stack().Children(
                    SampleTitle("Overview"),
                    TextBlock("Use NodeView for rendering flows.")))
               .Section(SplitView().SplitInMiddle().Resizable().H(600).WS().Left(nodeView).Right(CreateDebugger(nodeView)));
        }

        private IComponent CreateDebugger(NodeView nodeView)
        {
            var stack = VStack().S().ScrollY();
            var textArea = TextArea().WS().H(10).Grow();
            nodeView.OnChange(v => textArea.Text = v.GetState(true));
            stack.Add(textArea);
            return stack;
        }

        public HTMLElement Render()
        {
            return content.Render();
        }
    }
}