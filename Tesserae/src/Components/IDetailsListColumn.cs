using System;
using Tesserae.Helpers.HTML;

namespace Tesserae.Components
{
    public interface IDetailsListColumn : IComponent
    {
        string Name             { get; }

        WidthDimension MinWidth { get; }

        WidthDimension MaxWidth { get; }

        Action OnColumnClick    { get; }
    }
}
