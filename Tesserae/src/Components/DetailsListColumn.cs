using System;
using static Tesserae.UI;
using static Retyped.dom;

namespace Tesserae.Components
{
    public class DetailsListColumn<TDetailsListItem> : IDetailsListColumn<TDetailsListItem>
        where TDetailsListItem : class, IDetailsListItem
    {
        public DetailsListColumn(
            string title,
            UnitSize width,
            bool isRowHeader = false,
            Action<TDetailsListItem> onColumnClick = null)
        {
            Title         = title;
            Width         = width;
            IsRowHeader   = isRowHeader;
            OnColumnClick = onColumnClick;
        }

        public string Title                           { get; }

        public UnitSize Width                         { get; }

        public bool IsRowHeader                       { get; }

        public Action<TDetailsListItem> OnColumnClick { get; }

        public HTMLElement Render()
        {
            return TextBlock(Title)
                    .Regular()
                    .SemiBold()
                    .Render();
        }
    }
}
