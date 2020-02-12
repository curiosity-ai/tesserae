using System;
using System.Collections.Generic;
using System.Linq;
using static Tesserae.UI;
using static Retyped.dom;

namespace Tesserae.Components
{
    public class DetailsListItem : IDetailsListItem
    {
        public DetailsListItem(
            string iconName,
            string fileName,
            DateTime dateModified,
            string modifiedBy,
            int fileSize)
        {
            IconName     = iconName;
            FileName     = fileName;
            DateModified = dateModified;
            ModifiedBy   = modifiedBy;
            FileSize     = fileSize;
        }

        public string IconName       { get; }

        public string FileName       { get; }

        public DateTime DateModified { get; }

        public string ModifiedBy     { get; }

        public int FileSize          { get; }

        public IEnumerable<HTMLElement> Render(
            IList<IDetailsListColumn> columns,
            Func<IDetailsListColumn, Func<HTMLElement>, HTMLElement> createGridCellExpression)
        {
            yield return createGridCellExpression(columns[0], () => Span(_(text: IconName)));
            yield return createGridCellExpression(columns[1], () => Span(_(text: FileName)));
            yield return createGridCellExpression(columns[2], () => Span(_(text: DateModified.ToShortDateString())));
            yield return createGridCellExpression(columns[3], () => Span(_(text: ModifiedBy)));
            yield return createGridCellExpression(columns[4], () => Span(_(text: FileSize.ToString())));
        }
    }
}
