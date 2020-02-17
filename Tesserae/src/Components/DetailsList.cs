using System;
using System.Collections.Generic;
using System.Linq;
using Tesserae.HTML;
using static Tesserae.UI;
using static Retyped.dom;
using static Retyped.dom.Node;

namespace Tesserae.Components
{
    public class DetailsList<TDetailsListItem> : IComponent
        where TDetailsListItem : class, IDetailsListItem<TDetailsListItem>
    {
        private readonly int _rowsPerPage;
        private readonly List<IDetailsListColumn> _columns;
        private readonly ComponentCache<TDetailsListItem> _componentCache;
        private readonly HTMLElement _innerElement;

        private bool _columnHeadersCreated;
        private bool _listItemsContainerCreated;
        private bool _listItemsCreated;

        private HTMLDivElement _listContainer;
        private HTMLDivElement _listItemsContainer;

        private string _previousColumnSortingKey;
        private LineAwesome _currentLineAwesomeSortingIcon;
        private HTMLElement _columnSortingIcon;

        public DetailsList(int rowsPerPage = 8)
        {
            if (rowsPerPage <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(rowsPerPage));
            }

            _rowsPerPage    = rowsPerPage;
            _columns        = new List<IDetailsListColumn>();
            _componentCache = new ComponentCache<TDetailsListItem>(CreateListItem);
            _innerElement   = Div(_());

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
            if (column == null)
            {
                throw new ArgumentNullException(nameof(column));
            }

            return AddColumns(column);
        }

        public DetailsList<TDetailsListItem> WithColumns<TDetailsListColumn>(IEnumerable<TDetailsListColumn> columns)
            where TDetailsListColumn : class, IDetailsListColumn
        {
            if (columns == null)
            {
                throw new ArgumentNullException(nameof(columns));
            }

            var columnsArray = columns.ToArray();

            if (columnsArray.Any())
            {
                throw new ArgumentException(nameof(columns));
            }

            return AddColumns(columnsArray);
        }

        public DetailsList<TDetailsListItem> WithListItems(params TDetailsListItem[] listItems)
        {
            if (_listItemsCreated)
            {
                throw new InvalidOperationException("Can not add list items to the component after the " +
                                                    "existing list items have been created");
            }

            _componentCache.AddComponents(listItems);

            return this;
        }

        public DetailsList<TDetailsListItem> SortedBy(string columnSortingKey)
        {
            if (string.IsNullOrWhiteSpace(columnSortingKey))
            {
                throw new ArgumentException(nameof(columnSortingKey));
            }

            if (_columnHeadersCreated)
            {
                throw new InvalidOperationException("Can not pre-sort list items after column headers " +
                                                    "have been created");
            }

            if (_listItemsCreated)
            {
                throw new InvalidOperationException("Can not pre-sort list items after the existing " +
                                                    "list items have been created");
            }

            SortListItems(columnSortingKey);

            return this;
        }

        public HTMLElement Render()
        {
            if (!_columnHeadersCreated)
            {
                CreateColumnHeaders();
            }

            if (!_listItemsContainerCreated)
            {
                CreateListItemsContainer();
            }

            if (!_listItemsCreated)
            {
                CreateListItems();
            }

            return _innerElement;
        }

        private DetailsList<TDetailsListItem> AddColumns<TDetailsListColumn>(params TDetailsListColumn[] columns)
            where TDetailsListColumn : class, IDetailsListColumn
        {
            if (_columnHeadersCreated)
            {
                throw new InvalidOperationException("Can not add columns to the component after the " +
                                                    "existing column headers have been created");
            }

            _columns.AddRange(columns);

            return this;
        }

        private void CreateColumnHeaders()
        {
            if (!_columns.Any())
            {
                throw new InvalidOperationException("Can not create component without columns");
            }

            _listContainer = Div(_("tss-detailslist").WithRole("grid"));
            _innerElement.appendChild(_listContainer);

            var detailsListHeader = Div(_("tss-detailslist-header").WithRole("presentation"));
            _listContainer.appendChild(detailsListHeader);

            foreach (var column in _columns)
            {
                CreateColumnHeader(column, detailsListHeader);
            }

            var totalWidth = _columns.Sum(detailsListColumn => detailsListColumn.Width.Size);

            _listContainer.SetStyle(cssStyleDeclaration =>
            {
                cssStyleDeclaration.width = (totalWidth + (totalWidth / 4)).px().ToString();
            });

            _columnHeadersCreated = true;
        }

        private void CreateColumnHeader(IDetailsListColumn column, Interface detailsListHeader)
        {
            var columnHeader = Div(_("tss-detailslist-column-header").WithRole("columnheader"));

            // TODO: Add role of "button" to this element.
            var columnHtmlElement = column.Render();

            columnHeader.SetStyle(cssStyleDeclaration => { cssStyleDeclaration.width = column.Width.ToString(); });

            if (column.EnableOnColumnClickEvent || column.EnableColumnSorting)
            {
                columnHeader.classList.add("tss-cursor-pointer");
            }

            if (column.EnableColumnSorting && !column.EnableOnColumnClickEvent)
            {
                columnHeader.addEventListener(
                    "click", () => SortList(column.SortingKey, columnHtmlElement));
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

        private void CreateColumnSortingIcon()
        {
            _columnSortingIcon =
                I(_currentLineAwesomeSortingIcon, cssClass: "tss-detailslist-column-header-sorting-icon");
        }

        private void CreateListItemsContainer()
        {
            _listItemsContainer = Div(_("tss-detailslist-list-items-container").WithRole("presentation"));
            _listContainer.appendChild(_listItemsContainer);
            _listItemsContainerCreated = true;
        }

        private HTMLElement CreateListItem(
            (int Key, TDetailsListItem DetailsListItem) detailsListItemAndKey)
        {
            var (detailsListItemNumber, detailsListItem) = detailsListItemAndKey;

            var detailsListItemContainer = Div(_("tss-detailslist-list-item-container").WithRole("presentation"));
            var gridCellHtmlElements     = detailsListItem.Render(_columns, CreateGridCell).ToArray();

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

        private void CreateListItems()
        {
            _listItemsContainer.AppendChildren(_componentCache.RetrieveAllComponentsFromCache().ToArray());
            _listItemsCreated = true;
        }

        private void AttachOnLastGridCellMountedEvent(HTMLElement gridCell)
        {
            DomMountedObserver.NotifyWhenMounted(gridCell,
                () => OnLastGridCellMounted(gridCell.clientHeight));
        }

        private void OnLastGridCellMounted(int lastGridCellClientHeight)
        {
            var pageHeight = lastGridCellClientHeight * _rowsPerPage;

            _listContainer.SetStyle(cssStyleDeclaration =>
            {
                cssStyleDeclaration.height = pageHeight.px().ToString();
            });
        }

        private void SortList(string columnSortingKey, Interface columnHtmlElement)
        {
           _listItemsContainer.classList.add("fade");

            window.setTimeout(_ =>
            {
                _listItemsContainer.RemoveChildElements();

                if (_previousColumnSortingKey.Equals(columnSortingKey))
                {
                    _componentCache.ReverseComponentOrder();
                    UpdateColumnSortingIcon(columnHtmlElement, InvertLineAwesomeColumnSortingIcon);
                }
                else
                {
                    SortListItems(columnSortingKey);

                    if (!string.IsNullOrWhiteSpace(columnSortingKey))
                    {
                        UpdateColumnSortingIcon(columnHtmlElement, () =>
                        {
                            _currentLineAwesomeSortingIcon = LineAwesome.ArrowUp;
                        });
                    }
                }

                CreateListItems();

                _listItemsContainer.classList.remove("fade");
                _previousColumnSortingKey = columnSortingKey;
            }, 0100);

            /* The above magic number is the length of the fade out transition added to the _detailsListItemContainer
             * minus a completely arbitrary amount. Can we access this programmatically? Loop over the item opacity?
             * Could possibly do with adding a fade in transition as well to make the appearance of the newly ordered
             * list items smoother. - MB 16/02/2020 */
        }

        private void SortListItems(string columnSortingKey)
        {
             _componentCache.SortComponents(
                 (detailsListItem, detailsListItemOther)
                     => detailsListItem.CompareTo(detailsListItemOther, columnSortingKey));
        }

        private void InvertLineAwesomeColumnSortingIcon()
        {
            _currentLineAwesomeSortingIcon =
                _currentLineAwesomeSortingIcon == LineAwesome.ArrowUp ?
                    LineAwesome.ArrowDown
                    : LineAwesome.ArrowUp;
        }

        private void UpdateColumnSortingIcon(Interface htmlElement, Action setLineAwesomeIconExpression)
        {
            _columnSortingIcon.remove();
            setLineAwesomeIconExpression();
            CreateColumnSortingIcon();
            htmlElement.appendChild(_columnSortingIcon);
        }
    }
}
