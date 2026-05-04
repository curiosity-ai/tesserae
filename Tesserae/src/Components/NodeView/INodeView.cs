using System;

namespace Tesserae
{
    public interface INodeView
    {
        string Name { get; }
        INodeInput[] Inputs { get; }
        INodeOutput[] Outputs { get; }
    }
}
