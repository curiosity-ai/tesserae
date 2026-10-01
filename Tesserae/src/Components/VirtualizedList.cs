using System;
using System.Collections.Generic;
using System.Linq;
using static System.Math;
using static Transpose.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// A VirtualizedList component that renders only the visible portion of a large list to improve performance.
    /// </summary>
    [Transpose.Name("tss.VirtualizedList")]
    public class VirtualizedList : IComponent
    {
        private const int PagesToVirtualize    = 5;
        private const int InitialPagesToCreate = 2;

        private readonly ListPageCache<IComponent> _listPageCache;
        private readonly int                       _pagesToVirtualizeUpperBoundary;
        private readonly int                       _pagesToVirtualizeLowerBoundary;
        private readonly string                    _componentHeightInPercentage;
        private readonly string                    _componentWidthInPercentage;
        private readonly HTMLElement               _innerElement;
        private readonly HTMLDivElement            _basicListContainer;
        private readonly HTMLDivElement            _topSpacingDiv;
        private readonly HTMLDivElement            _bottomSpacingDiv;

        private bool             _initialPagesCreated;
        private Func<IComponent> _emptyListMessageGenerator;
        private int              _currentPage;
        private UnitSize         _componentHeight;
        private UnitSize         _pageHeight;

        /// <summary>
        /// Initializes a new instance of the VirtualizedList class.
        /// </summary>
        /// <param name="rowsPerPage">The number of rows per virtual page.</param>
        /// <param name="columnsPerRow">The number of columns per row.</param>
        public VirtualizedList(int rowsPerPage = 4, int columnsPerRow = 4)
        {
            if (rowsPerPage <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(rowsPerPage));
            }

            if (columnsPerRow <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(columnsPerRow));
            }

            _listPageCache = new ListPageCache<IComponent>(rowsPerPage, columnsPerRow, CreatePageHtmlElement, CreateComponentContainerHtmlElement);

            _pagesToVirtualizeUpperBoundary = (int)Floor((double)PagesToVirtualize   / 2);
            _pagesToVirtualizeLowerBoundary = (int)Ceiling((double)PagesToVirtualize / 2);

            _componentHeightInPercentage = (100 / rowsPerPage).percent().ToString();
            _componentWidthInPercentage  = (100 / columnsPerRow).percent().ToString();

            _innerElement       = Div(Att());
            _basicListContainer = Div(Att("tss-basiclist").WithRole("list"));
            _topSpacingDiv      = Div(Att("tss-basiclist-top-spacing"));
            _bottomSpacingDiv   = Div(Att("tss-basiclist-bottom-spacing"));

            _innerElement.appendChild(_basicListContainer);
            _basicListContainer.AppendChildren(_topSpacingDiv, _bottomSpacingDiv);
        }

        /// <summary>
        /// Sets a message to display when the list is empty.
        /// </summary>
        /// <param name="emptyListMessageGenerator">A function that returns the empty list message component.</param>
        /// <returns>The current instance of the type.</returns>
        public VirtualizedList WithEmptyMessage(Func<IComponent> emptyListMessageGenerator)
        {
            _emptyListMessageGenerator = emptyListMessageGenerator ?? throw new ArgumentNullException(nameof(emptyListMessageGenerator));

            return this;
        }

        /// <summary>
        /// Adds items to the virtualized list.
        /// </summary>
        /// <param name="listItems">The items to add.</param>
        /// <returns>The current instance of the type.</returns>
        public VirtualizedList WithListItems(IEnumerable<IComponent> listItems)
        {
            if (listItems == null)
            {
                throw new ArgumentNullException(nameof(listItems));
            }

            _listPageCache.AddComponents(listItems);

            if (_listPageCache.HasComponents && !_initialPagesCreated)
            {
                foreach (var page in _listPageCache.RetrievePagesFromCache(Enumerable.Range(1, InitialPagesToCreate)))
                {
                    _basicListContainer.insertBefore(page, _bottomSpacingDiv);
                }

                var lastComponentMounted = (HTMLElement)_basicListContainer.lastElementChild.previousElementSibling.lastChild;
                DomObserver.WhenMounted(lastComponentMounted, () => OnLastComponentMounted(lastComponentMounted.clientHeight));

                _basicListContainer.addEventListener("scroll", OnBasicListContainerScroll);

                _initialPagesCreated = true;
            }
            else if (_emptyListMessageGenerator != null)
            {
                _basicListContainer.appendChild(_emptyListMessageGenerator().Render());
            }

            return this;
        }

        /// <summary>
        /// Renders the virtualized list.
        /// </summary>
        /// <returns>The rendered HTMLElement.</returns>
        public HTMLElement Render() => _innerElement;

        private static void SetHeight(HTMLElement htmlElement, UnitSize height) => htmlElement.style.height = height.ToString();

        private HTMLElement CreatePageHtmlElement(int pageNumber)
        {
            return Div(
                Att("tss-basiclist-page")
                   .WithRole("presentation")
                   .WithData("tss-basiclist-pagenumber", pageNumber.ToString()));
        }

        private HTMLElement CreateComponentContainerHtmlElement((int key, IComponent component) componentAndKey)
        {
            var (key, component) = componentAndKey;

            return Div(
                Att("tss-basiclist-item",
                        styles: cssStyleDeclaration =>
                        {
                            cssStyleDeclaration.height = _componentHeightInPercentage;
                            cssStyleDeclaration.width  = _componentWidthInPercentage;
                        })
                   .WithRole("listitems")
                   .WithData("tss-basiclist-componentnumber", key.ToString()),
                component.Render());
        }

        private void OnLastComponentMounted(int lastComponentMountedClientHeight)
        {
            if (lastComponentMountedClientHeight <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(lastComponentMountedClientHeight));
            }

            _componentHeight = lastComponentMountedClientHeight.px();
            _pageHeight      = (_componentHeight.Size * _listPageCache.RowsPerPage).px();

            SetHeight(_basicListContainer, _pageHeight);
            SetHeight(_topSpacingDiv,      0.px());
            SetHeight(_bottomSpacingDiv,   ((_listPageCache.PagesCount - InitialPagesToCreate) * _pageHeight.Size).px());
        }

        private void OnBasicListContainerScroll(object listener)
        {
            var newPage = (int)Round(_basicListContainer.scrollTop / _pageHeight.Size, MidpointRounding.AwayFromZero);

            if (newPage != _currentPage)
            {
                // Reconcile the whole rendered window around the page nearest the viewport on every
                // change. A per-step add/remove is only valid mid-list with an already-aligned 5-page
                // window; rebuilding handles the boundaries (top/bottom) and multi-page jumps
                // uniformly, and reuses cached page elements so it stays cheap.
                RebuildRenderedPages(newPage);
                _currentPage = newPage;
            }
        }

        private void RebuildRenderedPages(int centerPage)
        {
            var rendered = _basicListContainer.getElementsByClassName("tss-basiclist-page");
            for (var i = (int)rendered.length - 1; i >= 0; i--)
            {
                _basicListContainer.removeChild(rendered[i]);
            }

            var firstPage = Max(1, centerPage - _pagesToVirtualizeLowerBoundary + 1);
            var lastPage  = Min(_listPageCache.PagesCount, centerPage + _pagesToVirtualizeUpperBoundary);

            for (var pageNumber = firstPage; pageNumber <= lastPage; pageNumber++)
            {
                var page = _listPageCache.RetrievePageFromCache(pageNumber);

                if (page != null)
                {
                    _basicListContainer.insertBefore(page, _bottomSpacingDiv);
                }
            }

            SetHeight(_topSpacingDiv,    ((firstPage - 1)                       * _pageHeight.Size).px());
            SetHeight(_bottomSpacingDiv, ((_listPageCache.PagesCount - lastPage) * _pageHeight.Size).px());
        }
    }
}