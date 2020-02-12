using System;
using Tesserae.Helpers.HTML;
using static Tesserae.UI;
using static Retyped.dom;

namespace Tesserae.Components
{
    public class DetailsListColumn : IDetailsListColumn
    {
        public DetailsListColumn(
            string title,
            WidthDimension minWidth,
            WidthDimension maxWidth,
            bool isRowHeader = false,
            Action onColumnClick = null)
        {
            Title         = title;
            MinWidth      = minWidth;
            MaxWidth      = maxWidth;
            IsRowHeader   = isRowHeader;
            OnColumnClick = onColumnClick;
        }

        public string Title            { get; }

        public WidthDimension MinWidth { get; }

        public WidthDimension MaxWidth { get; }

        public bool IsRowHeader        { get; }

        public Action OnColumnClick    { get; }

        public HTMLElement Render()
        {
            var htmlElement = Div(_());

            htmlElement.innerText = Title;

            return htmlElement;
        }
    }
}
