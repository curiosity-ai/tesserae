using System;
using System.Collections.Generic;
using static Retyped.dom;

namespace Tesserae.Components
{
    public interface IDetailsListItem
    {
        IEnumerable<HTMLElement> Render<T>(
            IList<IDetailsListColumn<T>> columns,
            Func<IDetailsListColumn<T>, Func<HTMLElement>, HTMLElement> createGridCellExpression)
                where T : class, IDetailsListItem;
    }
}
