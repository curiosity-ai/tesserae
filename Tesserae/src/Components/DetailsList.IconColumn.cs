using System;
using static Tesserae.UI;
using static Retyped.dom;

namespace Tesserae.Components
{
    public class DetailsListIconColumn : IDetailsListColumn
    {
        private readonly Action _onColumnClick;
        private HTMLElement InnerElement;

        public DetailsListIconColumn(LineAwesome lineAwesomeIcon, UnitSize width, LineAwesomeSize lineAwesomeSize = LineAwesomeSize.Default, bool enableColumnSorting = false, string sortingKey = null, Action onColumnClick = null)
        {
            if (enableColumnSorting && string.IsNullOrWhiteSpace(sortingKey))
            {
                throw new ArgumentException(nameof(sortingKey));
            }

            LineAwesomeIcon     = lineAwesomeIcon;
            LineAwesomeSize     = lineAwesomeSize;
            Width               = width      ?? throw new ArgumentNullException(nameof(width));
            SortingKey          = sortingKey ?? string.Empty;
            EnableColumnSorting = enableColumnSorting;

            if (onColumnClick != null)
            {
                _onColumnClick           = onColumnClick;
                EnableOnColumnClickEvent = true;
            }

            InnerElement = Div(_()).appendChild(I(LineAwesomeIcon, LineAwesomeSize));
        }

        public string SortingKey               { get; }

        public LineAwesome LineAwesomeIcon     { get; }

        public UnitSize Width                  { get; }

        public LineAwesomeSize LineAwesomeSize { get; }

        public bool IsRowHeader                => false;

        public bool EnableColumnSorting        { get; }

        public bool EnableOnColumnClickEvent   { get; }

        public void OnColumnClick()            => _onColumnClick?.Invoke();

        public HTMLElement Render() => InnerElement;
    }
}
