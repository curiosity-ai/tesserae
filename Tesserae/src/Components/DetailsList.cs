using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Tesserae.UI;
using static Transpose.Core.dom;
using static Transpose.Core.dom.Node;

namespace Tesserae
{
    /// <summary>
    /// A virtualised, sortable, multi-column table for displaying large lists of typed items, similar to Fluent UI's
    /// DetailsList.
    /// </summary>
    [Transpose.Name("tss.DetailsList")]
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

        // The sortable column headers, by sorting key, for aria-sort; and the key SortedBy pre-sorted on.
        private readonly Dictionary<string, HTMLElement> _sortableHeaders = new Dictionary<string, HTMLElement>();
        private string _sortedByKey;

        // The one row in the tab order (roving tabindex): the list is a single Tab stop, and the arrow keys
        // move between its rows.
        private HTMLElement _tabStopRow;

        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
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
            _listContainer  = Div(Att("tss-detailslist").WithRole("grid"));

            _container                = Div(Att("tss-detailslist-container"), _listContainer);
            _previousColumnSortingKey = string.Empty;
            _currentSortingIcon       = UIcons.ArrowUp;
        }

        /// <summary>
        /// Gets or sets the styling container.
        /// </summary>
        public HTMLElement StylingContainer => _container;

        /// <summary>
        /// Gets whether a sizing helper applied to this component should tag it so a wrapper-building container hoists the style onto the wrapper.
        /// </summary>
        public bool PropagateStylesToWrapper => false;

        /// <summary>
        /// Gets or sets a value indicating whether the component is rendered in compact form.
        /// </summary>
        public bool IsCompact
        {
            get => _listContainer.classList.contains("tss-small");
            set => _listContainer.UpdateClassIf(value, "tss-small");
        }

        /// <summary>
        /// Renders the component in a compact form.
        /// </summary>
        public DetailsList<TDetailsListItem> Compact()
        {
            IsCompact = true;
            return this;
        }

        private static IComponent CreateGridCell(IDetailsListColumn column, Func<IComponent> gridCellInnerHtmlExpression)
        {
            var role = column.IsRowHeader ? "rowheader" : "gridcell";

            var gridCellHtmlElement = Div(Att("tss-detailslist-list-item tss-text-ellipsis").WithRole(role));

            if (column.Width.Unit == Unit.FR)
            {
                gridCellHtmlElement.style.width = "10px";
                gridCellHtmlElement.style.flexGrow = column.Width.Size.ToString();
            }
            else
            {
                gridCellHtmlElement.style.width = column.Width.ToString();
                gridCellHtmlElement.style.flexGrow = "0";
            }
            
            gridCellHtmlElement.style.flexShrink = "0";

            if (column.MaxWidth is object) gridCellHtmlElement.style.maxWidth = column.MaxWidth.ToString();
            
            gridCellHtmlElement.appendChild(gridCellInnerHtmlExpression().Render());

            return Raw(gridCellHtmlElement);
        }

        /// <summary>
        /// Returns the component configured with the given empty message.
        /// </summary>
        public DetailsList<TDetailsListItem> WithEmptyMessage(Func<IComponent> emptyListMessageGenerator)
        {
            _emptyListMessageGenerator = emptyListMessageGenerator ?? throw new ArgumentNullException(nameof(emptyListMessageGenerator));

            return this;
        }
        /// <summary>
        /// Returns the component configured with the given paginated items.
        /// </summary>
        public DetailsList<TDetailsListItem> WithPaginatedItems(Func<Task<TDetailsListItem[]>> getNextItemPage)
        {
            _getNextItemPage = getNextItemPage;

            if (_listAlreadyCreated)
            {
                RefreshListItems();
            }

            return this;
        }

        /// <summary>
        /// Returns the component configured with the given list items.
        /// </summary>
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

        /// <summary>
        /// Configures the sorted by on the component.
        /// </summary>
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
            _sortedByKey = columnSortingKey;

            return this;
        }

        /// <summary>
        /// Renders the component's root HTML element.
        /// </summary>
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
            var detailsListHeader = Div(Att("tss-detailslist-header").WithRole("row"));
            _listContainer.appendChild(detailsListHeader);

            foreach (var column in _columns)
            {
                CreateColumnHeader(column, detailsListHeader);
            }

            _listItemsContainer = Div(Att("tss-detailslist-list-items-container").WithRole("rowgroup"));
            _listContainer.appendChild(_listItemsContainer);

            // One listener for every row, present and future: rows come and go with sorting and paging.
            _listItemsContainer.addEventListener("keydown", e => OnRowKeyDown(e.As<KeyboardEvent>()));
            _listItemsContainer.addEventListener("focusin", e => OnRowFocusIn(e));

            if (_columns.All(detailsListColumn => detailsListColumn.Width.Unit == Unit.Pixels))
            {
                var totalWidth = _columns.Sum(detailsListColumn => detailsListColumn.Width.Size + 4);
                detailsListHeader.style.width   = (totalWidth).px().ToString();
                _listContainer.style.width      = $"min(100%, {(totalWidth + 32).px()})";
                _listItemsContainer.style.width = (totalWidth).px().ToString();
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
                                UpdateRowTabStop();
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

                UpdateRowTabStop();
            }
            else if (_emptyListMessageGenerator is object)
            {
                _tabStopRow = null;

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
            var columnHeader = Div(Att("tss-detailslist-column-header").WithRole("columnheader"));

            // TODO: Add role of "button" to this element.
            var columnHtmlElement = column.Render();

            if (column.Width.Unit == Unit.FR)
            {
                columnHeader.style.width = "10px";
                columnHeader.style.flexGrow = column.Width.Size.ToString();
            }
            else
            {
                columnHeader.style.width = column.Width.ToString();
                columnHeader.style.flexGrow = "0";
            }

            columnHeader.style.flexShrink = "0";

            if (column.MaxWidth is object) columnHeader.style.maxWidth = column.MaxWidth.ToString();

            if (column.EnableOnColumnClickEvent || column.EnableColumnSorting)
            {
                columnHeader.classList.add("tss-cursor-pointer");
            }

            if (column.EnableColumnSorting)
            {
                columnHeader.addEventListener("click", () => SortList(column.SortingKey, columnHtmlElement));

                if (!string.IsNullOrWhiteSpace(column.SortingKey))
                {
                    _sortableHeaders[column.SortingKey] = columnHeader;
                    columnHeader.setAttribute("aria-sort", column.SortingKey == _sortedByKey ? "ascending" : "none");
                }
            }

            if (column.EnableOnColumnClickEvent)
            {
                columnHeader.addEventListener("click", column.OnColumnClick);
            }

            // A header that does something on click is reached with Tab and pressed with Enter or Space, the way
            // a button is; the click it raises runs exactly what the pointer's click does.
            if (column.EnableOnColumnClickEvent || column.EnableColumnSorting)
            {
                columnHeader.tabIndex = 0;
                columnHeader.classList.add("tss-detailslist-column-header-interactive");

                columnHeader.addEventListener("keydown", e =>
                {
                    var ke = e.As<KeyboardEvent>();

                    if (ke.target != columnHeader || (ke.key != "Enter" && ke.key != " ")) return;

                    ke.preventDefault();
                    columnHeader.click();
                });
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
            if(_columnSortingIcon is null)
            {
                _columnSortingIcon = I(_currentSortingIcon, cssClass: "tss-detailslist-column-header-sorting-icon");
            }
        }

        private HTMLElement CreateListItem((int Key, TDetailsListItem DetailsListItem) detailsListItemAndKey)
        {
            var (detailsListItemNumber, detailsListItem) = detailsListItemAndKey;

            var detailsListItemContainer = Div(Att("tss-detailslist-list-item-container").WithRole("row"));
            detailsListItemContainer.tabIndex = -1; //Roving: UpdateRowTabStop gives one row 0
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

                UpdateAriaSort(columnSortingKey);
            }, 0100);

            /* The above magic number is the length of the fade out transition added to the _detailsListItemContainer
             * minus a completely arbitrary amount. Can we access this programmatically? Loop over the item opacity?
             * Could possibly do with adding a fade in transition as well to make the appearance of the newly ordered
             * list items smoother. - MB 16/02/2020 */
        }

        private void UpdateAriaSort(string columnSortingKey)
        {
            foreach (var header in _sortableHeaders)
            {
                var direction = header.Key == columnSortingKey ? (_currentSortingIcon == UIcons.ArrowUp ? "ascending" : "descending") : "none";
                header.Value.setAttribute("aria-sort", direction);
            }
        }

        private List<HTMLElement> Rows()
        {
            var rows     = new List<HTMLElement>();
            var children = _listItemsContainer.children;

            for (uint i = 0; i < children.length; i++)
            {
                var child = (HTMLElement)children[i];
                if (child.classList.contains("tss-detailslist-list-item-container")) rows.Add(child);
            }

            return rows;
        }

        // Keeps the row the keyboard was last on as the list's Tab stop (rows are reused, so it survives a sort),
        // or the first row when that one is gone.
        private void UpdateRowTabStop()
        {
            if (_tabStopRow is object && _tabStopRow.parentElement == _listItemsContainer) return;

            var rows = Rows();
            _tabStopRow = rows.FirstOrDefault();

            if (_tabStopRow is object) _tabStopRow.tabIndex = 0;
        }

        private void MoveRowTabStop(HTMLElement row)
        {
            if (row is null || row == _tabStopRow) return;

            if (_tabStopRow is object) _tabStopRow.tabIndex = -1;

            _tabStopRow  = row;
            row.tabIndex = 0;
        }

        // A click (or a Tab into something inside a row) makes that row the one the keyboard continues from.
        private void OnRowFocusIn(Event e)
        {
            var target = e.target as HTMLElement;
            var row    = target?.closest(".tss-detailslist-list-item-container") as HTMLElement;

            if (row is object && row.parentElement == _listItemsContainer) MoveRowTabStop(row);
        }

        private void OnRowKeyDown(KeyboardEvent e)
        {
            if (e.ctrlKey || e.altKey || e.metaKey) return;

            var row = e.target as HTMLElement;

            // Only keys pressed on a row itself: a field or button inside a cell keeps its own keys.
            if (row is null || row.parentElement != _listItemsContainer || !row.classList.contains("tss-detailslist-list-item-container")) return;

            var rows  = Rows();
            var index = rows.IndexOf(row);
            HTMLElement next = null;

            switch (e.key)
            {
                case "ArrowDown": next = index + 1 < rows.Count ? rows[index + 1] : null; break;
                case "ArrowUp":   next = index > 0 ? rows[index - 1] : null; break;
                case "Home":      next = rows.FirstOrDefault(); break;
                case "End":       next = rows.LastOrDefault(); break;
                case "Enter":
                case " ":
                    e.preventDefault();
                    row.click(); //Runs the row's click handler, when it has one
                    return;
                default: return;
            }

            e.preventDefault();

            if (next is object)
            {
                MoveRowTabStop(next);
                next.focus();
            }
        }

        private void SortListItems(string columnSortingKey) => _componentCache.SortComponents((detailsListItem, detailsListItemOther) => detailsListItem.CompareTo(detailsListItemOther, columnSortingKey));

        private void InvertSortingIcon()
        {
            _currentSortingIcon = _currentSortingIcon == UIcons.ArrowUp ? UIcons.ArrowDown : UIcons.ArrowUp;
        }

        private void UpdateColumnSortingIcon(Interface htmlElement, Action setIconExpression)
        {
            if(_columnSortingIcon is object) _columnSortingIcon.remove();
            setIconExpression();
            CreateColumnSortingIcon();
            htmlElement.parentElement.appendChild(_columnSortingIcon);
        }
    }
}
