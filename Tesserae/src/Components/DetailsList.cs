using System;
using System.Collections.Generic;
using System.Linq;
using Tesserae.HTML;
using static Tesserae.UI;
using static Retyped.dom;

namespace Tesserae.Components
{
    public class DetailsList<TDetailsListItem> : IComponent
        where TDetailsListItem : IDetailsListItem<TDetailsListItem>
    {
        private readonly int _rowsPerPage;
        private readonly List<IDetailsListColumn> _detailsListColumns;
        private readonly List<TDetailsListItem> _detailsListItems;
        private readonly HTMLElement _innerElement;

        private bool _detailsListColumnHeadersRendered;
        private bool _detailsListItemsContainerRendered;
        private HTMLDivElement _detailsListContainer;
        private HTMLDivElement _detailsListItemsContainer;

        private string _previousColumnSortingKey;
        private HTMLElement _columnSortingIcon;

        public DetailsList(int rowsPerPage = 8)
        {
            _rowsPerPage        = rowsPerPage;
            _detailsListColumns = new List<IDetailsListColumn>();
            _detailsListItems   = new List<TDetailsListItem>();
            _innerElement       = Div(_());

            _previousColumnSortingKey = string.Empty;
        }

        public HTMLElement Render() => _innerElement;

        private HTMLDivElement CreateGridCell(
            IDetailsListColumn column,
            Func<HTMLElement> gridCellInnerHtmlExpression)
        {
            var role = column.IsRowHeader ?
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

        public DetailsList<TDetailsListItem> WithColumn<TDetailsListColumn>(TDetailsListColumn column)
            where TDetailsListColumn : class, IDetailsListColumn
        {
            _detailsListColumns.Add(column);

            return this;
        }

        public DetailsList<TDetailsListItem> WithColumns<TDetailsListColumn>(IEnumerable<TDetailsListColumn> columns)
            where TDetailsListColumn : class, IDetailsListColumn
        {
            _detailsListColumns.AddRange(columns);

            return this;
        }

        public DetailsList<TDetailsListItem> WithListItems(params TDetailsListItem[] listItems)
        {
            if (!_detailsListColumnHeadersRendered)
            {
                RenderColumnHeaders();
            }

            if (!_detailsListItemsContainerRendered)
            {
                RenderDetailsListItemsContainer();
            }

            _detailsListItems.AddRange(listItems);

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

                // TODO: Add role of "button" to this element.
                var columnHtmlElement = column.Render();

                columnHeader.SetStyle(cssStyleDeclaration =>
                {
                    cssStyleDeclaration.width = column.Width.ToString();
                });

                if (column.EnableOnColumnClickEvent || column.EnableColumnSorting)
                {
                    columnHeader.classList.add("tss-cursor-pointer");
                }

                if (column.EnableColumnSorting && !column.EnableOnColumnClickEvent)
                {
                    columnHeader.addEventListener(
                        "click", () => SortColumns(column.SortingKey, columnHtmlElement));
                }

                if (!column.EnableColumnSorting && !column.EnableOnColumnClickEvent)
                {
                    columnHeader.addEventListener("click", column.OnColumnClick);
                }

                columnHeader.appendChild(columnHtmlElement);

                if (column.IsRowHeader)
                {
                    _columnSortingIcon =
                        I(LineAwesome.ArrowUp, cssClass: "tss-detailslist-column-header-sorting-icon");

                    columnHtmlElement.appendChild(_columnSortingIcon);
                }

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

        private void RenderDetailsListItems(IEnumerable<TDetailsListItem> detailsListItems)
        {
            detailsListItems          = detailsListItems.ToList();
            var detailsListItemsCount = detailsListItems.Count();

            /* The overload of select which projects an item belonging to a collection with its index doesn't seem to
             * behave at runtime. - MB 12/02/2020 */

            // var detailsListItemsWithIndex =
                // detailsListItems.Select((item, index) => new { value = item, index = index + 1 });

            var index = 1;
            foreach (var detailsListItem in detailsListItems)
            {
                var detailsListItemContainer = Div(_("tss-detailslist-list-item-container").WithRole("presentation"));
                var gridCellHtmlElements     = detailsListItem.Render(_detailsListColumns, CreateGridCell).ToArray();

                if (detailsListItemsCount == index)
                {
                    var lastGridCellHtmlElement = gridCellHtmlElements.Last();
                    AttachOnLastGridCellMountedEvent(lastGridCellHtmlElement);
                }

                if (detailsListItem.EnableOnListItemClickEvent)
                {
                    var indexCopy = index;

                    detailsListItemContainer.addEventListener("click",
                        () => detailsListItem.OnListItemClick(indexCopy));

                    detailsListItemContainer.classList.add("tss-cursor-pointer");
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

        private void SortColumns(string columnSortingKey, HTMLElement columnHtmlElement)
        {
           _detailsListItemsContainer.classList.add("fade");

            window.setTimeout(_ =>
            {
                _detailsListItemsContainer.RemoveChildElements();

                if (_previousColumnSortingKey.Equals(columnSortingKey))
                {
                    _detailsListItems.Reverse();
                }
                else
                {
                    _detailsListItems.Sort(
                        (detailsListItem, detailsListItemOther)
                            => detailsListItem.CompareTo(detailsListItemOther, columnSortingKey));

                    if (!string.IsNullOrWhiteSpace(columnSortingKey))
                    {
                        columnHtmlElement.appendChild(_columnSortingIcon);
                    }
                }

                RenderDetailsListItems(_detailsListItems);

                _detailsListItemsContainer.classList.remove("fade");
                _previousColumnSortingKey = columnSortingKey;
            }, 0100);

            /* The above magic number is the length of the fade out transition added to the _detailsListItemContainer
             * minus a completely arbitrary amount. Can we access this programmatically? Loop over the item opacity?
             * Could possibly do with adding a fade in transition as well to make the appearance of the newly ordered
             * list items smoother. - MB 16/02/2020 */
        }
    }
}
