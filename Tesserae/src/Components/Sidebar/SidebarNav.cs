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
    /// <summary>
    /// A hierarchical navigation component for use within a Sidebar,
    /// allowing for nested ISidebarItem elements.
    /// </summary>
    public class SidebarNav : ISearchableSidebarItem
    {
        /// <summary>Event fired when sorting changes within the navigation.</summary>
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

        /// <summary>Gets or sets whether the navigation is collapsed.</summary>
        public bool IsCollapsed { get { return _collapsed.Value; } set { _collapsed.Value = value; } }
        /// <summary>Gets or sets whether the navigation is currently selected.</summary>
        public bool IsSelected  { get { return _selected.Value; }  set { _selected.Value  = value; if (value) CurrentRendered.ScrollIntoView();} }

        /// <summary>Gets an observable for the collapsed status.</summary>
        public IObservable<bool> CollapsedStatus => _collapsed;

        private IComponent _lastClosed;
        private IComponent _lastOpen;

        /// <summary>Gets the component that is currently rendered.</summary>
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

        /// <summary>Shows the navigation component.</summary>
        public void Show()
        {
            _lastOpen?.Show();
            _lastClosed?.Show();
            _isHidden = false;
        }

        /// <summary>Collapses the navigation component.</summary>
        public void Collapse()
        {
            _lastOpen?.Collapse();
            _lastClosed?.Collapse();
            _isHidden = true;
        }
        /// <summary>
        /// Marks the navigation component as not sortable.
        /// </summary>
        /// <returns>The current instance of the type.</returns>
        public SidebarNav NotSortable()
        {
            _lastOpen?.Class("tss-sortable-disable");
            _lastClosed?.Class("tss-sortable-disable");
            _notSortable = true;
            return this;
        }

        /// <summary>
        /// Sets the header text of the navigation.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>The current instance of the type.</returns>
        public SidebarNav SetText(string text)
        {
            _openHeaderButton.SetText(text);
            _closedHeader.Tooltip(text, placement: TooltipPlacement.Top);
            _text = text;
            return this;
        }

        /// <summary>
        /// Sets whether the navigation is collapsed.
        /// </summary>
        /// <param name="isCollapsed">Whether it is collapsed.</param>
        /// <returns>The current instance of the type.</returns>
        public SidebarNav Collapsed(bool isCollapsed = true)
        {
            _collapsed.Value = isCollapsed;
            return this;
        }

        /// <summary>
        /// Sets whether the navigation is selected.
        /// </summary>
        /// <param name="isSelected">Whether it is selected.</param>
        /// <returns>The current instance of the type.</returns>
        public SidebarNav Selected(bool isSelected = true)
        {
            _selected.Value = isSelected;
            return this;
        }

        /// <summary>
        /// Toggles the collapsed state.
        /// </summary>
        /// <returns>The current instance of the type.</returns>
        public SidebarNav Toggle()
        {
            _collapsed.Value = !_collapsed.Value;
            return this;
        }

        /// <summary>
        /// Shows a dot in the header if the navigation is empty.
        /// </summary>
        /// <returns>The current instance of the type.</returns>
        public SidebarNav ShowDotIfEmpty()
        {
            _openHeader.classList.add("tss-sidebar-nav-header-dot-if-empty");
            return this;
        }

        /// <summary>
        /// Adds a click event handler to the open header icon.
        /// </summary>
        /// <param name="action">The event handler action.</param>
        /// <returns>The current instance of the type.</returns>
        public SidebarNav OnOpenIconClick(Action<HTMLElement, MouseEvent> action)
        {
            _openHeaderButton.OnIconClick(action);
            _openHeaderButton.Class("tss-sidebar-btn-has-icon-click");
            return this;
        }

        /// <summary>
        /// Adds a click event handler to the open header icon.
        /// </summary>
        /// <param name="action">The event handler action.</param>
        /// <returns>The current instance of the type.</returns>
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

        /// <summary>
        /// Adds a click event handler to the navigation header.
        /// </summary>
        /// <param name="action">The event handler action.</param>
        /// <returns>The current instance of the type.</returns>
        public SidebarNav OnClick(Action action)
        {
            var wrapped = WrapAction(action);
            _closedHeader.OnClick(wrapped);
            _openHeaderButton.OnClick(wrapped);
            return this;
        }

        /// <summary>
        /// Adds a click event handler with SidebarNav argument.
        /// </summary>
        /// <param name="action">The event handler action.</param>
        /// <returns>The current instance of the type.</returns>
        public SidebarNav OnClick(Action<SidebarNav> action)
        {
            var wrapped = WrapAction(() => action(this));
            _closedHeader.OnClick(wrapped);
            _openHeaderButton.OnClick(wrapped);
            return this;
        }
        /// <summary>
        /// Adds a click event handler with button and mouse event arguments.
        /// </summary>
        /// <param name="action">The event handler action.</param>
        /// <returns>The current instance of the type.</returns>
        public SidebarNav OnClick(Action<Button, MouseEvent> action)
        {
            var wrapped = WrapAction(action);
            _closedHeader.OnClick((b,     e) => wrapped(b, e));
            _openHeaderButton.OnClick((b, e) => wrapped(b, e));
            return this;
        }

        /// <summary>
        /// Adds a context menu event handler.
        /// </summary>
        /// <param name="action">The event handler action.</param>
        /// <returns>The current instance of the type.</returns>
        public SidebarNav OnContextMenu(Action action)
        {
            _closedHeader.OnContextMenu(action);
            _openHeaderButton.OnContextMenu(action);
            return this;
        }

        /// <summary>
        /// Adds a context menu event handler with button and mouse event arguments.
        /// </summary>
        /// <param name="action">The event handler action.</param>
        /// <returns>The current instance of the type.</returns>
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


        /// <summary>
        /// Clears all items from the navigation.
        /// </summary>
        public void Clear()
        {
            _items.Value = new ISidebarItem[] { };
        }

        /// <summary>
        /// Adds an item to the navigation.
        /// </summary>
        /// <param name="item">The item to add.</param>
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

        /// <summary>
        /// Removes an item from the navigation.
        /// </summary>
        /// <param name="item">The item to remove.</param>
        public void Remove(ISidebarItem item)
        {
            var identifierWithGroupIdentifier = Identifier + "_|_" + item.Identifier;

            var newItems = _items.Value.As<ISidebarItem[]>();
            newItems     = newItems.Where(i => i.Identifier != identifierWithGroupIdentifier).ToArray();
            _items.Value = newItems;
            _itemOrder.Remove(identifierWithGroupIdentifier);
        }

        /// <summary>
        /// Adds a range of items to the navigation.
        /// </summary>
        /// <param name="items">The items to add.</param>
        /// <returns>The current instance of the type.</returns>
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

        /// <summary>Renders the navigation for the closed state of the sidebar.</summary>
        public IComponent RenderClosed() => _closedContent();

        /// <summary>Renders the navigation for the open state of the sidebar.</summary>
        public IComponent RenderOpen() => _openContent();

        /// <summary>
        /// Adds a rendered event handler.
        /// </summary>
        /// <param name="onRendered">The rendered event handler.</param>
        /// <returns>The current instance of the type.</returns>
        public ISidebarItem OnRendered(Action<HTMLElement> onRendered)
        {
            _onRendered += onRendered;
            return this;
        }

        /// <summary>Gets the full identifier of the navigation component.</summary>
        public string Identifier { get; private set; }

        /// <summary>Gets the own identifier of the navigation component.</summary>
        public string OwnIdentifier => Sidebar.GetOwnIdentifier(Identifier);

        /// <summary>Adds a group identifier prefix to the navigation component's identifier.</summary>
        public void AddGroupIdentifier(string groupIdentifier)
        {
            Identifier = groupIdentifier + Sidebar.GroupIdentifierSeparator + Identifier;
        }

        /// <summary>
        /// Loads the sorting order for navigation items.
        /// </summary>
        /// <param name="itemOrder">A dictionary mapping group identifiers to ordered item identifiers.</param>
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

        /// <summary>
        /// Gets the current sorting order of all items within this navigation.
        /// </summary>
        /// <returns>A dictionary mapping group identifiers to ordered item identifiers.</returns>
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

        /// <summary>
        /// Sets whether items within this navigation are sortable.
        /// </summary>
        /// <param name="sortable">Whether items are sortable.</param>
        /// <param name="sortableGroup">An optional group name for cross-nav sorting.</param>
        /// <returns>The current instance of the type.</returns>
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

        /// <summary>
        /// Adds a sorting change event handler.
        /// </summary>
        /// <param name="onSortingChanged">The event handler action.</param>
        /// <returns>The current instance of the type.</returns>
        public SidebarNav OnSortingChanged(Action<Dictionary<string, string[]>> onSortingChanged)
        {
            _onSortingChanged += onSortingChanged;
            return this;
        }

        /// <summary>
        /// Adds a parent changed event handler, for cross-nav sorting.
        /// </summary>
        /// <param name="onParentChanged">The event handler action.</param>
        /// <returns>The current instance of the type.</returns>
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

        public bool Search(string searchTerm)
        {
             if (string.IsNullOrWhiteSpace(searchTerm))
             {
                 Show();
                 foreach(var i in _items.Value)
                 {
                     if(i is ISearchableSidebarItem s) s.Search(searchTerm);
                     else i.Show();
                 }
                 return true;
             }

             bool anyChildMatch = false;
             foreach(var i in _items.Value)
             {
                 if (i is ISearchableSidebarItem s)
                 {
                     if(s.Search(searchTerm)) anyChildMatch = true;
                 }
                 else
                 {
                     i.Collapse();
                 }
             }

             bool selfMatch = _text.ToLower().Contains(searchTerm.ToLower());

             if (selfMatch || anyChildMatch)
             {
                 Show();
                 if (anyChildMatch && IsCollapsed)
                 {
                     Collapsed(false);
                 }
                 return true;
             }

             Collapse();
             return false;
        }
    }
}
