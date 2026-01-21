using System;
using System.Collections.Generic;
using H5.Core;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// A Text component for use within a Sidebar, supporting both open and closed states.
    /// </summary>
    public class SidebarText : ISidebarItem
    {
        private readonly TextBlock  _closed;
        private readonly TextBlock  _open;
        /// <summary>Gets the component that is currently rendered.</summary>
        public           IComponent CurrentRendered => _closed.IsMounted() ? _closed : _open;

        /// <summary>Gets or sets whether the item is currently selected.</summary>
        public bool IsSelected { get; set; }

        /// <summary>
        /// Initializes a new instance of the SidebarText class.
        /// </summary>
        /// <param name="identifier">The identifier for the text item.</param>
        /// <param name="text">The text to display when the sidebar is open.</param>
        /// <param name="closedText">The optional text to display when the sidebar is closed.</param>
        /// <param name="textSize">The size of the text.</param>
        /// <param name="textWeight">The weight of the text.</param>
        public SidebarText(string identifier, string text, string closedText = null, TextSize textSize = TextSize.Small, TextWeight textWeight = TextWeight.Regular)
        {
            Identifier = identifier;
            _closed    = TextBlock(closedText ?? "", textSize: textSize, textWeight: textWeight).Id(identifier);
            _open      = TextBlock(text,             textSize: textSize, textWeight: textWeight).Id(identifier);
        }

        /// <summary>Shows the text component.</summary>
        public void Show()
        {
            _closed.Show();
            _open.Show();
        }

        /// <summary>Collapses the text component.</summary>
        public void Collapse()
        {
            _closed.Collapse();
            _open.Collapse();
        }

        /// <summary>
        /// Marks the text component as not sortable.
        /// </summary>
        /// <returns>The current instance of the type.</returns>
        public SidebarText NotSortable()
        {
            _closed.Class("tss-sortable-disable");
            _open.Class("tss-sortable-disable");
            return this;
        }

        /// <summary>Gets the full identifier of the text component.</summary>
        public string Identifier { get; private set; }

        /// <summary>Gets the own identifier of the text component.</summary>
        public string OwnIdentifier => Sidebar.GetOwnIdentifier(Identifier);

        /// <summary>Adds a group identifier prefix to the text component's identifier.</summary>
        public void AddGroupIdentifier(string groupIdentifier)
        {
            Identifier = groupIdentifier + Sidebar.GroupIdentifierSeparator + Identifier;
        }


        /// <summary>
        /// Sets the text content for the open state.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>The current instance of the type.</returns>
        public SidebarText SetText(string text)
        {
            _open.Text = text;
            return this;
        }

        /// <summary>
        /// Sets the foreground color of the text.
        /// </summary>
        /// <param name="color">The color.</param>
        /// <returns>The current instance of the type.</returns>
        public SidebarText Foreground(string color)
        {
            _open.Foreground   = color;
            _closed.Foreground = color;
            return this;
        }

        /// <summary>
        /// Sets the top padding of the text.
        /// </summary>
        /// <param name="pixels">The padding in pixels.</param>
        /// <returns>The current instance of the type.</returns>
        public SidebarText PT(int pixels)
        {
            _open.PT(pixels);
            _closed.PT(pixels);
            return this;
        }

        /// <summary>
        /// Sets the bottom padding of the text.
        /// </summary>
        /// <param name="pixels">The padding in pixels.</param>
        /// <returns>The current instance of the type.</returns>
        public SidebarText PB(int pixels)
        {
            _open.PB(pixels);
            _closed.PB(pixels);
            return this;
        }

        /// <summary>
        /// Sets the left padding of the text.
        /// </summary>
        /// <param name="pixels">The padding in pixels.</param>
        /// <returns>The current instance of the type.</returns>
        public SidebarText PL(int pixels)
        {
            _open.PL(pixels);
            return this;
        }


        /// <summary>Renders the text for the closed state of the sidebar.</summary>
        public IComponent RenderClosed()
        {
            return _closed;
        }


        /// <summary>Renders the text for the open state of the sidebar.</summary>
        public IComponent RenderOpen()
        {
            return _open;
        }
    }
}