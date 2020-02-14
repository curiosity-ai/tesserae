using System;
using System.Runtime.InteropServices;
using static Tesserae.UI;
using static Retyped.dom;

namespace Tesserae.Components
{
    public class DetailsListLineAwesomeIconColumn : IDetailsListColumn
    {
        private readonly Action<DetailsListLineAwesomeIconColumn> _onColumnClick;

        public DetailsListLineAwesomeIconColumn(
            LineAwesome lineAwesomeIcon,
            string title,
            UnitSize width,
            LineAwesomeSize lineAwesomeSize = LineAwesomeSize.Default,
            Action<DetailsListLineAwesomeIconColumn> onColumnClick = null)
        {
            LineAwesomeIcon = lineAwesomeIcon;
            Title           = title;
            Width           = width;
            LineAwesomeSize = LineAwesomeSize;
            _onColumnClick  = onColumnClick;

            EnableOnColumnClickEvent = _onColumnClick != null;
        }

        public LineAwesome LineAwesomeIcon     { get; }

        public string Title                    { get; }

        public UnitSize Width                  { get; }

        public LineAwesomeSize LineAwesomeSize { get; }

        public bool IsRowHeader                 => false;

        public bool EnableColumnSorting         => false;

        public bool EnableOnColumnClickEvent    { get; }

        public void OnColumnClick()             => _onColumnClick?.Invoke(this);

        public HTMLElement Render()
        {
            return Div(_())
                .appendChild(
                    LA(LineAwesomeIcon, LineAwesomeSize));
        }
    }
}
