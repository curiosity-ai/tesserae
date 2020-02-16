using System;
using System.Collections.Generic;
using static Tesserae.UI;
using static Retyped.dom;

namespace Tesserae.Components
{
    public class DetailsListItem : IDetailsListItem<DetailsListItem>
    {
        public DetailsListItem(
            LineAwesome fileIcon,
            string fileName,
            DateTime dateModified,
            string modifiedBy,
            int fileSize,
            LineAwesomeSize lineAwesomeSize = LineAwesomeSize.Default)
        {
            FileIcon        = fileIcon;
            FileName        = fileName;
            DateModified    = dateModified;
            ModifiedBy      = modifiedBy;
            FileSize        = fileSize;
            LineAwesomeSize = lineAwesomeSize;
        }

        public LineAwesome FileIcon            { get; }

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

        public int CompareTo(DetailsListItem other, string columnSortingKey)
        {
            if (other == null)
            {
                throw new ArgumentException(nameof(other));
            }

            if (columnSortingKey.Equals(nameof(FileIcon)))
            {
            }

            if (columnSortingKey.Equals(nameof(FileName)))
            {
                return string.Compare(FileName, other.FileName, StringComparison.Ordinal);
            }

            if (columnSortingKey.Equals(nameof(DateModified)))
            {
                return DateModified.CompareTo(other.DateModified);
            }

            if (columnSortingKey.Equals(nameof(ModifiedBy)))
            {
                return string.Compare(FileName, other.FileName, StringComparison.Ordinal);
            }

            if (columnSortingKey.Equals(nameof(FileSize)))
            {
                return FileSize.CompareTo(other.FileSize);
            }

            throw new InvalidOperationException($"Can not match {columnSortingKey} to current list item");
        }

        public IEnumerable<HTMLElement> Render(
            IList<IDetailsListColumn> columns,
            Func<IDetailsListColumn, Func<HTMLElement>, HTMLElement> createGridCellExpression)
        {
            yield return createGridCellExpression(columns[0], () => LA(FileIcon, LineAwesomeSize));
            yield return createGridCellExpression(columns[1], () => Span(_(text: FileName)));
            yield return createGridCellExpression(columns[2], () => Span(_(text: DateModified.ToShortDateString())));
            yield return createGridCellExpression(columns[3], () => Span(_(text: ModifiedBy)));
            yield return createGridCellExpression(columns[4], () => Span(_(text: FileSize.ToString())));
        }
    }
}
