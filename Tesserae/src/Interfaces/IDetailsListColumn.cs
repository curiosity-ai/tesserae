using System;

namespace Tesserae
{
    [H5.Name("tss.IDetailsListColumn")]
    public interface IDetailsListColumn : IComponent
    {
        string   SortingKey               { get; }
        UnitSize Width                    { get; }
        UnitSize MaxWidth                 { get; }
        bool     IsRowHeader              { get; }
        bool     EnableColumnSorting      { get; }
        bool     EnableOnColumnClickEvent { get; }
        void     OnColumnClick();
    }
}