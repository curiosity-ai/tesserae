using System.Collections.Generic;
using System.Linq;
using Tesserae.HTML;
using static System.Math;
using static Retyped.dom;
using static Tesserae.UI;

namespace Tesserae.Components
{
    public class BasicList : IContainer<BasicList, IComponent>
    {
        private const int PagesToVirtualize = 7;
        private const int PagesToInitiallyRender = PagesToVirtualize - 1;

        private readonly List<IComponent> _components;
        private readonly int _rowsPerPage;
        private readonly int _columnsPerRow;

        private readonly int _componentsPerPage;
        private readonly int _initialComponentsToRender;
        private readonly int _pagesToVirtualizeMidpoint;
        private readonly int _pagesToVirtualizeBoundary;
        private readonly string _componentHeightInPercentage;
        private readonly string _componentWidthInPercentage;

        private int _componentsCount;
        private int _pagesCount;
        private int _rowsCount;
        private int _currentPage;

        private HTMLElement _innerElement;
        private HTMLDivElement _basicListContainer;
        private HTMLDivElement _topSpacingDiv;
        private HTMLDivElement _bottomSpacingDiv;
        private int _componentHeightInPixels;
        private int _pageHeightInPixels;
        private double _currentScrollPosition;
        private ScrollDirection _currentScrollDirection;

        public BasicList(
            IEnumerable<IComponent> components,
            int rowsPerPage   = 4,
            int columnsPerRow = 4)
        {
            _components = components.ToList();
            _rowsPerPage = rowsPerPage;
            _columnsPerRow = columnsPerRow;

            _componentsPerPage = _rowsPerPage * _columnsPerRow;
            _initialComponentsToRender = _componentsPerPage * PagesToInitiallyRender;
            _pagesToVirtualizeMidpoint = (int)Ceiling((double)PagesToVirtualize / 2);
            _pagesToVirtualizeBoundary = (int)Floor((double)PagesToVirtualize / 2);

            _componentHeightInPercentage = GetComponentDimensionInPercent(_rowsPerPage);
            _componentWidthInPercentage = GetComponentDimensionInPercent(_columnsPerRow);

            _componentsCount = _components.Count;
            _pagesCount = (int)Ceiling((double)_componentsCount / _componentsPerPage);
            _rowsCount = _rowsPerPage * _pagesCount;

            _currentScrollDirection = ScrollDirection.Down;

            Debug("Basic list init");

            _innerElement = CreateEmptyDiv();
            CreateBasicListContainer();
            CreateTopSpacingDiv();
            CreateBottomSpacingDiv();
            RenderComponents(_initialComponentsToRender);

            AttachOnLastComponentMountedEvent();
            AttachBasicListContainerOnScrollEvent();
        }

        private string GetComponentDimensionInPercent(int itemsCount) => $"{100 / itemsCount}%";

        public HTMLElement Render() => _innerElement;

        public void Add(IComponent component) => _components.Add(component);

        public void Clear() => _components.Clear();

        public void Replace(IComponent newComponent, IComponent oldComponent)
        {
        }

        private static HTMLDivElement CreateEmptyDiv() => Div(_());

        private void CreateBasicListContainer()
        {
            _basicListContainer = Div(_("tss-basiclist"));

            _innerElement.appendChild(_basicListContainer);
        }

        private void CreateTopSpacingDiv()
        {
            _topSpacingDiv = Div(_(styles: cssStyleDeclaration =>
            {
                cssStyleDeclaration.cssFloat = "left";
                cssStyleDeclaration.width = "100%";
            }));

            _basicListContainer.appendChild(_topSpacingDiv);
        }

        private void SetTopSpacingDivHeight(int heightInPixels)
        {
            _topSpacingDiv.SetStyle(cssStyleDeclaration =>
            {
                cssStyleDeclaration.height = $"{heightInPixels}px";
            });
        }

        private void CreateBottomSpacingDiv()
        {
            _bottomSpacingDiv = Div(_(styles: cssStyleDeclaration =>
            {
                cssStyleDeclaration.cssFloat = "left";
                cssStyleDeclaration.width = "100%";
            }));

            _basicListContainer.appendChild(_bottomSpacingDiv);
        }

         private void SetBottomSpacingDivHeight(int heightInPixels)
        {
            _bottomSpacingDiv.SetStyle(cssStyleDeclaration =>
            {
                cssStyleDeclaration.height = $"{heightInPixels}px";
            });
        }

        private void RenderComponents(
            int componentsToTake,
            int componentsToSkip = 0)
        {
            _components
                .Skip(componentsToSkip)
                .Take(componentsToTake)
                .Select(component => component.Render())
                .ForEach((htmlElement, index) =>
                {
                    var htmlElementToAppend =
                        Div(
                        _($"tss-basiclist-item",
                            styles: cssStyleDeclaration =>
                            {
                                cssStyleDeclaration.height = _componentHeightInPercentage;
                                cssStyleDeclaration.width  = _componentWidthInPercentage;
                            }),
                            htmlElement);

                    _basicListContainer.insertBefore(htmlElementToAppend, _bottomSpacingDiv);
                });
        }

        private void AttachOnLastComponentMountedEvent()
        {
            var lastHtmlElement =
                (HTMLElement)_basicListContainer.lastElementChild.previousElementSibling;

            DomMountedObserver.NotifyWhenMounted(lastHtmlElement, () =>
            {
                _componentHeightInPixels = lastHtmlElement.clientHeight;
                _pageHeightInPixels = _componentHeightInPixels * _rowsPerPage;

                SetBasicListContainerHeight();
                SetTopSpacingDivHeight(0);

                var initialBottomSpacingDivHeight =
                    (_pagesCount - PagesToInitiallyRender) * _pageHeightInPixels;

                SetBottomSpacingDivHeight(initialBottomSpacingDivHeight);

                Debug("Basic list ready");
            });
        }

        private void SetBasicListContainerHeight()
        {
            _basicListContainer.SetStyle(cssStyleDeclaration =>
            {
                cssStyleDeclaration.height = $"{_pageHeightInPixels}px";
            });
        }

        private void AttachBasicListContainerOnScrollEvent()
        {
            _basicListContainer.addEventListener("scroll", listener =>
            {
                var scrollTop = _basicListContainer.scrollTop;
                var newPage = (int)Ceiling(scrollTop / _pageHeightInPixels);
                var newScrollDirection = GetScrollDirection();

                if ((newPage != _currentPage || newScrollDirection != _currentScrollDirection) &&
                    newPage >= _pagesToVirtualizeMidpoint)
                {
                    var basicListItems = _basicListContainer.getElementsByClassName("tss-basiclist-item");

                    if (newScrollDirection == ScrollDirection.Down)
                    {
                        var i = 1;
                        while (basicListItems[0] != null && i <= _componentsPerPage)
                        {
                            basicListItems[0].parentNode.removeChild(basicListItems[0]);
                            i += 1;
                        }

                        var newTopSpacingDivHeight =
                            (newPage - _pagesToVirtualizeBoundary) * _pageHeightInPixels;

                        SetTopSpacingDivHeight(newTopSpacingDivHeight);

                        var componentsToSkip =
                            (newPage + (_pagesToVirtualizeBoundary - 1)) * _componentsPerPage;

                        RenderComponents(_componentsPerPage, componentsToSkip);

                        var newBottomSpacingDivHeight =
                            (_pagesCount - (newPage + _pagesToVirtualizeBoundary)) * _pageHeightInPixels;

                        SetBottomSpacingDivHeight(newBottomSpacingDivHeight);
                    }
                    else
                    {
                        var basicListItemsLength = (int)basicListItems.length - 1;
                        var i = 1;

                        while (basicListItems[basicListItemsLength] != null && i <= _componentsPerPage)
                        {
                            basicListItems[basicListItemsLength].parentNode.removeChild(basicListItems[basicListItemsLength]);
                            basicListItemsLength -= 1;
                            i += 1;
                        }

                        var newTopSpacingDivHeight =
                            (newPage - (_pagesToVirtualizeBoundary - 1)) * _pageHeightInPixels;

                        SetTopSpacingDivHeight(newTopSpacingDivHeight);

                        var componentsToSkip =
                            (newPage - (_pagesToVirtualizeBoundary - 1)) * _componentsPerPage;

                        RenderComponents(_componentsPerPage, componentsToSkip);

                        var newBottomSpacingDivHeight =
                            (_pagesCount - (newPage + _pagesToVirtualizeBoundary)) * _pageHeightInPixels;

                        SetBottomSpacingDivHeight(newBottomSpacingDivHeight);
                    }
                }

                _currentPage = newPage;
                _currentScrollPosition = scrollTop;
                _currentScrollDirection = newScrollDirection;
            });
        }

        private void Debug(string message)
        {
            console.log(
                message,
                new
                {
                    _rowsPerPage,
                    _columnsPerRow,
                    _componentsPerPage,
                    _componentHeightInPercentage,
                    _componentWidthInPercentage,
                    _componentsCount,
                    _pagesCount,
                    _rowsCount,
                    _currentPage,
                    _componentHeightInPixels,
                    _currentScrollPosition
                });
        }

        private ScrollDirection GetScrollDirection()
        {
            return _basicListContainer.scrollTop > _currentScrollPosition ? ScrollDirection.Down : ScrollDirection.Up;
        }

        private enum ScrollDirection
        {
            Up   = 0,
            Down = 1
        }
    }
}