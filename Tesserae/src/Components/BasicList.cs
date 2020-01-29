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
        private const int LoadBoundaryTolerance              = 100;

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

            _innerElement = CreateBasicListContainer(containerHeight);

            _components
                .Take(_initialComponentsToRenderCount)
                .ForEach((component, index) =>
                {
                    var htmlElement = component.Render();

                    if (index == _initialComponentsToRenderCount - 1)
                    {
                        DomMountedObserver.NotifyWhenMounted(htmlElement, () =>
                        {
                            var componentHeight =
                                htmlElement.parentElement.clientHeight;

                            var componentsNotRenderedCount =
                                _componentsCount - _initialComponentsToRenderCount;

                            var _bottomPaddingDivHeight =
                                componentsNotRenderedCount * componentHeight;

                            _renderedComponentsHeight =
                                (_initialComponentsToRenderCount / _rowsPerPage) * componentHeight;

                            _bottomPaddingDiv =
                                Div(_(styles: cssStyleDeclaration =>
                                {
                                    cssStyleDeclaration.height = $"{_bottomPaddingDivHeight}px";
                                }));

                            _innerElement.appendChild(_bottomPaddingDiv);
                        });
                    }

                    _innerElement.appendChild(
                        Div(
                            _("tss-basiclist-item",
                                styles: cssStyleDeclaration =>
                                {
                                    cssStyleDeclaration.height = _componentHeightPercentage;
                                    cssStyleDeclaration.width  = _componentWidthPercentage;
                                }),
                            htmlElement));
                });

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
        }

        public void Replace(IComponent newComponent, IComponent oldComponent)
        {
        }

        private static HTMLDivElement CreateBasicListContainer(int height)
        {
            return Div(_("tss-basiclist",
                styles: cssStyleDeclaration =>
                {
                    cssStyleDeclaration.height = $"{height}px";
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
                .ForEach((component, index) =>
                {
                    var htmlElement = component.Render();

                    if (index == componentsToRenderCount - 1)
                    {

                    }

                    if (_bottomPaddingDiv != null)
                    {
                        _innerElement.removeChild(_bottomPaddingDiv);
                        _bottomPaddingDiv = null;
                    }

                    _innerElement.appendChild(
                        Div(
                            _("tss-basiclist-item",
                                styles: cssStyleDeclaration =>
                                {
                                    cssStyleDeclaration.height = _componentHeightPercentage;
                                    cssStyleDeclaration.width  = _componentWidthPercentage;
                                }),
                            htmlElement));
                });
        }

        private void AttachOnScrollEvent()
        {
            _innerElement.addEventListener("scroll", listener =>
            {
                var scrollTop = _innerElement.scrollTop;
                var scrollDirection = GetScrollDirection(_innerElement.scrollTop);

                if (scrollDirection == ScrollDirection.Down)
                {
                    var loadBoundary =
                        (_renderedComponentsHeight - _innerElement.clientHeight) - LoadBoundaryTolerance;

                    if (scrollTop >= loadBoundary)
                    {
                        console.log("Start loading!");
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