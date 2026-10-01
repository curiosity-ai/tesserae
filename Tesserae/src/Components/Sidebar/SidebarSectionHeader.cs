using static Transpose.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// The row at the top of a sidebar that names the section the sidebar is showing: the section's glyph, its
    /// name and an optional line saying what is in it ("Data, AI and delivery"). It has the shape and height of
    /// <see cref="SidebarBrand"/> but is a heading rather than a control: it does not answer the pointer, takes
    /// no tab stop and carries no commands. Use it where a rail to the left (a <see cref="Sidenav"/>) picks the
    /// section and the sidebar beside it lists what is in it.
    /// <para>
    /// On the collapsed rail the header is the glyph alone, with the name (and the second line) in its tooltip.
    /// </para>
    /// </summary>
    public class SidebarSectionHeader : ISidebarItem
    {
        private readonly IComponent  _open;
        private readonly IComponent  _closed;
        private readonly HTMLElement _openOuter;
        private readonly HTMLElement _closedOuter;
        private readonly IComponent  _closedTarget;
        private readonly HTMLElement _titleSpan;
        private readonly HTMLElement _subtitleSpan;

        private string _title;
        private string _subtitle;

        /// <summary>
        /// Initializes a new instance of the SidebarSectionHeader class.
        /// </summary>
        /// <param name="identifier">The identifier for the item.</param>
        /// <param name="icon">The section's glyph.</param>
        /// <param name="title">The section's name.</param>
        /// <param name="subtitle">What is in the section. None leaves the header a single line.</param>
        /// <param name="weight">The glyph's weight.</param>
        public SidebarSectionHeader(string identifier, UIcons icon, string title, string subtitle = null, UIconsWeight weight = UIconsWeight.Regular)
        {
            Identifier = identifier;
            _title     = title    ?? string.Empty;
            _subtitle  = subtitle ?? string.Empty;

            _titleSpan    = Span(Att("tss-sidebar-identity-title",    text: _title));
            _subtitleSpan = Span(Att("tss-sidebar-identity-subtitle", text: _subtitle));

            var openRoot = Div(Att("tss-sidebar-section-header"),
                Div(Att("tss-sidebar-identity-content"),
                    Div(Att("tss-sidebar-brand-logo"), Icon(icon, weight, TextSize.Medium).Render()),
                    Div(Att("tss-sidebar-identity-lines"), _titleSpan, _subtitleSpan)));
            openRoot.id = identifier;

            var closedRoot = Div(Att("tss-sidebar-section-header-closed"),
                Div(Att("tss-sidebar-brand-logo"), Icon(icon, weight, TextSize.Medium).Render()));
            closedRoot.id = identifier;
            _closedTarget = Raw(closedRoot);

            //The divider .Separated() draws goes on a wrapper, as it does for the brand row, so it runs out to
            //the sidebar's edges without moving the header's own content
            _openOuter   = Div(Att("tss-sidebar-section-header-row"),                                          openRoot);
            _closedOuter = Div(Att("tss-sidebar-section-header-row tss-sidebar-section-header-row-closed"), closedRoot);

            _open   = Raw(_openOuter);
            _closed = Raw(_closedOuter);

            UpdateSubtitleVisibility();
            UpdateTooltip();
        }

        /// <summary>Gets or sets whether the item is currently selected. A heading is not a destination, so this is only carried for the sidebar's sake.</summary>
        public bool IsSelected { get; set; }

        /// <summary>Gets the component that is currently rendered.</summary>
        public IComponent CurrentRendered => _closed.IsMounted() ? _closed : _open;

        /// <summary>Gets the full identifier of the item, including group identifiers.</summary>
        public string Identifier { get; private set; }

        /// <summary>Gets the own identifier of the item, without group identifiers.</summary>
        public string OwnIdentifier => Sidebar.GetOwnIdentifier(Identifier);

        /// <summary>Adds a group identifier prefix to the item's identifier.</summary>
        public void AddGroupIdentifier(string groupIdentifier)
        {
            Identifier = Sidebar.WithGroupIdentifier(Identifier, groupIdentifier);
        }

        /// <summary>Shows the item.</summary>
        public void Show()
        {
            _open.Show();
            _closed.Show();
        }

        /// <summary>Collapses the item.</summary>
        public void Collapse()
        {
            _open.Collapse();
            _closed.Collapse();
        }

        /// <summary>
        /// Sets the section's name.
        /// </summary>
        /// <param name="title">The name.</param>
        /// <returns>The current instance of the type.</returns>
        public SidebarSectionHeader SetTitle(string title)
        {
            _title                 = title ?? string.Empty;
            _titleSpan.textContent = _title;
            UpdateTooltip();
            return this;
        }

        /// <summary>
        /// Sets the second line. Null or empty leaves the header a single line.
        /// </summary>
        /// <param name="subtitle">What is in the section.</param>
        /// <returns>The current instance of the type.</returns>
        public SidebarSectionHeader SetSubtitle(string subtitle)
        {
            _subtitle                 = subtitle ?? string.Empty;
            _subtitleSpan.textContent = _subtitle;
            UpdateSubtitleVisibility();
            UpdateTooltip();
            return this;
        }

        /// <summary>
        /// Draws a divider under the header, run out to the sidebar's own edges, between it and the rows it heads.
        /// </summary>
        /// <param name="separated">Whether to draw it.</param>
        /// <returns>The current instance of the type.</returns>
        public SidebarSectionHeader Separated(bool separated = true)
        {
            _openOuter.UpdateClassIf(separated,   "tss-sidebar-identity-separated");
            _closedOuter.UpdateClassIf(separated, "tss-sidebar-identity-separated");
            return this;
        }

        /// <summary>
        /// Marks the item as not draggable in a sortable sidebar.
        /// </summary>
        /// <returns>The current instance of the type.</returns>
        public SidebarSectionHeader NotSortable()
        {
            _openOuter.classList.add("tss-sortable-disable");
            _closedOuter.classList.add("tss-sortable-disable");
            return this;
        }

        /// <summary>Renders the item for the closed state of the sidebar.</summary>
        public IComponent RenderClosed() => _closed;

        /// <summary>Renders the item for the open state of the sidebar.</summary>
        public IComponent RenderOpen() => _open;

        private void UpdateSubtitleVisibility()
        {
            _subtitleSpan.style.display = string.IsNullOrWhiteSpace(_subtitle) ? "none" : "";
        }

        //The collapsed rail has room for the glyph only, so the name and the second line go in its tooltip
        private void UpdateTooltip()
        {
            var text = string.IsNullOrWhiteSpace(_subtitle) ? _title : _title + " - " + _subtitle;
            _closedTarget.Tooltip(text, placement: TooltipPlacement.Right);
        }
    }
}
