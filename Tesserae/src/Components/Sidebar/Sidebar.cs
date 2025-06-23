﻿using System;
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
        private readonly SettableObservable<IReadOnlyList<ISidebarItem>> _middleContent;
        private readonly ObservableList<ISidebarItem>                    _footer;
        private readonly SettableObservable<bool>                        _closed;
        private          double                                          _closedTimeout;
        private readonly Stack                                           _sidebar;
        private          bool                                            _isSortable;

        private Action<Dictionary<string, string[]>> _onSortingChanged;

        private List<string> _itemOrder = new List<string>();

        public const int SIDEBAR_TRANSITION_TIME = 300;

        public bool IsClosed { get { return _closed.Value; } set { _closed.Value = value; } }

        public Sidebar(bool sortable = false)
        {
            _isSortable = sortable;

            _header        = new ObservableList<ISidebarItem>();
            _middleContent = new SettableObservable<IReadOnlyList<ISidebarItem>>(new List<ISidebarItem>());
            _footer        = new ObservableList<ISidebarItem>();
            _closed        = new SettableObservable<bool>(false);
            _sidebar       = VStack().Class("tss-sidebar");

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


            var combined = new CombinedObservable<IReadOnlyList<ISidebarItem>, IReadOnlyList<ISidebarItem>, IReadOnlyList<ISidebarItem>, bool>(_header, _middleContent, _footer, _closed);

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
                    swapThreshold = 0.65,
                    filter = ".tss-sortable-disable",
                    onEnd = e =>
                    {
                        if (e.oldIndex != e.newIndex)
                        {
                            var old = _itemOrder[e.oldIndex];
                            _itemOrder.RemoveAt(e.oldIndex);
                            _itemOrder.Insert(e.newIndex, old);
                            _onSortingChanged?.Invoke(GetCurrentSorting());
                        }
                    }
                });

                foreach (var middleItem in middle)
                {
                    if (middleItem is SidebarNav middleNavItem)
                    {
                        middleNavItem.Sortable();

                        middleNavItem.OnSortingChanged(newItemOrder =>
                        {
                            var itemOrder = GetCurrentSorting();

                            foreach (var newItem in newItemOrder)
                            {
                                itemOrder[newItem.Key] = newItem.Value;
                            }

                            _onSortingChanged?.Invoke(itemOrder);
                        });
                    }
                }
            }

            _sidebar.Children(VStack().Class("tss-sidebar-header").WS().NoShrink().Children(header.Select(si => closed ? si.RenderClosed() : si.RenderOpen())),
                stackMiddle.Class("tss-sidebar-middle").WS().H(10).Grow().ScrollY().Children(middle.Select(si => closed ? si.RenderClosed() : si.RenderOpen())),
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

        public const string ROOT_SIDEBAR_FOR_ORDERING = "ROOT";

        public Sidebar AddContent(ISidebarItem item)
        {
            item.AddGroupIdentifier(ROOT_SIDEBAR_FOR_ORDERING);

            if (_middleContent.Value.Any(m => m.Identifier == item.Identifier))
            {
                return this; //nothing to do...
            }

            _middleContent.Value = _middleContent.Value?.Concat(new[] { item }).ToList();
            _itemOrder.Add(item.Identifier);
            return this;
        }

        public Sidebar RemoveContent(ISidebarItem item)
        {
            var identifierWithGroupIdentifier = ROOT_SIDEBAR_FOR_ORDERING + "_|_" + item.Identifier;

            if (_middleContent.Value.All(m => m.Identifier != identifierWithGroupIdentifier)) return this; //nothing to do

            _middleContent.Value = _middleContent.Value?.Where(m => m.Identifier != identifierWithGroupIdentifier).ToList();
            _itemOrder.Remove(identifierWithGroupIdentifier);
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
        public void ClearContent() => _middleContent.Value = new List<ISidebarItem>();
        public void ClearFooter()  => _footer.Clear();

        public HTMLElement Render() => _sidebar.Render();


        //Should be called after all items have been added
        public void LoadSorting(Dictionary<string, string[]> itemOrder)
        {
            if (itemOrder.TryGetValue(ROOT_SIDEBAR_FOR_ORDERING, out var topLevelOrder) && topLevelOrder is object)
            {
                var dict = new object();

                for (var i = 0; i < topLevelOrder.Length; i++)
                {
                    dict[topLevelOrder[i]] = i;
                }

                var itemOrderSorted = _itemOrder.OrderBy(i => dict.HasOwnProperty(i) ? dict[i] : int.MaxValue).Distinct().ToList();

                if (!_itemOrder.SequenceEqual(itemOrderSorted))
                {
                    _middleContent.Value = _middleContent.Value.OrderBy(i => dict.HasOwnProperty(i.Identifier) ? dict[i.Identifier] : int.MaxValue).ToArray();
                    _itemOrder           = itemOrderSorted;
                }
            }

            foreach (var middleItem in _middleContent.Value)
            {
                if (middleItem is SidebarNav middleNavItem)
                {
                    middleNavItem.LoadSorting(itemOrder);
                }
            }
        }

        public Dictionary<string, string[]> GetCurrentSorting()
        {
            var dict = new Dictionary<string, string[]>();

            foreach (var item in _middleContent.Value)
            {
                if (item is SidebarNav nav)
                {
                    foreach (var sorting in nav.GetCurrentSorting())
                    {
                        dict[sorting.Key] = sorting.Value.Distinct().ToArray();
                    }
                }
            }

            dict[ROOT_SIDEBAR_FOR_ORDERING] = _itemOrder.Distinct().ToArray();

            return dict;
        }

        public void Refresh()
        {
            RenderSidebar(_header.Value, _middleContent.Value, _footer.Value, _closed.Value);
        }

        public void OnSortingChanged(Action<Dictionary<string, string[]>> onSortingChanged)
        {
            _onSortingChanged = onSortingChanged;
        }

        public static string GetOwnIdentifier(string identifier)
        {
            var ix = identifier.IndexOf(GroupIdentifierSeparator);
            if (ix >= 0)
            {
                return identifier.Substring(ix + GroupIdentifierSeparator.Length);
            }
            return identifier;
        }

        public const string GroupIdentifierSeparator = "_|_";
    }
}