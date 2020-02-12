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
            IEnumerable<IDetailsListColumn> columns,
            Func<IDetailsListColumn, Func<HTMLElement>, HTMLElement> createGridCellExpression)
        {
            columns = columns.ToList();

            var column1 = columns.ElementAt(0);
            var column2 = columns.ElementAt(1);
            var column3 = columns.ElementAt(2);
            var column4 = columns.ElementAt(3);
            var column5 = columns.ElementAt(4);

            yield return createGridCellExpression(column1, () => Span(_(text: IconName)));
            yield return createGridCellExpression(column2, () => Span(_(text: FileName)));
            yield return createGridCellExpression(column3, () => Span(_(text: DateModified.ToShortDateString())));
            yield return createGridCellExpression(column4, () => Span(_(text: ModifiedBy)));
            yield return createGridCellExpression(column5, () => Span(_(text: FileSize.ToString())));
        }
    }
}
