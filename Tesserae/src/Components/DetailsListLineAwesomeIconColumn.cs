using System;
using static Tesserae.UI;
using static Retyped.dom;

namespace Tesserae.Components
{
    public class DetailsListLineAwesomeIconColumn : IDetailsListColumn
    {
        public DetailsListLineAwesomeIconColumn(
            LineAwesome lineAwesomeIcon,
            string title,
            UnitSize width,
            LineAwesomeSize lineAwesomeSize = LineAwesomeSize.Default,
            Action onColumnClick = null)
        {
            LineAwesomeIcon = lineAwesomeIcon;
            Title           = title;
            Width           = width;
            LineAwesomeSize = LineAwesomeSize;
            OnColumnClick   = onColumnClick;
        }

        public LineAwesome LineAwesomeIcon     { get; }

        public string Title                    { get; }

        public UnitSize Width                  { get; }

        public LineAwesomeSize LineAwesomeSize { get; }

        public bool IsRowHeader                => false;

        public Action OnColumnClick            { get; }

        public HTMLElement Render()
        {
            var htmlElement = Div(_());

            htmlElement.appendChild(LA(LineAwesomeIcon, LineAwesomeSize));

            return htmlElement;
        }
    }
}
