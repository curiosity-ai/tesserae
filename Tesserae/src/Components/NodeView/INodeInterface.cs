using System;

namespace Tesserae
{
    public interface INodeInput
    {
        string Name { get; }
        NodeView.NodeInterface Build();
    }

    public interface INodeOutput
    {
        string Name { get; }
        NodeView.NodeInterface Build();
    }
}
