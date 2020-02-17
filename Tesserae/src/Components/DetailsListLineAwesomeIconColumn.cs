using System;
using System.Runtime.InteropServices;
using static Tesserae.UI;
using static Retyped.dom;

namespace Tesserae.Components
{
    public class DetailsListLineAwesomeIconColumn : IDetailsListColumn
    {
        public DetailsListLineAwesomeIconColumn(
            string sortingKey,
            LineAwesome lineAwesomeIcon,
            UnitSize width,
            LineAwesomeSize lineAwesomeSize = LineAwesomeSize.Default)
        {
            if (string.IsNullOrWhiteSpace(sortingKey))
            {
                throw new ArgumentException(nameof(sortingKey));
            }

            Width            = width ?? throw new ArgumentNullException(nameof(width));
            SortingKey       = sortingKey;
            LineAwesomeIcon  = lineAwesomeIcon;
            LineAwesomeSize  = lineAwesomeSize;
        }

        public string SortingKey               { get; }

        public LineAwesome LineAwesomeIcon     { get; }

        public UnitSize Width                  { get; }

        public LineAwesomeSize LineAwesomeSize { get; }

        public bool IsRowHeader                => false;

        public bool EnableColumnSorting        => true;

        public bool EnableOnColumnClickEvent   => false;

        public void OnColumnClick()
        {
        }

        public HTMLElement Render()
        {
            return Div(_())
                .appendChild(
                    I(LineAwesomeIcon, LineAwesomeSize));
        }
    }
}
