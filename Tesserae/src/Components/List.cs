using System.Collections.Generic;
using static Retyped.dom;

namespace Tesserae.Components
{
    public class List : ComponentBase<List, HTMLDivElement>
    {
        private readonly int _rowsPerPage;
        private readonly List<HTMLElement> _htmlElements;

        public List(
            int rowsPerPage = 3)
        {
            _rowsPerPage = rowsPerPage;
            _htmlElements = new List<HTMLElement>();
        }

        public HTMLElement Add(HTMLElement htmlElement)
        {
            _htmlElements.Add(htmlElement);

            return htmlElement;
        }

        public override HTMLElement Render()
        {
            return InnerElement;
        }

        private void HandleScroll()
        {
            InnerElement.onscroll += x =>
            {
                var scrollTop = InnerElement.scrollTop;
                console.log(scrollTop);
            };
        }
    }
}