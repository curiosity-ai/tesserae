using System;
using static Tesserae.UI;
using static Retyped.dom;

namespace Tesserae.Components
{
    public class DetailsListColumn : IDetailsListColumn
    {
        private readonly Action<DetailsListColumn> _onColumnClick;

        public DetailsListColumn(
            string title,
            UnitSize width,
            bool isRowHeader = false,
            bool enableColumnSorting = true,
            Action<DetailsListColumn> onColumnClick = null)
        {
            Title               = title;
            Width               = width;
            IsRowHeader         = isRowHeader;
            EnableColumnSorting = enableColumnSorting;
            _onColumnClick      = onColumnClick;

            EnableOnColumnClickEvent = onColumnClick != null;
        }

        public string Title                  { get; }

        public UnitSize Width                { get; }

        public bool IsRowHeader              { get; }

        public bool EnableColumnSorting      { get; }

        public bool EnableOnColumnClickEvent { get; }

        public void OnColumnClick()          => _onColumnClick?.Invoke(this);

        public HTMLElement Render()
        {
            return TextBlock(Title)
                    .Regular()
                    .SemiBold()
                    .Render();
        }
    }
}
