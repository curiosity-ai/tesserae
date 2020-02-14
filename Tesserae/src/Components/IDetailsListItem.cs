using System;
using System.Collections.Generic;
using static Retyped.dom;

namespace Tesserae.Components
{
    public interface IDetailsListItem
    {
        bool EnableOnListItemClickEvent { get; }

        void OnListItemClick(int listItemIndex);

        IEnumerable<HTMLElement> Render(
            IList<IDetailsListColumn> columns,
            Func<IDetailsListColumn, Func<HTMLElement>, HTMLElement> createGridCellExpression);
    }
}
