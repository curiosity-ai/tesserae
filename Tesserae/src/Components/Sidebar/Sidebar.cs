using System;
using System.Collections.Generic;
using System.Linq;
using H5;
using TNT;
using static H5.Core.dom;
using static Tesserae.UI;

namespace Tesserae
{
    [H5.Name("tss.Sidebar")]
    public sealed class Sidebar : IComponent
    {
        private readonly ObservableList<ISidebarItem>                    _header;
        private readonly SettableObservable<IReadOnlyList<ISidebarItem>> _middle;
        private readonly ObservableList<ISidebarItem>                    _footer;
        private readonly SettableObservable<bool>                        _closed;
        private          double                                          _closedTimeout;
        private readonly Stack                                           _sidebar;
        private          bool                                            _isSortable;

        private event Action<string[], Dictionary<string, string[]>> _onSortingChanged;

        private string[]                     _itemOrder;
        private Dictionary<string, string[]> _itemOrderChildren = new Dictionary<string, string[]>();

        public const int SIDEBAR_TRANSITION_TIME = 300;

        public bool IsClosed { get { return _closed.Value; } set { _closed.Value = value; } }

        public Sidebar(bool sortable = false)
        {
            _isSortable = sortable;

            _header  = new ObservableList<ISidebarItem>();
            _middle  = new SettableObservable<IReadOnlyList<ISidebarItem>>(new List<ISidebarItem>());
            _footer  = new ObservableList<ISidebarItem>();
            _closed  = new SettableObservable<bool>(false);
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

            // disable Reordering in a closed sidebar
            _closed.ObserveFutureChanges(closed =>
            {
                if (closed)
                {
                    if (_isSortable)
                    {
                        var before = _isSortable;
                        _isSortable = false;
                        Refresh();
                        _isSortable = before;
                    }
                }
            });
        }

        public void Sortable(bool sortable = true)
        {
            _isSortable = sortable;
            Refresh();
        }

        private void RenderSidebar(IReadOnlyList<ISidebarItem> header, IReadOnlyList<ISidebarItem> middle, IReadOnlyList<ISidebarItem> footer, bool closed)
        {
            var stackMiddle = VStack();

            if (_isSortable)
            {
                var sortable = new Sortable(stackMiddle.Render(), new SortableOptions()
                {
                    animation  = 150,
                    invertSwap = true,
                    ghostClass = "tss-sortable-ghost",
                    onEnd = e =>
                    {
                        _itemOrder = _itemOrder ?? _middle.Value.Select(i => i.Identifier).ToArray();
                        _itemOrder.MoveItem(e.oldIndex, e.newIndex);
                        _onSortingChanged?.Invoke(_itemOrder, _itemOrderChildren);
                    }
                });

                foreach (var middleItem in middle)
                {
                    if (middleItem is SidebarNav middleNavItem)
                    {
                        middleNavItem.Sortable();

                        middleNavItem.OnSortingChanged((newItemOrder) =>
                        {
                            _itemOrderChildren[middleItem.Identifier] = newItemOrder;
                            _onSortingChanged?.Invoke(_itemOrder, _itemOrderChildren);
                        });
                    }
                }
            }

            if (_itemOrder is object)
            {
                middle = OrderItems(_itemOrder, middle);
            }

            if (_itemOrderChildren is object)
            {
                foreach (var middleItem in middle)
                {
                    if (middleItem is SidebarNav middleNavItem && _itemOrderChildren.TryGetValue(middleItem.Identifier, out var navOrder))
                    {
                        middleNavItem.LoadSorting(navOrder);
                    }
                }
            }

            _sidebar.Children(VStack().Class("tss-sidebar-header").WS().NoShrink().Children(header.Select(si => closed ? si.RenderClosed() : si.RenderOpen())),
                stackMiddle.Class("tss-sidebar-middle").WS().H(10).Grow().ScrollY().Children(middle.Select(si => closed ? si.RenderClosed() : si.RenderOpen())),
                VStack().Class("tss-sidebar-footer").WS().NoShrink().Children(footer.Select(si => closed ? si.RenderClosed() : si.RenderOpen()))
            );
        }

        internal static IReadOnlyList<ISidebarItem> OrderItems(string[] order, IReadOnlyList<ISidebarItem> items)
            => items.OrderBy(k =>
            {
                var index = Array.IndexOf(order, k.Identifier);
                return index >= 0 ? index : int.MaxValue;
            }).ToList();


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
            if (_middle.Value.Any(m => m.Identifier == item.Identifier)) throw new ArgumentException("Identifier already in use: " + item.Identifier);

            _middle.Value = _middle.Value?.Concat(new[] { item }).ToList();
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
        public void ClearContent() => _middle.Value = new List<ISidebarItem>();
        public void ClearFooter()  => _footer.Clear();

        public HTMLElement Render() => _sidebar.Render();

        public void LoadSorting(string[] topLevelOrder, Dictionary<string, string[]> children)
        {
            _itemOrder         = topLevelOrder;
            _itemOrderChildren = children;
        }

        public (string[] topLevelOrder, Dictionary<string, string[]> children) GetCurrentSorting()
        {
            var children = new Dictionary<string, string[]>();

            foreach (var item in _middle.Value)
            {
                if (item is SidebarNav nav)
                {
                    children[nav.Identifier] = nav.GetCurrentSorting();
                }
            }
            return (_itemOrder ?? _middle.Value.Select(i => i.Identifier).ToArray(), children);
        }

        public void Refresh()
        {
            RenderSidebar(_header.Value, _middle.Value, _footer.Value, _closed.Value);
        }

        public void OnSortingChanged(Action<string[], Dictionary<string, string[]>> onSortingChanged)
        {
            _onSortingChanged += onSortingChanged;
        }
    }
}