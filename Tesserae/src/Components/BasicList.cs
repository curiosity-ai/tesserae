using System.Collections.Generic;
using System.Linq;
using Retyped;
using Tesserae.HTML;
using static System.Math;
using static Retyped.dom;
using static Tesserae.UI;

namespace Tesserae.Components
{
    public class BasicList : IContainer<BasicList, IComponent>
    {
        private readonly HTMLElement _innerElement;
        private readonly int _rowsPerPage;
        private readonly int _columnsPerRow;
        private readonly List<IComponent> _components;
        private readonly int _componentsCount;
        private readonly int _pagesCount;
        private readonly int _componentsPerPage;
        private readonly string _componentHeightPercentage;
        private readonly string _componentWidthPercentage;
        private readonly int _initialCountOfComponentsToRender;

        public BasicList(
            IEnumerable<IComponent> components,
            int rowsPerPage   = 4,
            int columnsPerRow = 4,
            int height = 250)
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

            _initialCountOfComponentsToRender =
                _pagesCount > 2 ? _componentsPerPage * 3 : _componentsPerPage;

            _innerElement = CreateBasicListContainer(height);

            _components
                .Take(_initialCountOfComponentsToRender)
                .ForEach((component, index) =>
                {
                    var htmlElement = component.Render();

                    if (index == _initialCountOfComponentsToRender - 1)
                    {
                        DomMountedObserver.NotifyWhenMounted(htmlElement, () =>
                        {
                            var componentHeight = htmlElement.clientHeight;

                            var componentsNotRenderedCount =
                                _componentsCount - _initialCountOfComponentsToRender;

                            var emptyDivHeight =
                                componentsNotRenderedCount * componentHeight;

                            _innerElement.appendChild(Div(_(styles: cssStyleDeclaration =>
                            {
                                cssStyleDeclaration.height = $"{emptyDivHeight}px";
                            })));
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

        private void AttachOnScrollEvent() => _innerElement.addEventListener("scroll", OnScroll);

        private void OnScroll(object sender)
        {
            var scrollTop = _innerElement.scrollTop;
            var scrollHeight = _innerElement.scrollHeight;

            console.log($"scrollTop: {scrollTop}");
            console.log($"scrollHeight: {scrollHeight}");

            var com = _components.First();


        }
    }
}