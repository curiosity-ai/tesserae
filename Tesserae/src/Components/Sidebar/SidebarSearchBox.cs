using System;
using Transpose.Core;
using static Transpose.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    public class SidebarSearchBox : ISidebarItem
    {
        private readonly IComponent  _closed;
        private readonly IComponent  _open;
        private readonly SearchBox   _searchBox;
        private readonly HTMLElement _closedElement;
        private readonly HTMLElement _openElement;

        public bool IsSelected { get; set; }

        public IComponent CurrentRendered => _open.IsMounted() ? _open : _closed;

        public string Identifier { get; private set; }

        public string OwnIdentifier => Sidebar.GetOwnIdentifier(Identifier);

        private event Action<string> Searched;

        public SidebarSearchBox(string identifier, string placeholder = "Search...")
        {
            Identifier = identifier;

            _closedElement = Div(_("tss-sidebar-btn tss-sidebar-btn-closed-icon"), I(UIcons.Search));
            _closedElement.title = placeholder;

            _searchBox = SearchBox(placeholder).Underlined().SearchAsYouType().NoIcon();
            _searchBox.OnSearch((s, v) => Searched?.Invoke(v));

            _openElement = Div(_("tss-sidebar-btn-open tss-sidebar-searchbox"));
            _openElement.appendChild(_searchBox.Render());

            _closed = Raw(_closedElement);
            _open   = Raw(_openElement);
        }

        /// <summary>
        /// Renders the search box with rounded corners (defaults to a fully rounded "pill" shape,
        /// like the rounded SidebarButton). Removes the underlined style so the box shows a full
        /// rounded border.
        /// </summary>
        /// <param name="radius">The border radius to apply. Defaults to <see cref="BorderRadius.Full"/>.</param>
        /// <returns>The current instance of the type.</returns>
        public SidebarSearchBox Rounded(BorderRadius radius = BorderRadius.Full)
        {
            _searchBox.IsUnderlined = false;
            _searchBox.Rounded(radius);
            _openElement.classList.add("tss-sidebar-searchbox-rounded");
            _closedElement.classList.add("tss-sidebar-searchbox-rounded");
            return this;
        }

        public SidebarSearchBox OnSearch(Action<string> onSearch)
        {
            Searched += onSearch;
            return this;
        }

        public SidebarSearchBox SetKeyboardShortcut(params string[] keys)
        {
            _searchBox.SetKeyboardShortcut(keys);
            return this;
        }

        public void AddGroupIdentifier(string groupIdentifier)
        {
             Identifier = groupIdentifier + Sidebar.GroupIdentifierSeparator + Identifier;
        }

        public void Collapse()
        {
            _closed.Collapse();
            _open.Collapse();
        }

        public void Show()
        {
             _closed.Show();
             _open.Show();
        }

        public IComponent RenderClosed()
        {
            return _closed;
        }

        public IComponent RenderOpen()
        {
            return _open;
        }
    }
}
