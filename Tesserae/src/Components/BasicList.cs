using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly string _componentHeight;
        private readonly string _componentWidth;

        public BasicList(
            IEnumerable<IComponent> components,
            int rowsPerPage   = 4,
            int columnsPerRow = 4,
            int height = 250)
        {
            _rowsPerPage       = rowsPerPage;
            _columnsPerRow     = columnsPerRow;
            _components        = components.ToList();
            _componentsCount   = _components.Count;
            _componentsPerPage = rowsPerPage * columnsPerRow;

            _componentHeight = GetSize(_rowsPerPage);
            _componentWidth  = GetSize(_columnsPerRow);

            _pagesCount = (int)Math.Ceiling((double)_componentsCount / _rowsPerPage);

            var initialCountOfComponentsToRender =
                _pagesCount > 2 ? _componentsPerPage * 3 : _componentsPerPage;

            _innerElement =
                Div(_("tss-basiclist",
                    styles: cssStyleDeclaration =>
                    {
                        cssStyleDeclaration.height = $"{height}px";
                    }));

            _components
                .Take(initialCountOfComponentsToRender)
                .ForEach(x =>
                    _innerElement.appendChild(
                        Div(
                            _("tss-basiclist-item",
                                styles: cssStyleDeclaration =>
                                {
                                    cssStyleDeclaration.height = _componentHeight;
                                    cssStyleDeclaration.width  = _componentWidth;
                                }),
                            x.Render())));

            AttachOnScrollEvent();
        }

        public HTMLElement Render() => _innerElement;

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

        private string GetSize(int numberOfItems) => $"{100 / numberOfItems}%";

        private void AttachOnScrollEvent() => _innerElement.addEventListener("scroll", OnScroll);

        private void OnScroll(object sender)
        {
            var scrollTop = _innerElement.scrollTop;
            var scrollHeight = _innerElement.scrollHeight;

            console.log($"scrollTop: {scrollTop}");
            console.log($"scrollHeight: {scrollHeight}");


        }
    }
}