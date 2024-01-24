using System;
using System.Collections.Generic;
using H5.Core;
using TNT;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// A placeholder component to indicate where new items should be added to the sidebar
    /// </summary>
    public class SidebarAddItemsHereMarker : ISidebarItem
    {
        private readonly IComponent _closed;
        private readonly IComponent _open;
        public           IComponent CurrentRendered => _closed.IsMounted() ? _closed : _open;
        public           bool       IsSelected      { get; set; }

        public SidebarAddItemsHereMarker(IComponent placeholderComponenet = null)
        {
            placeholderComponenet = placeholderComponenet ?? Button("New Items".t()).Primary().SetIcon(UIcons.Add, weight: UIconsWeight.Solid).Class("tss-sidebar-btn");

            _closed = Empty(); // Should not be rendered in a closed sidebar

            var div = Div(_("tss-sidebar-btn-open"));
            div.appendChild(placeholderComponenet.Render());
            _open = Raw(div);
        }

        public void Show()
        {
            _closed.Show();
            _open.Show();
        }

        public void Collapse()
        {
            _closed.Collapse();
            _open.Collapse();
        }

        public const string NEW_ITEMS_ADDED_ORDER_KEY = "NEW_ITEMS_ADDED_HERE";

        public string Identifier      => NEW_ITEMS_ADDED_ORDER_KEY;
        public string GroupIdentifier { get; }

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