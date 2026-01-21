using System;

namespace Tesserae
{
    /// <summary>
    /// Defines a column in a DetailsList component.
    /// </summary>
    [H5.Name("tss.IDetailsListColumn")]
    public interface IDetailsListColumn : IComponent
    {
        /// <summary>Gets the key used for sorting this column.</summary>
        string   SortingKey               { get; }
        /// <summary>Gets the preferred width of the column.</summary>
        UnitSize Width                    { get; }
        /// <summary>Gets the maximum width of the column.</summary>
        UnitSize MaxWidth                 { get; }
        /// <summary>Gets whether this column acts as the row header.</summary>
        bool     IsRowHeader              { get; }
        /// <summary>Gets whether sorting is enabled for this column.</summary>
        bool     EnableColumnSorting      { get; }
        /// <summary>Gets whether click events are enabled for this column header.</summary>
        bool     EnableOnColumnClickEvent { get; }
        /// <summary>Handles the column header click event.</summary>
        void     OnColumnClick();
    }
}