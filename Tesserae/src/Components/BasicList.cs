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
        private const int InitialPagesToRenderInAdvanceCount = 3;
        private const int LoadBoundaryTolerance              = 10;

        private readonly HTMLElement _innerElement;
        private readonly int _rowsPerPage;
        private readonly int _columnsPerRow;
        private readonly List<IComponent> _components;
        private readonly int _componentsCount;
        private readonly int _pagesCount;
        private readonly int _componentsPerPage;
        private readonly string _componentHeightPercentage;
        private readonly string _componentWidthPercentage;
        private readonly int _initialComponentsToRenderCount;

        private int _currentPage;
        private int _renderedComponentsHeight;
        private double _currentScrollPosition;
        private HTMLDivElement _bottomPaddingDiv;

        public BasicList(
            IEnumerable<IComponent> components,
            int rowsPerPage   = 4,
            int columnsPerRow = 4,
            int containerHeight = 250)
        {
            _rowsPerPage   = rowsPerPage;
            _columnsPerRow = columnsPerRow;
            _components    = components.ToList();

            _componentsCount   = _components.Count;
            _componentsPerPage = _rowsPerPage * _columnsPerRow;

            string GetComponentSize(int numberOfItems) => $"{100 / numberOfItems}%";

            _componentHeightPercentage = GetComponentSize(_rowsPerPage);
            _componentWidthPercentage  = GetComponentSize(_columnsPerRow);

            _pagesCount = (int)Ceiling((double)_componentsCount / _componentsPerPage);

            var renderPagesInAdvance = _pagesCount > (InitialPagesToRenderInAdvanceCount - 1);

            _initialComponentsToRenderCount = renderPagesInAdvance ?
                _componentsPerPage * InitialPagesToRenderInAdvanceCount
                : _componentsPerPage;

            _innerElement     = CreateBasicListContainer(containerHeight);
            _bottomPaddingDiv = Div(_());

            _innerElement.appendChild(_bottomPaddingDiv);

            RenderComponents(_initialComponentsToRenderCount);

            CreateBottomPaddingDivHeight();

            _currentPage = renderPagesInAdvance ?
                InitialPagesToRenderInAdvanceCount
                : 1;

            AttachOnScrollEvent();
        }

        public HTMLElement Render()
        {
            return _innerElement;
        }

        public void Add(IComponent component)
        {
            _components.Add(component);
        }

        public void Clear()
        {
            _components.Clear();
        }

        public void Replace(IComponent newComponent, IComponent oldComponent)
        {
        }

        private static HTMLDivElement CreateBasicListContainer(int containerHeight)
        {
            return Div(_("tss-basiclist",
                styles: cssStyleDeclaration =>
                {
                    cssStyleDeclaration.height = $"{containerHeight}px";
                }));
        }

        private void RenderComponents(
            int? componentsToRenderCount = null)
        {
            componentsToRenderCount =
                componentsToRenderCount ?? _componentsPerPage;

            var componentsToSkipCount = _currentPage * _componentsPerPage;

            _components
                .Skip(componentsToSkipCount)
                .Take(componentsToRenderCount.Value)
                .Select(component => component.Render())
                .ForEach((htmlElement, index) =>
                {
                    var x =
                        Div(
                        _("tss-basiclist-item",
                            styles: cssStyleDeclaration =>
                            {
                                cssStyleDeclaration.height = _componentHeightPercentage;
                                cssStyleDeclaration.width  = _componentWidthPercentage;
                            }),
                            htmlElement);

                    _innerElement.insertBefore(x, _bottomPaddingDiv);
                });
        }

        private void CreateBottomPaddingDivHeight()
        {
            var lastHtmlElement =
                (HTMLElement)_innerElement.lastElementChild.previousElementSibling;

            DomMountedObserver.NotifyWhenMounted(lastHtmlElement, () =>
            {
                var componentHeight =
                    lastHtmlElement.clientHeight;

                var componentsNotRenderedCount =
                    _componentsCount - _initialComponentsToRenderCount;

                var bottomPaddingDivHeight =
                    componentsNotRenderedCount * componentHeight;

                _renderedComponentsHeight =
                    (_initialComponentsToRenderCount / _rowsPerPage) * componentHeight;

                _bottomPaddingDiv.SetStyle(cssStyleDeclaration =>
                {
                    cssStyleDeclaration.height = $"{bottomPaddingDivHeight}px";
                });
            });
       }

        private void AttachOnScrollEvent()
        {
            _innerElement.addEventListener("scroll", listener =>
            {
                var scrollTop= _innerElement.scrollTop;
                var scrollDirection = GetScrollDirection(_innerElement.scrollTop);

                if (scrollDirection == ScrollDirection.Down)
                {
                    var loadBoundary =
                        (_renderedComponentsHeight - _innerElement.clientHeight) - LoadBoundaryTolerance;

                    if (scrollTop >= loadBoundary)
                    {
                        RenderComponents();
                        _currentPage += 1;
                    }
                }
                else
                {
                }

                _currentScrollPosition = scrollTop;
            });
        }

        private ScrollDirection GetScrollDirection(double scrollTop)
        {
            return scrollTop > _currentScrollPosition ? ScrollDirection.Down : ScrollDirection.Up;
        }

        private enum ScrollDirection
        {
            Up   = 0,
            Down = 1
        }
    }
}