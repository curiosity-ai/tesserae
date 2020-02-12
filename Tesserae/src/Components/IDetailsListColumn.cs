using System;
using Tesserae.Helpers.HTML;

namespace Tesserae.Components
{
    public interface IDetailsListColumn : IComponent
    {
        string Title         { get; }

        WidthDimension Width { get; }

        bool IsRowHeader     { get; }

        Action OnColumnClick { get; }
    }
}
