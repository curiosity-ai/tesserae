using System;

namespace Tesserae.Components
{
    public interface IDetailsListColumn : IComponent
    {
        string SortingKey             { get; }

        string Title                  { get; }

        UnitSize Width                { get; }

        bool IsRowHeader              { get; }

        bool EnableColumnSorting      { get; }

        bool EnableOnColumnClickEvent { get; }

        void OnColumnClick();
    }
}
