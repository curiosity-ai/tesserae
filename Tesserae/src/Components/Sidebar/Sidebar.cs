using System.Collections.Generic;
using System.Linq;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    [H5.Name("tss.Sidebar")]
    public sealed class Sidebar : IComponent
    {
        private readonly ObservableList<ISidebarItem> _header;
        private readonly ObservableList<ISidebarItem> _middle;
        private readonly ObservableList<ISidebarItem> _footer;
        private readonly SettableObservable<bool>     _closed;
        private          double                       _closedTimeout;
        private readonly Stack                        _sidebar;

        public const int SIDEBAR_TRANSITION_TIME = 300;

        public bool IsClosed { get { return _closed.Value; } set { _closed.Value = value; } }

        public Sidebar()
        {
            _header = new ObservableList<ISidebarItem>();
            _middle = new ObservableList<ISidebarItem>();
            _footer = new ObservableList<ISidebarItem>();
            _closed = new SettableObservable<bool>(false);
            _sidebar = VStack().Class("tss-sidebar");

            _closed.Observe(isClosed =>
            {
                //Do this on a timeout to improve the animation behaviour
                window.clearTimeout(_closedTimeout);

                _closedTimeout = window.setTimeout((_) =>
                {
                    if (isClosed)
                    {
                        _sidebar.Class("tss-sidebar-closed");
                    }
                    else
                    {
                        _sidebar.RemoveClass("tss-sidebar-closed");
                    }
                }, 15);
            });

            var combined = new CombinedObservable<IReadOnlyList<ISidebarItem>, IReadOnlyList<ISidebarItem>, IReadOnlyList<ISidebarItem>, bool>(_header, _middle, _footer, _closed);

            combined.ObserveFutureChanges(content => RenderSidebar(content.first, content.second, content.third, content.forth));
        }

        private void RenderSidebar(IReadOnlyList<ISidebarItem> header, IReadOnlyList<ISidebarItem> middle, IReadOnlyList<ISidebarItem> footer, bool closed)
        {
            _sidebar.Children(VStack().Class("tss-sidebar-header").WS().NoShrink().Children(header.Select(si => closed ? si.RenderClosed() : si.RenderOpen())),
                VStack().Class("tss-sidebar-middle").WS().H(10).Grow().ScrollY().Children(middle.Select(si => closed ? si.RenderClosed() : si.RenderOpen())),
                VStack().Class("tss-sidebar-footer").WS().NoShrink().Children(footer.Select(si => closed ? si.RenderClosed() : si.RenderOpen()))
            );
        }

        public Sidebar Closed(bool isClosed = true)
        {
            _closed.Value = isClosed;
            return this;
        }

        public Sidebar Toggle()
        {
            _closed.Value = !_closed.Value;
            return this;
        }

        public Sidebar AddHeader(ISidebarItem item)
        {
            _header.Add(item);
            return this;
        }
        public Sidebar AddContent(ISidebarItem item)
        {
            _middle.Add(item);
            return this;
        }
        public Sidebar AddFooter(ISidebarItem item)
        {
            _footer.Add(item);
            return this;
        }

        public void Clear()
        {
            ClearHeader();
            ClearContent();
            ClearFooter();
        }
        public void ClearHeader()  => _header.Clear();
        public void ClearContent() => _middle.Clear();
        public void ClearFooter()  => _footer.Clear();

        public HTMLElement Render() => _sidebar.Render();
    }
}