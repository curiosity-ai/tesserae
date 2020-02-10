using System;
using Tesserae.Helpers.HTML;

namespace Tesserae.Components
{
    public class DetailsListColumn : IDetailsListColumn<DetailsListItem>
    {
        public DetailsListColumn(
            string name,
            WidthDimension minWidth,
            WidthDimension maxWidth,
            bool isIconOnly = false,
            Func<DetailsListItem, IComponent> onItemRender = null,
            Action onColumnClick = null)
        {
            Name          = name;
            OnItemRender  = onItemRender;
            MinWidth      = minWidth;
            MaxWidth      = maxWidth;
            IsIconOnly    = isIconOnly;
            OnColumnClick = onColumnClick;
        }

        public string Name                                    { get; }

        public WidthDimension MinWidth                        { get; }

        public WidthDimension MaxWidth                        { get; }

        public bool IsIconOnly                                { get; }

        public Func<DetailsListItem, IComponent> OnItemRender { get; }

        public Action OnColumnClick                           { get; }
    }
}
