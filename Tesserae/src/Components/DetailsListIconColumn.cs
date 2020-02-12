using System;
using Tesserae.Helpers.HTML;
using static Tesserae.UI;
using static Retyped.dom;

namespace Tesserae.Components
{
    public class DetailsListIconColumn : IDetailsListColumn
    {
        public DetailsListIconColumn(
            string title,
            WidthDimension width,
            string icon,
            Action onColumnClick = null)
        {
            Title         = title;
            Width         = width;
            Icon          = icon;
            OnColumnClick = onColumnClick;
        }

        public string Title         { get; }

        public WidthDimension Width { get; }

        public string Icon          { get; }

        public bool IsRowHeader => false;

        public Action OnColumnClick { get; }

        public HTMLElement Render()
        {
            var htmlElement = Div(_());

            htmlElement.appendChild(Icon(Icon).Render());

            return htmlElement;
        }
    }
}
