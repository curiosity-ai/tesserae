using System;
using System.Collections.Generic;
using static H5.Core.dom;

namespace Tesserae
{
    /// <summary>
    /// Defines an item in a DetailsList component.
    /// </summary>
    [H5.Name("tss.IDetailsListItem")]
    public interface IDetailsListItem
    {
        /// <summary>Gets whether click events are enabled for this list item.</summary>
        bool EnableOnListItemClickEvent { get; }

        /// <summary>Handles the list item click event.</summary>
        /// <param name="listItemIndex">The index of the clicked item.</param>
        void OnListItemClick(int listItemIndex);

        /// <summary>Renders the list item components based on the provided columns.</summary>
        /// <param name="columns">The list of columns.</param>
        /// <param name="createGridCellExpression">A function to create a grid cell expression.</param>
        /// <returns>A collection of rendered components.</returns>
        IEnumerable<IComponent> Render(IList<IDetailsListColumn> columns, Func<IDetailsListColumn, Func<IComponent>, IComponent> createGridCellExpression);
    }

    /// <summary>
    /// Defines a typed item in a DetailsList component, supporting comparison for sorting.
    /// </summary>
    /// <typeparam name="TDetailsListItem">The type of the list item.</typeparam>
    [H5.Name("tss.IDetailsListItemT")]
    public interface IDetailsListItem<in TDetailsListItem> : IDetailsListItem where TDetailsListItem : IDetailsListItem<TDetailsListItem>
    {
        /// <summary>Compares the current item with another item for sorting.</summary>
        /// <param name="other">The other item.</param>
        /// <param name="columnSortingKey">The sorting key of the column.</param>
        /// <returns>An integer indicating the relative order of the items.</returns>
        int CompareTo(TDetailsListItem other, string columnSortingKey);
    }
}