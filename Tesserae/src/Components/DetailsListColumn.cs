using System;
using static Tesserae.UI;
using static Retyped.dom;

namespace Tesserae.Components
{
    public class DetailsListColumn : IDetailsListColumn
    {
        public DetailsListColumn(
            string sortingKey,
            string title,
            UnitSize width,
            bool isRowHeader = false)
        {
            if (string.IsNullOrWhiteSpace(sortingKey))
            {
                throw new ArgumentException(nameof(sortingKey));
            }

            if (string.IsNullOrWhiteSpace(title))
            {
                throw new ArgumentException(nameof(title));
            }

            Width       = width ?? throw new ArgumentNullException(nameof(width));
            SortingKey  = sortingKey;
            Title       = title;
            IsRowHeader = isRowHeader;
        }

        public string SortingKey             { get; }

        public string Title                  { get; }

        public UnitSize Width                { get; }

        public bool IsRowHeader              { get; }

        public bool EnableColumnSorting      => true;

        public bool EnableOnColumnClickEvent => false;

        public void OnColumnClick()
        {
        }

        public HTMLElement Render()
        {
            return TextBlock(Title)
                    .Regular()
                    .SemiBold()
                    .Render();
        }
    }
}
