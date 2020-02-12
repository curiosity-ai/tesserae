using System;
using Tesserae.Helpers.HTML;

namespace Tesserae.Components
{
    public interface IDetailsListColumn : IComponent
    {
        string Title           { get; }

        WidthDimension MinWidth { get; }

        WidthDimension MaxWidth { get; }

        bool IsRowHeader        { get; }

        Action OnColumnClick    { get; }
    }
}
