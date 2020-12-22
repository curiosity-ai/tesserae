using System;

namespace Tesserae
{
    public interface IDetailsListColumn : IComponent
    {
        string SortingKey             { get; }

        UnitSize Width                { get; }

        bool IsRowHeader              { get; }

        bool EnableColumnSorting      { get; }

        bool EnableOnColumnClickEvent { get; }

        void OnColumnClick();
    }
}
