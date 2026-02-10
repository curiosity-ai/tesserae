using System;
using H5.Core;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    public class SidebarSearchBox : ISidebarItem
    {
        private readonly IComponent _closed;
        private readonly IComponent _open;
        private readonly SearchBox  _searchBox;

        public bool IsSelected { get; set; }

        public IComponent CurrentRendered => _open.IsMounted() ? _open : _closed;

        public string Identifier { get; private set; }

        public string OwnIdentifier => Sidebar.GetOwnIdentifier(Identifier);

        private event Action<string> Searched;

        public SidebarSearchBox(string identifier, string placeholder = "Search...")
        {
            Identifier = identifier;

            var closedElement = Div(_("tss-sidebar-btn tss-sidebar-btn-closed-icon"), I(UIcons.Search));
            closedElement.title = placeholder;

            _searchBox = SearchBox(placeholder).Underlined().SearchAsYouType().NoIcon();
            _searchBox.OnSearch((s, v) => Searched?.Invoke(v));

            var openElement = Div(_("tss-sidebar-btn-open tss-sidebar-searchbox"));
            openElement.appendChild(_searchBox.Render());

            _closed = Raw(closedElement);
            _open   = Raw(openElement);
        }

        public SidebarSearchBox OnSearch(Action<string> onSearch)
        {
            Searched += onSearch;
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
