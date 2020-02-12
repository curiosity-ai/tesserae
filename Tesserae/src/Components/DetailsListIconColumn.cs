using System;
using Tesserae.Helpers.HTML;
using static Tesserae.UI;
using static Retyped.dom;

namespace Tesserae.Components
{
    public class DetailsListIconColumn : IDetailsListColumn
    {
        public DetailsListIconColumn(
            string name,
            WidthDimension minWidth,
            WidthDimension maxWidth,
            string icon,
            Action onColumnClick = null)
        {
            Name          = name;
            MinWidth      = minWidth;
            MaxWidth      = maxWidth;
            Icon          = icon;
            OnColumnClick = onColumnClick;
        }

        public string Name             { get; }

        public WidthDimension MinWidth { get; }

        public WidthDimension MaxWidth { get; }

        public string Icon             { get; }

        public Action OnColumnClick    { get; }

        public HTMLElement Render()
        {
            var htmlElement = Div(_());

            htmlElement.appendChild(Icon(Icon).Render());

            return htmlElement;
        }
    }
}
