using System;
using static Tesserae.UI;
using static Retyped.dom;

namespace Tesserae.Components
{
    public class DetailsListLineAwesomeIconColumn<TDetailsListItem> : IDetailsListColumn<TDetailsListItem>
        where TDetailsListItem : class, IDetailsListItem
    {
        public DetailsListLineAwesomeIconColumn(
            LineAwesome lineAwesomeIcon,
            string title,
            UnitSize width,
            LineAwesomeSize lineAwesomeSize = LineAwesomeSize.Default,
            Action<TDetailsListItem> onColumnClick = null)
        {
            LineAwesomeIcon = lineAwesomeIcon;
            Title           = title;
            Width           = width;
            LineAwesomeSize = LineAwesomeSize;
            OnColumnClick   = onColumnClick;
        }

        public LineAwesome LineAwesomeIcon            { get; }

        public string Title                           { get; }

        public UnitSize Width                         { get; }

        public LineAwesomeSize LineAwesomeSize        { get; }

        public bool IsRowHeader                       => false;

        public Action<TDetailsListItem> OnColumnClick { get; }

        public HTMLElement Render()
        {
            return Div(_())
                .appendChild(
                    LA(LineAwesomeIcon, LineAwesomeSize));
        }
    }
}
