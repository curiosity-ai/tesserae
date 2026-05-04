using System;

namespace Tesserae
{
    public interface INodeView
    {
        string Name { get; }
        void BuildNode(NodeView.InterfaceBuilder ib);
    }
}
