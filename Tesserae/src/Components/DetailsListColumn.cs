using System;
using static Tesserae.UI;
using static Retyped.dom;

namespace Tesserae.Components
{
    public class DetailsListColumn : IDetailsListColumn
    {
        public DetailsListColumn(
            string title,
            UnitSize width,
            bool isRowHeader = false,
            Action onColumnClick = null)
        {
            Title         = title;
            Width         = width;
            IsRowHeader   = isRowHeader;
            OnColumnClick = onColumnClick;
        }

        public string Title            { get; }

        public UnitSize Width          { get; }

        public bool IsRowHeader        { get; }

        public Action OnColumnClick    { get; }

        public HTMLElement Render()
        {
            var htmlElement = TextBlock(Title).Regular().SemiBold();

            return htmlElement.Render();
        }
    }
}
