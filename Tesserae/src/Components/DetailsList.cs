using System;
using System.Collections.Generic;
using System.Linq;
using Tesserae.HTML;
using static Tesserae.UI;
using static Retyped.dom;

namespace Tesserae.Components
{
    public class DetailsList<TDetailListItem> : IComponent
        where TDetailListItem : class, IDetailsListItem
    {
        private readonly int _rowsPerPage;
        private readonly List<IDetailsListColumn<TDetailListItem>> _detailsListColumns;
        private readonly List<IEnumerable<TDetailListItem>> _detailsListItems;
        private readonly HTMLElement _innerElement;

        private bool _detailsListColumnHeadersRendered;
        private bool _detailsListItemsContainerRendered;
        private HTMLDivElement _detailsListContainer;
        private HTMLDivElement _detailsListItemsContainer;

        public DetailsList(
            int rowsPerPage = 8)
        {
            _rowsPerPage        = rowsPerPage;
            _detailsListColumns = new List<IDetailsListColumn<TDetailListItem>>();
            _detailsListItems   = new List<IEnumerable<TDetailListItem>>();
            _innerElement       = Div(_());
        }

        public HTMLElement Render() => _innerElement;

        private static HTMLDivElement CreateGridCell(
            IDetailsListColumn<TDetailListItem> column,
            Func<HTMLElement> gridCellInnerHtmlExpression)
        {
            string role = column.IsRowHeader ?
                "rowheader"
                : "gridcell";

            var gridCellHtmlElement = Div(
                _("tss-detailslist-list-item tss-text-ellipsis",
                    styles: cssStyleDeclaration =>
                    {
                        cssStyleDeclaration.width = column.Width.ToString();
                    })
                    .WithRole(role));

            gridCellHtmlElement.appendChild(gridCellInnerHtmlExpression());

            return gridCellHtmlElement;
        }

        public DetailsList<TDetailListItem> WithColumn<TColumn>(TColumn column)
            where TColumn : class, IDetailsListColumn<TDetailListItem>
        {
            _detailsListColumns.Add(column);
            return this;
        }

        public DetailsList<TDetailListItem> WithColumns<TColumn>(IEnumerable<TColumn> columns)
            where TColumn : class, IDetailsListColumn<TDetailListItem>
        {
            _detailsListColumns.AddRange(columns);
            return this;
        }

        public DetailsList<TDetailListItem> WithListItems(params TDetailListItem[] listItems)
        {
            if (!_detailsListColumnHeadersRendered)
            {
                RenderColumnHeaders();
            }

            if (!_detailsListItemsContainerRendered)
            {
                RenderDetailsListItemsContainer();
            }

            _detailsListItems.Add(listItems);

            RenderDetailsListItems(listItems);

            return this;
        }

        private void RenderColumnHeaders()
        {
            _detailsListContainer = Div(_("tss-detailslist").WithRole("grid"));
            _innerElement.appendChild(_detailsListContainer);

            var detailsListHeader = Div(_("tss-detailslist-header").WithRole("presentation"));
            _detailsListContainer.appendChild(detailsListHeader);

            foreach (var column in _detailsListColumns)
            {
                var columnHeader = Div(_("tss-detailslist-column-header").WithRole("columnheader"));

                var columnHtmlElement = column.Render();

                columnHeader.SetStyle(cssStyleDeclaration =>
                {
                    cssStyleDeclaration.width = column.Width.ToString();
                });

                columnHeader.appendChild(columnHtmlElement);
                detailsListHeader.appendChild(columnHeader);
            }

            var totalWidth = _detailsListColumns.Sum(detailsListColumn => detailsListColumn.Width.Size);

            _detailsListContainer.SetStyle(cssStyleDeclaration =>
            {
                cssStyleDeclaration.width = (totalWidth + (totalWidth / 4)).px().ToString();
            });

            _detailsListColumnHeadersRendered = true;
        }

        private void RenderDetailsListItemsContainer()
        {
            _detailsListItemsContainer = Div(_("tss-detailslist-list-items-container").WithRole("presentation"));

            _detailsListContainer.appendChild(_detailsListItemsContainer);

            _detailsListItemsContainerRendered = true;
        }

        private void RenderDetailsListItems(IEnumerable<TDetailListItem> detailsListItems)
        {
            detailsListItems          = detailsListItems.ToList();
            var detailsListItemsCount = detailsListItems.Count();

            /* The overload of select which projects an item belonging to a collection with its index doesn't seem to
             * behave at runtime - MB 12/02/2020 */
            // var detailsListItemsWithIndex =
                // detailsListItems.Select((item, index) => new { value = item, index = index + 1 });

            var index = 1;
            foreach (var detailsListItem in detailsListItems)
            {
                var detailsListItemContainer = Div(_("tss-detailslist-list-item-container").WithRole("presentation"));
                var gridCellHtmlElements     = detailsListItem.Render(_detailsListColumns, CreateGridCell).ToArray();

                if (detailsListItemsCount == index)
                {
                    console.log("hit");
                    var lastGridCellHtmlElement = gridCellHtmlElements.Last();
                    AttachOnLastGridCellMountedEvent(lastGridCellHtmlElement);
                }

                detailsListItemContainer.AppendChildren(gridCellHtmlElements);
                _detailsListItemsContainer.appendChild(detailsListItemContainer);

                index += 1;
            }
        }

        private void AttachOnLastGridCellMountedEvent(HTMLElement gridCell)
        {
            DomMountedObserver.NotifyWhenMounted(gridCell,
                () => OnLastGridCellMounted(gridCell.clientHeight));
        }

        private void OnLastGridCellMounted(int lastGridCellClientHeight)
        {
            var pageHeight = lastGridCellClientHeight * _rowsPerPage;

            _detailsListContainer.SetStyle(cssStyleDeclaration =>
            {
                cssStyleDeclaration.height = pageHeight.px().ToString();
            });
        }
    }
}
