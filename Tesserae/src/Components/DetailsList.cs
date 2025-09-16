using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Tesserae.UI;
using static H5.Core.dom;
using static H5.Core.dom.Node;

namespace Tesserae
{
    [H5.Name("tss.DetailsList")]
    public class DetailsList<TDetailsListItem> : IComponent, ISpecialCaseStyling where TDetailsListItem : class, IDetailsListItem<TDetailsListItem>
    {
        private readonly List<IDetailsListColumn>         _columns;
        private readonly ComponentCache<TDetailsListItem> _componentCache;
        private readonly HTMLElement                      _container;
        private readonly HTMLDivElement                   _listContainer;

        private bool _listAlreadyCreated;

        private HTMLDivElement _listItemsContainer;

        private string           _previousColumnSortingKey;
        private UIcons           _currentSortingIcon;
        private HTMLElement      _columnSortingIcon;
        private Func<IComponent> _emptyListMessageGenerator;

        private Func<Task<TDetailsListItem[]>> _getNextItemPage = null;

        public DetailsList(params IDetailsListColumn[] columns)
        {
            if (columns == null)
            {
                throw new ArgumentNullException(nameof(columns));
            }

            if (!columns.Any())
            {
                throw new ArgumentException(nameof(columns));
            }

            _columns        = columns.ToList();
            _componentCache = new ComponentCache<TDetailsListItem>(CreateListItem);
            _listContainer  = Div(_("tss-detailslist").WithRole("grid"));

            _container                = Div(_("tss-detailslist-container"), _listContainer);
            _previousColumnSortingKey = string.Empty;
            _currentSortingIcon       = UIcons.ArrowUp;
        }

        public HTMLElement StylingContainer => _container;

        public bool PropagateToStackItemParent => false;

        public bool IsCompact
        {
            get => _listContainer.classList.contains("tss-small");
            set => _listContainer.UpdateClassIf(value, "tss-small");
        }

        public DetailsList<TDetailsListItem> Compact()
        {
            IsCompact = true;
            return this;
        }

        private static IComponent CreateGridCell(IDetailsListColumn column, Func<IComponent> gridCellInnerHtmlExpression)
        {
            var role = column.IsRowHeader ? "rowheader" : "gridcell";

            var gridCellHtmlElement = Div(_("tss-detailslist-list-item tss-text-ellipsis").WithRole(role));
            gridCellHtmlElement.style.width = column.Width.ToString();
            if (column.MaxWidth is object) gridCellHtmlElement.style.maxWidth = column.MaxWidth.ToString();
            gridCellHtmlElement.appendChild(gridCellInnerHtmlExpression().Render());

            return Raw(gridCellHtmlElement);
        }

        public DetailsList<TDetailsListItem> WithEmptyMessage(Func<IComponent> emptyListMessageGenerator)
        {
            _emptyListMessageGenerator = emptyListMessageGenerator ?? throw new ArgumentNullException(nameof(emptyListMessageGenerator));

            return this;
        }
        public DetailsList<TDetailsListItem> WithPaginatedItems(Func<Task<TDetailsListItem[]>> getNextItemPage)
        {
            _getNextItemPage = getNextItemPage;

            if (_listAlreadyCreated)
            {
                RefreshListItems();
            }

            return this;
        }

        public DetailsList<TDetailsListItem> WithListItems(params TDetailsListItem[] listItems)
        {
            if (listItems is object && listItems.Any())
            {
                _componentCache.AddComponents(listItems);
            }

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

        private void CreateList()
        {
            var detailsListHeader = Div(_("tss-detailslist-header").WithRole("presentation"));
            _listContainer.appendChild(detailsListHeader);

            foreach (var column in _columns)
            {
                CreateColumnHeader(column, detailsListHeader);
            }

            _listItemsContainer = Div(_("tss-detailslist-list-items-container").WithRole("presentation"));
            _listContainer.appendChild(_listItemsContainer);

            if (_columns.All(detailsListColumn => detailsListColumn.Width.Unit == Unit.Pixels))
            {
                var totalWidth = _columns.Sum(detailsListColumn => detailsListColumn.Width.Size + 4);
                detailsListHeader.style.width   = (totalWidth).px().ToString();
                _listContainer.style.width      = $"min(100%, {(totalWidth + 32).px()})";
                _listItemsContainer.style.width = (totalWidth).px().ToString();
            }
            else if (_columns.Any(detailsListColumn => detailsListColumn.Width.Unit == Unit.FR))
            {
                detailsListHeader.style.width   = "100%";
                _listContainer.style.width      = "100%";
                _listItemsContainer.style.width = "100%";
            }
            else
            {
                detailsListHeader.style.width   = "100%";
                _listContainer.style.width      = "100%";
                _listItemsContainer.style.width = "100%";
            }

            DomObserver.WhenMounted(detailsListHeader, () =>
            {
                var rect = (DOMRect)detailsListHeader.getBoundingClientRect();
                _listItemsContainer.style.minHeight = "calc(100% - " + rect.height + "px)";
            });

            RefreshListItems();

            _listAlreadyCreated = true;
        }

        private void RefreshListItems()
        {
            if (_componentCache.HasComponents)
            {
                if (_getNextItemPage is object)
                {
                    var vis = VisibilitySensor(v =>
                    {
                        Task.Run<Task>(async () =>
                        {
                            var nextPageItems = await _getNextItemPage();

                            var vElement = v.Render();
                            _listItemsContainer.removeChild(vElement);

                            if (nextPageItems is object && nextPageItems.Any())
                            {
                                _componentCache.AddComponents(nextPageItems);
                                //this works since a node on the DOM can only exist at one place at a time
                                _listItemsContainer.AppendChildren(_componentCache.GetAllRenderedComponentsFromCache().ToArray());

                                v.Reset();
                                _listItemsContainer.appendChild(vElement);
                            }
                            // if there are no new items, don't add the visibility sensor again, to not trigger repeated updates

                        }).FireAndForget();
                    }, message: TextBlock("Loading..."));

                    _listItemsContainer.RemoveChildElements();
                    _listItemsContainer.AppendChildren(_componentCache.GetAllRenderedComponentsFromCache().ToArray());
                    _listItemsContainer.appendChild(vis.Render());
                }
                else
                {
                    _listItemsContainer.RemoveChildElements();
                    _listItemsContainer.AppendChildren(_componentCache.GetAllRenderedComponentsFromCache().ToArray());
                }
            }
            else if (_emptyListMessageGenerator is object)
            {
                //We render the message so that it fits the whole area from the _listItemsContainer, if it has a pre-defined height, otherwise, we set a min height of 64 px
                var emptyMessage = _emptyListMessageGenerator().Render();

                DomObserver.WhenMounted(emptyMessage, () =>
                {
                    var rect = (DOMRect)_listItemsContainer.getBoundingClientRect();
                    emptyMessage.style.height = Math.Max(64, rect.height) + "px";
                    emptyMessage.style.width  = "100%";
                });
                _listItemsContainer.appendChild(emptyMessage);
            }
        }

        private void CreateColumnHeader(IDetailsListColumn column, Interface detailsListHeader)
        {
            var columnHeader = Div(_("tss-detailslist-column-header").WithRole("columnheader"));

            // TODO: Add role of "button" to this element.
            var columnHtmlElement = column.Render();

            columnHeader.style.width = column.Width.ToString();
            if (column.MaxWidth is object) columnHeader.style.maxWidth = column.MaxWidth.ToString();


            if (column.EnableOnColumnClickEvent || column.EnableColumnSorting)
            {
                columnHeader.classList.add("tss-cursor-pointer");
            }

            if (column.EnableColumnSorting)
            {
                columnHeader.addEventListener("click", () => SortList(column.SortingKey, columnHtmlElement));
            }

            if (column.EnableOnColumnClickEvent)
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
            _columnSortingIcon = I(_currentSortingIcon, cssClass: "tss-detailslist-column-header-sorting-icon");
        }

        private HTMLElement CreateListItem((int Key, TDetailsListItem DetailsListItem) detailsListItemAndKey)
        {
            var (detailsListItemNumber, detailsListItem) = detailsListItemAndKey;

            var detailsListItemContainer = Div(_("tss-detailslist-list-item-container").WithRole("presentation"));
            var gridCellHtmlElements     = detailsListItem.Render(_columns, CreateGridCell).ToArray();

            if (detailsListItem.EnableOnListItemClickEvent)
            {
                var detailsListItemNumberCopy = detailsListItemNumber;

                detailsListItemContainer.addEventListener("click", () => detailsListItem.OnListItemClick(detailsListItemNumberCopy));

                detailsListItemContainer.classList.add("tss-cursor-pointer");
            }

            detailsListItemContainer.AppendChildren(gridCellHtmlElements.Select(c => c.Render()).ToArray());

            return detailsListItemContainer;
        }

        private void SortList(string columnSortingKey, Interface columnHtmlElement)
        {
            _listItemsContainer.classList.add("tss-fade");

            window.setTimeout(_ =>
            {
                if (_previousColumnSortingKey.Equals(columnSortingKey))
                {
                    _componentCache.ReverseComponentOrder();
                    UpdateColumnSortingIcon(columnHtmlElement, InvertSortingIcon);
                }
                else
                {
                    SortListItems(columnSortingKey);

                    if (!string.IsNullOrWhiteSpace(columnSortingKey))
                    {
                        UpdateColumnSortingIcon(columnHtmlElement, () =>
                        {
                            _currentSortingIcon = UIcons.ArrowUp;
                        });
                    }
                }

                RefreshListItems();

                _listItemsContainer.classList.remove("tss-fade");
                _previousColumnSortingKey = columnSortingKey;
            }, 0100);

            /* The above magic number is the length of the fade out transition added to the _detailsListItemContainer
             * minus a completely arbitrary amount. Can we access this programmatically? Loop over the item opacity?
             * Could possibly do with adding a fade in transition as well to make the appearance of the newly ordered
             * list items smoother. - MB 16/02/2020 */
        }

        private void SortListItems(string columnSortingKey) => _componentCache.SortComponents((detailsListItem, detailsListItemOther) => detailsListItem.CompareTo(detailsListItemOther, columnSortingKey));

        private void InvertSortingIcon()
        {
            _currentSortingIcon = _currentSortingIcon == UIcons.ArrowUp ? UIcons.ArrowDown : UIcons.ArrowUp;
        }

        private void UpdateColumnSortingIcon(Interface htmlElement, Action setIconExpression)
        {
            _columnSortingIcon.remove();
            setIconExpression();
            CreateColumnSortingIcon();
            htmlElement.parentElement.appendChild(_columnSortingIcon);
        }
    }
}
