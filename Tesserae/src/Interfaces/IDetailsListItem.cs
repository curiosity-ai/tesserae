using System;
using System.Collections.Generic;
using static H5.Core.dom;

namespace Tesserae
{
    [H5.Name("tss.IDetailsListItem")]
    public interface IDetailsListItem
    {
        bool EnableOnListItemClickEvent { get; }

        void OnListItemClick(int listItemIndex);

        IEnumerable<IComponent> Render(IList<IDetailsListColumn> columns, Func<IDetailsListColumn, Func<IComponent>, IComponent> createGridCellExpression);
    }

    [H5.Name("tss.IDetailsListItemT")]
    public interface IDetailsListItem<in TDetailsListItem> : IDetailsListItem where TDetailsListItem : IDetailsListItem<TDetailsListItem>
    {
        int CompareTo(TDetailsListItem other, string columnSortingKey);
    }
}