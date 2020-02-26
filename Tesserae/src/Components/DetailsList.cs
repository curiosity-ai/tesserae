using System;
using System.Collections.Generic;
using System.Linq;
using Tesserae.HTML;
using static Tesserae.UI;
using static Retyped.dom;
using static Retyped.dom.Node;

namespace Tesserae.Components
{
    public class DetailsList<TDetailsListItem> : IComponent, ISpecialCaseStyling where TDetailsListItem : class, IDetailsListItem<TDetailsListItem>
    {
        private readonly List<IDetailsListColumn> _columns;
        private readonly ComponentCache<TDetailsListItem> _componentCache;
        private readonly HTMLElement _container;

        private bool _listAlreadyCreated;

        private HTMLDivElement _listContainer;
        private HTMLDivElement _listItemsContainer;

        private string _previousColumnSortingKey;
        private LineAwesome _currentLineAwesomeSortingIcon;
        private HTMLElement _columnSortingIcon;

        public HTMLElement StylingContainer => _listContainer;

        public bool PropagateToStackItemParent => false;

        public DetailsList(bool small = false)
        {
            _columns        = new List<IDetailsListColumn>();
            _componentCache = new ComponentCache<TDetailsListItem>(CreateListItem);
            _listContainer = Div(_("tss-detailslist").WithRole("grid"));

            if(small)
            {
                _listContainer.classList.add("small");
            }
            _container      = DIV(_listContainer);

            _previousColumnSortingKey      = string.Empty;
            _currentLineAwesomeSortingIcon = LineAwesome.ArrowUp;
        }

        private static HTMLDivElement CreateGridCell(IDetailsListColumn column, Func<HTMLElement> gridCellInnerHtmlExpression)
        {
            var role = column.IsRowHeader ? "rowheader" : "gridcell";

            var gridCellHtmlElement = Div(_("tss-detailslist-list-item tss-text-ellipsis").WithRole(role));
            gridCellHtmlElement.style.width  = column.Width.ToString();
            gridCellHtmlElement.appendChild(gridCellInnerHtmlExpression());

            return gridCellHtmlElement;
        }

        public DetailsList<TDetailsListItem> WithColumn<TDetailsListColumn>(TDetailsListColumn column) where TDetailsListColumn : class, IDetailsListColumn
        {
            if (column == null)
            {
                throw new ArgumentNullException(nameof(column));
            }

            return AddColumns(column);
        }

        public DetailsList<TDetailsListItem> WithColumns<TDetailsListColumn>(IEnumerable<TDetailsListColumn> columns) where TDetailsListColumn : class, IDetailsListColumn
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
            _componentCache.Clear();
            _componentCache.AddComponents(listItems);

            if (_listAlreadyCreated)
            {
                RefreshListItems();
            }

            return this;
        }

        public DetailsList<TDetailsListItem> SortedBy(string columnSortingKey)
        {
            if (string.IsNullOrWhiteSpace(columnSortingKey))
            {
                throw new ArgumentException(nameof(columnSortingKey));
            }

            if (_listAlreadyCreated)
            {
                throw new InvalidOperationException("Can not pre-sort list items after column headers have been created");
            }

            SortListItems(columnSortingKey);

            return this;
        }

        public HTMLElement Render()
        {
            if (!_listAlreadyCreated)
            {
                CreateList();
            }

            return _container;
        }

        private DetailsList<TDetailsListItem> AddColumns<TDetailsListColumn>(params TDetailsListColumn[] columns) where TDetailsListColumn : class, IDetailsListColumn
        {
            if (_listAlreadyCreated)
            {
                throw new InvalidOperationException("Can not add columns to the component after the " +
                                                    "existing column headers have been created");
            }

            _columns.AddRange(columns);

            return this;
        }

        private void CreateList()
        {
            if (!_columns.Any())
            {
                throw new InvalidOperationException("Can not create component without columns");
            }

            var totalWidth = _columns.Sum(detailsListColumn => detailsListColumn.Width.Size + 4);
            
            var detailsListHeader = Div(_("tss-detailslist-header").WithRole("presentation"));
            detailsListHeader.style.width   = (totalWidth).px().ToString();
            _listContainer.appendChild(detailsListHeader);
            _listContainer.style.width = $"min(100%, {(totalWidth + 32).px().ToString()})";


            foreach (var column in _columns)
            {
                CreateColumnHeader(column, detailsListHeader);
            }

            _listItemsContainer = Div(_("tss-detailslist-list-items-container").WithRole("presentation"));
            _listItemsContainer.style.width = (totalWidth).px().ToString();
            _listContainer.appendChild(_listItemsContainer);

            RefreshListItems();

            _listAlreadyCreated = true;
        }

        private void RefreshListItems()
        {
            _listItemsContainer.RemoveChildElements();
            _listItemsContainer.AppendChildren(_componentCache.RetrieveAllComponentsFromCache().ToArray());
        }

        private void CreateColumnHeader(IDetailsListColumn column, Interface detailsListHeader)
        {
            var columnHeader = Div(_("tss-detailslist-column-header").WithRole("columnheader"));

            // TODO: Add role of "button" to this element.
            var columnHtmlElement = column.Render();

            columnHeader.style.width = column.Width.ToString();

            if (column.EnableOnColumnClickEvent || column.EnableColumnSorting)
            {
                columnHeader.classList.add("tss-cursor-pointer");
            }

            if (column.EnableColumnSorting && !column.EnableOnColumnClickEvent)
            {
                columnHeader.addEventListener("click", () => SortList(column.SortingKey, columnHtmlElement));
            }

            if (!column.EnableColumnSorting && !column.EnableOnColumnClickEvent)
            {
                columnHeader.addEventListener("click", column.OnColumnClick);
            }

            columnHeader.appendChild(columnHtmlElement);

            if (column.IsRowHeader)
            {
                CreateColumnSortingIcon();

                columnHeader.appendChild(_columnSortingIcon);
            }

            detailsListHeader.appendChild(columnHeader);
        }

        private void CreateColumnSortingIcon()
        {
            _columnSortingIcon = I(_currentLineAwesomeSortingIcon, cssClass: "tss-detailslist-column-header-sorting-icon");
        }

        private HTMLElement CreateListItem((int Key, TDetailsListItem DetailsListItem) detailsListItemAndKey)
        {
            var (detailsListItemNumber, detailsListItem) = detailsListItemAndKey;

            var detailsListItemContainer = Div(_("tss-detailslist-list-item-container").WithRole("presentation"));
            var gridCellHtmlElements     = detailsListItem.Render(_columns, CreateGridCell).ToArray();

            if (_componentCache.ComponentsCount == detailsListItemNumber)
            {
                var lastGridCellHtmlElement = gridCellHtmlElements.Last();
            }

            if (detailsListItem.EnableOnListItemClickEvent)
            {
                var detailsListItemNumberCopy = detailsListItemNumber;

                detailsListItemContainer.addEventListener("click", () => detailsListItem.OnListItemClick(detailsListItemNumberCopy));

                detailsListItemContainer.classList.add("tss-cursor-pointer");
            }

            detailsListItemContainer.AppendChildren(gridCellHtmlElements);

            return detailsListItemContainer;
        }

        private void SortList(string columnSortingKey, Interface columnHtmlElement)
        {
           _listItemsContainer.classList.add("fade");

            window.setTimeout(_ =>
            {
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

                RefreshListItems();

                _listItemsContainer.classList.remove("fade");
                _previousColumnSortingKey = columnSortingKey;
            }, 0100);

            /* The above magic number is the length of the fade out transition added to the _detailsListItemContainer
             * minus a completely arbitrary amount. Can we access this programmatically? Loop over the item opacity?
             * Could possibly do with adding a fade in transition as well to make the appearance of the newly ordered
             * list items smoother. - MB 16/02/2020 */
        }

        private void SortListItems(string columnSortingKey) => _componentCache.SortComponents((detailsListItem, detailsListItemOther) => detailsListItem.CompareTo(detailsListItemOther, columnSortingKey));

        private void InvertLineAwesomeColumnSortingIcon()
        {
            _currentLineAwesomeSortingIcon = _currentLineAwesomeSortingIcon == LineAwesome.ArrowUp ? LineAwesome.ArrowDown : LineAwesome.ArrowUp;
        }

        private void UpdateColumnSortingIcon(Interface htmlElement, Action setLineAwesomeIconExpression)
        {
            _columnSortingIcon.remove();
            setLineAwesomeIconExpression();
            CreateColumnSortingIcon();
            htmlElement.parentElement.appendChild(_columnSortingIcon);
        }
    }
}
