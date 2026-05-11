using System;

namespace Tesserae
{
    /// <summary>
    /// Defines the interface for items that can be displayed within a Sidenav (vertical icon-only navigation rail).
    /// </summary>
    public interface ISidenavItem
    {
        /// <summary>Renders the item.</summary>
        IComponent Render();

        /// <summary>Gets or sets whether the item is currently selected.</summary>
        bool IsSelected { get; set; }

        /// <summary>Gets the identifier of the item.</summary>
        string Identifier { get; }

        /// <summary>Shows the item.</summary>
        void Show();

        /// <summary>Collapses the item.</summary>
        void Collapse();
    }
}
