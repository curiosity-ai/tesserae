using System;
using H5.Core;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    public class SidebarSeparator : ISidebarItem
    {
        private readonly IComponent _closed;
        private readonly IComponent _open;

        public bool IsSelected { get; set; }

        public IComponent CurrentRendered => _open.IsMounted() ? _open : _closed;

        public string Identifier { get; private set; }

        public string OwnIdentifier => Sidebar.GetOwnIdentifier(Identifier);

        public SidebarSeparator(string identifier, string text = null)
        {
            Identifier = identifier;
            var closedElement = Div(_("tss-sidebar-separator"));
            var openElement = Div(_("tss-sidebar-separator"));

            if(!string.IsNullOrEmpty(text))
            {
                openElement.classList.add("tss-sidebar-separator-with-text");
                openElement.textContent = text;
            }

            _closed = Raw(closedElement);
            _open = Raw(openElement);
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
