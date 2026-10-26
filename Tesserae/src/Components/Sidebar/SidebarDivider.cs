using System;
using Transpose.Core;
using static Transpose.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// A horizontal rule across a <see cref="Sidebar"/>, run out to the rail's own edges - the line that sets the account
    /// row apart from the navigation above it, as an item of its own, so it can go between any two items in the header,
    /// the content or the footer. Unlike <see cref="SidebarSeparator"/>, which leaves a gap or a caption, it always draws a line,
    /// in both the open and the closed state.
    /// </summary>
    public class SidebarDivider : ISidebarItem
    {
        private readonly IComponent _closed;
        private readonly IComponent _open;

        /// <summary>Gets or sets whether the item is currently selected. A divider is never selected.</summary>
        public bool IsSelected { get; set; }

        /// <summary>Gets the component that is currently rendered.</summary>
        public IComponent CurrentRendered => _open.IsMounted() ? _open : _closed;

        /// <summary>Gets the full identifier of the divider, including group identifiers.</summary>
        public string Identifier { get; private set; }

        /// <summary>Gets the own identifier of the divider, without group identifiers.</summary>
        public string OwnIdentifier => Sidebar.GetOwnIdentifier(Identifier);

        /// <summary>
        /// Initializes a new instance of the SidebarDivider class.
        /// </summary>
        /// <param name="identifier">The identifier for the divider, unique within the sidebar.</param>
        public SidebarDivider(string identifier)
        {
            Identifier = identifier;
            //A rule is not something to drag: in a sortable sidebar it stays where it was added.
            _closed    = Raw(Div(Att("tss-sidebar-divider tss-sortable-disable", role: "separator")));
            _open      = Raw(Div(Att("tss-sidebar-divider tss-sortable-disable", role: "separator")));
        }

        /// <summary>Adds a group identifier prefix to the divider's identifier.</summary>
        public void AddGroupIdentifier(string groupIdentifier)
        {
            Identifier = Sidebar.WithGroupIdentifier(Identifier, groupIdentifier);
        }

        /// <summary>Hides the divider.</summary>
        public void Collapse()
        {
            _closed.Collapse();
            _open.Collapse();
        }

        /// <summary>Shows the divider.</summary>
        public void Show()
        {
            _closed.Show();
            _open.Show();
        }

        /// <summary>Renders the divider for the closed state of the sidebar.</summary>
        public IComponent RenderClosed() => _closed;

        /// <summary>Renders the divider for the open state of the sidebar.</summary>
        public IComponent RenderOpen() => _open;
    }
}
