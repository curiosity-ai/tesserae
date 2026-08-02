using Tesserae;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// A sidebar item that is simply a component of the host's own - a list, a picker, a whole panel -
    /// for the parts of a sidebar that buttons and separators can't express. A chat history, a tree of
    /// spaces, a filter form: whatever it is, it keeps its own state and only asks the sidebar for a
    /// place to stand.
    /// <para>
    /// The closed (icon-rail) state gets a component of its own, since almost nothing that is worth
    /// putting here fits in a 48px rail. Passing none - the default - leaves the item out of the rail
    /// entirely, which is usually what a list wants.
    /// </para>
    /// </summary>
    public class SidebarComponent : ISidebarItem
    {
        private readonly IComponent _open;
        private readonly IComponent _closed;

        /// <summary>
        /// Initializes a new instance of the SidebarComponent class.
        /// </summary>
        /// <param name="identifier">The identifier for the item.</param>
        /// <param name="component">What the item shows while the sidebar is open.</param>
        /// <param name="closedComponent">What it shows while the sidebar is collapsed to its rail. Null - the default - shows nothing at all there.</param>
        public SidebarComponent(string identifier, IComponent component, IComponent closedComponent = null)
        {
            Identifier = identifier;

            _open   = (component ?? Empty()).Class("tss-sidebar-component").Id(identifier);
            _closed = closedComponent is object ? closedComponent.Class("tss-sidebar-component") : Empty();
        }

        /// <summary>Gets or sets whether the item is currently selected. A hosted component owns its own selection, so this is only carried for the sidebar's sake.</summary>
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
            Identifier = groupIdentifier + Sidebar.GroupIdentifierSeparator + Identifier;
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
        /// Marks the item as not draggable in a sortable sidebar - which is what a hosted component
        /// usually wants, since it is a region rather than an entry in a list.
        /// </summary>
        public SidebarComponent NotSortable()
        {
            _open.Class("tss-sortable-disable");
            _closed.Class("tss-sortable-disable");
            return this;
        }

        /// <summary>
        /// Lets the item take the leftover height of the sidebar's middle section, for a component
        /// that scrolls its own content (a list of chats, a tree).
        /// </summary>
        public SidebarComponent Grow()
        {
            _open.Grow();
            return this;
        }

        /// <summary>Renders the item for the closed state of the sidebar.</summary>
        public IComponent RenderClosed() => _closed;

        /// <summary>Renders the item for the open state of the sidebar.</summary>
        public IComponent RenderOpen() => _open;
    }
}
