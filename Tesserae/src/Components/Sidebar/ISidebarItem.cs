using System;
using System.Collections.Generic;

namespace Tesserae
{
    /// <summary>
    /// Defines the interface for items that can be displayed within a Sidebar.
    /// </summary>
    public interface ISidebarItem
    {
        /// <summary>Renders the item for the closed (collapsed) state of the sidebar.</summary>
        IComponent RenderClosed();
        /// <summary>Renders the item for the open (expanded) state of the sidebar.</summary>
        IComponent RenderOpen();
        /// <summary>Gets or sets whether the item is currently selected.</summary>
        bool       IsSelected      { get; set; }
        /// <summary>Gets the component that is currently rendered.</summary>
        IComponent CurrentRendered { get; }
        /// <summary>Shows the item.</summary>
        void       Show();
        /// <summary>Collapses the item.</summary>
        void       Collapse();
        /// <summary>Gets the full identifier of the item, including group identifiers.</summary>
        string     Identifier { get; }
        /// <summary>Gets the own identifier of the item, without group identifiers.</summary>
        string     OwnIdentifier { get; }
        /// <summary>Adds a group identifier prefix to the item's identifier.</summary>
        void       AddGroupIdentifier(string groupIdentifier);
    }
}