using System;

namespace Tesserae
{
    public interface IDynamicNodeView : INodeView
    {
        void UpdateNode(NodeView.InputsState inputs, NodeView.OutputsState outputs, Action<INodeInput> addInput, Action<INodeOutput> addOutput);
    }
}
