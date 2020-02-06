using System;
using System.Collections.Generic;
using System.Linq;
using Tesserae.HTML;
using static System.Math;
using static Retyped.dom;
using static Tesserae.UI;

namespace Tesserae.Components
{
    public class BasicList : IComponent
    {
        private const int PagesToVirtualize      = 7;
        private const int PagesToInitiallyRender = PagesToVirtualize - 1;

        private readonly int _rowsPerPage;
        private readonly int _columnsPerRow;

        private readonly Dictionary<int, IComponent> _components;
        private readonly List<List<KeyValuePair<int, IComponent>>> _pages;
        private readonly Dictionary<int, HTMLElement> _pageCache;

        private readonly int _componentsPerPage;
        private readonly int _initialComponentsToRender;
        private readonly int _pagesToVirtualizeMidpoint;
        private readonly int _pagesToVirtualizeBoundary;
        private readonly string _componentHeightInPercentage;
        private readonly string _componentWidthInPercentage;

        private readonly HTMLElement _innerElement;
        private readonly HTMLDivElement _basicListContainer;
        private readonly HTMLDivElement _topSpacingDiv;
        private readonly HTMLDivElement _bottomSpacingDiv;

        private int _componentsCount;
        private int _pagesCount;
        private int _rowsCount;
        private int _currentPage;

        private int _componentHeightInPixels;
        private int _pageHeightInPixels;
        private double _currentScrollPosition;
        private ScrollDirection _currentScrollDirection;

        public BasicList(
            IEnumerable<IComponent> components,
            int rowsPerPage   = 4,
            int columnsPerRow = 4)
        {
            _rowsPerPage   = rowsPerPage;
            _columnsPerRow = columnsPerRow;
            _components    = CreateComponentsDictionary(components);

            _componentsPerPage         = _rowsPerPage * _columnsPerRow;
            _initialComponentsToRender = _componentsPerPage * PagesToInitiallyRender;

            _pages     = _components.InGroupsOf(_componentsPerPage);
            _pageCache = new Dictionary<int, HTMLElement>();

            _pagesToVirtualizeMidpoint = (int)Ceiling((double)PagesToVirtualize / 2);
            _pagesToVirtualizeBoundary = (int)Floor((double)PagesToVirtualize / 2);

            _componentHeightInPercentage = GetComponentDimensionInPercent(_rowsPerPage);
            _componentWidthInPercentage  = GetComponentDimensionInPercent(_columnsPerRow);

            CalculateCounts();

            Debug("Basic list initialized");

            _innerElement       = CreateInnerElementHtmlDivElement();
            _basicListContainer = CreateBasicListContainerHtmlDivElement();
            _topSpacingDiv      = CreateTopSpacingHtmlDivElement();
            _bottomSpacingDiv   = CreateBottomSpacingHtmlDivElement();

            _innerElement.appendChild(_basicListContainer);
            AppendChildrenToBasicListContainerHtmlDivElement(_topSpacingDiv, _bottomSpacingDiv);

            RenderPagesDownwards(GetInitialPages());

            AttachOnLastComponentMountedEvent();
            AttachBasicListContainerOnScrollEvent();
        }

        public HTMLElement Render() => _innerElement;

        private static Dictionary<int, IComponent> CreateComponentsDictionary(
            IEnumerable<IComponent> components)
        {
            return components
                .Select((component, index) => new
                {
                    component,
                    componentNumber = index + 1
                })
                .ToDictionary(
                    item => item.componentNumber,
                    item => item.component);
        }

        private static string GetComponentDimensionInPercent(int itemsCount) => $"{100 / itemsCount}%";

        private static HTMLDivElement CreateInnerElementHtmlDivElement() => Div(_());

        private static HTMLDivElement CreateSpacingHtmlDivElement(string className) => Div(_(className));

        private static void SetHtmlElementHeight(HTMLElement htmlElement, int heightInPixels)
        {
            htmlElement.SetStyle(cssStyleDeclaration =>
            {
                cssStyleDeclaration.height = $"{heightInPixels}px";
            });
        }

        private static void RenderPage(HTMLElement page, Action<HTMLElement> renderingAction) => renderingAction(page);

        private void CalculateCounts()
        {
            _componentsCount = _components.Count;
            _pagesCount      = _pages.Count;
            _rowsCount       = _rowsPerPage * _pagesCount;
        }

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

        private void SetBasicListContainerHeight() => SetHtmlElementHeight(_basicListContainer, _pageHeightInPixels);

        private void SetTopSpacingDivHeight(int heightInPixels) => SetHtmlElementHeight(_topSpacingDiv, heightInPixels);

        private void SetBottomSpacingDivHeight(int heightInPixels)
        {
            SetHtmlElementHeight(_bottomSpacingDiv, heightInPixels);
        }

        private IEnumerable<HTMLElement> GetInitialPages() => GetPages(Enumerable.Range(1, PagesToInitiallyRender));

        private IEnumerable<HTMLElement> GetPages(IEnumerable<int> rangeOfPageNumbersToGet)
        {
            return RetrievePagesFromCache(rangeOfPageNumbersToGet);
        }

        private HTMLElement GetPage(int pageNumber) => RetrievePageFromCache(pageNumber);

        private IEnumerable<HTMLElement> RetrievePagesFromCache(IEnumerable<int> rangeOfPageNumbersToRetrieve)
        {
            return rangeOfPageNumbersToRetrieve.Select(RetrievePageFromCache);
        }

        private HTMLElement RetrievePageFromCache(int pageNumberToRetrieve)
        {
            if (_pageCache.ContainsKey(pageNumberToRetrieve))
            {
                return _pageCache.GetValueOrDefault(pageNumberToRetrieve);
            }

            var page = CreatePageHtmlElement(pageNumberToRetrieve);

            page.AppendChildren(
                GetComponentsForPage(pageNumberToRetrieve)
                    .Select(CreateComponentContainerHtmlElement)
                    .ToArray());

            _pageCache.Add(pageNumberToRetrieve, page);

            return page;
        }

        private HTMLElement CreatePageHtmlElement(int pageNumber)
        {
            return Div(
                _("tss-basiclist-page")
                    .WithRole("presentation")
                    .WithData("tss-basiclist-pagenumber", pageNumber.ToString()));
        }

        private HTMLElement CreateComponentContainerHtmlElement(KeyValuePair<int, IComponent> componentAndNumber)
        {
            var (componentNumber, component) = componentAndNumber;

            return Div(
                _("tss-basiclist-item",
                    styles: cssStyleDeclaration =>
                    {
                        cssStyleDeclaration.height = _componentHeightInPercentage;
                        cssStyleDeclaration.width = _componentWidthInPercentage;
                    })
                    .WithRole("listitems")
                    .WithData("tss-basiclist-componentnumber", componentNumber.ToString()),
                component.Render());
        }

        private IEnumerable<KeyValuePair<int, IComponent>> GetComponentsForPage(int pageNumber)
        {
            return _pages.ElementAt(pageNumber - 1);
        }

        private void RenderPageUpwards(HTMLElement page)
        {
            RenderPage(page, pageToRender =>
            {
                _topSpacingDiv.insertAdjacentElement(InsertPosition.afterend, pageToRender);
            });
        }

        private void RenderPagesDownwards(IEnumerable<HTMLElement> pages)
        {
            foreach (var page in pages)
            {
                RenderPageDownwards(page);
            }
        }

        private void RenderPageDownwards(HTMLElement page)
        {
            RenderPage(page, pageToRender =>
            {
                _basicListContainer.insertBefore(page, _bottomSpacingDiv);
            });
        }

        private NodeListOf<Element> GetRenderedPages()
        {
            return _basicListContainer.getElementsByClassName("tss-basiclist-page");
        }

        private void RemoveFirstPageFromBasicListContainer()
        {
            RemovePageFromBasicListContainer(GetRenderedPages(), 0);
        }

        private void RemoveLastPageFromBasicListContainer()
        {
            var pages = GetRenderedPages();

            RemovePageFromBasicListContainer(pages, (int)pages.length - 1);
        }

        private void RemovePageFromBasicListContainer(NodeListOf<Element> pages, int index)
        {
            _basicListContainer.removeChild(pages[index]);
        }

        private void AttachOnLastComponentMountedEvent()
        {
            var lastComponentMounted =
                (HTMLElement)_basicListContainer.lastElementChild.previousElementSibling.lastChild;

            DomMountedObserver.NotifyWhenMounted(lastComponentMounted,
                () => OnLastComponentMounted(lastComponentMounted.clientHeight));
        }

        private void AttachBasicListContainerOnScrollEvent()
        {
            _basicListContainer.addEventListener("scroll", OnBasicListContainerScroll);
        }

        private void OnLastComponentMounted(int lastComponentMountedClientHeight)
        {
            _componentHeightInPixels = lastComponentMountedClientHeight;
            _pageHeightInPixels      = _componentHeightInPixels * _rowsPerPage;

            SetBasicListContainerHeight();
            SetTopSpacingDivHeight(0);

            var initialBottomSpacingDivHeight = (_pagesCount - PagesToInitiallyRender) * _pageHeightInPixels;

            SetBottomSpacingDivHeight(initialBottomSpacingDivHeight);

            Debug("Basic list ready");
        }

        private void OnBasicListContainerScroll(object listener)
        {
            var scrollTop          = _basicListContainer.scrollTop;
            var newScrollDirection = GetScrollDirection();

            var newPage = newScrollDirection == ScrollDirection.Down ?
                (int) Ceiling(scrollTop / _pageHeightInPixels)
                : (int) Floor((scrollTop + _pageHeightInPixels) / _pageHeightInPixels);

            console.log($"New page: {newPage}");
            console.log($"Scroll top: {scrollTop}");

            if ((newPage != _currentPage) && newPage >= _pagesToVirtualizeMidpoint)
            {
                if (newScrollDirection == ScrollDirection.Down)
                {
                    RemoveFirstPageFromBasicListContainer();

                    var newTopSpacingDivHeight = (newPage - _pagesToVirtualizeBoundary) * _pageHeightInPixels;
                    SetTopSpacingDivHeight(newTopSpacingDivHeight);

                    var pageNumberToAdd = newPage + _pagesToVirtualizeBoundary;
                    RenderPageDownwards(GetPage(pageNumberToAdd));

                    var newBottomSpacingDivHeight =
                        (_pagesCount - (newPage + _pagesToVirtualizeBoundary)) * _pageHeightInPixels;

                    SetBottomSpacingDivHeight(newBottomSpacingDivHeight);

                    console.log("Scroll down");
                    console.log($"Top spacing div height: {newTopSpacingDivHeight}");
                    console.log($"Page number to add: {pageNumberToAdd}");
                    console.log($"Bottom spacing div height: {newTopSpacingDivHeight}");
                }
                else
                {
                    RemoveLastPageFromBasicListContainer();

                    var newTopSpacingDivHeight = (newPage - (_pagesToVirtualizeBoundary - 1)) * _pageHeightInPixels;
                    SetTopSpacingDivHeight(newTopSpacingDivHeight);

                    var pageNumberToAdd = newPage - 2;
                    RenderPageUpwards(GetPage(pageNumberToAdd));

                    var newBottomSpacingDivHeight =
                        (_pagesCount - (newPage + _pagesToVirtualizeBoundary)) * _pageHeightInPixels;

                    SetBottomSpacingDivHeight(newBottomSpacingDivHeight);

                    console.log("Scroll up");
                    console.log($"Top spacing div height: {newTopSpacingDivHeight}");
                    console.log($"Page number to add: {pageNumberToAdd}");
                    console.log($"Bottom spacing div height: {newTopSpacingDivHeight}");
                }
            }

            _currentPage            = newPage;
            _currentScrollPosition  = scrollTop;
            _currentScrollDirection = newScrollDirection;
        }

        private void Debug(string message)
        {
            console.log(
                message,
                new
                {
                    PagesToVirtualize,
                    PagesToInitiallyRender,
                    _rowsPerPage,
                    _columnsPerRow,
                    _componentsPerPage,
                    _initialComponentsToRender,
                    _pagesToVirtualizeMidpoint,
                    _pagesToVirtualizeBoundary,
                    _componentHeightInPercentage,
                    _componentWidthInPercentage,
                    _componentsCount,
                    _pagesCount,
                    _rowsCount,
                    _currentPage,
                    _componentHeightInPixels,
                    _pageHeightInPixels,
                    _currentScrollPosition,
                    _currentScrollDirection
                });
        }

        private ScrollDirection GetScrollDirection()
        {
            return _basicListContainer.scrollTop > _currentScrollPosition ?
                ScrollDirection.Down
                : ScrollDirection.Up;
        }

        private enum ScrollDirection
        {
            Neutral = 0,
            Up,
            Down
        }
    }
}
