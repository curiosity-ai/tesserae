using System;
using System.Collections.Generic;
using System.Linq;
using H5;
using TNT;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    /// <summary>
    /// A vertical icon navigation rail (Sidenav) intended to be used as the leftmost
    /// navigation in an application. Each item is rendered as an icon with an optional
    /// label below it. Behaves similarly to a Sidebar but is always narrow and
    /// icon-centric. Often combined with a Sidebar to its right.
    /// </summary>
    [H5.Name("tss.Sidenav")]
    public sealed class Sidenav : IComponent
    {
        private readonly ObservableList<ISidenavItem> _header;
        private readonly ObservableList<ISidenavItem> _middleContent;
        private readonly ObservableList<ISidenavItem> _footer;
        private readonly Stack _sidenav;

        /// <summary>
        /// Initializes a new instance of the Sidenav class.
        /// </summary>
        public Sidenav()
        {
            _header        = new ObservableList<ISidenavItem>();
            _middleContent = new ObservableList<ISidenavItem>();
            _footer        = new ObservableList<ISidenavItem>();
            _sidenav       = VStack().Class("tss-sidenav");

            var combined = new CombinedObservable<IReadOnlyList<ISidenavItem>, IReadOnlyList<ISidenavItem>, IReadOnlyList<ISidenavItem>>(_header, _middleContent, _footer);

            combined.ObserveFutureChanges(content => RenderSidenav(content.first, content.second, content.third));

            RenderSidenav(_header.Value, _middleContent.Value, _footer.Value);
        }

        /// <summary>
        /// Renders the sidenav using the secondary background color.
        /// </summary>
        public Sidenav Secondary()
        {
            _sidenav.Class("tss-sidenav-secondary");
            return this;
        }

        /// <summary>
        /// Hides the labels under the icons.
        /// </summary>
        public Sidenav HideLabels(bool hide = true)
        {
            if (hide)
            {
                _sidenav.Class("tss-sidenav-no-labels");
            }
            else
            {
                _sidenav.RemoveClass("tss-sidenav-no-labels");
            }
            return this;
        }

        /// <summary>
        /// Adds an item to the sidenav header section (top of the rail).
        /// </summary>
        public Sidenav AddHeader(ISidenavItem item)
        {
            _header.Add(item);
            return this;
        }

        /// <summary>
        /// Adds an item to the sidenav middle content section.
        /// </summary>
        public Sidenav AddContent(ISidenavItem item)
        {
            _middleContent.Add(item);
            return this;
        }

        /// <summary>
        /// Removes an item from the middle content section.
        /// </summary>
        public Sidenav RemoveContent(ISidenavItem item)
        {
            _middleContent.Remove(item);
            return this;
        }

        /// <summary>
        /// Adds an item to the sidenav footer section (bottom of the rail).
        /// </summary>
        public Sidenav AddFooter(ISidenavItem item)
        {
            _footer.Add(item);
            return this;
        }

        /// <summary>
        /// Clears all sections of the sidenav.
        /// </summary>
        public void Clear()
        {
            ClearHeader();
            ClearContent();
            ClearFooter();
        }

        /// <summary>Clears the header section.</summary>
        public void ClearHeader()  => _header.Clear();
        /// <summary>Clears the middle content section.</summary>
        public void ClearContent() => _middleContent.Clear();
        /// <summary>Clears the footer section.</summary>
        public void ClearFooter()  => _footer.Clear();

        /// <summary>
        /// Selects the item with the given identifier (and deselects all the others).
        /// </summary>
        public Sidenav Select(string identifier)
        {
            foreach (var item in _header.Value.Concat(_middleContent.Value).Concat(_footer.Value))
            {
                item.IsSelected = (item.Identifier == identifier);
            }
            return this;
        }

        private void RenderSidenav(IReadOnlyList<ISidenavItem> header, IReadOnlyList<ISidenavItem> middle, IReadOnlyList<ISidenavItem> footer)
        {
            _sidenav.Children(
                VStack().Class("tss-sidenav-header").NoShrink().Children(header.Select(si => si.Render())),
                VStack().Class("tss-sidenav-middle").H(10).Grow().ScrollY().Children(middle.Select(si => si.Render())),
                VStack().Class("tss-sidenav-footer").NoShrink().Children(footer.Select(si => si.Render()))
            );
        }

        /// <summary>
        /// Renders the sidenav.
        /// </summary>
        public HTMLElement Render() => _sidenav.Render();
    }
}
