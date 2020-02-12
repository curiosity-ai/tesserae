using System;

namespace Tesserae.Components
{
    public interface IDetailsListColumn : IComponent
    {
        string Title         { get; }

        UnitSize Width       { get; }

        bool IsRowHeader     { get; }

        Action OnColumnClick { get; }
    }
}
