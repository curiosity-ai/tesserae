using System;
using static Tesserae.UI;
using static Transpose.Core.dom;

namespace Tesserae
{
    /// <summary>
    /// A typed column definition used to declare how a property of <typeparamref name="T"/> is rendered inside a
    /// <see cref="DetailsList{T}"/>.
    /// </summary>
    [Transpose.Name("tss.DetailsListColumn")]
    public class DetailsListColumn : IDetailsListColumn
    {
        private readonly Action      _onColumnClick;
        private readonly HTMLElement InnerElement;

        /// <summary>
        /// Initializes a new instance of this class.
        /// </summary>
        public DetailsListColumn(string title, UnitSize width, UnitSize maxWidth, bool isRowHeader = false, bool enableColumnSorting = false, string sortingKey = null, Action onColumnClick = null)
        {
            if (enableColumnSorting && string.IsNullOrWhiteSpace(sortingKey))
            {
                throw new ArgumentException(nameof(sortingKey));
            }

            Width               = width ?? throw new ArgumentNullException(nameof(width));
            MaxWidth            = maxWidth;
            SortingKey          = sortingKey ?? string.Empty;
            Title               = title ?? "";
            IsRowHeader         = isRowHeader;
            EnableColumnSorting = enableColumnSorting;

            if (onColumnClick != null)
            {
                _onColumnClick           = onColumnClick;
                EnableOnColumnClickEvent = true;
            }

            InnerElement = TextBlock(Title).SemiBold().Render();
        }

        /// <summary>
        /// Gets or sets the sorting key.
        /// </summary>
        public string      SortingKey               { get; }
        /// <summary>
        /// Gets or sets the title of the component.
        /// </summary>
        public string      Title                    { get; }
        /// <summary>
        /// Gets or sets the CSS width of the component.
        /// </summary>
        public UnitSize    Width                    { get; }
        /// <summary>
        /// Gets or sets the CSS max-width of the component.
        /// </summary>
        public UnitSize    MaxWidth                 { get; }
        /// <summary>
        /// Returns a value indicating whether the component is row header.
        /// </summary>
        public bool        IsRowHeader              { get; }
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