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
        public readonly  SettableObservable<IReadOnlyList<ISidebarItem>> _middle;
        private readonly ObservableList<ISidebarItem>                    _footer;
        private readonly SettableObservable<bool>                        _closed;
        private          double                                          _closedTimeout;
        private readonly Stack                                           _sidebar;
        public           bool                                            ReorderMode = false;

        private string[] _itemOrder;

        public const int SIDEBAR_TRANSITION_TIME = 300;


        public bool IsClosed { get { return _closed.Value; } set { _closed.Value = value; } }

        public Sidebar()
        {
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
                    if (ReorderMode)
                    {
                        Refresh();
                    }
                    ReorderMode = false;
                }
            });
        }

        private void RenderSidebar(IReadOnlyList<ISidebarItem> header, IReadOnlyList<ISidebarItem> middle, IReadOnlyList<ISidebarItem> footer, bool closed)
        {
            var stackMiddle = VStack();

            if (ReorderMode)
            {
                var middleArray = middle.ToArray();

                middleArray.Push(new SidebarAddItemsHereMarker());
                middle = middleArray;


                Sortable.Create(stackMiddle.Render(), new SortableOptions()
                {
                    animation  = 150,
                    invertSwap = true,
                    ghostClass = "tss-sortable-ghost",
                    onEnd = e =>
                    {
                        _itemOrder = _itemOrder ?? _middle.Value.Select(i => i.Identifier).ToArray();
                        if (!_itemOrder.Contains(SidebarAddItemsHereMarker.NEW_ITEMS_ADDED_ORDER_KEY)) _itemOrder.Push(SidebarAddItemsHereMarker.NEW_ITEMS_ADDED_ORDER_KEY);

                        _itemOrder.MoveItem(e.oldIndex, e.newIndex);
                    }
                });
            }

            if (_itemOrder is object)
            {
                middle = OrderItems(middle);
            }

            foreach (var item in middle)
            {
                if (item is SidebarNav nav)
                {
                    nav.ReorderMode = ReorderMode;
                }
            }

            _sidebar.Children(VStack().Class("tss-sidebar-header").WS().NoShrink().Children(header.Select(si => closed ? si.RenderClosed() : si.RenderOpen())),
                stackMiddle.Class("tss-sidebar-middle").WS().H(10).Grow().ScrollY().Children(middle.Select(si => closed ? si.RenderClosed() : si.RenderOpen())),
                VStack().Class("tss-sidebar-footer").WS().NoShrink().Children(footer.Select(si => closed ? si.RenderClosed() : si.RenderOpen()))
            );
        }

        private IReadOnlyList<ISidebarItem> OrderItems(IReadOnlyList<ISidebarItem> items)
        {
            var unorderedValue                     = Array.IndexOf(_itemOrder, SidebarAddItemsHereMarker.NEW_ITEMS_ADDED_ORDER_KEY);
            if (unorderedValue < 0) unorderedValue = int.MaxValue;

            var dict = new object();

            for (var i = 0; i < _itemOrder.Length; i++)
            {
                dict[_itemOrder[i]] = i;
            }

            return items.OrderBy(k => dict.HasOwnProperty(k.Identifier) ? dict[k.Identifier] : unorderedValue).ToList();
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

        [ObjectLiteral]
        public class SidebarOrder
        {
            public string[]                     TopLevelOrder { get; set; }
            public Dictionary<string, string[]> Children      { get; set; }
        }

        public void InitSorting(SidebarOrder order)
        {
            _itemOrder = order.TopLevelOrder;

            foreach (var item in _middle.Value)
            {
                if (item is SidebarNav nav && order.Children.TryGetValue(item.Identifier, out string[] itemOrder))
                {
                    nav.InitSorting(itemOrder);
                }
            }
        }

        public SidebarOrder GetSorting()
        {
            var children = new Dictionary<string, string[]>();

            foreach (var item in _middle.Value)
            {
                if (item is SidebarNav nav)
                {
                    children[nav.Identifier] = nav.GetSorting();
                }
            }

            return new SidebarOrder()
            {
                TopLevelOrder = _itemOrder ?? _middle.Value.Select(i => i.Identifier).ToArray(),
                Children      = children
            };
        }
        public void Refresh()
        {
            RenderSidebar(_header.Value, _middle.Value, _footer.Value, _closed.Value);
        }
    }
}