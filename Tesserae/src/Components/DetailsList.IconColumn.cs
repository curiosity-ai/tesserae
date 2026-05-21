using System;
using static Tesserae.UI;
using static H5.Core.dom;

namespace Tesserae
{
    /// <summary>
    /// A specialized <see cref="DetailsList{T}"/> column that renders a single icon per row.
    /// </summary>
    [H5.Name("tss.DetailsListIconColumn")]
    public class DetailsListIconColumn : IDetailsListColumn
    {
        private readonly Action      _onColumnClick;
        private readonly HTMLElement InnerElement;

        public DetailsListIconColumn(Icon icon, UnitSize width, UnitSize maxWidth, bool enableColumnSorting = false, string sortingKey = null, Action onColumnClick = null)
        {
            if (enableColumnSorting && string.IsNullOrWhiteSpace(sortingKey))
            {
                throw new ArgumentException(nameof(sortingKey));
            }

            Icon                = icon;
            Width               = width ?? throw new ArgumentNullException(nameof(width));
            MaxWidth            = maxWidth;
            SortingKey          = sortingKey ?? string.Empty;
            EnableColumnSorting = enableColumnSorting;

            if (onColumnClick != null)
            {
                _onColumnClick           = onColumnClick;
                EnableOnColumnClickEvent = true;
            }

            InnerElement = Div(_()).appendChild(Icon.Render());
        }

        public string      SortingKey               { get; }
        public Icon        Icon                     { get; }
        /// <summary>
        /// Gets or sets the CSS width of the component.
        /// </summary>
        public UnitSize    Width                    { get; }
        /// <summary>
        /// Gets or sets the CSS max-width of the component.
        /// </summary>
        public UnitSize    MaxWidth                 { get; }
        public bool        IsRowHeader              => false;
        /// <summary>
        /// Enables the column sorting on the component.
        /// </summary>
        public bool        EnableColumnSorting      { get; }
        /// <summary>
        /// Enables the on column click event on the component.
        /// </summary>
        public bool        EnableOnColumnClickEvent { get; }
        /// <summary>
        /// Registers a callback invoked when the column click event fires.
        /// </summary>
        public void        OnColumnClick()          => _onColumnClick?.Invoke();
        /// <summary>
        /// Renders the component's root HTML element.
        /// </summary>
        public HTMLElement Render()                 => InnerElement;
    }
}