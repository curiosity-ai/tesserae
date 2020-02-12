using System;
using System.Collections.Generic;
using static Retyped.dom;

namespace Tesserae.Components
{
    public interface IDetailsListItem
    {
        IEnumerable<HTMLElement> Render(
            IEnumerable<IDetailsListColumn> columns,
            Func<IDetailsListColumn, Func<HTMLElement>, HTMLElement> createGridCellExpression);
    }
}
