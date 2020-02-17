using System;
using System.Collections.Generic;
using System.Linq;
using Tesserae.HTML;
using static Tesserae.UI;
using static Retyped.dom;

namespace Tesserae.Components
{
    public class DetailsList<TDetailsListItem> : IComponent
        where TDetailsListItem : class, IDetailsListItem<TDetailsListItem>
    {
        private readonly int _rowsPerPage;
        private readonly List<IDetailsListColumn> _detailsListColumns;
        private readonly ComponentCache<TDetailsListItem> _componentCache;
        private readonly HTMLElement _innerElement;

        private bool _detailsListColumnHeadersRendered;
        private bool _detailsListItemsContainerRendered;
        private bool _detailsListItemsRendered;

        private HTMLDivElement _detailsListContainer;
        private HTMLDivElement _detailsListItemsContainer;

        private string _previousColumnSortingKey;
        private LineAwesome _currentLineAwesomeSortingIcon;
        private HTMLElement _columnSortingIcon;

        public DetailsList(int rowsPerPage = 8)
        {
            if (rowsPerPage <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(rowsPerPage));
            }

            _rowsPerPage        = rowsPerPage;
            _detailsListColumns = new List<IDetailsListColumn>();
            _componentCache     = new ComponentCache<TDetailsListItem>(CreateDetailsListItem);
            _innerElement       = Div(_());

            _previousColumnSortingKey      = string.Empty;
            _currentLineAwesomeSortingIcon = LineAwesome.ArrowUp;
        }

        private static HTMLDivElement CreateGridCell(
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
            return AddColumns(column);
        }

        public DetailsList<TDetailsListItem> WithColumns<TDetailsListColumn>(IEnumerable<TDetailsListColumn> columns)
            where TDetailsListColumn : class, IDetailsListColumn
        {
            return AddColumns(columns.ToArray());
        }

        public DetailsList<TDetailsListItem> WithListItems(params TDetailsListItem[] listItems)
        {
            if (_detailsListItemsRendered)
            {
                throw new InvalidOperationException("Can not add list items to the component after the " +
                                                    "existing list items have been rendered");
            }

            _componentCache.AddComponents(listItems);

            return this;
        }

        public HTMLElement Render()
        {
            if (!_detailsListColumnHeadersRendered)
            {
                RenderColumnHeaders();
            }

            if (!_detailsListItemsContainerRendered)
            {
                RenderDetailsListItemsContainer();
            }

            if (!_detailsListItemsRendered)
            {
                RenderDetailsListItems();
            }

            return _innerElement;
        }

        private DetailsList<TDetailsListItem> AddColumns<TDetailsListColumn>(params TDetailsListColumn[] columns)
            where TDetailsListColumn : class, IDetailsListColumn
        {
            if (_detailsListColumnHeadersRendered)
            {
                throw new InvalidOperationException("Can not add columns to the component after the " +
                                                    "existing columns have been rendered");
            }

            _detailsListColumns.AddRange(columns);

            return this;
        }

        private void RenderColumnHeaders()
        {
            if (!_detailsListColumns.Any())
            {
                throw new InvalidOperationException("Can not render component without columns");
            }

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
                    CreateColumnSortingIcon();

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

        private void CreateColumnSortingIcon()
        {
            _columnSortingIcon =
                I(_currentLineAwesomeSortingIcon, cssClass: "tss-detailslist-column-header-sorting-icon");
        }

        private void RenderDetailsListItemsContainer()
        {
            _detailsListItemsContainer = Div(_("tss-detailslist-list-items-container").WithRole("presentation"));
            _detailsListContainer.appendChild(_detailsListItemsContainer);
            _detailsListItemsContainerRendered = true;
        }

        private HTMLElement CreateDetailsListItem(
            (int Key, TDetailsListItem DetailsListItem) detailsListItemAndKey)
        {
            var (detailsListItemNumber, detailsListItem) = detailsListItemAndKey;

            var detailsListItemContainer = Div(_("tss-detailslist-list-item-container").WithRole("presentation"));
            var gridCellHtmlElements     = detailsListItem.Render(_detailsListColumns, CreateGridCell).ToArray();

            if (_componentCache.ComponentsCount == detailsListItemNumber)
            {
                var lastGridCellHtmlElement = gridCellHtmlElements.Last();
                AttachOnLastGridCellMountedEvent(lastGridCellHtmlElement);
            }

            if (detailsListItem.EnableOnListItemClickEvent)
            {
                var detailsListItemNumberCopy = detailsListItemNumber;

                detailsListItemContainer.addEventListener("click",
                    () => detailsListItem.OnListItemClick(detailsListItemNumberCopy));

                detailsListItemContainer.classList.add("tss-cursor-pointer");
            }

            detailsListItemContainer.AppendChildren(gridCellHtmlElements);

            return detailsListItemContainer;
        }

        private void RenderDetailsListItems()
        {
            _detailsListItemsContainer.AppendChildren(_componentCache.RetrieveAllComponentsFromCache().ToArray());

            _detailsListItemsRendered = true;
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
                    // _detailsListItems.Reverse();

                    UpdateColumnSortingIcon(columnHtmlElement, InvertLineAwesomeColumnSortingIcon);
                }
                else
                {
                    // _detailsListItems.Sort(
                    //     (detailsListItem, detailsListItemOther)
                    //         => detailsListItem.CompareTo(detailsListItemOther, columnSortingKey));

                    if (!string.IsNullOrWhiteSpace(columnSortingKey))
                    {
                        UpdateColumnSortingIcon(columnHtmlElement, () =>
                        {
                            _currentLineAwesomeSortingIcon = LineAwesome.ArrowUp;
                        });
                    }
                }

                RenderDetailsListItems();

                _detailsListItemsContainer.classList.remove("fade");
                _previousColumnSortingKey = columnSortingKey;
            }, 0100);

            /* The above magic number is the length of the fade out transition added to the _detailsListItemContainer
             * minus a completely arbitrary amount. Can we access this programmatically? Loop over the item opacity?
             * Could possibly do with adding a fade in transition as well to make the appearance of the newly ordered
             * list items smoother. - MB 16/02/2020 */
        }

        private void InvertLineAwesomeColumnSortingIcon()
        {
            _currentLineAwesomeSortingIcon =
                _currentLineAwesomeSortingIcon == LineAwesome.ArrowUp ?
                    LineAwesome.ArrowDown
                    : LineAwesome.ArrowUp;
        }

        private void UpdateColumnSortingIcon(HTMLElement htmlElement, Action setLineAwesomeIconExpression)
        {
            _columnSortingIcon.remove();
            setLineAwesomeIconExpression();
            CreateColumnSortingIcon();
            htmlElement.appendChild(_columnSortingIcon);
        }
    }
}
