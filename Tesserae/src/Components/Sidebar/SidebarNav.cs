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

        private List<string> _itemOrder = new List<string>();

        private event Action<HTMLElement> _onRendered;

        public bool IsCollapsed { get { return _collapsed.Value; } set { _collapsed.Value = value; } }
        public bool IsSelected  { get { return _selected.Value; }  set { _selected.Value  = value; } }

        public IObservable<bool> CollapsedStatus => _collapsed;

        private IComponent _lastClosed;
        private IComponent _lastOpen;

        public IComponent CurrentRendered => (_lastClosed is object && _lastClosed.IsMounted()) ? _lastClosed : _lastOpen;

        public SidebarNav(string identifier, Emoji  icon, string       text,   bool   initiallyCollapsed, params SidebarCommand[] commands) : this(identifier, $"ec {icon}", text, initiallyCollapsed, commands) { }
        public SidebarNav(string identifier, UIcons icon, string       text,   bool   initiallyCollapsed, params SidebarCommand[] commands) : this(identifier, Icon.Transform(icon, UIconsWeight.Regular), text, initiallyCollapsed, commands) { }
        public SidebarNav(string identifier, UIcons icon, UIconsWeight weight, string text,               bool                    initiallyCollapsed, params SidebarCommand[] commands) : this(identifier, Icon.Transform(icon, weight), text, initiallyCollapsed, commands) { }

        private const string collapse_local_storage_save_key = "tss_sidebar_nav_collapse_state_";

        public SidebarNav(string identifier, string icon, string text, bool initiallyCollapsed, params SidebarCommand[] commands)
        {
            Identifier    = identifier;
            _text         = text;
            _closedHeader = Button().SetIcon(icon).Class("tss-sidebar-nav-header").Class("tss-sidebar-btn");
            _openHeader   = Div(_("tss-sidebar-nav-header tss-sidebar-btn-open tss-sidebar-nav-header-empty"));

            _arrow = Button().Class("tss-sidebar-nav-arrow");

            _openHeaderButton = Button(text).SetIcon(icon).Class("tss-sidebar-nav-button");
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
                var collapseState = localStorage.getItem(collapse_local_storage_save_key + identifier);

                if (bool.TryParse(collapseState, out var cs))
                {
                    _collapsed.Value = cs;
                }

                _collapsed.ObserveFutureChanges(c =>
                {
                    localStorage.setItem(collapse_local_storage_save_key + identifier, c.ToString());
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

        public SidebarNav SetText(string text)
        {
            _openHeaderButton.SetText(text);
            _closedHeader.Tooltip(text, placement: TooltipPlacement.Top);
            _text = text;
            return this;
        }

        public SidebarNav SetIcon(string icon)
        {
            _openHeaderButton.SetIcon(icon);
            _closedHeader.SetIcon(icon);
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

            var nav = Div(_("tss-sidebar-nav"));
            nav.appendChild(_openHeader);

            var children = VStack();

            if (_isSortable)
            {
                var sortable = new Sortable(children.Render(), new SortableOptions()
                {
                    animation  = 150,
                    ghostClass = "tss-sortable-ghost",
                    onEnd = e =>
                    {
                        var old = _itemOrder[e.oldIndex];
                        _itemOrder[e.oldIndex] = _itemOrder[e.newIndex];
                        _itemOrder[e.newIndex] = old;
                        _onSortingChanged?.Invoke(GetCurrentSorting());
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

        private IComponent RenderClosed(IReadOnlyList<ISidebarItem> items)
        {
            _closedHeader.Tooltip(_text, placement: TooltipPlacement.Top);

            var nav = Div(_("tss-sidebar-nav"));
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
            var newItems = _items.Value.As<ISidebarItem[]>();
            newItems.Push(item);
            _items.Value = newItems;
            _itemOrder.Add(item.Identifier);
        }

        public IComponent RenderClosed() => _closedContent();

        public IComponent RenderOpen() => _openContent();

        public ISidebarItem OnRendered(Action<HTMLElement> onRendered)
        {
            _onRendered += onRendered;
            return this;
        }

        public string Identifier { get; private set; }
        public void AddGroupIdentifier(string groupIdentifier)
        {
            Identifier = groupIdentifier + "_|_" + Identifier;
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

        public void Sortable(bool sortable = true)
        {
            _isSortable = sortable;

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
        }

        public void OnSortingChanged(Action<Dictionary<string, string[]>> onSortingChanged)
        {
            _onSortingChanged += onSortingChanged;
        }
    }
}