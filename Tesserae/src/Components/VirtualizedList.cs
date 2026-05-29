using System;
using System.Collections.Generic;
using System.Linq;
using static System.Math;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// A VirtualizedList component that renders only the visible portion of a large list to improve performance.
    /// </summary>
    [H5.Name("tss.VirtualizedList")]
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

            _componentHeightInPercentage = GetComponentSize(rowsPerPage);
            _componentWidthInPercentage  = GetComponentSize(columnsPerRow);

            _innerElement       = CreateInnerElementHtmlDivElement();
            _basicListContainer = CreateBasicListContainerHtmlDivElement();
            _topSpacingDiv      = CreateTopSpacingHtmlDivElement();
            _bottomSpacingDiv   = CreateBottomSpacingHtmlDivElement();

            _innerElement.appendChild(_basicListContainer);
            AppendChildrenToBasicListContainerHtmlDivElement(_topSpacingDiv, _bottomSpacingDiv);
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
                CreatePagesDownwards(GetInitialPages());

                AttachOnLastComponentMountedEvent();
                AttachBasicListContainerOnScrollEvent();

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

        private static string GetComponentSize(int itemsCount) => (100 / itemsCount).percent().ToString();

        private static HTMLDivElement CreateInnerElementHtmlDivElement() => Div(_());

        private static HTMLDivElement CreateSpacingHtmlDivElement(string className) => Div(_(className));

        private static void SetHtmlElementHeight(HTMLElement htmlElement, UnitSize height)
        {
            htmlElement.SetStyle(cssStyleDeclaration =>
            {
                cssStyleDeclaration.height = height.ToString();
            });
        }

        private static void CreatePage(HTMLElement page, Action<HTMLElement> renderingAction) => renderingAction(page);

        private HTMLDivElement CreateBasicListContainerHtmlDivElement() => Div(_("tss-basiclist").WithRole("list"));

        private void AppendChildrenToBasicListContainerHtmlDivElement(params HTMLElement[] htmlElements)
        {
            _basicListContainer.AppendChildren(htmlElements);
        }

        private HTMLDivElement CreateTopSpacingHtmlDivElement()
        {
            return CreateSpacingHtmlDivElement("tss-basiclist-top-spacing");
        }

        private HTMLDivElement CreateBottomSpacingHtmlDivElement()
        {
            return CreateSpacingHtmlDivElement("tss-basiclist-bottom-spacing");
        }

        private void SetBasicListContainerHeight() => SetHtmlElementHeight(_basicListContainer, _pageHeight);

        private void SetTopSpacingDivHeight(UnitSize height) => SetHtmlElementHeight(_topSpacingDiv, height);

        private void SetBottomSpacingDivHeight(UnitSize height)
        {
            SetHtmlElementHeight(_bottomSpacingDiv, height);
        }

        private IEnumerable<HTMLElement> GetInitialPages()
        {
            return RetrievePagesFromCache(Enumerable.Range(1, InitialPagesToCreate));
        }

        private IEnumerable<HTMLElement> RetrievePagesFromCache(IEnumerable<int> rangeOfPageNumbersToRetrieve)
        {
            return _listPageCache.RetrievePagesFromCache(rangeOfPageNumbersToRetrieve);
        }

        private HTMLElement RetrievePageFromCache(int pageNumberToRetrieve)
        {
            return _listPageCache.RetrievePageFromCache(pageNumberToRetrieve);
        }

        private HTMLElement CreatePageHtmlElement(int pageNumber)
        {
            return Div(
                _("tss-basiclist-page")
                   .WithRole("presentation")
                   .WithData("tss-basiclist-pagenumber", pageNumber.ToString()));
        }

        private HTMLElement CreateComponentContainerHtmlElement((int key, IComponent component) componentAndKey)
        {
            var (key, component) = componentAndKey;

            return Div(
                _("tss-basiclist-item",
                        styles: cssStyleDeclaration =>
                        {
                            cssStyleDeclaration.height = _componentHeightInPercentage;
                            cssStyleDeclaration.width  = _componentWidthInPercentage;
                        })
                   .WithRole("listitems")
                   .WithData("tss-basiclist-componentnumber", key.ToString()),
                component.Render());
        }

        private void CreatePagesDownwards(IEnumerable<HTMLElement> pages)
        {
            foreach (var page in pages)
            {
                CreatePageDownwards(page);
            }
        }

        private void CreatePageDownwards(HTMLElement page)
        {
            CreatePage(page, pageToCreate =>
            {
                _basicListContainer.insertBefore(page, _bottomSpacingDiv);
            });
        }

        private NodeListOf<Element> GetRenderedPages()
        {
            return _basicListContainer.getElementsByClassName("tss-basiclist-page");
        }

        private void AttachOnLastComponentMountedEvent()
        {
            var lastComponentMounted =
                (HTMLElement)_basicListContainer.lastElementChild.previousElementSibling.lastChild;

            DomObserver.WhenMounted(lastComponentMounted,
                () => OnLastComponentMounted(lastComponentMounted.clientHeight));
        }

        private void AttachBasicListContainerOnScrollEvent()
        {
            _basicListContainer.addEventListener("scroll", OnBasicListContainerScroll);
        }

        private void OnLastComponentMounted(int lastComponentMountedClientHeight)
        {
            if (lastComponentMountedClientHeight <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(lastComponentMountedClientHeight));
            }

            _componentHeight = lastComponentMountedClientHeight.px();
            _pageHeight      = (_componentHeight.Size * _listPageCache.RowsPerPage).px();

            SetBasicListContainerHeight();
            SetTopSpacingDivHeight(0.px());

            var initialBottomSpacingDivHeight =
                ((_listPageCache.PagesCount - InitialPagesToCreate) * _pageHeight.Size).px();

            SetBottomSpacingDivHeight(initialBottomSpacingDivHeight);
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
            var rendered = GetRenderedPages();
            for (var i = (int)rendered.length - 1; i >= 0; i--)
            {
                _basicListContainer.removeChild(rendered[i]);
            }

            var firstPage = Max(1, centerPage - _pagesToVirtualizeLowerBoundary + 1);
            var lastPage  = Min(_listPageCache.PagesCount, centerPage + _pagesToVirtualizeUpperBoundary);

            for (var pageNumber = firstPage; pageNumber <= lastPage; pageNumber++)
            {
                var page = RetrievePageFromCache(pageNumber);

                if (page != null)
                {
                    _basicListContainer.insertBefore(page, _bottomSpacingDiv);
                }
            }

            SetTopSpacingDivHeight(((firstPage - 1)                       * _pageHeight.Size).px());
            SetBottomSpacingDivHeight(((_listPageCache.PagesCount - lastPage) * _pageHeight.Size).px());
        }
    }
}