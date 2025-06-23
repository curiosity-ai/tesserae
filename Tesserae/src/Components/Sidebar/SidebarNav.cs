using System;
using System.Collections.Generic;
using System.Linq;
using H5;
using H5.Core;
using static H5.Core.dom;
using static Tesserae.UI;
using TNT;
using static TNT.T;
using static Tesserae.ArrayExtensions;

namespace Tesserae
{
    public class SidebarNav : ISidebarItem
    {
        public event Action<Dictionary<string, string[]>> _onSortingChanged;
        public event Action<ParentChangedEvent>     _onParentChanged;

        private          string                                          _text;
        private readonly Button                                          _closedHeader;
        private readonly HTMLElement                                     _openHeader;
        private readonly Button                                          _arrow;
        private readonly Button                                          _openHeaderButton;
        private readonly SettableObservable<IReadOnlyList<ISidebarItem>> _items;
        private readonly SettableObservable<bool>                        _collapsed;
        private readonly SettableObservable<bool>                        _selected;
        private readonly Func<IComponent>                                _closedContent;
        private readonly Func<IComponent>                                _openContent;
        private          SidebarCommand[]                                _commands;
        private          bool                                            _isHidden;
        private          bool                                            _isSortable = false;
        private          string                                          _sortableGroup = null;

        private List<string> _itemOrder = new List<string>();

        private event Action<HTMLElement> _onRendered;

        public bool IsCollapsed { get { return _collapsed.Value; } set { _collapsed.Value = value; } }
        public bool IsSelected  { get { return _selected.Value; }  set { _selected.Value  = value; } }

        public IObservable<bool> CollapsedStatus => _collapsed;

        private IComponent _lastClosed;
        private IComponent _lastOpen;

        public IComponent CurrentRendered => (_lastClosed is object && _lastClosed.IsMounted()) ? _lastClosed : _lastOpen;

        public SidebarNav(string identifier, Emoji  icon, string       text,   bool   initiallyCollapsed, params SidebarCommand[] commands) : this(identifier, text, initiallyCollapsed, b => b.SetIcon(icon), commands) { }
        public SidebarNav(string identifier, UIcons icon, string       text,   bool   initiallyCollapsed, params SidebarCommand[] commands) : this(identifier, text, initiallyCollapsed, b => b.SetIcon(icon), commands) { }
        public SidebarNav(string identifier, UIcons icon, UIconsWeight weight, string text,               bool                    initiallyCollapsed, params SidebarCommand[] commands) : this(identifier, text, initiallyCollapsed, b => b.SetIcon(icon, weight: weight), commands) { }

        public const string collapse_local_storage_save_key = "tss_sidebar_nav_collapse_state_";

        public static void WithCollapseStatePersist(Func<string, string> get, Action<string, string> set)
        {
            _getCollapseState = get;
            _setCollapseState = set;
        }

        private static Func<string, string>   _getCollapseState = null;
        private static Action<string, string> _setCollapseState = null;
        private bool _notSortable;

        private SidebarNav(string identifier, string text, bool initiallyCollapsed, Action<Button> setButtonIcon, params SidebarCommand[] commands)
        {
            Identifier    = identifier;
            _text         = text;
            _closedHeader = Button().Class("tss-sidebar-nav-header").Class("tss-sidebar-btn").Id(identifier + "-closed");
            setButtonIcon(_closedHeader);
            _openHeader = Div(_("tss-sidebar-nav-header tss-sidebar-btn-open tss-sidebar-nav-header-empty", id: identifier + "-open"));

            _arrow = Button().Class("tss-sidebar-nav-arrow");

            _openHeaderButton = Button().SetText(text).Class("tss-sidebar-nav-button");
            setButtonIcon(_openHeaderButton);

            _openHeader.appendChild(_openHeaderButton.Render());
            _openHeader.appendChild(_arrow.Render());

            _commands = commands;

            if (commands.Length > 0)
            {
                var divCmd = Div(_("tss-sidebar-commands"));
                _openHeader.appendChild(divCmd);

                foreach (var c in commands)
                {
                    divCmd.appendChild(c.Render());
                }

                var hookContextMenu = _commands.FirstOrDefault(c => c.ShouldHookToContextMenu);

                if (hookContextMenu is object)
                {
                    OnContextMenu((b, e) => hookContextMenu.RaiseOnClick(e));
                }
            }

            _items = new SettableObservable<IReadOnlyList<ISidebarItem>>(new ISidebarItem[] { });

            _collapsed = new SettableObservable<bool>(initiallyCollapsed);

            if (!string.IsNullOrWhiteSpace(identifier))
            {
                var collapseState = _getCollapseState is object
                    ? _getCollapseState(collapse_local_storage_save_key    + identifier)
                    : localStorage.getItem(collapse_local_storage_save_key + identifier);

                if (bool.TryParse(collapseState, out var cs))
                {
                    _collapsed.Value = cs;
                }

                _collapsed.ObserveFutureChanges(c =>
                {
                    if (_setCollapseState is object)
                    {
                        _setCollapseState(collapse_local_storage_save_key + identifier, c.ToString());
                    }
                    else
                    {
                        localStorage.setItem(collapse_local_storage_save_key + identifier, c.ToString());
                    }
                });
            }

            _selected = new SettableObservable<bool>(false);

            _closedContent = () => DeferSync(_items, (items) => RenderClosed(items));
            _openContent   = () => DeferSync(_items, (items) => RenderOpened(items));

            _arrow.OnClick(() =>
            {
                _collapsed.Value = !_collapsed.Value;
            });
        }

        public void Show()
        {
            _lastOpen?.Show();
            _lastClosed?.Show();
            _isHidden = false;
        }

        public void Collapse()
        {
            _lastOpen?.Collapse();
            _lastClosed?.Collapse();
            _isHidden = true;
        }
        public SidebarNav NotSortable()
        {
            _lastOpen?.Class("tss-sortable-disable");
            _lastClosed?.Class("tss-sortable-disable");
            _notSortable = true;
            return this;
        }

        public SidebarNav SetText(string text)
        {
            _openHeaderButton.SetText(text);
            _closedHeader.Tooltip(text, placement: TooltipPlacement.Top);
            _text = text;
            return this;
        }

        public SidebarNav Collapsed(bool isCollapsed = true)
        {
            _collapsed.Value = isCollapsed;
            return this;
        }

        public SidebarNav Selected(bool isSelected = true)
        {
            _selected.Value = isSelected;
            return this;
        }

        public SidebarNav Toggle()
        {
            _collapsed.Value = !_collapsed.Value;
            return this;
        }

        public SidebarNav ShowDotIfEmpty()
        {
            _openHeader.classList.add("tss-sidebar-nav-header-dot-if-empty");
            return this;
        }

        public SidebarNav OnOpenIconClick(Action<HTMLElement, MouseEvent> action)
        {
            _openHeaderButton.OnIconClick(action);
            _openHeaderButton.Class("tss-sidebar-btn-has-icon-click");
            return this;
        }

        public SidebarNav OnOpenIconClick(Action action)
        {
            _openHeaderButton.OnIconClick((_, __) => action());
            _openHeaderButton.Class("tss-sidebar-btn-has-icon-click");
            return this;
        }

        private Action WrapAction(Action action)
        {
            return () =>
            {
                if (IsSelected && _items.Value.Count > 0)
                {
                    Toggle();
                }
                else
                {
                    action();
                }
            };
        }

        private Action<Button, MouseEvent> WrapAction(Action<Button, MouseEvent> action)
        {
            return (b, e) =>
            {
                if (IsSelected && _items.Value.Count > 0)
                {
                    Toggle();
                }
                else
                {
                    action(b, e);
                }
            };
        }

        public SidebarNav OnClick(Action action)
        {
            var wrapped = WrapAction(action);
            _closedHeader.OnClick(wrapped);
            _openHeaderButton.OnClick(wrapped);
            return this;
        }

        public SidebarNav OnClick(Action<SidebarNav> action)
        {
            var wrapped = WrapAction(() => action(this));
            _closedHeader.OnClick(wrapped);
            _openHeaderButton.OnClick(wrapped);
            return this;
        }
        public SidebarNav OnClick(Action<Button, MouseEvent> action)
        {
            var wrapped = WrapAction(action);
            _closedHeader.OnClick((b,     e) => wrapped(b, e));
            _openHeaderButton.OnClick((b, e) => wrapped(b, e));
            return this;
        }

        public SidebarNav OnContextMenu(Action action)
        {
            _closedHeader.OnContextMenu(action);
            _openHeaderButton.OnContextMenu(action);
            return this;
        }

        public SidebarNav OnContextMenu(Action<Button, MouseEvent> action)
        {
            _closedHeader.OnContextMenu((b,     e) => action(b, e));
            _openHeaderButton.OnContextMenu((b, e) => action(b, e));
            return this;
        }

        private IComponent RenderOpened(IReadOnlyList<ISidebarItem> items)
        {
            if (items.Count > 0)
            {
                _openHeader.classList.remove("tss-sidebar-nav-header-empty");
            }
            else
            {
                _openHeader.classList.add("tss-sidebar-nav-header-empty");
            }

            foreach (var c in _commands) c.RefreshTooltip();

            var nav = Div(_("tss-sidebar-nav", id: Identifier));
            nav["tssOwner"] = this;
            nav.appendChild(_openHeader);

            var children = VStack();

            if (_isSortable)
            {
                var sortable = new Sortable(children.Render(), new SortableOptions()
                {
                    animation  = 150,
                    ghostClass = "tss-sortable-ghost",
                    swapThreshold = 0.65,
                    filter = ".tss-sortable-disable",
                    group = _sortableGroup,
                    onEnd = e =>
                    {
                        if(e.from != e.to)
                        {
                            _onParentChanged?.Invoke(ConvertEvent(e));
                        }
                        else if (e.oldIndex != e.newIndex)
                        {
                            var old = _itemOrder[e.oldIndex];
                            _itemOrder.RemoveAt(e.oldIndex);
                            _itemOrder.Insert(e.newIndex, old);
                            _onSortingChanged?.Invoke(GetCurrentSorting());
                        }
                    }
                });
            }

            nav.appendChild(children.Class("tss-sidebar-nav-children").Children(items.Select(i => i.RenderOpen())).Render());

            CollapsedChanged(_collapsed.Value);
            SelectedChanged(_selected.Value);

            DomObserver.WhenMounted(nav, () =>
            {
                _collapsed.Observe(CollapsedChanged);
                _selected.Observe(SelectedChanged);

                DomObserver.WhenRemoved(nav, () =>
                {
                    _collapsed.StopObserving(CollapsedChanged);
                    _selected.StopObserving(SelectedChanged);
                });
            });

            var comp = Raw(nav);
            _lastOpen = comp;
            _onRendered?.Invoke(_openHeader);

            if (_isHidden)
            {
                comp.Collapse();
            }

            if (_notSortable)
            {
                comp.Class("tss-sortable-disable");
            }

            return comp;

            void CollapsedChanged(bool isCollapsed)
            {
                if (isCollapsed)
                {
                    nav.classList.remove("tss-sidebar-nav-open");
                    _arrow.Tooltip("Expand".t(), placement: TooltipPlacement.Top);
                }
                else
                {
                    nav.classList.add("tss-sidebar-nav-open");
                    _arrow.Tooltip("Collapse".t(), placement: TooltipPlacement.Top);
                }
            }

            void SelectedChanged(bool isSelected)
            {
                if (isSelected)
                {
                    nav.classList.add("tss-sidebar-selected");
                }
                else
                {
                    nav.classList.remove("tss-sidebar-selected");
                }
            }
        }

        private ParentChangedEvent ConvertEvent(SortableEvent e)
        {
            SidebarNav GetParent(HTMLElement el)
            {
                if (el.classList.contains("tss-sidebar-nav-children"))
                {
                    return el.parentElement["tssOwner"].As<SidebarNav>();
                }
                return el["tssOwner"].As<SidebarNav>();
            }

            SidebarNav GetChild(HTMLElement el)
            {
                return el.querySelector(".tss-sidebar-nav")["tssOwner"].As<SidebarNav>();
            }

            return new ParentChangedEvent()
            {
                From = GetParent(e.from),
                To   = GetParent(e.to),
                Item = GetChild(e.item),
                Cancel = () => e.from.insertBefore(e.item, e.from.children[(uint)e.oldIndex])
            };
        }

        private IComponent RenderClosed(IReadOnlyList<ISidebarItem> items)
        {
            _closedHeader.Tooltip(_text, placement: TooltipPlacement.Top);

            var nav = Div(_("tss-sidebar-nav", id: Identifier));
            nav["tssOwner"] = this;
            nav.appendChild(_closedHeader.Render());
            nav.appendChild(VStack().Class("tss-sidebar-nav-children").Children(items.Select(i => i.RenderClosed())).Render());

            CollapsedChanged(_collapsed.Value);
            SelectedChanged(_selected.Value);

            DomObserver.WhenMounted(nav, () =>
            {
                _collapsed.Observe(CollapsedChanged);
                _selected.Observe(SelectedChanged);

                DomObserver.WhenRemoved(nav, () =>
                {
                    _collapsed.StopObserving(CollapsedChanged);
                    _selected.StopObserving(SelectedChanged);
                });
            });

            var comp = Raw(nav);
            _lastClosed = comp;
            _onRendered?.Invoke(_closedHeader.Render());

            if (_isHidden)
            {
                comp.Collapse();
            }

            if (_notSortable)
            {
                comp.Class("tss-sortable-disable");
            }

            return comp;

            void CollapsedChanged(bool isCollapsed)
            {
                if (isCollapsed)
                {
                    nav.classList.remove("tss-sidebar-nav-open");
                    _arrow.Tooltip("Expand".t(), placement: TooltipPlacement.Top);
                }
                else
                {
                    nav.classList.add("tss-sidebar-nav-open");
                    _arrow.Tooltip("Collapse".t(), placement: TooltipPlacement.Top);
                }
            }

            void SelectedChanged(bool isSelected)
            {
                if (isSelected)
                {
                    nav.classList.add("tss-sidebar-selected");
                }
                else
                {
                    nav.classList.remove("tss-sidebar-selected");
                }
            }
        }


        public void Clear()
        {
            _items.Value = new ISidebarItem[] { };
        }

        public void Add(ISidebarItem item)
        {
            item.AddGroupIdentifier(Identifier);

            var items = _items.Value.As<ISidebarItem[]>();

            if (items.Any(m => m.Identifier == item.Identifier))
            {
                return; //nothing to do...
            }

            items.Push(item);
            _items.Value = items.ToArray();
            _itemOrder.Add(item.Identifier);
        }

        public void Remove(ISidebarItem item)
        {
            var identifierWithGroupIdentifier = Identifier + "_|_" + item.Identifier;

            var newItems = _items.Value.As<ISidebarItem[]>();
            newItems     = newItems.Where(i => i.Identifier != identifierWithGroupIdentifier).ToArray();
            _items.Value = newItems;
            _itemOrder.Remove(identifierWithGroupIdentifier);
        }

        public SidebarNav AddRange(IEnumerable<ISidebarItem> items)
        {
            var newItems = _items.Value.As<ISidebarItem[]>();

            var itemsToAdd = items as ISidebarItem[] ?? items.ToArray();

            foreach (var item in itemsToAdd)
            {
                item.AddGroupIdentifier(Identifier);

                if (newItems.Any(m => m.Identifier == item.Identifier))
                {
                    continue; //already there...
                }

                newItems.Push(item);
            }

            _items.Value = newItems.ToArray();
            _itemOrder.AddRange(itemsToAdd.Select(i => i.Identifier));
            return this;
        }

        public IComponent RenderClosed() => _closedContent();

        public IComponent RenderOpen() => _openContent();

        public ISidebarItem OnRendered(Action<HTMLElement> onRendered)
        {
            _onRendered += onRendered;
            return this;
        }

        public string Identifier { get; private set; }

        public string OwnIdentifier => Sidebar.GetOwnIdentifier(Identifier);

        public void AddGroupIdentifier(string groupIdentifier)
        {
            Identifier = groupIdentifier + Sidebar.GroupIdentifierSeparator + Identifier;
        }

        public void LoadSorting(Dictionary<string, string[]> itemOrder)
        {
            if (itemOrder.TryGetValue(Identifier, out var childrenOrder) && childrenOrder is object)
            {
                var dict = new object();

                for (var i = 0; i < childrenOrder.Length; i++)
                {
                    dict[childrenOrder[i]] = i;
                }

                var itemOrderSorted = _itemOrder.OrderBy(i => dict.HasOwnProperty(i) ? dict[i] : int.MaxValue).Distinct().ToList();

                if (!_itemOrder.SequenceEqual(itemOrderSorted))
                {
                    _items.Value = _items.Value.OrderBy(i => dict.HasOwnProperty(i.Identifier) ? dict[i.Identifier] : int.MaxValue).ToArray();
                    _itemOrder   = itemOrderSorted;
                }
            }

            foreach (var item in _items.Value)
            {
                if (item is SidebarNav nav)
                {
                    nav.LoadSorting(itemOrder);
                }
            }
        }

        public Dictionary<string, string[]> GetCurrentSorting()
        {
            var dict = new Dictionary<string, string[]>();

            dict[Identifier] = _itemOrder.Distinct().ToArray();

            foreach (var sidebarItem in _items.Value)
            {
                if (sidebarItem is SidebarNav sidebarNav)
                {
                    foreach (var sorting in sidebarNav.GetCurrentSorting())
                    {
                        dict[sorting.Key] = sorting.Value;
                    }
                }
            }

            return dict;
        }

        public SidebarNav Sortable(bool sortable = true, string sortableGroup = null)
        {
            _isSortable = sortable;
            _sortableGroup = sortable ? sortableGroup : null;

            foreach (var sidebarItem in _items.Value)
            {
                if (sidebarItem is SidebarNav sidebarNav)
                {
                    sidebarNav.Sortable(sortable);

                    if (sortable)
                    {
                        sidebarNav.OnSortingChanged(newItemOrder =>
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

            return this;
        }

        public SidebarNav OnSortingChanged(Action<Dictionary<string, string[]>> onSortingChanged)
        {
            _onSortingChanged += onSortingChanged;
            return this;
        }

        public SidebarNav OnParentChanged(Action<ParentChangedEvent> onParentChanged)
        {
            _onParentChanged += onParentChanged;
            return this;
        }

        public class ParentChangedEvent
        {
            public SidebarNav Item { get; set; } 
            public SidebarNav To { get; set; } 
            public SidebarNav From { get; set; } 
            public int OldIndex { get; set; }
            public int NewIndex { get; set; }
            public Action Cancel { get; set; }
        }
    }
}