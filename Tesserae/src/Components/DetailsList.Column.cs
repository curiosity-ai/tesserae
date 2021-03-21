using System;
using static Tesserae.UI;
using static H5.Core.dom;

namespace Tesserae
{
    public class DetailsListColumn : IDetailsListColumn
    {
        private readonly Action      _onColumnClick;
        private readonly HTMLElement InnerElement;

        public DetailsListColumn(string title, UnitSize width, UnitSize maxWidth, bool isRowHeader = false, bool enableColumnSorting = false, string sortingKey = null, Action onColumnClick = null)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                throw new ArgumentException(nameof(title));
            }

            if (enableColumnSorting && string.IsNullOrWhiteSpace(sortingKey))
            {
                throw new ArgumentException(nameof(sortingKey));
            }

            Width = width ?? throw new ArgumentNullException(nameof(width));
            MaxWidth = maxWidth;
            SortingKey = sortingKey ?? string.Empty;
            Title = title;
            IsRowHeader = isRowHeader;
            EnableColumnSorting = enableColumnSorting;

            if (onColumnClick != null)
            {
                _onColumnClick = onColumnClick;
                EnableOnColumnClickEvent = true;
            }

            InnerElement = TextBlock(Title).Regular().SemiBold().Render();
        }

        public string      SortingKey               { get; }
        public string      Title                    { get; }
        public UnitSize    Width                    { get; }
        public UnitSize    MaxWidth                 { get; }
        public bool        IsRowHeader              { get; }
        public bool        EnableColumnSorting      { get; }
        public bool        EnableOnColumnClickEvent { get; }
        public void        OnColumnClick()          => _onColumnClick?.Invoke();
        public HTMLElement Render()                 => InnerElement;
    }
}