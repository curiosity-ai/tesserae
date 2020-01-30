using System;
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

        private readonly int _rowsPerPage;
        private readonly List<IComponent> _components;
        private readonly int _componentsCount;
        private readonly int _componentsPerPage;
        private readonly string _componentHeightPercentage;
        private readonly string _componentWidthPercentage;

        private HTMLElement _innerElement;
        private int _currentPage;
        private double _currentScrollPosition;
        private HTMLDivElement _bottomEmptyDiv;
        private int _componentHeightPixels;

        public BasicList(
            IEnumerable<IComponent> components,
            int rowsPerPage   = 4,
            int columnsPerRow = 4,
            int containerHeight = 250)
        {
            _rowsPerPage = rowsPerPage;
            _components  = components.ToList();

            _componentsCount   = _components.Count;
            _componentsPerPage = _rowsPerPage * columnsPerRow;

            string GetComponentSize(int numberOfItems) => $"{100 / numberOfItems}%";

            _componentHeightPercentage = GetComponentSize(_rowsPerPage);
            _componentWidthPercentage  = GetComponentSize(columnsPerRow);

            var pagesCount = (int)Ceiling((double)_componentsCount / _componentsPerPage);

            var renderPagesInAdvance = pagesCount > (InitialPagesToRenderInAdvanceCount - 1);

            var initialComponentsToRenderCount = renderPagesInAdvance ?
                _componentsPerPage * InitialPagesToRenderInAdvanceCount
                : _componentsPerPage;

            CreateBasicListContainer(containerHeight);
            CreateBottomEmptyDiv();
            AddBottomEmptyDivToBasicListContainer();
            RenderComponents(initialComponentsToRenderCount);

            SetCurrentPage(renderPagesInAdvance ?
                InitialPagesToRenderInAdvanceCount
                : 1);

            OnLastComponentMounted(
                SetComponentHeightPixels,
                SetInitialBottomEmptyDivHeight);

            AttachBasicListContainerOnScrollEvent();
        }

        private int ComponentsRenderedCount => _currentPage * _componentsPerPage;

        private int RenderedComponentsHeight => (ComponentsRenderedCount / _rowsPerPage) * _componentHeightPixels;

        public HTMLElement Render() => _innerElement;

        public void Add(IComponent component) => _components.Add(component);

        public void Clear() => _components.Clear();

        public void Replace(IComponent newComponent, IComponent oldComponent)
        {
        }

        private static HTMLDivElement CreateEmptyDiv() => Div(_());

        private void CreateBasicListContainer(int containerHeight)
        {
            _innerElement =
                Div(_("tss-basiclist",
                    styles: cssStyleDeclaration =>
                    {
                        cssStyleDeclaration.height = $"{containerHeight}px";
                    }));
        }

        private void CreateBottomEmptyDiv() => _bottomEmptyDiv = CreateEmptyDiv();

        private void AddBottomEmptyDivToBasicListContainer() => _innerElement.appendChild(_bottomEmptyDiv);

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
                    var htmlElementToAppend =
                        Div(
                        _("tss-basiclist-item",
                            styles: cssStyleDeclaration =>
                            {
                                cssStyleDeclaration.height = _componentHeightPercentage;
                                cssStyleDeclaration.width  = _componentWidthPercentage;
                            }),
                            htmlElement);

                    _innerElement.insertBefore(htmlElementToAppend, _bottomEmptyDiv);
                });
        }

        private void SetCurrentPage(int currentPage) => _currentPage = currentPage;

        private void IncrementCurrentPage() => SetCurrentPage(_currentPage + 1);

        private void DecrementCurrentPage() => SetCurrentPage(_currentPage - 1);

        private void OnLastComponentMounted(params Action<HTMLElement>[] callbacks)
        {
            var lastHtmlElement =
                (HTMLElement)_innerElement.lastElementChild.previousElementSibling;

            DomMountedObserver.NotifyWhenMounted(lastHtmlElement, () =>
                callbacks.ForEach(callback => callback.Invoke(lastHtmlElement)));
        }

        private void SetComponentHeightPixels(HTMLElement lastHtmlElement)
        {
            _componentHeightPixels = lastHtmlElement.clientHeight;
        }

        private void SetInitialBottomEmptyDivHeight(HTMLElement lastHtmlElement)
        {
            var bottomEmptyDivHeightPixels = GetBottomPaddingDivHeight();

            _bottomEmptyDiv.SetStyle(cssStyleDeclaration =>
            {
                cssStyleDeclaration.height = $"{bottomEmptyDivHeightPixels}px";
            });
       }

        private int GetBottomPaddingDivHeight()
        {
            var componentsNotRenderedCount =
                _componentsCount - ComponentsRenderedCount;

            var bottomEmptyDivHeightPixels =
                componentsNotRenderedCount * _componentHeightPixels;

            return bottomEmptyDivHeightPixels;
        }

        private void AttachBasicListContainerOnScrollEvent()
        {
            _innerElement.addEventListener("scroll", listener =>
            {
                var scrollTop= _innerElement.scrollTop;
                var scrollDirection = GetScrollDirection(_innerElement.scrollTop);

                if (scrollDirection == ScrollDirection.Down)
                {
                    var loadBoundary =
                        RenderedComponentsHeight - _innerElement.clientHeight;

                    if (scrollTop >= loadBoundary)
                    {
                        RenderComponents();
                        IncrementCurrentPage();
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