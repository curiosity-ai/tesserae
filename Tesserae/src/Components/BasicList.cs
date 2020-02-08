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
        private const int PagesToVirtualize = 5;
        private const int PagesToRender     = PagesToVirtualize;

        private readonly int _rowsPerPage;
        private readonly int _columnsPerRow;

        private readonly Dictionary<int, IComponent> _components;
        private readonly List<List<KeyValuePair<int, IComponent>>> _pages;
        private readonly Dictionary<int, HTMLElement> _pageCache;

        private readonly int _componentsPerPage;
        private readonly int _initialComponentsToRender;
        private readonly int _pagesToVirtualizeUpperBoundary;
        private readonly int _pagesToVirtualizeLowerBoundary;
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
            if (components == null)
            {
                throw new ArgumentNullException(nameof(components));
            }

            components = components.ToList();

            if (!components.Any())
            {
                throw new ArgumentException(nameof(components));
            }

            if (rowsPerPage <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(rowsPerPage));
            }

            if (columnsPerRow <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(columnsPerRow));
            }

            _rowsPerPage   = rowsPerPage;
            _columnsPerRow = columnsPerRow;
            _components    = CreateComponentsDictionary(components);

            _componentsPerPage         = _rowsPerPage * _columnsPerRow;
            _initialComponentsToRender = _componentsPerPage * PagesToRender;

            _pages     = _components.InGroupsOf(_componentsPerPage);
            _pageCache = new Dictionary<int, HTMLElement>();

            _pagesToVirtualizeUpperBoundary = (int)Floor((double)PagesToVirtualize / 2);
            _pagesToVirtualizeLowerBoundary = (int)Ceiling((double)PagesToVirtualize / 2);

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

        private IEnumerable<HTMLElement> GetInitialPages() => GetPages(Enumerable.Range(1, PagesToRender));

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
                console.log($"Retrieved page number {pageNumberToRetrieve} from cache");
                return _pageCache.GetValueOrDefault(pageNumberToRetrieve);
            }

            var page = CreatePageHtmlElement(pageNumberToRetrieve);

            page.AppendChildren(
                GetComponentsForPage(pageNumberToRetrieve)
                    .Select(CreateComponentContainerHtmlElement)
                    .ToArray());

            console.log($"Adding page number {pageNumberToRetrieve} to cache");
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
            if (lastComponentMountedClientHeight <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(lastComponentMountedClientHeight));
            }

            _componentHeightInPixels = lastComponentMountedClientHeight;
            _pageHeightInPixels      = _componentHeightInPixels * _rowsPerPage;

            SetBasicListContainerHeight();
            SetTopSpacingDivHeight(0);

            var initialBottomSpacingDivHeight = (_pagesCount - PagesToRender) * _pageHeightInPixels;

            SetBottomSpacingDivHeight(initialBottomSpacingDivHeight);

            Debug("Basic list ready");
        }

        private void OnBasicListContainerScroll(object listener)
        {
            var scrollTop       = _basicListContainer.scrollTop;
            var scrollDirection = GetScrollDirection(scrollTop);

            if (scrollDirection == ScrollDirection.Neutral)
            {
                console.log("Scroll neutral");
            }

            var scrollPosition = scrollTop;

            var newPage = (int)Round(scrollPosition / _pageHeightInPixels, MidpointRounding.AwayFromZero);

            if ((newPage != _currentPage) && newPage > _pagesToVirtualizeLowerBoundary)
            {
                if (scrollDirection == ScrollDirection.Down)
                {
                    console.log($"Scroll down - new page: {newPage}");

                    RemoveFirstPageFromBasicListContainer();

                    var newTopSpacingDivHeight = (newPage - _pagesToVirtualizeLowerBoundary) * _pageHeightInPixels;
                    SetTopSpacingDivHeight(newTopSpacingDivHeight);

                    var pageNumberToAdd = newPage + _pagesToVirtualizeUpperBoundary;
                    RenderPageDownwards(GetPage(pageNumberToAdd));

                    var newBottomSpacingDivHeight =
                        (_pagesCount - (newPage + _pagesToVirtualizeUpperBoundary)) * _pageHeightInPixels;

                    SetBottomSpacingDivHeight(newBottomSpacingDivHeight);
                }
                else if (scrollDirection == ScrollDirection.Up)
                {
                    console.log($"Scroll up - new page: {newPage}");

                    RemoveLastPageFromBasicListContainer();

                    var newTopSpacingDivHeight =
                        (newPage - (_pagesToVirtualizeUpperBoundary - 1)) * _pageHeightInPixels;
                    SetTopSpacingDivHeight(newTopSpacingDivHeight);

                    var pageNumberToAdd = newPage - _pagesToVirtualizeUpperBoundary;
                    RenderPageUpwards(GetPage(pageNumberToAdd));

                    var newBottomSpacingDivHeight =
                        (_pagesCount - (newPage + _pagesToVirtualizeLowerBoundary)) * _pageHeightInPixels;

                    SetBottomSpacingDivHeight(newBottomSpacingDivHeight);
                }
            }

            _currentPage            = newPage;
            _currentScrollPosition  = scrollPosition;
            _currentScrollDirection = scrollDirection;
        }

        private void Debug(string message)
        {
            console.log(
                message,
                new
                {
                    PagesToVirtualize,
                    PagesToRender,
                    _rowsPerPage,
                    _columnsPerRow,
                    _componentsPerPage,
                    _initialComponentsToRender,
                    _pagesToVirtualizeUpperBoundary,
                    _pagesToVirtualizeLowerBoundary,
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

        private ScrollDirection GetScrollDirection(double scrollTop)
        {
            if (scrollTop > _currentScrollPosition)
            {
                return ScrollDirection.Down;
            }

            if (scrollTop < _currentScrollPosition)
            {
                return ScrollDirection.Up;
            }

            return  ScrollDirection.Neutral;
        }

        private enum ScrollDirection
        {
            Neutral = 0,
            Up,
            Down
        }
    }
}
