using System;
using System.Collections.Generic;
using static Tesserae.UI;
using static Retyped.dom;

namespace Tesserae.Components
{
    public class DetailsListItem : IDetailsListItem
    {
        public DetailsListItem(
            LineAwesome lineAwesomeIcon,
            string fileName,
            DateTime dateModified,
            string modifiedBy,
            int fileSize,
            LineAwesomeSize lineAwesomeSize = LineAwesomeSize.Default)
        {
            LineAwesomeIcon = lineAwesomeIcon;
            FileName        = fileName;
            DateModified    = dateModified;
            ModifiedBy      = modifiedBy;
            FileSize        = fileSize;
            LineAwesomeSize = lineAwesomeSize;
        }

        public LineAwesome LineAwesomeIcon     { get; }

        public string FileName                 { get; }

        public DateTime DateModified           { get; }

        public string ModifiedBy               { get; }

        public int FileSize                    { get; }

        public LineAwesomeSize LineAwesomeSize { get; }

        public bool EnableOnListItemClickEvent => true;

        public void OnListItemClick(int listItemIndex)
        {
            alert($"You clicked me! List item index: {listItemIndex}, my name is {FileName}");
        }

        public IEnumerable<HTMLElement> Render(
            IList<IDetailsListColumn> columns,
            Func<IDetailsListColumn, Func<HTMLElement>, HTMLElement> createGridCellExpression)
        {
            yield return createGridCellExpression(columns[0], () => LA(LineAwesomeIcon, LineAwesomeSize));
            yield return createGridCellExpression(columns[1], () => Span(_(text: FileName)));
            yield return createGridCellExpression(columns[2], () => Span(_(text: DateModified.ToShortDateString())));
            yield return createGridCellExpression(columns[3], () => Span(_(text: ModifiedBy)));
            yield return createGridCellExpression(columns[4], () => Span(_(text: FileSize.ToString())));
        }
    }
}
