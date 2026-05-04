using System;

namespace Tesserae
{
    public interface IDynamicNodeView : INodeView
    {
        void UpdateNode(NodeView.InputsState inputs, NodeView.OutputsState outputs, NodeView.InterfaceBuilder ib);
    }
}
