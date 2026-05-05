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

    public interface INodeInput<T> : INodeInput
    {
        T DefaultValue { get; }
    }

    public interface INodeOutput<T> : INodeOutput
    {
        T DefaultValue { get; }
    }
}
